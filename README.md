<div align="center">

# 🏡 COMP5424 Group 9 - Virtual Apartment XR Review System
> **The Hong Kong Polytechnic University (PolyU) — COMP5424 Virtual Reality**  
> An immersive architectural & interior design XR inspection system built on Unity 2022 LTS, SteamVR / OpenXR, and On-Device Whisper AI Speech Recognition.

<p align="center">
  <a href="./README.md">
    <img src="https://img.shields.io/badge/README-English-2ea44f?style=for-the-badge&logo=googletranslate&logoColor=white" alt="English">
  </a>
  &nbsp;&nbsp;
  <a href="./README_zh.md">
    <img src="https://img.shields.io/badge/说明文档-简体中文-0969da?style=for-the-badge&logo=googletranslate&logoColor=white" alt="简体中文">
  </a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2022.3.62f3_LTS-black?style=flat&logo=unity" alt="Unity Version">
  <img src="https://img.shields.io/badge/Render_Pipeline-URP_14.0-blue?style=flat" alt="Render Pipeline">
  <img src="https://img.shields.io/badge/Platform-SteamVR_%2F_OpenXR-orange?style=flat" alt="Platform">
  <img src="https://img.shields.io/badge/AI_Engine-Whisper_Offline_STT-green?style=flat" alt="Whisper STT">
</p>

</div>

---

## 👥 Team Members & Responsibilities

