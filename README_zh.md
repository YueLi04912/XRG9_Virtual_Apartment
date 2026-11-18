<div align="center">

# 🏡 COMP5424 Group 9 - Virtual Apartment XR Review System
> **香港理工大学 (PolyU) COMP5424 Virtual Reality** 课程团队项目  
> 基于 Unity 2022 LTS + SteamVR / OpenXR + 端侧离线 Whisper AI 的沉浸式虚拟公寓建筑与室内装潢评审协作系统。

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

## 👥 团队成员与分工 (Team Members & Roles)

| 姓名 (Name) | 学号 (Student ID) | GitHub 用户名 | 角色与职责 (Roles & Responsibilities) |
| :--- | :--- | :--- | :--- |
| **LI Yue** | 26106438G | [@YueLi040912](https://github.com/YueLi040912) | **Team Leader** / 架构设计、XR 核心交互、Whisper 语音集成与主场景集成 |
| **WEI Yilin** | 26106399G | [@weiyilin2002](https://github.com/weiyilin2002) | 场景空间布局、环境材质与 URP 视觉渲染调优 |
| **LI Haochen** | 26138162G | [@SkyKeeperLee](https://github.com/SkyKeeperLee) | 物体空间交互、尺度调整与物理碰撞机制开发 |
| **MENG Yuting** | 26050779G | [@Youtte-cloud](https://github.com/Youtte-cloud) | VR 任务指示看板系统 (Instruction Board) 与 UI/UX 动效 |
| **YI Chenxi** | 26093165G | [@YiArxia](https://github.com/YiArxia) | 空间可交互陈设/道具逻辑、评审数据报表管线 |
| **GU Bingchen** | 26095785G | [@Francis-067](https://github.com/Francis-067) | 空间音频、多麦克风硬件适配与音频剪辑导出优化 |

---

## 📌 项目概述 (Project Overview)

本项目构建了一个沉浸式高保真虚拟公寓评审与交互系统（**Virtual Apartment XR Review System**）。系统面向建筑室内设计、空间规划与用户体验评审场景，支持评审人员与客户在 1:1 虚拟空间中自由漫游并交互，实现：
1. **沉浸式空间漫游与实时交互**：基于 URP 管线与精细烘焙光影，提供真实的空间尺度与光照体验；
2. **多模态空间批注系统 (Spatial Annotation)**：允许用户使用 VR 激光射线在三维空间中精准打点，标记问题区域并按三色等级设置优先级；
3. **多麦克风智能录音与端侧 Whisper 语音转写 (AI STT)**：解决无线串流或 PCVR 下多麦克风冲突痛点，并发采集并自动仲裁最佳音轨，使用本地离线 Whisper 模型实时将语音批注转译为文本；
4. **便携式手腕 HUD 与召唤式写字板**：Alyx 风格手腕状态指示屏，左手随时召唤带粒子特效的任务说明看板；
5. **一键导出评审报告 (CSV + WAV)**：会话结束时一键导出完整的空间坐标、优先级、录音文件与文字转录记录，方便后续工程整改跟踪。

---

## 🛠️ 技术栈与开发环境 (Tech Stack & Prerequisites)

> [!IMPORTANT]
> 为避免因编辑器版本差异导致的 `.meta` 文件混乱或场景 YAML 序列化冲突，**所有团队成员必须使用完全一致的 Unity 版本**！

* **Unity Editor**: `2022.3.62f3 LTS` (Windows 64-bit)
* **Render Pipeline**: Universal Render Pipeline (URP `14.0.12`)
* **XR 运行时与 SDK**:
  * **OpenXR Plugin**: `1.14.3`
  * **SteamVR / OpenVR**: `com.valvesoftware.unity.openvr` (`1.2.1`)
  * **Meta XR Core SDK**: `com.meta.xr.sdk.all` (`201.0.0`)
* **AI 与音频引擎**:
  * **Whisper.unity**: 端侧离线语音识别 (`ggml-base.en.bin` 存放于 `Assets/StreamingAssets`)
* **工具与扩展**:
  * **Unity MCP**: CoplayDev Unity-MCP 支持
  * **ProBuilder**: `5.2.4`
  * **TextMeshPro**: `3.0.7`
* **支持硬件设备**:
  * Meta Quest 2 / Quest 3 / Quest Pro (通过 Quest Link / AirLink 串流)
  * Pico Neo 3 / Pico 4 / Pico 4 Ultra (通过 Pico 互联 / Streaming Assistant 串流)
  * HTC Vive / Valve Index (原生 SteamVR)

---

## 🕹️ 核心系统与功能架构 (Core Architecture & Features)

```
                     ┌────────────────────────────────────────┐
                     │          XR Input & Controller         │
                     │ (SteamVR Actions / OpenXR Controllers) │
                     └───────────────────┬────────────────────┘
                                         │
                 ┌───────────────────────┴───────────────────────┐
                 ▼                                               ▼
   ┌───────────────────────────┐                   ┌───────────────────────────┐
   │    MarkerTool.cs (右手)   │                   │   ClipboardSync.cs (左手)  │
   │  - 激光指示器 / 平滑防抖    │                   │  - 悬浮说明板召唤与跟随   │
   │  - 三级优先级颜色切换     │                   │  - Particle 粒子生成特效  │
   │  - 空间小球打点吸附生成   │                   └───────────────────────────┘
   └─────────────┬─────────────┘
                 │
                 ├──► FeedbackMarker.cs: 空间吸附点 / 呼吸缩放动效 / Billboard 编号标签
                 │
                 ├──► 多麦克风并发仲裁录音: 并发采集各硬件麦克风 -> 波形振幅判定 -> 动态增益补偿 (Auto-Gain)
                 │
                 ├──► Whisper.unity: 本地端侧异步语音识别 (STT) -> 绑定转录文本至对应 Marker
                 │
                 ▼
   ┌───────────────────────────────────────────────────────────────────────────┐
   │                    AnnotationExporter.cs / SafeQuitManager.cs             │
   │  - 录音无损保存为标准 WAV 格式                                             │
   │  - 自动输出结构化报表 AnnotationReport_*.csv (UTF-8 BOM 带空间三维坐标)     │
   │  - 拦截 Esc / 窗口关闭事件，确保数据完全落盘                               │
   └───────────────────────────────────────────────────────────────────────────┘
```

### 1. 空间批注与交互工具 (`MarkerTool.cs` & `FeedbackMarker.cs`)
* **Alyx 风格手腕 UI**：手腕实时跟随显示当前操作模式与选中优先级。
* **射线平滑与打点**：射线带平滑插值滤波，碰撞检测表面法线并按指定偏移生成发光小球。
* **三级优先级状态**：`Low (绿色)`、`Medium (黄色)`、`High (红色)`，一键循环切换。
* **小球动态效果**：悬浮上下浮动动画、激光悬停缩放（1.5x）、文字始终朝向头显（Billboard 机制）。

### 2. 多路并发麦克风监听与自动增益仲裁
* 针对 VR 串流经常出现的“头显麦克风”与“宿主机麦克风”通道混乱问题，系统采用**全通道并发拾音**策略；
* 录音结束后遍历所有音频通道，计算波形绝对峰值（Peak Amplitude），自动过滤静音与底噪，提取最清晰的通道；
* 自动执行**自适应动态增益补偿 (Dynamic Auto-Gain)**，提升语音识别率。

### 3. 端侧离线 Whisper 语音转录
* 搭载 OpenAI Whisper `ggml-base.en.bin` 本地权重；
* 无需连网、无需 API Key，保护隐私且零额外延迟；
* 识别成功后将转录文本实时写入对应小球的数据列表，并在空间标签上显示录音与转录状态。

### 4. 便携式任务写字板 (`ClipboardSync.cs`)
* 左手按键一键开闭，自动计算头显前方视野锚点平滑呼出；
* 附带 Summon 特效粒子；解决评审中频繁脱戴头显看设计图纸/任务清单的问题。

### 5. 数据导出与安全退出 (`AnnotationExporter.cs` & `SafeQuitManager.cs`)
* 按 `Esc` 或触发退出时，系统拦截关闭信号并启动协程落盘；
* **WAV 音频生成**：自动生成标准 16-bit PCM WAV 音频文件；
* **CSV 报表导出**：输出格式为 `ID, Priority, PositionX, PositionY, PositionZ, AudioFiles, Transcriptions`；
* **数据存储目录隔离**：
  * **编辑器模式 (Editor)**：导出至用户桌面 `VirtualApartment_ReviewData/`（避免污染代码仓）；
  * **打包构建版 (Build)**：导出至程序根目录 `AnnotationData/`。

---

## 📂 项目目录结构规范 (Project Structure)

```
XRG9_Virtual_Apartment/
├── Assets/
│   ├── Fonts/                     # UI 字体资源
│   ├── Hovl Studio/               # 粒子与视觉特效包
│   ├── Materials/                 # 场景通用材质与 PBR 贴图
│   ├── Models/                    # 公寓家具、房间架构与道具 3D 模型
│   │   └── Chinese Room/          # 风格化特色房间资产
│   ├── Prefabs/                   # 核心可复用预制件 (Marker、UI、玩家 Rig)
│   ├── Scenes/                    # 场景文件
│   │   ├── RealMainVirtualApartment.unity   # ★ 主发布场景 (集成主场景)
│   │   ├── MainVirtualApartment.unity       # 主场景备用/过渡版
│   │   ├── Dev_Marker.unity                 # 批注系统独立开发测试场景
│   │   ├── Dev_InteractableFoods.unity      # 道具交互开发测试场景
│   │   ├── Dev_ObjectScale.unity            # 缩放机制测试场景
│   │   ├── MENG_InstructionBoard.unity      # 看板功能测试场景
│   │   └── RealMainVirtualApartment/        # 光照贴图与反射探针烘焙缓存
│   ├── Scripts/                   # 核心业务逻辑脚本
│   │   ├── AnnotationExporter.cs  # CSV 报表与 WAV 导出静态工具
│   │   ├── ClipboardSync.cs       # 便携指示看板逻辑
│   │   ├── FeedbackMarker.cs      # 空间标记球实体与视觉反馈
│   │   ├── MarkerTool.cs          # 激光批注工具、录音与 Whisper STT
│   │   └── SafeQuitManager.cs     # 安全退出与落盘管理器
│   ├── Settings/                  # URP 渲染管线配置资产 (URP-High/Medium/Low)
│   ├── SteamVR/                   # SteamVR 官方插件包
│   ├── SteamVR_Input/             # SteamVR 自动生成的动作绑定 C# 类
│   ├── StreamingAssets/           # 外部二进制与动态配置
│   │   ├── ggml-base.en.bin       # ★ Whisper 本地模型权重 (由 Git LFS 托管)
│   │   └── SteamVR/               # SteamVR 手柄动作映射 JSON 配置文件
│   └── XR/ & XRI/                 # XR Interaction Toolkit 动作配置与预设
├── Packages/                      # Unity Package Manager 依赖清单
├── ProjectSettings/               # Unity 项目全局设定
├── .gitattributes                 # Git LFS 与 Unity 智能合并配置
├── .gitignore                     # Git 忽略规则
├── README.md                      # 项目主说明文档 (English)
└── README_zh.md                   # 项目中文说明文档 (简体中文)
```

---

## 🚀 快速上手与本地运行 (Quick Start)

### 1. 克隆仓库与配置 Git LFS

> [!WARNING]
> 本项目包含大型 3D 模型与 147MB 的 Whisper 模型二进制文件，**必须预先安装 Git LFS**，否则模型文件将只是指针文件导致运行时报错！

```bash
# 1. 安装并初始化 git-lfs (若未安装过)
git lfs install

# 2. 克隆仓库
git clone https://github.com/YueLi040912/XRG9_Virtual_Apartment.git
cd XRG9_Virtual_Apartment

# 3. 确保所有 LFS 大文件完整拉取
git lfs pull
```

### 2. 使用 Unity Hub 打开工程
1. 打开 **Unity Hub**，点击 **Add project from disk**；
2. 选择本仓库根目录；
3. 确保所选 Editor 版本为 `2022.3.62f3`；
4. 首次打开会进行 Shader 编译和 Package 解析，请耐心等待 3~10 分钟。

### 3. 配置 SteamVR / OpenXR 输入映射
* 若首次打开弹出 **SteamVR Input** 提示对话框，点击 **Yes** 确认；
* 依次点击顶部菜单：`Window -> SteamVR Input`；
* 检查 Action 列表是否完整，点击 **Save and generate** 生成动作绑定文件。

### 4. 运行体验
1. 启动 VR 头显并连接串流软件（Quest Link / Pico 互联 / SteamVR）；
2. 在 Unity Project 视图中打开场景：`Assets/Scenes/RealMainVirtualApartment.unity`；
3. 点击 Unity 编辑器顶部的 **Play** 按钮即可在头显中开始漫游体验。

---

## 🖥️ 构建版独立运行指南 (How to Run the Built Executable)

如果您直接运行已打包好的独立可执行程序（`.exe` 构建版本），请务必遵循以下操作流程：

### 1. 启动前置与 SteamVR 串流确认
* **SteamVR 运行状态**：双击启动项目的 `.exe` 文件后，**务必确保 SteamVR 已经启动并且与头显、手柄连接正常**（SteamVR 状态面板上头显与手柄图标均应显示为绿色常亮）。
* 若 SteamVR 尚未启动，建议先手动启动 SteamVR 并戴上头显确认进入 VR 虚空空间后，再双击运行本项目的 `.exe`。

### 2. Windows 麦克风输入配置（关键）
> [!IMPORTANT]
> 项目的端侧 Whisper 语音批注依赖系统麦克风。如果使用 SteamVR 串流，系统默认麦克风可能仍然停留在 PC 板载声卡上，导致头显麦克风无声音。
* 在 Windows 电脑桌面右下角右键点击喇叭图标，选择 **「声音设置」(Sound Settings)**（或按 `Win + I` 打开系统设置 -> 系统 -> 声音）；
* 在 **「输入」(Input)** 区域的“选择用于说话或录音的设备”下拉列表中，**必须手动选择 Steam 麦克风**（通常命名为 `Steam Streaming Microphone` 或 `SteamVR Microphone`）；
* 对着头显麦克风试说几句话，确认系统输入音量条产生跳动。

### 3. 体验结束与数据安全落盘导出
> [!WARNING]
> 本项目的 CSV 报表和 WAV 音频导出逻辑绑定在程序退出生命周期中。**体验流程结束后，一定要主动正常结束运行 EXE 程序**，否则当前会话的数据将无法写入磁盘！

您可以选择以下任意一种方式结束程序，两者均已接入安全数据保存：
* **方式 1 (推荐)**：直接按下电脑键盘上的 **`Esc`** 键，触发 `SafeQuitManager` 安全退出流程，程序会在控制台完成文件流落盘后自动优雅关闭；
* **方式 2**：直接点击程序窗口右上角的 **`X` 关闭按钮**（或按 `Alt + F4`），程序内置的拦截器会捕获关闭信号并执行紧急数据保存。
* 退出后，生成的评审报表和音频文件将自动保存在可执行文件同级的 `AnnotationData/` 文件夹下。

---

## 🎮 控制器操作指南 (VR Controller Mapping)

| 控制器 (Controller) | 按键 / 动作 (Input Action) | 功能描述 (Function) |
| :--- | :--- | :--- |
| **右手 (Right Hand)** | **摇杆 (Joystick)** |  瞬移 (Teleport) |
| **右手 (Right Hand)** | **扳机键 (Trigger)** | 激活激光；指向物体表面松开以生成批注标记小球 |
| **右手 (Right Hand)** | **握持键 (Grip)** | 循环切换批注优先级颜色（绿：Low → 黄：Medium → 红：High） |
| **右手 (Right Hand)** | **指向标记球 + 长按录音** | 触发多麦克风拾音，松开后自动进行 Whisper 语音转录 |
| **右手 (Right Hand)** | **指向标记球 + 长按抓取** | 持续对准标记球 1.5 秒可删除该标记及对应的本地音频 |
| **左手 (Left Hand)** | **摇杆 (Joystick)** | 定角转向 (Snap Turn) |
| **左手 (Left Hand)** | **X / Primary 键** | 呼出 / 收起便携式任务说明看板 (Instruction Clipboard) |
| **PC 键盘 (Keyboard)** | **Esc 键** | 引导员快捷键：触发数据导出并安全退出程序 |

---

## 🤝 团队协作与 Git 开发规范 (Team Collaboration Guidelines)

为了保证多人协作的高效与稳定，防止场景冲突、资源丢失或代码覆盖，请全员严格遵守以下规范：

### 1. 分支管理策略 (Branching Model)
* `main`：**稳定主干分支**。仅供集成后可运行的发布版本，所有向 main 的提交必须通过 Pull Request 并经组长审查合并；
* `PhaseX_LiYue`：**阶段性评审与集成分支**；
* `developing_<Name>` 或 `feature/<功能名>`：**个人开发分支**。所有组员的日常开发在自己的分支进行，例如：
  * `developing_Wei_Yinlin`
  * `Meng_yuting`
  * `Li,-Haochen`
  * `YI_Chenxi`
* **合流流程**：个人功能开发测试无误后 -> `git fetch origin` -> 在本地将最新 `main` 合并进自己的分支解决可能存在的冲突 -> 提交 PR 请求合入主分支。

### 2. 场景与预制体防冲突原则 (Scene & Prefab Workflow)
> [!CAUTION]
> **严禁多人在各自的分支同时修改主场景 `RealMainVirtualApartment.unity`！** Unity 场景文件的 YAML 冲突极其复杂且极易造成损坏。

* **功能隔离开发**：各成员在自己专属的子场景（`Assets/Scenes/Dev_*.unity`）中编写与调试功能；
* **预制件化 (Prefab First)**：所有做好的功能模块必须制作为独立的 Prefab（存放在 `Assets/Prefabs/`）；
* **统一主场景组装**：由 Team Leader 或指定集成人员在主场景中统一引入并更新 Prefab 实例，其他人不碰主场景文件。

### 3. Meta 文件规范
* Unity 中每一个资产（脚本、模型、贴图、材质等）都对应一个同名的 `.meta` 文件（记录了资产的唯一 GUID）；
* **添加、删除、重命名、移动资产时，必须连同对应的 `.meta` 文件一并提交**；
* 提交前使用 `git status` 确认没有遗留未追踪的 `.meta`，也没有出现资产已删除但 `.meta` 仍在的“孤儿元数据”。

### 4. Git 提交信息规范 (Commit Messages)
请采用业界通用的语义化提交格式：
```
<type>: <subject>

[可选的详细说明]
```
常用 `type` 类型：
* `feat`: 新增功能 (Feature)
* `fix`: 修复 Bug
* `scene`: 场景搭建、光照烘焙或物体摆放
* `assets`: 导入或更新模型、贴图、音频等美术资源
* `refactor`: 代码重构（无功能变更）
* `docs`: 文档变更（如更新 README）
* `perf`: 性能调优（如降低 DrawCall、优化光照贴图）

示例：
```bash
git commit -m "feat: 添加手腕 Alyx 风格优先级显示 HUD"
git commit -m "fix: 修复无线串流时麦克风静音的波形判断逻辑"
git commit -m "assets: 更新客厅中式家具模型与 PBR 材质"
```

---

## 📊 评审报表输出规范 (Review Data Output)

当用户结束评审并退出程序后，系统会在本地生成结构化数据：

### CSV 报表样例文本
文件命名：`AnnotationReport_YYYYMMDD_HHMMSS.csv` (UTF-8 编码)
```csv
ID,Priority,PositionX,PositionY,PositionZ,AudioFiles,Transcriptions
1,High,1.240,1.150,-3.420,Marker_1_20261110_143000.wav,"The height of this kitchen counter seems too low for comfortable use."
2,Medium,-0.850,2.100,-1.200,Marker_2_20261110_143215.wav,"Consider changing the pendant light material to warm brass."
```

### 存储路径
* **Unity 编辑器运行**：`C:\Users\<用户名>\Desktop\VirtualApartment_ReviewData\`
* **Standalone 独立打包程序**：`<游戏exe根目录>\AnnotationData\`

---

## ❓ 常见问题排查 (Troubleshooting)

1. **头显看不到画面 / 显示黑屏或无输入？**
   * 检查 SteamVR 或 Oculus Link 状态是否正常识别头显与手柄；
   * 打开 `Edit -> Project Settings -> XR Plug-in Management`，确保勾选了 **OpenXR** 或 **Oculus**。
2. **麦克风录音没有声音或 Whisper 识别为空？**
   * 检查 Windows 设置中的“应用声音麦克风权限”是否开启；
   * 串流软件（如 Virtual Desktop / Quest Link / Pico 互联）需开启“传回麦克风音频”开关；
   * 项目已内置多麦克风波形自动判定，若环境声音较小，可适当靠近麦克风发声。
3. **打开场景后模型显示粉红色 (Missing Shader)？**
   * 本项目使用 URP 管线，选中粉色材质球，点击顶部菜单 `Window -> Rendering -> Render Pipeline Converter`，选择 `Built-in to URP` 进行材质修复与转换。
4. **Git 提示 LFS 错误？**
   * 确保运行了 `git lfs install`，并尝试执行 `git lfs pull` 强制更新二进制资产。

---

*© 2026 COMP5424 Group 9. All Rights Reserved.*
