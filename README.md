This is an [ACT](https://advancedcombattracker.com/home.php) plugin for BnS NEO.

> [!WARNING]  
> This plugin connects to the game's memory to read combat log data, this is very much against the game's EULA. I am not sure whether the Anti-cheat detects memory reads, but, if it makes you feel better - I am using this personally on my main account.
>
> This is the first time I'm writing any .NET/C# code so don't make fun of me thx.
>
> It is also still not 100% accurate, things like DOT damage and skill names with apostrophes are inconsistent.

![image](https://github.com/user-attachments/assets/766a99c0-7986-4164-8e9e-709ab4b0db77)
![image](https://github.com/user-attachments/assets/9e32022f-e340-4d0b-934b-883d810b7702)

## Usage

### Installation

1. Download the latest `.cs` file from [Releases](https://github.com/azuradara/neo-act-plugin/releases).
2. Run ACT as an administrator, otherwise the plugin will not be able to access your game's memory.
3. Go to the "Plugins" tab and click the "Browse" button then locate the file you just downloaded and click OK.
4. Enable the plugin and run BnS.
5. Make sure you always run it with other characters **shown**, otherwise their crit hit rate will always 0%.

### Adding overlays

This plugin is only responsible for parsing combat logs to a form that ACT can understand, if you want an overlay over your game you will have to install other plugins on top of this one. Here's a quick walkthrough of how to install "Ember" (The only overlay I tested).

1. Download the latest version of the ACT setup file from their [Downloads page](https://advancedcombattracker.com/download.php) and install it.

2.  Open ACT and go to the "Plugins" tab.

3. Click "Get Plugins..." on the right and pick "Overlay Plugin" then click "Download and Enable".
   
4. Once finished, click the "OverlayPlugin.dll" tab and click "New" on the left.
   
5. Pick "Custom" from the Preset dropdown and then give it any name you'd like.
   
6. Select "MiniParse" from the Type dropdown and click OK.
    
7. Click the new overlay you just added on the sidebar on the left, and change the "URL" to:
  ```
  https://azuradara.github.io/neo-act-plugin/overlays/live-like/
  ```

8. Move it somewhere you like, then click "Enable clickthrough" to prevent your mouse from clicking it while you're playing.
    
9. For the time being, overlay doesn't recognize BNSR.exe as being the game client, so you will need to do some extra steps to make it stay on top:
  - Go to Plugins > OverlayPlugin.dll > General
  - Uncheck "Automatically hide overlays when the game is in the background"

10. If you would like to configure hotkeys for locking/unlocking and toggling visibility:
  - Click the overlay you just added on the sidebar and go to the "Hotkeys" tab.
  - Click "Add new hotkey", select your action, and bind it to whatever key you'd like.

The overlay will update automatically every time we push something new to this repository (unlike the plugin), so check back from time to time to see what changed.

There are tons of overlay configuration videos on YouTube - mostly related to FFXIV - but it's the same principal, overlays should be mostly game-agnostic.

You can even use one provided in our github from [Overlays](https://github.com/azuradara/neo-act-plugin/tree/main/Overlays)

### Troubleshooting

- The parser does not read DPS and/or reads logs from a different chat (e.g. faction chat).
    - Fix: Click "Chat Tab Options" (the cog icon on the left of ur chat tabs) > Reset > All Settings. This is because the plugin reads from the default combat chat tab and does not support custom ones.
- It keeps spewing "Failed to reolve pointer chain" errors:
  - Make sure ACT is running as administrator.

## Limitations

- This is a POC - it works fine but it's missing support for a lot of features provided by ACT (e.g. blocks, buffs, debuffs, etc..).
- If you have characters hidden (CTRL+F, either partially or fully), the combat log does not specify if their damage is a crit or not, so their %CH will always be 0, but the damage itself will be parsed correctly.
- Only supports EN.
- Damage over time skills and damage from effects that have no explicit actor in the combat log will be attributed to an "Unknown" actor.
- ~~Zone names are not provided in the combat log, so all logs are combined into a single zone on ACT.~~
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
- Custom overlay (?)

## Contributing

I'm too lazy to write a contribution guidelines doc, feel free to submit PRs or issues if you'd like. This project will always be free.
If you don't want to submit an issue, contact `azuradara` on Discord.

## Motivation

That shit ain't worth $15 dawg.
