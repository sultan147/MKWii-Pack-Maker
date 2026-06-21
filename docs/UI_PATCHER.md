# UI Patcher Notes

MKWii does not load loose PNG cup icons or loose TXT track names.

Game-ready files must be patched SZS archives:

- Cup icons / character menu icons: regular `Award.szs`, `Channel.szs`, `Event.szs`, `Globe.szs`, `MenuMulti.szs`, `MenuSingle.szs`, `MenuOther.szs`, `Present.szs`, `Race.szs`.
- Race/minimap 32x32 icons: `Race.szs`.
- Track and character names: `Common.bmg` inside language files such as `MenuSingle_U.szs`, `MenuMulti_U.szs`, `Race_U.szs`, etc.
- Character select 3D model: `Driver.szs` at `/Scene/Model/Driver.szs`.
- Vehicle select models: `*-allkart.szs` at `/Scene/Model/Kart/`.
- In-game character/kart files: `/Race/Kart/`.

The UI Patcher page creates a structured workspace:

```text
01_base_ui_copy/
02_put_patched_szs_here/
03_source_inputs/
04_scripts/
```

After editing the SZS files with BrawlCrate/Wiimms SZS Tools, put the finished SZS files in `02_put_patched_szs_here` and click **Import Patched UI**. They will be added to UI / REL Assets and mapped to `/Scene/UI` during export.
