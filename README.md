
<div align="center">

# ⚡ SokuMenu

### A powerful and customizable Among Us mod menu.

[![Development](https://img.shields.io/badge/STATUS-IN_DEVELOPMENT-ff9800?style=for-the-badge)](#)
[![BepInEx](https://img.shields.io/badge/BepInEx-6_IL2CPP-8a2be2?style=for-the-badge)](https://builds.bepinex.dev/projects/bepinex_be)
[![CSharp](https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)](#)
[![Windows](https://img.shields.io/badge/Windows-Supported-0078D6?style=for-the-badge&logo=windows&logoColor=white)](#)

<br>

[![Discord](https://img.shields.io/badge/Discord-Join_Community-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/gaQvGxK5F8)

<br>

> [!WARNING]
> **SokuMenu is currently under active development!**
> Features may be incomplete, unstable, or subject to change.

</div>

---

## 📖 About

**SokuMenu** is a feature-rich Among Us mod menu built with **BepInEx 6 (IL2CPP)**.

Designed with simplicity, customization, and usability in mind, SokuMenu provides a wide variety of features through an easy-to-use interface.

### ✨ Highlights

- 🎨 Customizable RGB interface
- 🔍 Global feature search
- ⌨️ Custom hotkey bindings
- 💾 Persistent configuration profiles
- 👥 Live player management
- 🎭 Advanced visor catalogue
- 🖥️ Built-in event console
- 🚀 More features coming soon!

---

## 📥 Installation

> [!IMPORTANT]
> Make sure you are using the **IL2CPP x64** version of BepInEx 6.

**1.** Download [BepInEx 6 IL2CPP](https://builds.bepinex.dev/projects/bepinex_be).

**2.** Extract BepInEx into your Among Us directory.

**3.** Launch Among Us once to generate the required folders.

**4.** Close the game and place `SidaMenu.dll` inside:

```text
Among Us/
├── Among Us.exe
└── BepInEx/
    ├── config/
    └── plugins/
        └── SidaMenu.dll
```

**5.** Launch the game and press **`DELETE`** to open SokuMenu.

> [!TIP]
> You can move the menu anywhere on your screen by dragging its header.

---

## 🎮 Menu Controls

| Key / Action | Function |
|:---:|:---|
| `DELETE` | Open / Close menu |
| `ESC` | Clear selected hotkey |
| `Right Click` | Teleport to cursor (when enabled) |
| `Mouse Wheel` | Adjust zoom (when enabled) |
| `Drag Header` | Move menu window |
| `Hotkey Chip` | Assign custom hotkey |

> [!NOTE]
> Hotkeys are automatically disabled while typing in chat.

---

## 🧩 Features

<details open>
<summary><b>🧍 Self — Player Modifications</b></summary>

<br>

| Feature | Description |
|---|---|
| ⚡ Speed Multiplier | Adjustable movement speed |
| 👻 No Clip | Move through obstacles |
| 📍 Teleport to Cursor | Right-click teleportation |
| 🎥 Free Camera | Camera movement with adjustable speed |
| 🔭 Zoom Out | Adjustable zoom using the mouse wheel |
| 💡 Full Bright | Enhanced scene visibility |
| 👁️ See Ghosts | Display ghost players |
| 🌑 No Shadows | Disable shadows |
| 💬 Always Chat | Keep chat available |
| ⏱️ No Kill Cooldown | Remove local cooldown restrictions |
| 🕳️ Unlock Vents | Access vents regardless of role or distance |
| 🚶 Move While Venting | Move while inside vents |

</details>

<details>
<summary><b>🎭 Visor — Cosmetic Customization</b></summary>

<br>

Browse the complete Among Us visor catalogue and equip cosmetic entries directly.

**Features:**

- Browse available visors
- Search by product ID
- Equip hidden cosmetics
- Filter using **Hidden Only**
- Access unused visor entries

**Special cosmetic:**

```text
visor_impostorShine
```

Also known as **Totally Innocent**.

> [!NOTE]
> Visor product IDs can replicate to other clients even without host privileges because the IDs are registered by the game. Other cosmetic texture changes may remain local.

</details>

<details>
<summary><b>👑 Host — Lobby Management</b></summary>

<br>

| Category | Available Features |
|---|---|
| 👥 Lobby | Max Players, Force Start |
| 🎮 Game | Block Game End, Force End |
| 📋 Tasks | Reset Tasks |
| 🚨 Sabotages | Repair All, Lights, Reactor, Oxygen, Comms |
| 📢 Meetings | Call Meeting, Skip Meeting |
| 🎯 Player Actions | Kill Crewmates, Kill Impostors, Kill Everyone |
| 🚪 Administration | Kick Everyone |

</details>

<details>
<summary><b>👥 Players — Live Player Manager</b></summary>

<br>

Access a real-time list of connected players.

**Displayed information:**

- Player role
- Distance
- Player color
- Player identity

**Available actions:**

| Action | Function |
|---|---|
| 📍 Teleport To | Teleport to selected player |
| 🎯 Kill | Perform a kill action |
| 📢 Report | Report selected player |
| 🚪 Kick | Remove player from lobby |

</details>

<details>
<summary><b>🛠️ Misc — Additional Utilities</b></summary>

<br>

| Feature | Description |
|---|---|
| 👻 Invisible | Toggle invisibility |
| 🔴 Red Quick Chat | Modified quick chat appearance |
| 🔊 Windows Sound Exploit | Affects certain third-party chat loggers |
| 📣 Promote Menu | Share menu information |
| 💬 Custom Chat | Send custom messages |
| 🔁 Spam x6 | Repeat a custom message six times |
| 📋 Copy Lobby Code | Copy lobby code instantly |
| 🎭 Platform Spoofer | Change reported platform |
| 🌈 RGB Menu | Animated RGB interface |

**Supported platform options:**

`Steam` · `Epic Games` · `PlayStation` · `Xbox` · `Nintendo Switch` · `Android` · `iPhone`

</details>

<details>
<summary><b>🖥️ Console — Event Logger</b></summary>

<br>

Track actions performed through SokuMenu.

- Timestamped events
- Action history
- Copy console contents
- Clear console history

</details>

<details>
<summary><b>⚙️ Config — Profile Management</b></summary>

<br>

Personalize your experience and save your preferences.

- Save profile
- Load profile
- Reset profile
- Configure hotkeys
- Save toggles and sliders

**Default profile path:**

```text
BepInEx/config/SidaMenu_Profile.txt
```

</details>

<details>
<summary><b>🔒 Safety — Telemetry Options</b></summary>

<br>

The menu includes:

- Ban-point-related controls
- Unity analytics disabling
- Performance reporting disabling

> [!CAUTION]
> These features do not guarantee protection from detection, bans, or other enforcement actions.

</details>

---

## ⌨️ Hotkey System

SokuMenu includes a customizable keybinding system.

1. Find a feature with a hotkey chip.
2. Click its chip.
3. Press the key you want to assign.
4. The hotkey will be saved in your profile.

> [!TIP]
> Press **`ESC`** while assigning a hotkey to clear the binding.

---

## 💾 Configuration

SokuMenu automatically stores your preferences in a local configuration profile.

```text
BepInEx/
└── config/
    └── SidaMenu_Profile.txt
```

Settings include:

- Feature states
- Slider values
- Custom keybindings

---

## 🏗️ Building From Source

### Requirements

- .NET SDK
- BepInEx 6 IL2CPP
- Among Us game libraries

### Build

```bash
dotnet restore
dotnet build SidaMenu.sln
```

> [!NOTE]
> `AmongUs.GameLibs` and BepInEx packages are retrieved through the BepInEx NuGet feed configured in `NuGet.config`.

---

## 🗺️ Roadmap

- [x] Custom menu interface
- [x] Searchable features
- [x] Custom hotkey system
- [x] Configuration profiles
- [x] Player management
- [x] Visor browser
- [x] Built-in console
- [ ] Additional features
- [ ] Further UI improvements
- [ ] Bug fixes and optimization

---

## 🤝 Community

Have an idea for a new feature? Found a bug?

Your feedback is welcome!

<div align="center">

### 💬 Join the SokuMenu Community

[![Join Discord](https://img.shields.io/badge/JOIN_OUR_DISCORD-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/gaQvGxK5F8)

</div>

---

## ❤️ Credits

Special thanks to the projects that inspired SokuMenu.

| Project | Repository |
|---|---|
| **MalumMenu** | [scp222thj/MalumMenu](https://github.com/scp222thj/MalumMenu) |
| **SickoMenu** | [g0aty/SickoMenu](https://github.com/g0aty/SickoMenu) |
| **BepInEx** | [BepInEx/BepInEx](https://github.com/BepInEx/BepInEx) |

---

<div align="center">

### ⚡ SokuMenu

**Simple. Customizable. Powerful.**

<sub>Made with C# and BepInEx 6 IL2CPP.</sub>

<br><br>

![Development](https://img.shields.io/badge/MADE_WITH-❤️-red?style=flat-square)
![Status](https://img.shields.io/badge/PROJECT-ACTIVE-success?style=flat-square)

</div>
