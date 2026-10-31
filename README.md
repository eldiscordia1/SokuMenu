<h1 align="center">🦠SidaMenu🦠</h1>
<p align="center">
  <img src="sidapng.png" alt="sidapng" width="500"> <br>
</p>

<h1 align="center">:warning:CURRENTLY ON DEVELOPMENT:warning:</h1>

SidaMenu is an Among Us mod menu that aims to give the best experience to the user with new features upcoming.

We have a discord so you can share your ideas! https://discord.gg/gaQvGxK5F8

## Install

1. Install [BepInEx 6 (IL2CPP, x64)](https://builds.bepinex.dev/projects/bepinex_be) into your Among Us folder.
2. Launch the game once so BepInEx generates its folders, then close it.
3. Drop `SidaMenu.dll` into `BepInEx/plugins/`.
4. Start the game and press **DELETE**.

## Using the menu

- **DELETE** shows/hides the menu. Drag it by the header.
- The search box filters every feature across all tabs.
- The small chip next to each toggle binds a hotkey — click it, press a key, done. **ESC** clears a bind.
- Hotkeys are disabled while you are typing in chat.
- **Config** saves your toggles, sliders and keybinds to `BepInEx/config/SidaMenu_Profile.txt`.

## Features

### Self
- Speed multiplier slider
- No Clip
- Teleport To Cursor (right click)
- Free Camera with adjustable speed
- Zoom Out with adjustable zoom level (mouse wheel while enabled)
- Full Bright
- See Ghosts
- No Shadows
- Always Chat
- No Kill Cooldown
- Unlock Vents (any role, any distance)
- Move While Venting

### Visor
- Browse the game's whole visor catalogue and equip any entry, or type any visor product id.
- The **Hidden only** filter surfaces wardrobe-hidden visors, including the unused `visor_impostorShine` (Totally Innocent), which still replicates because every client registers the id.
- Cosmetic ids are client-authoritative, so this works while you are **not** the host and every player in the lobby sees it. Non-visor textures stay local.

### Host
- Max Players (configurable cap)
- Block Game End
- Force Start / Force End
- Reset Tasks
- Repair All Sabotages
- Sabotage Lights / Reactor / Oxygen / Comms
- Call Meeting / Skip Meeting
- Kill Crewmates / Kill Impostors / Kill Everyone
- Kick Everyone

### Players
Live player list showing role, distance and colour, with per-player:
- Teleport To
- Kill
- Report
- Kick

### Misc
- Invisible
- Red Quick Chat
- Windows Sound Exploit (only affects users running a chat logger)
- Promote Menu
- Custom chat message with Send / Spam x6
- Copy Lobby Code
- Platform spoofer (Steam, Epic, Playstation, Xbox, Switch, Android, iPhone)
- RGB menu

### Console
Timestamped event log of every action, with copy and clear.

### Config
Profile save/load/reset, hotkey overview and the profile path.

### Safety
- Ban points are kept at zero
- Unity analytics and performance reporting disabled

## Building

```sh
dotnet build SidaMenu.sln
```

The `AmongUs.GameLibs` and BepInEx packages come from the BepInEx NuGet feed, which
`NuGet.config` in this repo already configures.

## Credits

Menu layout, feature catalogue and hotkey ideas were inspired by
[MalumMenu](https://github.com/scp222thj/MalumMenu) and
[SickoMenu](https://github.com/g0aty/SickoMenu).
