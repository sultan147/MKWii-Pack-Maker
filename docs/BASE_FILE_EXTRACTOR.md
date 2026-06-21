# Base File Extractor

MKWii Pack Maker 10.3.2 adds a simple Setup-page extractor for users who already have a legally dumped Mario Kart Wii disc image.

## What it does

The Setup page now has an **Extract WBFS/ISO** button.

The user selects their own `.wbfs` or `.iso` file, then the app runs:

```cmd
wit.exe EXTRACT --psel DATA --pmode AUTO --overwrite --files "+/files/Scene/UI/*" --files "+/files/Scene/Model/*" --files "+/files/Scene/Model/Kart/*" --files "+/files/Race/Kart/*" --files "+/files/Race/Course/*" --files "+/files/Sound/*" --files "+/files/Sound/strm/*" "input.wbfs" "Data/DiscExtract/<timestamp>"
```

After extraction, only these folders are copied into `base_files`:

```text
Scene/UI
Scene/Model
Race/Kart
Race/Course
Sound
```

The app then refreshes the Setup status checks.

## Legal note

The app does not ship Nintendo base files and should not include any user-extracted files in GitHub. Users must provide their own legally obtained Mario Kart Wii dump.

## Required tool

`tools/wit.exe` must be present beside the other Wiimm tools. The project file now copies the `tools` folder into build output so `wit.exe`, `wszst.exe`, `wbmgt.exe`, and `wimgt.exe` stay beside the app after building.
