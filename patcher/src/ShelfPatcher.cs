using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class ShelfPatcher
{
    private const string SupportedOriginalSha256 = "2EBAEF80F9FCF1C0565B6E6120D6478804D72574A15E82931C5AE07D599D9A2B";

    private static int Main(string[] args)
    {
        if (args.Length != 2 || args[0] != "patch")
        {
            Console.Error.WriteLine("Usage: ShelfPatcher.exe patch <Assembly-CSharp.dll>");
            return 2;
        }

        string input = Path.GetFullPath(args[1]);
        if (!File.Exists(input))
        {
            Console.Error.WriteLine("Target DLL was not found: " + input);
            return 3;
        }

        string actualHash = Sha256(input);
        if (!actualHash.Equals(SupportedOriginalSha256, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Unsupported or already modified DLL.");
            Console.Error.WriteLine("Expected: " + SupportedOriginalSha256);
            Console.Error.WriteLine("Actual:   " + actualHash);
            return 4;
        }

        string temporary = input + ".local-rom-shelf.tmp";
        try
        {
            PatchAssembly(input, temporary);
            VerifyPatchedAssembly(temporary);
            File.Copy(temporary, input, true);
            Console.WriteLine("Patched successfully.");
            Console.WriteLine("SHA-256: " + Sha256(input));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Patch failed: " + ex.Message);
            return 5;
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static void PatchAssembly(string input, string output)
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(input));
        var reader = new ReaderParameters { AssemblyResolver = resolver, ReadWrite = false, InMemory = true };

        using (var assembly = AssemblyDefinition.ReadAssembly(input, reader))
        {
            ModuleDefinition module = assembly.MainModule;
            TypeDefinition loader = RequireType(module, "GameLoader");
            TypeDefinition gameData = RequireType(module, "GameData");
            MethodDefinition populate = loader.Methods.Single(m => m.Name == "PopulateGames" && !m.HasParameters);
            FieldDefinition gameDictionary = loader.Fields.Single(f => f.Name == "sGameData");
            FieldDefinition platformManager = loader.Fields.Single(f => f.Name == "mPlatformManager");
            FieldDefinition appId = gameData.Fields.Single(f => f.Name == "mAppId");
            FieldDefinition romName = gameData.Fields.Single(f => f.Name == "mRomName");
            FieldDefinition isOwned = gameData.Fields.Single(f => f.Name == "mIsOwned");

            MethodReference dictionaryGetItem = FindCalledMethod(populate, "System.Collections.Generic.Dictionary`2<System.Int32,GameData>", "get_Item");
            MethodReference getDistributionApi = FindCalledMethod(populate, "PlatformManager", "get_DistributionAPI");
            MethodReference isDlcOwned = FindCalledMethod(populate, "DistributionInterface", "IsDLCOwned");
            MethodReference getDataPath = FindMethodReference(module, "UnityEngine.Application", "get_dataPath");

            MethodReference getDirectoryName = module.ImportReference(typeof(Path).GetMethod("GetDirectoryName", new[] { typeof(string) }));
            MethodReference combine = module.ImportReference(typeof(Path).GetMethod("Combine", new[] { typeof(string), typeof(string) }));
            MethodReference exists = module.ImportReference(typeof(File).GetMethod("Exists", new[] { typeof(string) }));

            IList<Instruction> instructions = populate.Body.Instructions;
            int start = IndexOfOwnershipBlock(instructions, platformManager, isOwned);
            int end = IndexOfOwnershipStore(instructions, start, isOwned);
            Instruction loopIncrement = instructions[end + 1];
            var il = populate.Body.GetILProcessor();

            var localExists = il.Create(OpCodes.Call, exists);
            var setOwned = il.Create(OpCodes.Stfld, isOwned);
            var ownedTrue = il.Create(OpCodes.Ldc_I4_1);
            var replacement = new List<Instruction>
            {
                il.Create(OpCodes.Ldsfld, gameDictionary),
                il.Create(OpCodes.Ldloc_1),
                il.Create(OpCodes.Callvirt, dictionaryGetItem),
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Ldfld, platformManager),
                il.Create(OpCodes.Callvirt, getDistributionApi),
                il.Create(OpCodes.Ldsfld, gameDictionary),
                il.Create(OpCodes.Ldloc_1),
                il.Create(OpCodes.Callvirt, dictionaryGetItem),
                il.Create(OpCodes.Ldfld, appId),
                il.Create(OpCodes.Callvirt, isDlcOwned),
                il.Create(OpCodes.Brtrue, ownedTrue),
                il.Create(OpCodes.Call, getDataPath),
                il.Create(OpCodes.Call, getDirectoryName),
                il.Create(OpCodes.Ldstr, "data"),
                il.Create(OpCodes.Call, combine),
                il.Create(OpCodes.Ldsfld, gameDictionary),
                il.Create(OpCodes.Ldloc_1),
                il.Create(OpCodes.Callvirt, dictionaryGetItem),
                il.Create(OpCodes.Ldfld, romName),
                il.Create(OpCodes.Call, combine),
                localExists,
                il.Create(OpCodes.Br, setOwned),
                ownedTrue,
                setOwned
            };

            Instruction oldStart = instructions[start];
            foreach (var instruction in instructions)
            {
                if (instruction.Operand == oldStart) instruction.Operand = replacement[0];
            }

            for (int i = end; i >= start; i--) il.Remove(instructions[i]);
            foreach (Instruction instruction in replacement) il.InsertBefore(loopIncrement, instruction);
            assembly.Write(output);
        }
    }

    private static int IndexOfOwnershipBlock(IList<Instruction> instructions, FieldDefinition platformManager, FieldDefinition isOwned)
    {
        for (int i = 0; i < instructions.Count; i++)
            if (instructions[i].OpCode == OpCodes.Ldarg_0 && i + 1 < instructions.Count && instructions[i + 1].Operand == platformManager)
                for (int j = i; j < Math.Min(i + 20, instructions.Count); j++)
                    if (instructions[j].Operand == isOwned) return i;
        throw new InvalidOperationException("Original ownership block was not found.");
    }

    private static int IndexOfOwnershipStore(IList<Instruction> instructions, int start, FieldDefinition isOwned)
    {
        for (int i = start; i < instructions.Count; i++)
            if (instructions[i].OpCode == OpCodes.Stfld && instructions[i].Operand == isOwned) return i;
        throw new InvalidOperationException("Original mIsOwned store was not found.");
    }

    private static TypeDefinition RequireType(ModuleDefinition module, string name)
    {
        TypeDefinition type = module.Types.SingleOrDefault(t => t.Name == name);
        if (type == null) throw new InvalidOperationException("Required type not found: " + name);
        return type;
    }

    private static MethodReference FindCalledMethod(MethodDefinition method, string declaringType, string name)
    {
        return method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>()
            .First(m => m.DeclaringType.FullName == declaringType && m.Name == name);
    }

    private static MethodReference FindMethodReference(ModuleDefinition module, string declaringType, string name)
    {
        MethodReference method = module.GetMemberReferences().OfType<MethodReference>()
            .FirstOrDefault(m => m.DeclaringType.FullName == declaringType && m.Name == name);
        if (method == null) throw new InvalidOperationException("Required method reference not found: " + declaringType + "::" + name);
        return method;
    }

    private static void VerifyPatchedAssembly(string path)
    {
        using (var assembly = AssemblyDefinition.ReadAssembly(path))
        {
            MethodDefinition method = RequireType(assembly.MainModule, "GameLoader").Methods
                .Single(m => m.Name == "PopulateGames" && !m.HasParameters);
            bool hasFileExists = method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>()
                .Any(m => m.DeclaringType.FullName == "System.IO.File" && m.Name == "Exists");
            if (!hasFileExists) throw new InvalidOperationException("Post-write verification failed.");
        }
    }

    private static string Sha256(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
    }
}
