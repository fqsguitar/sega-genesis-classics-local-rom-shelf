# Patcher

This patcher is limited to the one original `Assembly-CSharp.dll` build whose SHA-256 is:

```text
2EBAEF80F9FCF1C0565B6E6120D6478804D72574A15E82931C5AE07D599D9A2B
```

It refuses unknown or already modified DLLs, creates a backup before writing, and restores the backup automatically if patching fails. Mono.Cecil 0.11.6 is downloaded from NuGet at build time and is not committed to this repository.

The supported build was tested successfully against a live installation: the patch completed with the same deterministic SHA-256 produced during isolated testing, the Game Room loaded, local shelf entries appeared, and a local game launched normally. Restoration was verified on an isolated copy; keep the generated backup until you have independently confirmed restoration on your system.

Run from PowerShell with the game closed:

```powershell
.\patcher\patch.ps1 -GameDirectory "G:\SteamLibrary\steamapps\common\Sega Classics"
```

Restore with:

```powershell
.\patcher\restore.ps1 -GameDirectory "G:\SteamLibrary\steamapps\common\Sega Classics"
```

Only use personal game dumps you legally own. This tooling does not include or create ROM/PAK content.
