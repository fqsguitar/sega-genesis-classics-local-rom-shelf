# SEGA Genesis Classics Local ROM Shelf

Research and installation notes for restoring local-ROM shelf detection in the Windows release of **SEGA Mega Drive & Genesis Classics** (also installed as **Sega Classics**).

The Game Room contains an internal catalog of games and the presentation data needed to place their boxes on its 3D shelf. In the investigated build, `GameLoader.PopulateGames()` removes entries that the platform API does not report as owned. The tested modification keeps that behavior and additionally treats an expected local `.pak` as available.

> [!IMPORTANT]
> **No piracy. Bring your own legally owned games.** This project does not contain or distribute ROMs, SEGA artwork, game data, original or modified SEGA DLLs, keys, or download links for copyrighted content. Use only personal dumps of games you legally own. Testing for this research was performed with legally purchased games.

## Project status

The repository includes a **source-based patcher** for one verified Windows build. It validates the original DLL by SHA-256, creates a backup, performs a method-level edit, and refuses unknown or already modified builds. The patcher has been tested both on an isolated copy and against the corresponding live game installation. See [Patcher](patcher/README.md). Builds can differ, so never replace DLLs with binaries downloaded from another person.

## What the modification changes

The original shelf population flow uses catalog fields including:

- `GameData.mRomName` — expected local package name
- `GameData.mAppId` — platform/DLC ownership identifier
- `GameData.mShowOnShelf` — whether the entry is eligible for the shelf
- `GameData.mIsOwned` — runtime availability flag

The investigated logic effectively requires all relevant shelf predicates to pass. The tested change sets availability using:

```text
platform reports the game as owned OR the catalog-named local .pak exists
```

It does not add arbitrary games to the catalog, provide missing artwork, convert ROM dumps to the proprietary package format, or bypass the need to own the game.

## Requirements

- A legitimate installed copy of SEGA Mega Drive & Genesis Classics for Windows
- Legally obtained personal ROM dumps for the corresponding games
- dnSpyEx or another compatible .NET assembly editor
- Basic familiarity with copying files and restoring backups

The relevant assembly is normally below the game's install directory:

```text
Sega Classics/SEGAGameRoom_Data/Managed/Assembly-CSharp.dll
```

Steam libraries may be installed on a different drive. Use Steam's **Manage → Browse local files** action to locate the correct root.

## Read before changing anything

1. Close the game and Game Room.
2. Copy `Assembly-CSharp.dll` to a safe location outside the game directory.
3. Record the game version and optionally a SHA-256 hash of the original DLL.
4. Keep Steam's file verification available as a final recovery method.

See [Installation](docs/INSTALLATION.md), [Technical notes](docs/TECHNICAL.md), [Troubleshooting](docs/TROUBLESHOOTING.md), the [patcher](patcher/README.md), and the [PscdPack guide](EXTRAS/README.md).

## Repository policy

This repository is intentionally structured for original documentation, source code, and future patch definitions only. Do not submit:

- ROM, `.pak`, save, or BIOS files
- proprietary game DLLs, whether original or modified
- extracted SEGA artwork, audio, fonts, or other assets
- instructions or links for obtaining games without authorization

Future tooling should patch a user's own verified installation in place, create a backup, validate known versions/hashes, and offer restoration. It must not bundle proprietary input or output files.

## Compatibility and catalog count

The initial hands-on investigation referred to **57 supported slots/entries**. A later catalog extraction from the same research counted **58 primary records with `mShowOnShelf = true`**, plus 13 hidden regional records. The difference may be a counting convention, a special/duplicate entry, or build-specific data. Treat 57 as the observed working count and 58 as the extracted record count—not as a promise that every installation exposes the same set.

The shelf is catalog-driven: a correctly named local package can expose an existing entry, but a ROM for a title absent from `sGameData` will not automatically gain a box, metadata, or artwork.

## Tested result

The research successfully displayed **Alex Kidd in the Enchanted Castle** using the catalog-expected `g0011.pak`, alongside owned entries such as Comix Zone and Golden Axe. This statement documents the test; `g0011.pak` is not included here.

## License and trademarks

Original documentation and original source code in this repository are licensed under the [MIT License](LICENSE). The license does not apply to SEGA software, ROMs, artwork, names, trademarks, or other third-party material.

SEGA, Mega Drive, Genesis, Steam, and related names are trademarks of their respective owners. This is an unofficial, community research project and is not affiliated with or endorsed by SEGA or Valve.
