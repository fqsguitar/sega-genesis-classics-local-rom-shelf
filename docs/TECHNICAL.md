# Technical notes

## Scope

These notes describe observed behavior in one Windows build of SEGA Mega Drive & Genesis Classics. Names come from decompiling the user's legally installed `Assembly-CSharp.dll`. They are research observations, not a stable public API.

## Relevant components

The investigated control flow was:

```text
GameLibrary / catalog initialization
  → GameLoader.sGameData (indexed GameData records)
  → GameLoader.PopulateGames()
  → SetGenesisGame(...)
  → a box instance on the Game Room shelf
```

`GameData` was observed to contain fields such as:

| Field | Observed role |
| --- | --- |
| `mGameID` | Internal game identifier |
| `mBoxArt` | Internal presentation/artwork key |
| `mRomName` | Expected package filename, such as `g0011.pak` |
| `mGameName` | Display name |
| `mAppId` | Platform/DLC ownership identifier |
| `mIsOwned` | Runtime ownership/availability state |
| `mIsFavourite` | Saved favorite state |
| `mShowOnShelf` | Catalog eligibility for normal shelf display |
| `mROMRegion`, `mAlternateROMs` | Region and alternate-ROM relationships |

Defaults observed in the decompiled `GameData` class included `mIsOwned = true` and `mShowOnShelf = true`. `PopulateGames()` then changed availability based on the platform ownership response.

## `PopulateGames()` filtering

The observed method performs three important jobs:

1. Clears the current shelf and updates each catalog record's ownership state.
2. Adds favorite games that pass `mShowOnShelf && mIsOwned && mIsFavourite`.
3. Adds non-favorites that pass `mShowOnShelf && mIsOwned && !mIsFavourite`.

Multiplayer/matchmaking modes apply an additional `GameAvailableMultiplayer(...)` test. Successful entries are passed to `SetGenesisGame(...)`; skipped entries affect the calculated shelf index.

The original ownership loop only changed records to unavailable when `DistributionAPI.IsDLCOwned(mAppId)` returned false. The tested edit recalculated availability as:

```csharp
GameData gameData = GameLoader.sGameData[i];
bool steamOwned =
    this.mPlatformManager.DistributionAPI.IsDLCOwned(gameData.mAppId);

string pakPath = System.IO.Path.Combine(
    System.IO.Path.Combine(
        System.IO.Path.GetDirectoryName(Application.dataPath),
        "data"
    ),
    gameData.mRomName
);

bool localPakExists = System.IO.File.Exists(pakPath);
gameData.mIsOwned = steamOwned || localPakExists;
```

Nested two-argument `Path.Combine` calls were necessary in the tested older Unity/.NET profile; its compiler rejected a three-argument overload.

This excerpt is documentation of the locally tested method edit. It is not a complete replacement assembly and may require adaptation if a different build decompiles differently.

## Path resolution

For this Unity layout, `Application.dataPath` points at `SEGAGameRoom_Data`. Taking its parent and appending `data` produces the sibling directory:

```text
Sega Classics/data/<catalog mRomName>
```

The filename must match the existing catalog record. A random name does not create a new catalog entry.

## Catalog size: 57 observed, 58 extracted

The hands-on investigation initially described 57 supported shelf slots/entries. A subsequent extraction counted 58 primary records whose `mShowOnShelf` value was true, as well as 13 region-related hidden records (`g9001` through `g9013`) whose shelf flag was false.

This repository preserves both observations. Plausible explanations include a special or duplicate record, different definitions of “supported game,” or build/version differences. Until a reproducible catalog extractor and version hashes are published, do not treat either count as universal.

## Known test

The internal record for Alex Kidd in the Enchanted Castle expected `g0011.pak`. Once the ownership loop accepted local file presence and the package used that filename, its box appeared on the shelf. Earlier use of `g1000.pak` did not match the catalog and therefore did not produce the intended entry.

## Boundaries

This change affects shelf eligibility only. It does not:

- create `GameData` records for uncataloged games;
- supply or generate proprietary box art;
- document or implement ROM-to-PAK packaging;
- validate the contents or legal provenance of a package;
- guarantee launch, save, controller, regional, or multiplayer compatibility.

## Patcher implementation

The experimental patcher contains only original source and obtains Mono.Cecil from its official NuGet package at runtime. It accepts only the known original SHA-256, backs up the source DLL, locates the expected types, fields, calls, and ownership block structurally, writes to a temporary file, reopens the output, and verifies that the patched method calls `System.IO.File.Exists`. Unknown builds fail closed rather than receiving a fuzzy binary rewrite.

The patcher was tested on an isolated copy of the supported original DLL. The patch operation produced a readable assembly containing the expected local-file test; the restore operation reproduced the original SHA-256 exactly. Testing against the actual installed game remains a separate manual acceptance step.