| Name | Student ID | GitHub Profile | Roles & Key Responsibilities |
| :--- | :--- | :--- | :--- |
| **LI Yue** | 26106438G | [@YueLi040912](https://github.com/YueLi040912) | **Team Leader** / XR Architecture, Core Interaction System, Whisper STT Pipeline & Main Scene Integration |
| **WEI Yilin** | 26106399G | [@weiyilin2002](https://github.com/weiyilin2002) | Spatial Layout, Environment Materials & URP Visual Optimization |
| **LI Haochen** | 26138162G | [@SkyKeeperLee](https://github.com/SkyKeeperLee) | Spatial Object Interaction, Object Rescaling & Physics Collision Tuning |
| **MENG Yuting** | 26050779G | [@Youtte-cloud](https://github.com/Youtte-cloud) | VR Instruction Board System & Floating UI/UX Animations |
| **YI Chenxi** | 26093165G | [@YiArxia](https://github.com/YiArxia) | Interactive Props / Furnishing Mechanics & Review Data Reporting Pipeline |
| **GU Bingchen** | 26095785G | [@Francis-067](https://github.com/Francis-067) | Spatial Audio, Multi-Mic Hardware Adaptation & Audio Export Optimization |

---

## 📌 Project Overview

**Virtual Apartment XR Review System** is an immersive virtual reality review and inspection application designed for architectural design, spatial planning, and interior layout evaluation. Reviewers, designers, and clients can roam through a 1:1 scale apartment model in real time and perform multi-modal spatial inspections:

1. **High-Fidelity Spatial Immersion**: Built upon Universal Render Pipeline (URP) with pre-baked lightmaps and reflection probes to provide photorealistic lighting and true-to-scale perception.
2. **Multi-Modal Spatial Annotation**: Point and shoot VR laser raycasts onto any surface to spawn 3D feedback markers with 3-tier priority levels (Low / Medium / High).
3. **Concurrent Multi-Mic Recording & Auto-Gain**: Overcomes the common audio route mismatch in PCVR streaming (HMD vs. Host PC) by concurrently listening to all available recording channels, determining the winning channel via peak waveform amplitude, and applying dynamic auto-gain compensation.
4. **On-Device Whisper AI Speech-to-Text (STT)**: Offline speech recognition powered by OpenAI Whisper (`ggml-base.en.bin`). No cloud dependency or latency penalties; spoken feedback is transcribed and attached to spatial markers in real time.
5. **Alyx-Style Wrist HUD & Summonable Clipboard**: Real-time wrist-mounted HUD for status tracking and an summonable floating task board with particle effects toggled via left controller.
6. **Automated Data Persistence & Safe Quit**: Export all spatial coordinates, priority tags, raw WAV audio files, and STT transcriptions into structured CSV reports upon exiting.

---

## 🛠️ Tech Stack & Prerequisites

> [!IMPORTANT]
> To prevent broken `.meta` files and YAML scene merge conflicts, **all team members must strictly use the exact same Unity version**!

* **Unity Editor**: `2022.3.62f3 LTS` (Windows 64-bit)
* **Render Pipeline**: Universal Render Pipeline (URP `14.0.12`)
* **XR Runtime & SDKs**:
  * **OpenXR Plugin**: `1.14.3`
  * **SteamVR / OpenVR**: `com.valvesoftware.unity.openvr` (`1.2.1`)
  * **Meta XR Core SDK**: `com.meta.xr.sdk.all` (`201.0.0`)
* **AI & Speech Engine**:
  * **Whisper.unity**: Offline Speech-to-Text model (`ggml-base.en.bin` located in `Assets/StreamingAssets`)
* **Tools & Extensions**:
  * **Unity MCP**: CoplayDev Unity-MCP Integration
  * **ProBuilder**: `5.2.4`
  * **TextMeshPro**: `3.0.7`
* **Supported VR Hardware**:
  * Meta Quest 2 / Quest 3 / Quest Pro (via Quest Link / AirLink)
  * Pico Neo 3 / Pico 4 / Pico 4 Ultra (via Pico Connect / Streaming Assistant)
  * HTC Vive / Valve Index (Native SteamVR)

---

## 🕹️ System Architecture & Workflow

```
                     ┌────────────────────────────────────────┐
                     │          XR Input & Controller         │
                     │ (SteamVR Actions / OpenXR Controllers) │
                     └───────────────────┬────────────────────┘
                                         │
                 ┌───────────────────────┴───────────────────────┐
                 ▼                                               ▼
   ┌───────────────────────────┐                   ┌───────────────────────────┐
   │    MarkerTool.cs (Right)  │                   │   ClipboardSync.cs (Left) │
   │  - Smoothed Laser Pointer │                   │  - Floating Instruction UI│
   │  - 3-Level Priority Cycle │                   │  - Particle Summon Effect │
   │  - Surface Marker Spawn   │                   └───────────────────────────┘
   └─────────────┬─────────────┘
                 │
                 ├──► FeedbackMarker.cs: Floating animation / Scaling hover / Billboard label
                 │
                 ├──► Multi-Mic Audio Arbitration: Concurrent multi-channel recording -> Waveform peak detection -> Dynamic auto-gain
                 │
                 ├──► Whisper.unity: On-device asynchronous speech recognition -> Append transcript to marker
                 │
                 ▼
   ┌───────────────────────────────────────────────────────────────────────────┐
   │                    AnnotationExporter.cs / SafeQuitManager.cs             │
   │  - Export lossless 16-bit PCM WAV audio files                             │
   │  - Output structured CSV report (AnnotationReport_*.csv with 3D positions)│
   │  - Trap Esc / Window Close events for safe data flushing                  │
   └───────────────────────────────────────────────────────────────────────────┘
```

---

## 📂 Project Directory Structure

```
XRG9_Virtual_Apartment/
├── Assets/
│   ├── Fonts/                     # UI font assets
│   ├── Hovl Studio/               # Particle and visual effect packages
│   ├── Materials/                 # PBR materials and shader graphs
│   ├── Models/                    # 3D furniture, room geometry, and props
│   │   └── Chinese Room/          # Stylized apartment room assets
│   ├── Prefabs/                   # Reusable components (Markers, UI, Rig)
│   ├── Scenes/                    # Scene files
│   │   ├── RealMainVirtualApartment.unity   # ★ Integrated Master Release Scene
│   │   ├── MainVirtualApartment.unity       # Fallback scene
│   │   ├── Dev_Marker.unity                 # Standalone annotation test scene
│   │   ├── Dev_InteractableFoods.unity      # Standalone food interaction scene
│   │   ├── Dev_ObjectScale.unity            # Object scaling test scene
│   │   ├── MENG_InstructionBoard.unity      # Instruction board test scene
│   │   └── RealMainVirtualApartment/        # Baked lightmaps & reflection probes
│   ├── Scripts/                   # Core application logic
│   │   ├── AnnotationExporter.cs  # Static CSV/WAV data export utility
│   │   ├── ClipboardSync.cs       # Floating instruction board controller
│   │   ├── FeedbackMarker.cs      # Spatial marker entity & visual feedback
│   │   ├── MarkerTool.cs          # Laser tool, recording & Whisper STT pipeline
│   │   └── SafeQuitManager.cs     # Safe shutdown & data flush manager
│   ├── Settings/                  # URP Render Pipeline Settings
│   ├── SteamVR/                   # SteamVR Official Plugin
│   ├── SteamVR_Input/             # SteamVR Auto-generated Action C# classes
│   ├── StreamingAssets/           # Dynamic binary assets
│   │   ├── ggml-base.en.bin       # ★ Whisper offline model (Managed by Git LFS)
│   │   └── SteamVR/               # Action binding JSON files
│   └── XR/ & XRI/                 # XR Interaction Toolkit settings
├── Packages/                      # Unity Package Manager manifest
├── ProjectSettings/               # Unity project-level settings
├── .gitattributes                 # Git LFS & Unity YAML merge rules
├── .gitignore                     # Git ignore rules
├── README.md                      # Primary Documentation (English)
└── README_zh.md                   # 中文文档 (Simplified Chinese)
```

---

## 🚀 Quick Start Guide

### 1. Clone with Git LFS

> [!WARNING]
> This repository contains 3D models and a 147MB Whisper model binary file. **Git LFS is strictly required**. Without LFS, binary files will be downloaded as tiny pointer files, crashing the project.

```bash
# 1. Install & configure Git LFS (if not done previously)
git lfs install

# 2. Clone the repository
git clone https://github.com/YueLi040912/XRG9_Virtual_Apartment.git
cd XRG9_Virtual_Apartment

# 3. Pull all LFS tracked binary assets
git lfs pull
```

### 2. Open via Unity Hub
1. Launch **Unity Hub** and click **Add project from disk**;
2. Select the repository root folder;
3. Ensure the Unity Editor version is **`2022.3.62f3`**;
4. Wait for package resolution and shader compilation (takes ~3–10 minutes on first load).

### 3. Generate SteamVR Input Actions
* If prompted with a SteamVR Input pop-up window upon launch, click **Yes**;
* Navigate to the top menu: `Window -> SteamVR Input`;
* Verify the action set and click **Save and generate** to update action bindings.

### 4. Run in VR
1. Connect your VR headset to your PC (via Quest Link, Pico Connect, or SteamVR);
2. In the Project window, open `Assets/Scenes/RealMainVirtualApartment.unity`;
3. Press the **Play** button in Unity Editor to start your session.

---

## 🎮 VR Controller Mapping

| Controller | Action / Input | Function |
| :--- | :--- | :--- |
| **Right Hand** | **Thumbstick** | Smooth Locomotion / Teleport |
| **Right Hand** | **Trigger** | Activate laser; release on a surface to spawn a marker |
| **Right Hand** | **Grip** | Cycle marker priority color (Green: Low → Yellow: Medium → Red: High) |
| **Right Hand** | **Hover Marker + Press Record** | Record voice feedback; auto-transcribed via Whisper on release |
| **Right Hand** | **Hover Marker + Hold Grip** | Hold for 1.5s to delete the targeted marker and its associated audio |
| **Left Hand** | **Thumbstick** | Snap / Smooth Turning |
| **Left Hand** | **X / Primary Button** | Toggle the summonable instruction clipboard |
| **PC Keyboard** | **Escape (Esc)** | Facilitator safe quit: flushes data to disk and closes application |

---

## 🤝 Team Collaboration & Git Guidelines

To maintain development stability and avoid catastrophic scene merge conflicts:

### 1. Branching Strategy
* `main`: **Production branch**. Clean, stable, and tested releases only. Direct commits are restricted; changes must be merged via Pull Request reviewed by the Team Leader.
* `PhaseX_LiYue`: **Milestone integration branches** for major sprint deliverables.
* `developing_<Name>` / `feature/<FeatureName>`: **Individual feature branches**. All day-to-day development must occur in isolated branches, e.g.:
  * `developing_Wei_Yinlin`
  * `Meng_yuting`
  * `Li,-Haochen`
  * `YI_Chenxi`
* **Syncing**: Keep your branch up to date by running `git fetch origin` and merging `origin/main` into your local branch before opening a PR.

### 2. Scene & Prefab First Policy
> [!CAUTION]
> **Never modify the master scene (`RealMainVirtualApartment.unity`) directly in feature branches!** Merging conflicting scene YAML files is error-prone and frequently leads to data corruption.

* **Dev Scenes**: Develop mechanics in isolated scenes (`Assets/Scenes/Dev_*.unity`).
* **Prefab Workflow**: Package modular features as Prefabs in `Assets/Prefabs/`.
* **Scene Integration**: Only the Team Leader or designated integrator updates the master scene by pulling in validated Prefabs.

### 3. `.meta` File Integrity
* Every Unity asset has a paired `.meta` file that records its unique GUID.
* **Always commit newly added, modified, or moved `.meta` files together with their source assets**.
* Run `git status` prior to committing to ensure no untracked `.meta` or orphan `.meta` files are left behind.

### 4. Semantic Commit Messages
Follow conventional commit standards:
```
<type>: <subject>

[optional body]
```
Common types:
* `feat`: New feature
* `fix`: Bug fix
* `scene`: Scene setup, lighting bake, or object layout
* `assets`: Importing/updating 3D models, textures, audio
* `refactor`: Code refactoring without behavioral change
* `docs`: Documentation updates
* `perf`: Performance tuning (draw calls, lightmap compression)

---

## 📊 Review Data Output Format

When an inspection session ends, report data is automatically saved to disk:

### CSV Report Sample
File naming: `AnnotationReport_YYYYMMDD_HHMMSS.csv` (Encoded with UTF-8 BOM)
```csv
ID,Priority,PositionX,PositionY,PositionZ,AudioFiles,Transcriptions
1,High,1.240,1.150,-3.420,Marker_1_20261110_143000.wav,"The height of this kitchen counter seems too low for comfortable use."
2,Medium,-0.850,2.100,-1.200,Marker_2_20261110_143215.wav,"Consider changing the pendant light material to warm brass."
```

### File Locations
* **Unity Editor Mode**: `%USERPROFILE%\Desktop\VirtualApartment_ReviewData\`
* **Standalone Build Mode**: `<App_Executable_Directory>\AnnotationData\`

---

## ❓ Troubleshooting

1. **Black screen or no VR input?**
   * Verify whether SteamVR or Oculus Link detects your headset and controllers properly.
   * Go to `Edit -> Project Settings -> XR Plug-in Management` and confirm **OpenXR** or **Oculus** is enabled.
2. **Microphone is silent or Whisper transcription returns empty?**
   * Verify Windows Microphone Privacy Settings allow desktop app access.
   * In your streaming client (Virtual Desktop / Quest Link / Pico Connect), verify that microphone pass-through is toggled **ON**.
3. **Materials appear magenta / pink (Missing Shader)?**
   * The project uses URP. Select the pink materials, open `Window -> Rendering -> Render Pipeline Converter`, choose `Built-in to URP`, and convert them.
4. **Git LFS issues or broken binaries?**
   * Run `git lfs install` followed by `git lfs pull` to replace pointer files with genuine binaries.

---

*© 2026 COMP5424 Group 9. All Rights Reserved.*
