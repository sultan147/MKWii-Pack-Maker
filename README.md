[README.md](https://github.com/user-attachments/files/29169506/README.md)
# MKWii Pack Maker V11

**MKWii Pack Maker** is a Windows desktop tool for creating Mario Kart Wii HNS / Riivolution custom packs with a simple workflow.

It helps pack makers:

1. Add custom tracks.
2. Add custom track names.
3. Add custom characters and vehicle files.
4. Add custom BRSTM music.
5. Pick optional custom cup icons.
6. Auto-name cups from custom cup icons.
7. Export a ready-to-use HNS / Riivolution pack for Dolphin or Riivolution.

The goal is to make Mario Kart Wii Hide and Seek pack creation easier without manually editing every folder, SZS archive, BMG text file, or Riivolution XML.

---

## Important Legal Notice

This tool does **not** include Nintendo game files.

Users must provide their own legally obtained Mario Kart Wii dump, such as their own `.wbfs` or `.iso`, or their own already extracted MKWii files.

Do **not** upload or share Nintendo base files such as:

- `.szs`
- `.brsar`
- `.brres`
- extracted `Scene`, `Race`, or `Sound` game folders
- `.wbfs` or `.iso` game images

The public GitHub repository should only include an empty `base_files` folder with instructions.

---

## Requirements

- Windows 10/11
- .NET 8 Desktop Runtime for the small EXE build
- A legally obtained Mario Kart Wii WBFS/ISO or extracted Mario Kart Wii base files

The build package includes the required Wiimm tools in the `tools` folder:

```text
tools/wit.exe
tools/wszst.exe
tools/wbmgt.exe
tools/wimgt.exe
```

---

## First-Time Setup

### Recommended Method: Use the Built-In Extractor

1. Open `MKWiiPackMaker.exe`.
2. Go to **Setup**.
3. Click **Extract base files from WBFS/ISO**.
4. Select your legally dumped Mario Kart Wii `.wbfs` or `.iso`.
5. Wait for the progress window to finish.
6. The app copies only the required base folders into `base_files`.

The app extracts only the required folders:

```text
base_files/
├─ Scene/
│  ├─ UI/
│  └─ Model/
│     └─ Kart/
├─ Race/
│  ├─ Kart/
│  └─ Course/
└─ Sound/
```

The original WBFS/ISO is not modified.

### Manual Method

If you already extracted your game, copy these folders into `base_files`:

```text
Scene\UI      -> base_files\Scene\UI
Scene\Model   -> base_files\Scene\Model
Race\Kart     -> base_files\Race\Kart
Race\Course   -> base_files\Race\Course
Sound          -> base_files\Sound
```

The software uses these files as clean base copies. It copies them into a temporary working folder before patching, so the original `base_files` are not edited.

---

## How to Use

1. Open `MKWiiPackMaker.exe`.
2. Complete **Setup** by extracting or copying your base files.
3. Go to **Tracks** and add custom `.szs` tracks.
4. Set custom track names if needed.
5. Go to **Characters** and import character packs.
6. Go to **Music** and use the Music Slot Helper to assign normal/final-lap BRSTM files.
7. Go to **Cup Icons** and assign custom icons only for the cups you want to change.
8. Go to **Dashboard** and set:
   - Pack Name
   - Pack ID / folder name
   - Riivolution Option Name
9. Go to **Export** and build the HNS / Riivolution pack.

Empty cup icon slots keep the original Mario Kart Wii cup icon.

---

## What Works

- Custom track replacement
- Custom track names through BMG patching
- Custom cup icons through UI SZS patching
- Automatic cup names from selected cup icon filenames
- Custom character model routing
- Character select model support
- Character tab character detection and icons
- Duplicate character target conflict checker
- Character sounds through `revo_kart.brsar`
- Custom BRSTM music support
- Music slot helper for normal/final-lap music
- Built-in WBFS/ISO base file extractor
- Export progress window
- Riivolution / Dolphin-ready export
- Open last exported pack folder
- Project cleanup tool
- Logs for debugging

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

## GitHub / Publishing Notes

Do not commit or upload:

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
```

Keep only:

```text
base_files/README.txt
```

Built release ZIPs should be uploaded under **GitHub Releases**, not committed as source files unless intentionally included.

---

## Credits

- Wiimm for Wiimms SZS / ISO toolsets
- HNS community testers and pack creators

This project is not affiliated with Nintendo.
