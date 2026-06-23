# MKWii Pack Maker V12.1.0

**MKWii Pack Maker** is a Windows desktop tool for creating **Mario Kart Wii HNS / Riivolution custom packs** with a simple workflow.

It helps pack makers:

1. Add custom tracks.
2. Add custom track names.
3. Add custom characters and vehicle files.
4. Add custom BRSTM music.
5. Pick optional custom cup icons.
6. Auto-name cups from custom cup icons.
7. Preview export paths and Riivolution XML before building.
8. Export a ready-to-use HNS / Riivolution pack for Wii, Wii U, Dolphin, or Riivolution-style setups.

The goal is to make Mario Kart Wii Hide and Seek pack creation easier without manually editing every folder, SZS archive, BMG text file, or Riivolution XML.

---

## Requirements

- Windows 10/11, might work on MacOS
- .NET 8 Desktop Runtime for the small EXE build
- A legally obtained Mario Kart Wii disc image or extracted Mario Kart Wii base files

The build package includes the required Wiimm tools in the `tools` folder:

```text
tools/wit.exe
tools/wszst.exe
tools/wbmgt.exe
tools/wimgt.exe
```

---

## How to Use

1. Open `MKWiiPackMaker.exe`.
2. Complete **Setup** by extracting or copying your base files.
3. Go to **Tracks** and add custom `.szs` tracks.
4. Use **Validate Track SZS** to check for common course internals like `course.kmp`, `course.kcl`, and `course_model.brres`.
5. Set custom track names if needed.
6. Go to **Characters** and import character packs.
7. Use **Check Conflicts** and **Character Summary** to find duplicate targets or incomplete character packs.
8. Go to **Music** and use the Music Slot Helper to assign normal/final-lap BRSTM files.
9. Use **Auto Pair Music** or **Music Report** to identify missing normal/final-lap pairs.
10. Go to **Advanced Files** for UI, REL, BRSAR, and other advanced assets. Use **Identify Files** to understand targets.
11. Go to **Cup Icons** and assign custom icons only for cups you want to change.
12. Go to **Dashboard** and set:
    - Pack Name
    - Pack ID / folder name
    - Riivolution Option Name
13. Go to **Export**.
14. Use **Preview Export**, **XML Preview**, and **Estimate Size**.
15. Click **Build Export** to build the HNS / Riivolution pack.

Empty cup icon slots keep the original Mario Kart Wii cup icon.

---

## What Works

- Custom track replacement
- Track SZS validation
- Custom track names through BMG patching
- Custom cup icons through UI SZS patching
- Automatic cup names from selected cup icon filenames
- Custom character model routing
- Character select model support
- Character tab character detection and icons
- Duplicate character target conflict checker
- Character pack summary / completeness report
- Character sounds through `revo_kart.brsar`
- Custom BRSTM music support
- Music slot helper for normal/final-lap music
- Auto music pair helper
- Music file identification report
- Advanced file identification for SZS / REL / BRSAR / BRRES files
- Built-in base file extractor for ISO, WBFS, RVZ, GCZ, WIA, WDF, CISO, and NKit ISO inputs
- Export progress window
- Export preview / dry run
- Riivolution XML preview
- Estimated final pack folder size
- Riivolution-ready export for Wii, Wii U, and emulator workflows
- Open last exported pack folder
- Project cleanup tool
- Public release safety check
- Logs for debugging

---

## Export Notes

The tool exports game-loadable files only. Loose PNG/TXT files are not treated as final MKWii files unless they are patched into real SZS/BMG/UI files.

The export can take time because the app may need to:

- copy clean base SZS files
- patch character UI sources into real UI SZS archives
- patch cup icons into menu UI archives
- patch track and cup names into BMG text
- rebuild SZS archives with Wiimm tools
- write Riivolution XML and reports

---

## GitHub / Publishing Notes

Do **not** commit or upload:

```text
base_files/Scene/
base_files/Race/
base_files/Sound/
Data/
logs/
release/
release_portable/
bin/
obj/
*_hns_riivolution/
*.wbfs
*.iso
*.rvz
*.gcz
*.wia
*.wdf
*.ciso
*.nkit.iso
```

Keep only:

```text
base_files/README.txt
```

Built release ZIPs should be uploaded under **GitHub Releases**, not committed as source files.

If `assets/character_icons` contains Mario Kart character artwork, only publish it if you have the right to share those files. Otherwise, remove them from the public repo and let users add icons locally.

---

## Logs

If something does not work, check:

```text
logs/latest.log
logs/startup_error.log
```

When reporting issues, include:

- what you imported
- what you expected
- what happened in-game
- `logs/latest.log`

---

## Credits

- Wiimm for Wiimms SZS / ISO toolsets
- HNS community testers and pack creators

---

## V12.1.0 Update

V12.1.0 adds expanded game image extraction support. The Setup extractor now accepts ISO, WBFS, RVZ, GCZ, WIA, WDF, CISO, and NKit ISO inputs.

It also updates the README, release notes, and safety checks so public releases clearly warn against uploading game image files or extracted Nintendo base files.

Dolphin auto-install is not included because the tool is intended to support Wii, Wii U, and Riivolution-style workflows.
