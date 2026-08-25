# PscdPack

[PscdPack](https://github.com/GMMan/PscdPack) is a third-party utility by **GMMan** for opening and creating the `.pak` files used by SEGA Genesis Classics. It is not authored, maintained, or distributed by this project.

## Download and attribution

Download PscdPack from the author's official GitHub release page:

- Repository: <https://github.com/GMMan/PscdPack>
- Latest published release: [v1.0r3](https://github.com/GMMan/PscdPack/releases/tag/v1.0r3)
- Release list: <https://github.com/GMMan/PscdPack/releases>

The `PscdPack.exe` used during this project's testing reported version `1.0.0.0` and had this SHA-256:

```text
F3D58ADE414FE0A89F873F17149FD681B82864737371930291389A62AA942EFB
```

Verify the file you download yourself. A different official release may legitimately have a different hash.

> PscdPack's upstream repository does not currently display an explicit software license. For that reason, this project links to the author's release instead of redistributing `PscdPack.exe`. Please obtain the utility directly from its author.

## Suggested location

Create or use this directory inside the game installation:

```text
Sega Classics/EXTRAS/
```

Place your officially downloaded copy at:

```text
Sega Classics/EXTRAS/PscdPack.exe
```

The tool does not have to be inside `EXTRAS` to work; this location simply keeps third-party utilities separate from official game files and from this repository's patcher.

## Create a package for a legally owned ROM

1. Close SEGA Genesis Classics and back up any existing `.pak` you intend to replace.
2. Run `EXTRAS/PscdPack.exe`.
3. Select **New/Open**.
4. Choose a working location and enter the exact catalog filename expected by the Game Room, such as `g0011.pak` for the documented Alex Kidd test.
5. Select **Replace ROM** and choose a raw BIN-format dump made from a game you legally own.
6. Review the detected name and region. Configure SRAM or EEPROM only when the cartridge requires it; consult PscdPack's upstream README for those fields.
7. Select **Save**, then **Close** so the package is no longer locked by PscdPack.
8. Copy the finished package to:

   ```text
   Sega Classics/data/
   ```

9. Start the Game Room and verify the corresponding catalog entry.

The package filename must match an existing internal `GameData.mRomName`. PscdPack can create a valid container, but it does not create a new Game Room catalog record, artwork, or metadata.

## Extract a ROM from your own package

1. Run `PscdPack.exe` and select **New/Open**.
2. Open a `.pak` from your own legally purchased installation.
3. Select **Extract ROM**.
4. Save the extracted ROM somewhere outside the game installation and keep it private.

## Legal notice

Use PscdPack only with games you legally own and personal dumps you are authorized to make. Do not upload or share ROMs or `.pak` files. This repository does not provide game content and does not endorse piracy.
