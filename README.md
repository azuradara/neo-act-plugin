# Blade & Soul NEO - ACT Plugin & DPS Meter

> [!NOTE]
> [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/azuradara)
>
> If you find this plugin helpful, please consider supporting its development with a small donation. Every contribution helps maintain and improve the plugin!

This is an [ACT](https://advancedcombattracker.com/home.php) plugin for Blade & Soul NEO that parses combat logs and sends them to ACT for processing. It also includes an overlay that you can use to display your DPS in-game.

![image](https://github.com/user-attachments/assets/766a99c0-7986-4164-8e9e-709ab4b0db77)
![image](https://github.com/user-attachments/assets/9e32022f-e340-4d0b-934b-883d810b7702)

## Usage

### Installation

If you're updating from a previous version (pre `v0.2.0`), make sure to delete the old `NeoActPlugin.cs` from your list of plugins on ACT before you install this one.

1. Download the latest `.7z` or `.zip` archive from [Releases](https://github.com/azuradara/neo-act-plugin/releases).
2. Extract the contents of the archive to a folder of your choice.
3. Run ACT as an administrator, otherwise the plugin will not be able to access your game's memory.
4. Go to the "Plugins" tab and click the "Browse" button then locate `NeoActPlugin.dll` in the folder you extracted the archive to.
5. Enable the plugin and run BnS.
6. In the `NeoActPlugin.dll` tab, choose your region (regions that are not `Global` require an **English Patch** to work).
7. Make sure you always run it with other characters **shown**, otherwise their crit hit rate will always be 0%.

### Adding overlays

This plugin is only responsible for parsing combat logs to a form that ACT can understand, if you want an overlay over your game you will have to install other plugins on top of this one. Here's a quick walkthrough of how to install our custom "Live-like" overlay:

1. Download the latest version of the ACT setup file from their [Downloads page](https://advancedcombattracker.com/download.php) and install it.

2. Open ACT and go to the "Plugins" tab.

3. Click "Get Plugins..." on the right and pick "Overlay Plugin" then click "Download and Enable".

4. Once finished, click the "OverlayPlugin.dll" tab and click "New" on the left.

5. Pick "Custom" from the Preset dropdown and then give it any name you'd like.

6. Select "MiniParse" from the Type dropdown and click OK.

7. Click the new overlay you just added on the sidebar on the left, and change the "URL" to: `https://azuradara.github.io/neo-act-plugin/overlays/live-like/`

8. Move it somewhere you like, then click "Enable clickthrough" to prevent your mouse from clicking it while you're playing.

9. For the time being, overlay doesn't recognize BNSR.exe as being the game client, so to make it always show up when you're in-game, you will have to disable the "Automatically hide overlays when the game is in the background" option in Plugins > OverlayPlugin.dll > General.
  
10. If you would like to configure hotkeys for locking/unlocking and toggling visibility, click the overlay you just added on the sidebar and go to the "Hotkeys" tab. Then click "Add new hotkey", select your action, and bind it to whatever key you'd like.

The overlay will update automatically every time we push something new to this repository.

There are tons of overlay configuration videos on YouTube - mostly related to FFXIV - but it's the same principle, overlays should be mostly game-agnostic.

### Troubleshooting

- The parser does not read DPS and/or reads logs from a different chat (e.g. faction chat).
  - Fix: Click "Chat Tab Options" (the cog icon on the left of ur chat tabs) > Reset > All Settings. This is because the plugin reads from the default combat chat tab and does not support custom ones.
- It keeps spewing "Failed to reolve pointer chain" errors:
  - Make sure ACT is running as administrator.
  - Make sure you have selected the correct region.
  - Make sure you have the latest version of the plugin.

### How to add the overlay to OBS (for streamers)

1. In ACT, go to Plugins > OverlayPlugin WSServer > Stream/Local overlay.
2. Keep the default settings, click "Start".
3. Add a browser source in OBS and paste this URL, making sure the substitue the host and port if you changed the default settings.: `https://azuradara.github.io/neo-act-plugin/overlays/live-like?HOST_PORT=ws://127.0.0.1:10501/`.
4. Enable "Refresh browser when scene becomes active" in the browser source settings.

## Limitations

- This is a POC - it works fine but it's missing support for a lot of features provided by ACT (e.g. blocks, buffs, debuffs, etc..).
- If you have characters hidden (CTRL+F, either partially or fully), the combat log does not specify if their damage is a crit or not, so their %CH will always be 0, but the damage itself will be parsed correctly.
- Only supports EU/NA with English localization, or JP/TW with EN Patch.
- Damage over time skills and damage from effects that have no explicit actor in the combat log will be attributed to an "Unknown" actor.
- Zone names are not provided in the combat log, so all logs are combined into a single zone on ACT.
- This will break every time NC updates the .exe, which means you will have to wait a while until I (or a benevolent soul) update the offsets.
- Overlay does not show skills breakdown like in live, you will have to use the ACT main window for that until we add it.
- ~~It doesn't distinguish between encounters automatically, you will have to end/start encounters from ACT manually.~~
- ~~Sometimes the game goes crazy, probably because I skipped refreshing pointers cuz I was lazy but I'll fix that later.~~
- ~~Does not distinguish between crits and non-crits.~~
- ~~Skills with apostrophes are clunky.~~

## Roadmap

- Fix the limitations
- Find more limitations
- Repeat

## Alternatives

If you don't want to use ACT, or prefer to go with something easier to set up, you can join <a href="https://discord.gg/gMsCaHhtzv" target="_blank">Caera's Discord</a> for a non-free but standalone alternative.

## Contributing

I'm too lazy to write a contribution guidelines doc, feel free to submit PRs or issues if you'd like. This project will always be free.
If you don't want to submit an issue, contact `azuradara` on Discord.

### Building from source

Follow these instructions if you would like to build the plugin from source or modify it. If you only want to install it, check the [Installation](#installation) section.

- Install Visual Studio 2019 or later and .NET desktop workload with .NET 4.8.1 SDK.
- Clone this repository or download the source code as an archive.
- Run `tools/fetch_deps.py` to download the required dependencies.
- Run `build.ps1` to build the project.

Once done, the `NeoActPlugin.dll` will be in the `out/` folder, either in `Release` or `Debug` depending on the configuration you built it with.

## License

MIT License, see [LICENSE](LICENSE) for more information.
