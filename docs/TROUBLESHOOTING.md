# Troubleshooting

## The game does not start after saving the DLL

- Restore the external backup before trying further edits.
- Confirm the edited module was saved as a DLL and retained its original name.
- Reopen the file in the editor and check whether `GameLoader.PopulateGames()` decompiles normally.
- If necessary, verify the installation through Steam to recover official files.
- Do not use a DLL from another game build.

## The editor reports `No overload for method 'Combine' takes 3 arguments`

The tested Unity/.NET profile lacked the three-argument overload. Nest two two-argument calls:

```csharp
System.IO.Path.Combine(
    System.IO.Path.Combine(parentDirectory, "data"),
    gameData.mRomName
)
```

## A local game does not appear

Check all of the following:

1. The package is in `<Sega Classics>/data`, not under `SEGAGameRoom_Data`.
2. The filename exactly matches the catalog's `mRomName`, including the `.pak` extension.
3. The corresponding catalog record has `mShowOnShelf = true`.
4. The edited ownership loop is actually present in the DLL currently loaded by the game.
5. The Game Room was fully restarted after copying or renaming the file.

A package named outside the catalog scheme does not create a record. In the documented test, `g1000.pak` did not map to Alex Kidd; the internal entry expected `g0011.pak`.

## A box appears but the game fails to launch

Shelf detection checks existence, not content correctness. The package may be malformed, regionally incompatible, incorrectly built, or otherwise unsuitable. Restore the DLL if necessary and test only with your own legally created content. This project does not currently validate or generate PAK files.

## Artwork or metadata is wrong

Artwork and metadata come from the existing internal catalog. The shelf modification does not add or replace those assets. A mismatched filename can select the wrong existing record; an uncataloged title has no automatic artwork path.

## Favorites or multiplayer results differ

`PopulateGames()` separately filters favorites and non-favorites, and multiplayer/matchmaking invokes `GameAvailableMultiplayer(...)`. The local-file availability change does not remove those later filters.

## The modification disappeared

Steam verification, repair, or a game update can replace `Assembly-CSharp.dll`. Compare hashes and inspect the method again. Reapply an edit only after making a fresh backup of the newly installed official DLL and confirming the build is compatible.

## The shelf count is 57, 58, or something else

The research first observed 57 supported slots/entries, while a later extraction counted 58 primary `mShowOnShelf = true` records plus hidden regional variants. Counts can reflect build differences or counting conventions. Report the game version, DLL hash, how records were counted, and whether hidden/alternate records were included.

## How to report a useful issue

Include:

- Windows and game build information
- SHA-256 of the unmodified DLL (never attach the DLL)
- dnSpyEx version
- exact error text or screenshot
- expected catalog filename and observed behavior
- whether restoration fixes the problem

Do not attach ROMs, PAKs, SEGA assets, or proprietary binaries.
