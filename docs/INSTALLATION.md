# Installation

## Legal and safety requirements

Proceed only with a legitimate game installation and personal dumps of games you legally own. This repository supplies no ROMs, PAKs, SEGA assets, or DLL binaries. Modifying game files is unsupported by the publisher and is done at your own risk.

## 1. Locate the installation

In Steam, open the title's properties or **Manage → Browse local files**. The target is:

```text
<Steam library>/steamapps/common/Sega Classics/SEGAGameRoom_Data/Managed/Assembly-CSharp.dll
```

The local package directory used by the tested logic is:

```text
<Steam library>/steamapps/common/Sega Classics/data/
```

## 2. Create a recoverable backup

Close the game, Game Room, Steam launch processes for the title, and your assembly editor. Copy the target DLL outside the installation directory, for example:

```text
backups/<game-version>/Assembly-CSharp.dll
```

Optionally calculate a SHA-256 hash in PowerShell:

```powershell
Get-FileHash -Algorithm SHA256 "C:\path\to\Assembly-CSharp.dll"
```

Keep the original filename in the backup. Do not commit the backup to this repository or share it.

## 3. Apply the documented edit manually

1. Open your own `Assembly-CSharp.dll` in dnSpyEx.
2. Navigate to `GameLoader` → `PopulateGames()`.
3. Choose **Edit Method (C#)**.
4. Find the first loop that queries `DistributionAPI.IsDLCOwned(...)` and updates `mIsOwned`.
5. Adapt that loop so `mIsOwned` is true when the platform reports ownership **or** the catalog-named file exists under the sibling `data` directory. Use the reviewed excerpt in [Technical notes](TECHNICAL.md) as a reference.
6. Compile the method. Resolve errors before saving; decompiler warnings alone may not indicate failure.
7. Save the module as a DLL over the installed `Assembly-CSharp.dll` only after confirming your external backup exists.

Do not replace your DLL with one downloaded from another person. Different builds can be incompatible, and redistributing that binary is outside this project's policy.

## 4. Supply your own local game package

Place only a package you created from a game you legally own in the game's `data` directory. Its filename must exactly match `mRomName` in an existing internal catalog record. The documented Alex Kidd test used the catalog name `g0011.pak`; no such file is included here.

This project currently does not document or provide a ROM-to-PAK converter. Do not obtain packages from unauthorized sources.

## 5. Test

1. Start the game normally.
2. Enter the Game Room and inspect the shelf.
3. Confirm previously owned Steam entries still appear.
4. Confirm only the expected local catalog entry appears.
5. Launch and test cautiously; shelf appearance alone does not prove full compatibility.

If the game fails or the shelf is wrong, close it and restore immediately.

## Restoration

With the game closed, copy your backed-up original DLL back to:

```text
Sega Classics/SEGAGameRoom_Data/Managed/Assembly-CSharp.dll
```

If the backup is unavailable, use Steam's **Verify integrity of game files** feature. Verification may restore official files and remove the modification. Recheck any other local changes separately; do not assume verification preserves them.

After restoration, compare the SHA-256 hash with the value recorded before editing, then start the game and verify its normal shelf behavior.
