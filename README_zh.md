<div align="center">

# 🏡 COMP5424 Group 9 - 虚拟公寓 XR 评审系统
> **香港理工大学 (PolyU) — COMP5424 Virtual Reality** 课程项目  
> 基于 Unity 2022 LTS、SteamVR / OpenXR 与端侧 Whisper AI 语音识别构建的沉浸式建筑与室内设计 XR 检查系统。

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

## 👥 团队成员与分工

| 姓名 | 学号 | GitHub 主页 | 角色与主要职责 |
| :--- | :--- | :--- | :--- |
| **LI Yue** | 26106438G | [@YueLi040912](https://github.com/YueLi040912) | **团队负责人** / XR 架构、核心交互系统、Whisper 语音转写管线与主场景集成 |
| **WEI Yilin** | 26106399G | [@weiyilin2002](https://github.com/weiyilin2002) | 空间布局、环境材质与 URP 视觉优化 |
| **LI Haochen** | 26138162G | [@SkyKeeperLee](https://github.com/SkyKeeperLee) | 空间物体交互、物体缩放与物理碰撞调校 |
| **MENG Yuting** | 26050779G | [@Youtte-cloud](https://github.com/Youtte-cloud) | VR 指示板系统与悬浮 UI/UX 动画 |
| **YI Chenxi** | 26093165G | [@YiArxia](https://github.com/YiArxia) | 交互道具 / 家具机制与评审数据报告管线 |
| **GU Bingchen** | 26095785G | [@Francis-067](https://github.com/Francis-067) | 空间音频、多麦克风硬件适配与音频导出优化 |

---

## 📌 项目概述

**虚拟公寓 XR 评审系统 (Virtual Apartment XR Review System)** 是一款面向建筑设计、空间规划与室内布局评估的沉浸式虚拟现实评审与检查应用。评审人员、设计师和客户可以实时漫游 1:1 比例的公寓模型，并进行多模态空间检查：

1. **高保真空间沉浸感**：基于通用渲染管线 (URP)、预烘焙光照贴图与反射探针，提供逼真的光照效果和真实比例的空间感知。
2. **多模态空间批注**：使用 VR 激光射线指向任意表面，生成带有三级优先级（低 / 中 / 高）的三维反馈标记。
3. **多麦克风并发录音与自动增益**：针对 PCVR 串流中头显与主机麦克风音频通道不一致的问题，同时监听所有可用录音通道，根据波形峰值自动选择最佳通道并进行动态增益补偿。
4. **端侧 Whisper AI 语音转文字 (STT)**：使用 OpenAI Whisper (`ggml-base.en.bin`) 进行离线语音识别，无需云端依赖，也不会产生额外网络延迟；语音反馈会实时转写并附加到空间标记上。
5. **Alyx 风格手腕 HUD 与可召唤写字板**：通过手腕 HUD 实时查看状态，并使用左手控制器召唤带粒子效果的悬浮任务板。
6. **自动数据持久化与安全退出**：退出时将所有空间坐标、优先级标签、原始 WAV 音频文件与语音转写内容导出为结构化 CSV 报告。

---

## 🛠️ 技术栈与运行前置条件

> [!IMPORTANT]
> 为避免 `.meta` 文件损坏和 YAML 场景合并冲突，**所有团队成员必须严格使用完全相同的 Unity 版本**！

* **Unity Editor**：`2022.3.62f3 LTS`（Windows 64 位）
* **渲染管线**：Universal Render Pipeline（URP `14.0.12`）
* **XR 运行时与 SDK**：
  * **OpenXR Plugin**：`1.14.3`
  * **SteamVR / OpenVR**：`com.valvesoftware.unity.openvr`（`1.2.1`）
  * **Meta XR Core SDK**：`com.meta.xr.sdk.all`（`201.0.0`）
* **AI 与语音引擎**：
  * **Whisper.unity**：端侧离线语音转文字模型（`ggml-base.en.bin` 位于 `Assets/StreamingAssets`）
* **工具与扩展**：
  * **Unity MCP**：CoplayDev Unity-MCP 集成
  * **ProBuilder**：`5.2.4`
  * **TextMeshPro**：`3.0.7`
* **支持的 VR 硬件**：
  * Meta Quest 2 / Quest 3 / Quest Pro（通过 Quest Link / AirLink）
  * Pico Neo 3 / Pico 4 / Pico 4 Ultra（通过 Pico Connect / Streaming Assistant）
  * HTC Vive / Valve Index（原生 SteamVR）

---

## 🕹️ 系统架构与工作流程

```
                     ┌────────────────────────────────────────┐
                     │          XR 输入与控制器               │
                     │ (SteamVR Actions / OpenXR Controllers) │
                     └───────────────────┬────────────────────┘
                                         │
                 ┌───────────────────────┴───────────────────────┐
                 ▼                                               ▼
   ┌───────────────────────────┐                   ┌───────────────────────────┐
   │    MarkerTool.cs (右手)   │                   │   ClipboardSync.cs (左手) │
   │  - 平滑激光指示器          │                   │  - 悬浮指示板 UI           │
   │  - 三级优先级循环          │                   │  - 粒子召唤特效            │
   │  - 生成表面标记            │                   └───────────────────────────┘
   └─────────────┬─────────────┘
                 │
                 ├──► FeedbackMarker.cs：悬浮动画 / 悬停缩放 / Billboard 标签
                 │
                 ├──► 多麦克风音频仲裁：多通道并发录音 -> 波形峰值检测 -> 动态自动增益
                 │
                 ├──► Whisper.unity：端侧异步语音识别 -> 将转写内容附加到标记
                 │
                 ▼
   ┌───────────────────────────────────────────────────────────────────────────┐
   │                    AnnotationExporter.cs / SafeQuitManager.cs             │
   │  - 导出无损 16 位 PCM WAV 音频文件                                         │
   │  - 输出结构化 CSV 报告（AnnotationReport_*.csv，包含三维坐标）            │
   │  - 捕获 Esc / 窗口关闭事件，确保数据安全写入                             │
   └───────────────────────────────────────────────────────────────────────────┘
```

---

## 📂 项目目录结构

```
XRG9_Virtual_Apartment/
├── Assets/
│   ├── Fonts/                     # UI 字体资源
│   ├── Hovl Studio/               # 粒子和视觉特效包
│   ├── Materials/                 # PBR 材质与着色器图
│   ├── Models/                    # 家具、房间几何体与道具 3D 模型
│   │   └── Chinese Room/          # 风格化公寓房间资源
│   ├── Prefabs/                   # 可复用组件（标记、UI、Rig）
│   ├── Scenes/                    # 场景文件
│   │   ├── RealMainVirtualApartment.unity   # ★ 集成主发布场景
│   │   ├── MainVirtualApartment.unity       # 备用场景
│   │   ├── Dev_Marker.unity                 # 独立批注测试场景
│   │   ├── Dev_InteractableFoods.unity      # 独立食品交互场景
│   │   ├── Dev_ObjectScale.unity            # 独立物体缩放测试场景
│   │   ├── MENG_InstructionBoard.unity      # 指示板测试场景
│   │   └── RealMainVirtualApartment/        # 烘焙光照贴图与反射探针
│   ├── Scripts/                   # 核心应用逻辑
│   │   ├── AnnotationExporter.cs  # 静态 CSV/WAV 数据导出工具
│   │   ├── ClipboardSync.cs       # 悬浮指示板控制器
│   │   ├── FeedbackMarker.cs      # 空间标记实体与视觉反馈
│   │   ├── MarkerTool.cs          # 激光工具、录音与 Whisper STT 管线
│   │   └── SafeQuitManager.cs     # 安全关闭与数据写入管理器
│   ├── Settings/                  # URP 渲染管线设置
│   ├── SteamVR/                   # SteamVR 官方插件
│   ├── SteamVR_Input/             # SteamVR 自动生成的 Action C# 类
│   ├── StreamingAssets/           # 动态二进制资源
│   │   ├── ggml-base.en.bin       # ★ Whisper 离线模型（由 Git LFS 管理）
│   │   └── SteamVR/               # Action 绑定 JSON 文件
│   └── XR/ & XRI/                 # XR Interaction Toolkit 设置
├── Packages/                      # Unity Package Manager 清单
├── ProjectSettings/               # Unity 项目级设置
├── .gitattributes                 # Git LFS 与 Unity YAML 合并规则
├── .gitignore                     # Git 忽略规则
├── README.md                      # 主要文档（英文）
└── README_zh.md                   # 中文文档（简体中文）
```

---

## 🚀 快速开始指南

### 1. 使用 Git LFS 克隆

> [!WARNING]
> 本仓库包含 3D 模型和一个 147MB 的 Whisper 模型二进制文件，**必须使用 Git LFS**。如果未使用 LFS，二进制文件会被下载为很小的指针文件，从而导致项目崩溃。

```bash
# 1. 安装并配置 Git LFS（如果之前尚未安装）
git lfs install

# 2. 克隆仓库
git clone https://github.com/YueLi040912/XRG9_Virtual_Apartment.git
cd XRG9_Virtual_Apartment

# 3. 拉取所有由 LFS 管理的二进制资源
git lfs pull
```

### 2. 通过 Unity Hub 打开
1. 启动 **Unity Hub**，点击 **Add project from disk**；
2. 选择仓库根目录；
3. 确保 Unity Editor 版本为 **`2022.3.62f3`**；
4. 等待依赖包解析和着色器编译（首次加载约需 3~10 分钟）。

### 3. 生成 SteamVR 输入动作
* 如果启动时出现 SteamVR Input 弹窗，点击 **Yes**；
* 在顶部菜单中进入：`Window -> SteamVR Input`；
* 检查 Action Set，并点击 **Save and generate** 更新动作绑定。

### 4. 在 VR 中运行
1. 将 VR 头显连接到电脑（通过 Quest Link、Pico Connect 或 SteamVR）；
2. 在 Project 窗口中打开 `Assets/Scenes/RealMainVirtualApartment.unity`；
3. 点击 Unity 编辑器中的 **Play** 按钮开始体验。

---

## 🖥️ 如何运行构建版可执行程序

如果运行预先构建的独立程序（`.exe`），请遵循以下必要操作步骤。**无线串流推荐在 Quest 3 上使用 Steam Link 应用。**

### 1. 启动与 SteamVR 串流检查
* **启动前**：确保 Quest 3 上的 Steam Link 应用已打开，并连接到运行项目的电脑。
* **确保 SteamVR 已启动**：双击项目 `.exe` 文件后，**确认 SteamVR 正在运行，且头显与控制器均已正确追踪**（SteamVR 桌面小组件中的图标应显示为绿色常亮）。
* 如果 SteamVR 尚未启动，建议先启动 SteamVR，戴上头显进入 VR Home 空间后，再启动应用程序 `.exe`。

### 2. Windows 麦克风配置（关键）
> [!IMPORTANT]
> 端侧 Whisper 语音识别依赖 Windows 默认麦克风输入。使用 SteamVR 串流时，Windows 可能会继续选择电脑内置或桌面麦克风，而不是头显麦克风。
* 右键点击 Windows 任务栏上的扬声器图标，选择 **声音设置**（或打开 `设置 -> 系统 -> 声音`）；
* 在 **输入** 中找到“选择用于说话或录音的设备”，并**手动选择 Steam 麦克风**（通常名为 `Steam Streaming Microphone` 或 `SteamVR Microphone`）；
* 对着 VR 头显麦克风说话，确认输入音量条会动态变化。

### 3. 结束会话与安全写入数据
> [!WARNING]
> 报告生成（CSV）和 WAV 音频导出会在应用程序关闭流程中触发。**评审结束后必须正确终止可执行程序**，否则当前会话的评审数据不会保存到磁盘！

可以使用以下任一方式安全结束应用程序：
* **方式 1（推荐）**：按下电脑键盘上的 **`Escape (Esc)`** 键。该操作会调用 `SafeQuitManager`，将所有文件流写入磁盘并优雅关闭应用程序。
* **方式 2**：点击 Windows 窗口右上角的 **`[X]`** 关闭按钮（或按 `Alt + F4`）。内置拦截器会捕获退出事件并立即写入待保存数据。
* 导出的 CSV 报告和对应 WAV 录音会自动保存到 `.exe` 文件所在目录旁的 `AnnotationData/` 文件夹。

---

## 🎮 VR 控制器映射

| 控制器 | 操作 / 输入 | 功能 |
| :--- | :--- | :--- |
| **右手** | **摇杆** | 平滑移动 / 瞬移 |
| **右手** | **扳机键** | 激活激光；在表面松开以生成标记 |
| **右手** | **握持键** | 循环切换标记优先级颜色（绿色：低 → 黄色：中 → 红色：高） |
| **右手** | **悬停标记 + 按住录音键** | 录制语音反馈；松开后由 Whisper 自动转写 |
| **右手** | **悬停标记 + 按住握持键** | 持续 1.5 秒删除目标标记及其关联音频 |
| **左手** | **摇杆** | 定角 / 平滑转向 |
| **左手** | **X / Primary 按钮** | 切换可召唤的指示写字板 |
| **PC 键盘** | **Escape (Esc)** | 引导员安全退出：将数据写入磁盘并关闭应用程序 |

---

## 🤝 团队协作与 Git 指南

为保持开发稳定性并避免严重的场景合并冲突：

### 1. 分支策略
* `main`：**生产分支**。仅保留干净、稳定且经过测试的发布版本。禁止直接提交；变更必须通过 Pull Request，并经团队负责人审核。
* `PhaseX_LiYue`：主要迭代交付使用的**里程碑集成分支**。
* `developing_<Name>` / `feature/<FeatureName>`：**个人功能分支**。日常开发必须在独立分支中进行，例如：
  * `developing_Wei_Yinlin`
  * `Meng_yuting`
  * `Li,-Haochen`
  * `YI_Chenxi`
* **同步代码**：在创建 PR 前运行 `git fetch origin`，并将 `origin/main` 合并到本地分支，使分支保持最新。

### 2. 场景与预制体优先原则
> [!CAUTION]
> **禁止在功能分支中直接修改主场景 (`RealMainVirtualApartment.unity`)！** 合并冲突的场景 YAML 文件容易出错，并可能导致数据损坏。

* **开发场景**：在独立场景（`Assets/Scenes/Dev_*.unity`）中开发功能；
* **预制体工作流**：将模块化功能制作成 `Assets/Prefabs/` 中的 Prefab；
* **场景集成**：仅由团队负责人或指定集成人员将经过验证的 Prefab 引入主场景。

### 3. `.meta` 文件完整性
* 每个 Unity 资源都有一个记录唯一 GUID 的配套 `.meta` 文件。
* **新增、修改或移动 `.meta` 文件时，必须与对应源资源一起提交**。
* 提交前运行 `git status`，确保没有未追踪的 `.meta` 文件或孤立的 `.meta` 文件。

### 4. 语义化提交信息
请遵循以下约定式提交格式：
```
<type>: <subject>

[可选正文]
```
常见类型包括：
* `feat`：新增功能
* `fix`：修复问题
* `scene`：场景设置、光照烘焙或物体布局
* `assets`：导入或更新 3D 模型、纹理、音频等资源
* `refactor`：代码重构，不改变行为
* `docs`：文档更新
* `perf`：性能优化（如减少绘制调用、优化光照贴图）

---

## 📊 评审数据输出格式

评审会话结束时，报告数据会自动保存到磁盘：

### CSV 报告示例
文件名格式：`AnnotationReport_YYYYMMDD_HHMMSS.csv`（使用 UTF-8 BOM 编码）
```csv
ID,Priority,PositionX,PositionY,PositionZ,AudioFiles,Transcriptions
1,High,1.240,1.150,-3.420,Marker_1_20261110_143000.wav,"这个厨房台面的高度似乎太低，使用起来不够舒适。"
2,Medium,-0.850,2.100,-1.200,Marker_2_20261110_143215.wav,"可以考虑将吊灯材质改成暖色黄铜。"
```

### 文件位置
* **Unity 编辑器模式**：`%USERPROFILE%\Desktop\VirtualApartment_ReviewData\`
* **独立构建模式**：`<应用程序可执行文件目录>\AnnotationData\`

---

## ❓ 常见问题排查

1. **黑屏或没有 VR 输入？**
   * 确认 SteamVR 或 Oculus Link 能正确识别头显与控制器；
   * 进入 `Edit -> Project Settings -> XR Plug-in Management`，确认已启用 **OpenXR** 或 **Oculus**。
2. **麦克风无声或 Whisper 转写为空？**
   * 确认 Windows 麦克风隐私设置允许桌面应用访问麦克风；
   * 在串流客户端（Virtual Desktop / Quest Link / Pico Connect）中确认麦克风传输已开启。
3. **材质显示为洋红色 / 粉色（缺少着色器）？**
   * 项目使用 URP。选择粉色材质，打开 `Window -> Rendering -> Render Pipeline Converter`，选择 `Built-in to URP` 并进行转换。
4. **Git LFS 出现问题或二进制文件损坏？**
   * 运行 `git lfs install`，然后运行 `git lfs pull`，用真实二进制文件替换指针文件。

---

*© 2026 COMP5424 Group 9. 保留所有权利。*