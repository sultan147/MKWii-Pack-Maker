# HNS Export Structure

MKWii Pack Maker 4.8.0 exports the HNS/Riivolution layout in the structured style shown by the supplied example pack.

```text
<export root>/
├─ hns/
│  └─ <pack id>/
│     ├─ Tracks/
│     │  ├─ beginner_course.szs
│     │  ├─ farm_course.szs
│     │  └─ ...
│     ├─ Music/
│     │  ├─ revo_kart.brsar
│     │  └─ Tracks/
│     │     ├─ custom_track_n.brstm
│     │     └─ custom_track_f.brstm
│     ├─ StaticR/
│     │  ├─ StaticE.rel
│     │  ├─ StaticJ.rel
│     │  ├─ StaticK.rel
│     │  └─ StaticP.rel
│     ├─ Menu/
│     │  ├─ Award.szs
│     │  ├─ Channel.szs
│     │  ├─ Event.szs
│     │  ├─ Globe.szs
│     │  ├─ MenuMulti.szs
│     │  ├─ MenuSingle.szs
│     │  ├─ MenuOther.szs
│     │  ├─ Race.szs
│     │  └─ Language/
│     │     ├─ Award_U.szs
│     │     ├─ Channel_U.szs
│     │     ├─ Event_U.szs
│     │     ├─ Globe_U.szs
│     │     ├─ MenuMulti_U.szs
│     │     ├─ MenuSingle_U.szs
│     │     ├─ MenuOther_U.szs
│     │     └─ Race_U.szs
│     ├─ Characters/
│     │  ├─ Karts/
│     │  ├─ Menu/
│     │  ├─ Text/
│     │  ├─ Vocies/
│     │  └─ Inject/
│     ├─ cup_icons/
│     └─ ui_patch_inputs/
└─ riivolution/
   └─ <pack id>.xml
```

## Correct MKWii disc paths

- Custom tracks: `/Race/Course/<slot>.szs`
- In-game kart/bike character files: `/Race/Kart/<file>.szs`
- Character selection driver model: `/Scene/Model/Driver.szs`
- Vehicle selection allkart files: `/Scene/Model/Kart/*-allkart.szs`
- Menu/Race/Award/Globe UI files: `/Scene/UI/<file>.szs`
- Streamed BRSTM music: `/sound/strm/<file>.brstm`
- BRSAR voice/sound archive: `/sound/revo_kart.brsar`

## Important UI notes

Loose PNG cup icons and TXT names are not loaded by Mario Kart Wii directly. Cup icons, character UI icons and minimap/race icons must be patched into UI SZS files. Track and character names must be patched into `Common.bmg` inside the language-specific UI SZS files.

The export writes helper files under `ui_patch_inputs/`, but the game-ready files are the patched SZS files imported through the UI / REL Assets tab.
