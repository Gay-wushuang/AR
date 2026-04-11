# EEG_AI_AR Project - Agent Guide

## 📋 项目概述

**项目名称**: EEG_AI_AR (My project)
**项目类型**: Unity 6 增强现实 (AR) 应用
**Unity 版本**: 6000.4.0f1
**渲染管线**: Universal Render Pipeline (URP) 17.4.0
**目标平台**: **Rokid AR Lite (Station 2)** / 移动设备 / AR 设备
**开发机配置**: i5-12600KF + RTX 4060 + 64GB DDR4 3600

### 项目描述

这是一个基于 Unity 6 的 AR (增强现实) 项目，结合了脑电波 (EEG) 数据与人工智能 (AI) 技术，构建沉浸式的太空探索体验。项目包含完整的月球表面场景和高保真的宇航员角色模型，目标部署到 **Rokid AR Lite** 设备上运行。

---

## 🎯 目标设备: Rokid AR Lite

### 硬件规格
| 参数 | 规格 |
|------|------|
| **处理器** | 高通骁龙 6 Gen1 (4nm, 4×A78@2.2GHz + 4×A55@1.8GHz) |
| **GPU** | Adreno 710 (集成显卡，共享内存) |
| **运行内存** | **8 GB LPDDR5** (实际可用约 4-5GB) |
| **存储空间** | **128 GB ROM** |
| **电池** | 5000mAh / 18W 快充 / 续航约 4 小时 |
| **显示屏** | 双眼 Sony Micro-OLED 0.68", 单眼 1080×1200, 最高 90/120Hz |
| **视场角 (FOV)** | ~50° |
| **亮度** | 600 nits |
| **眼镜重量** | 75g |
| **操作系统** | YodaOS-Master (空间计算系统) |
| **追踪方式** | 3DoF 头部追踪 |
| **交互方式** | 射线交互、多窗口布局、蓝牙键鼠 |

### Rokid SDK 兼容性（重要！）
| SDK 版本 | 支持 Unity 版本 | **适配设备** | 核心功能 |
|---------|----------------|-------------|---------|
| **UXR 2.0** ✅ | Unity 2020/2021/**2022 LTS** | **Station 2、AR Lite** ✅ | 基础4类手势(捏合/握拳/手掌/松开)、射线交互、远近场切换 |
| UXR 3.0 | Unity 2022/**2023.3 LTS** | Station Pro、Max 2、AR Studio | +图像识别(扫二维码)、手势置信度过滤、模型轻量化30%、26骨骼点追踪 |

> ⚠️ **关键约束**: Rokid AR Lite (Station 2) 官方仅支持 **UXR 2.0**，最高兼容 **Unity 2022 LTS**。
> 当前项目使用 Unity 6000.4.0f1 (Unity 6)，超出 UXR 2.0 支持范围。
> **备选方案**: Station 2 同时支持标准 OpenXR 协议，可尝试 Unity 6 + OpenXR 路线（部分 Rokid 专属功能可能不可用）。

### Rokid SDK 家族
```
Rokid SDK 全家桶:
├── CXR-M   → 跑在手机端，"手机当大脑，眼镜当屏幕"（字幕/翻译/遥控）
├── CXR-S   → 跑在 Sprite 系统眼镜端，配合 CXR-M 使用
└── UXR     → 跑在 Unity 里，让 Unity 3D 场景能在 Rokid 上跑 ✅ 本项目用这个
    ├── UXR 2.0  ← AR Lite 用这个
    └── UXR 3.0  ← 更高级设备用
```

---

## 🏗️ 项目架构

```
AR/
├── My project/                    # Unity 主项目目录
│   ├── Assets/
│   │   ├── Models/               # 模型目录
│   │   ├── Scenes/               # Unity 场景文件
│   │   │   └── SampleScene.unity # 示例场景
│   │   ├── Scripts/              # C# 脚本
│   │   │   ├── EnhanceMoonScene.cs    # 增强月球场景脚本
│   │   │   └── SetupMoonSurface.cs    # 月球表面设置脚本
│   │   ├── Settings/             # URP 和质量设置
│   │   │   ├── Mobile_RPAsset.asset      # 移动端渲染管线资产
│   │   │   ├── PC_RPAsset.asset          # PC 端渲染管线资产
│   │   │   └── DefaultVolumeProfile.asset # 默认后处理配置
│   │   └── TutorialInfo/         # 教程信息脚本
│   ├── Packages/                 # 依赖包管理
│   ├── ProjectSettings/          # 项目配置
│   ├── Library/                  # Unity 缓存（不版本控制）
│   ├── Temp/                     # 构建临时文件（不版本控制）
│   └── UserSettings/             # 用户设置
├── astronaut/                    # 宇航员 3D 资源
│   ├── source/
│   │   └── Astronaut.glb        # GLB 格式宇航员模型
│   └── textures/                # 模型纹理贴图
├── moon/                         # 月球场景资源
│   ├── textures/                # 月球表面纹理集合
│   │   ├── moon_02_diff/nor/rough/disp.png  # 月球基础材质
│   │   ├── moon_footprints_02_*.png          # 足迹细节
│   │   ├── moon_macro_01_*.png              # 宏观纹理
│   │   ├── moon_meteor_01_*.png             # 陨石坑
│   │   ├── moon_rock_01~07_*.png            # 岩石变体
│   │   ├── earth_diff/norm.png              # 地球纹理
│   │   └── ph_grid/lens_dirt.png            # 特效纹理
│   └── ph_moon.blend            # Blender 源文件
├── textures/                     # 月球和宇航员纹理资源（主目录）
│   ├── earth_diff.png           # 地球漫反射
│   ├── earth_norm.png           # 地球法线
│   ├── Marker_07.png            # 标记纹理
│   ├── moon_02_*.png            # 月球主体纹理（diff/nor/rough/disp）
│   ├── moon_footprints_02_*.png # 足迹纹理
│   ├── moon_macro_01_*.png      # 宏观地形纹理
│   ├── moon_meteor_01_*.png     # 陨石坑纹理
│   ├── moon_rock_01~07_*.png    # 岩石变体纹理
│   ├── ph_grid.png              # 网格纹理
│   ├── ph_lens_dirt.png         # 镜头污垢特效
│   └── VisorScratch.png         # 面罩划痕细节
├── AGENTS.md                     # 项目文档
├── .gitattributes               # Git LFS 配置
├── .gitignore                   # Git 忽略文件配置
└── chatgpt建议.md               # 参考文档
```

---

## 🛠️ 技术栈

### 核心技术
- **引擎**: Unity 6 (6000.4.0f1)
- **语言**: C# (.NET)
- **渲染**: URP (Universal Render Pipeline) 17.4.0
- **脚本**: Visual Scripting + C#
- **3D 格式**: GLB/GLTF, Blend (Blender)
- **AR SDK**: UXR 2.0 (首选) / OpenXR (备选)

### 主要依赖包
| 包名 | 版本 | 用途 |
|------|------|------|
| com.unity.render-pipelines.universal | 17.4.0 | 通用渲染管线 |
| com.unity.inputsystem | 1.19.0 | 新输入系统 |
| com.unity.ai.navigation | 2.0.11 | AI 导航 |
| com.unity.modules.vr | 1.0.0 | VR 支持 |
| com.unity.modules.xr | 1.0.0 | XR 支持 |
| com.unity.visualscripting | 1.9.10 | 可视化脚本 |
| com.unity.timeline | 1.8.11 | 时间线动画 |
| com.unity.multiplayer.center | 1.0.1 | 多人游戏支持 |
| com.unity.ide.rider | 3.0.39 | Rider IDE 集成 |

### 开发工具
- **IDE**: JetBrains Rider / Visual Studio
- **3D 建模**: Blender v5.01+
- **版本控制**: Git + Git LFS 3.5.1 (已安装)

---

## 💾 资产规模与优化需求（关键！）

### 当前资产总览
| 文件类型 | 数量 | 单文件最大 | 总计估算 |
|---------|------|-----------|---------|
| `.blend` 文件 | 2 个 | **1,814 MB** (~1.8 GB) | ~3.6 GB |
| 大型 PNG 纹理 | ~10 个 | **170 MB** | ~1 GB |
| 中型 PNG 纹理 | ~30+ 个 | 5~90 MB | ~500 MB |
| `Astronaut.glb` | 1 个 | **44 MB** | 44 MB |
| **合计** | **~70+ 文件** | — | **~5 GB+** |

### ⚠️ 为什么必须优化：原始资产 vs Rokid Lite 的差距

| 资源 | 当前规格 | 问题 | Rokid Lite 合理上限 |
|------|---------|------|--------------------|
| 宇航员三角面 | **56,200 面** | 超标 5-6x | **8,000-15,000** |
| 单张纹理大小 | 最大 **170 MB** | 超标 42x | **≤ 4 MB** |
| 纹理分辨率 | 可能 8K/16K | 远超屏幕需求 | **1024-2048** |
| 骨骼数量 | **90 根** | 过多 | **25-30 根** |
| 运行时显存 | 可能 > 2 GB | 会崩溃 | **< 300 MB** |
| 总资源包体 | ~5 GB | 存储和加载问题 | **50-100 MB** |

### 优化目标对比
```
优化前（原始素材）              优化后（Rokid Lite 可用）
─────────────────────         ─────────────────────
总大小:  ~5 GB                 总大小:  ~50-100 MB ↓ 98%
宇航员:  44MB / 56K面           宇航员:  ~5MB / 8-12K面
单纹理:  最大 170MB             单纹理:  最大 4MB
显存:    可能 > 2GB             显存:    < 300MB
帧率:    可能 < 10 FPS          帧率:    目标 45-60 FPS
```

### 必须执行的优化操作
1. **模型减面**: Blender Decimate Modifier → 宇航员从 56K → 8-12K 三角面
2. **骨骼精简**: 90根 → 25-30根（删除手指细节、面部表情、IK辅助骨等）
3. **纹理降分辨率**: 8K/16K → 1024 或 2048
4. **纹理压缩**: PNG → ASTC 4×4 (Android) / ETC2
5. **LOD 分级**: 生成 LOD0/LOD1/LOD2 多级细节
6. **图集合并**: 30+ 张独立纹理 → 几张合并大图 (Atlas)，减少 Draw Call
7. **Shader 精简**: 使用 Mobile/Lite 版 Shader，关闭实时光影
8. **异步加载**: Addressables/AssetBundle 按需加载

---

## 🔄 开发工作流（两阶段法）

### 第一阶段：PC 端创作与验证（当前阶段）

在开发机 (i5-12600KF + RTX 4060 + 64GB RAM) 上使用原始高精度资产搭建场景：

```
Step 1: 导入资产
  ├─ Astronaut.glb 直接导入 Unity
  ├─ 月球纹理直接导入
  └─ ph_moon.blend 在 Blender 中参考

Step 2: 搭建场景
  ├─ 月球地形 + 岩石分布
  ├─ 宇航员放置 + 动画
  ├─ 光照设置 (Directional Light 模拟阳光)
  ├─ 地球天体 (天空盒背景)
  └─ 后处理 (Bloom, Color Grading 等)

Step 3: 验证效果
  ├─ 视觉效果是否满意
  ├─ 动画/交互逻辑是否正确
  └─ 整体氛围对不对
```

> PC 配置远超 Rokid Lite (~15-20x GPU 性能)，原始资产在 PC 上运行无压力。

### 第二阶段：移动端优化与适配（确认效果后执行）

针对 Rokid AR Lite 进行全面优化：

```
Step 4: 模型优化     → 减面 + 骨骼精简 + LOD 生成
Step 5: 纹理优化     → 降分辨率 + ASTC 压缩 + 图集合并
Step 6: 渲染优化     → Mobile Shader + SRP Batching + GPU Instancing
Step 7: 平台适配     → Android Build Support + UXR/OpenXR + ARM64 + IL2CPP
Step 8: 打包测试     → 部署到 Rokid Lite + Profiler 分析 + 迭代调优
```

---

## 📦 Git LFS 配置

### 为什么需要 Git LFS
- 项目包含 ~5GB+ 的大型二进制文件（GLB、PNG、BLEND）
- GitHub 单文件上限 100MB，推荐 < 50MB
- 不使用 LFS 会导致 Git 仓库爆炸式膨胀

### 当前状态
- ✅ Git LFS 已安装: **版本 3.5.1** (GitHub 官方 Windows 版)
- ✅ `.gitattributes` 已配置并生效
- ✅ `.gitignore` 已配置（包含 .blend1 等规则）
- ✅ Git 仓库已初始化，可开始版本控制

### 👥 协作者拉取指南（重要！）

#### 前置要求
协作者必须安装 Git LFS，否则拉取的大文件会是空的指针文件！

**Windows/macOS/Linux:**
```bash
# 1. 安装 Git LFS
git lfs install

# 2. 克隆仓库（快速，只下载元数据）
git clone <repository-url>
cd <project-directory>

# 3. 下载实际大文件（可能需要几分钟）
git lfs pull
```

**验证安装：**
```bash
# 检查 LFS 是否正常工作
git lfs ls-files

# 应该看到 66 个被追踪的文件列表
# 例如：
# 1b028051fa * astronaut/source/Astronaut.glb
# 3a9bfef914 * moon/ph_moon.blend
# ...
```

#### ⚠️ 常见问题

**问题 1：拉取后模型/纹理文件是空的或只有几 KB**
```bash
# 原因：未安装 Git LFS，下载到的是指针文件而非实际内容
# 解决：
git lfs install
git lfs pull  # 重新下载大文件
```

**问题 2：推送时被 GitHub 拒绝 "File exceeds 100MB"**
```bash
# 原因：某些大文件未被 LFS 追踪
# 解决：检查 .gitattributes 是否包含该文件类型
git lfs track "*.你的文件格式"
git add .gitattributes
git commit -m "Add LFS tracking for new file type"
```

**问题 3：clone 速度极慢**
```bash
# 解决方案：先 clone 空仓库，再单独拉取 LFS 文件
git clone --no-checkout <repository-url>
cd <project-directory>
git checkout main      # 或你的分支名
git lfs pull           # 按需下载大文件
```

### 已追踪的文件类型
```
*.glb            # 3D 模型（宇航员等）
*.blend          # Blender 源文件
*.fbx            # FBX 格式模型
textures/*.png   # 月球和宇航员纹理（50+ 张）
*.wav, *.mp3     # 音频文件
```

### 需要追踪的文件类型
```
*.glb            # 宇航员模型等 3D 文件
*.blend          # Blender 源文件（注意排除 .blend1 备份）
*.fbx            # 未来可能用的格式
textures/*.png   # 月球和宇航员纹理贴图
*.wav *.mp3      # 音频文件（如有）
```

### .gitignore 补充项
```
# Blender 自动备份（不应纳入版本控制）
*.blend1
```

### 配置命令（待执行）
```bash
git lfs install                                    # 初始化 LFS
git lfs track "*.glb" "*.blend" "*.fbx" "*.png"    # 追踪大文件
git lfs track "*.wav" "*.mp3"
git add .gitattributes                             # 提交配置
git commit -m "Enable Git LFS for large assets"
```

---

## 🔧 Android 构建环境

### 必须安装的 Unity Hub 模块
在 Unity Hub → Installs → 选择 Unity 6000.4.0f1 → Add Modules 中勾选：
- ☑️ **Android Build Support** — 打包 APK/ABB 必须
- ☑️ **Android SDK & NDK Tools** — 编译 ARM64 原生代码必须
- ☑️ **OpenJDK** — Java 打包工具链必须

三个模块总计约 5-8 GB 磁盘空间。

### Player Settings 关键配置（Android/Rokid）
- **Scripting Backend**: IL2CPP
- **Target Architectures**: ARM64（唯一）
- **Graphics APIs**: OpenGLES3 或 Vulkan（需测试选择）
- **Color Space**: Linear
- **minSdk**: ≥ 28 (CXR-M 要求)
- **XR Plug-in Management**: 启用 OpenXR 或 UXR 2.0

---

## 🎮 核心功能模块

### 1. AR/VR 场景系统
- **月球表面环境**: 高保真 PBR 材质的月球场景
- **多平台渲染**: 支持移动端 (Mobile RP) 和 PC 端 (PC RP) 双渲染管线
- **后处理效果**: 通过 Volume Profile 管理画面效果

### 2. 3D 角色系统
- **宇航员模型**: 完整的宇航员 GLB 模型
- **PBR 材质**: 包含 Albedo、Normal、Roughness、AO 等完整贴图通道
- **细节纹理**: 面罩划痕、标记等细节纹理

### 3. 输入与交互
- **新输入系统**: 使用 Input System 1.19.0
- **跨平台输入**: 支持触屏、手柄、键盘鼠标
- **AR 交互**: 手势识别(UXR 2.0: 4种基础手势)、射线交互、空间定位

### 4. AI 系统
- **导航网格**: Unity NavMesh 支持
- **路径寻找**: AI 角色自动寻路
- **行为树**: 可扩展的 AI 行为框架

### 5. 动画与时间线
- **Timeline**: 电影级动画序列控制
- **Visual Scripting**: 无代码/低代码逻辑开发
- **状态机**: 角色状态管理

---

## 📁 关键文件说明

### 配置文件
- [manifest.json](My project/Packages/manifest.json): UPM 包依赖定义
- [ProjectSettings.asset](My project/ProjectSettings/ProjectSettings.asset): 核心项目设置
- [settings.json](My project/.vscode/settings.json): VSCode 工作区配置
- [EditorBuildSettings.asset](My project/ProjectSettings/EditorBuildSettings.asset): 构建场景列表

### 资源文件

#### 🧑‍🚀 宇航员模型 - [Astronaut.glb](astronaut/source/Astronaut.glb)

**文件格式**: glTF 2.0 (Binary)
**导出工具**: Microsoft GLTF Exporter 2.8.3.2
**文件大小**: **44 MB**

**几何数据**:
| 网格 | 顶点数 | 面片数 | 边界框范围 |
|------|--------|--------|------------|
| 主网格 (身体) | 110,034 | ~36,900 三角形 | X: ±36.47m, Y: 67.72~229.81m, Z: -41.96~+29.23m |
| 第二网格 (装备) | 57,969 | ~19,300 三角形 | X: ±130.97m, Y: -0.06~+237.09m, Z: -23.54~+25.62m |
| 小对象 (面罩/细节) | 8 | 12 索引 | 小型附件 |

**骨骼动画系统**:
- 骨骼数量: **90 个关节** (MAT4 变换矩阵) — 优化时需精简至 25-30 根
- 支持蒙皮骨骼动画 (Skinned Mesh)
- 支持形变目标 (Morph Targets)

**坐标系特征**:
Y 轴范围 67~229 米，模型采用非标准坐标系或较大缩放比例。Unity 导入时需设置 Scale Factor。

**纹理贴图** ([astronaut/textures/](astronaut/textures/)):
- gltf_embedded_0.png - 基础颜色贴图
- gltf_embedded_1@channels=RGB.png / @channels=A.png - RGB + Alpha 分离贴图
- gltf_embedded_2.png ~ gltf_embedded_7.png - 法线、粗糙度、金属度、AO 等 PBR 通道

---

#### 🌙 月球场景 - [ph_moon.blend](moon/ph_moon.blend)

**文件格式**: Blender Native (.blend)
**文件大小**: **1,814 MB × 2** (含 .blend1 备份)
**Blender 版本**: v4.0+ (64位 Little-Endian)
**场景名称**: "Scene"

**工作区配置** (Workspace Layouts):
- **Animation** - 关键帧动画编辑器
- **Compositing .001** - 后期特效合成节点
- **Sculpting 01** - 高精度雕刻模式 ⭐ (月球表面主要制作工具)
- **Shading .001** - 材质着色器节点编辑器
- **UV Editing 1** - UV 纹理坐标展开与布局

**内部场景结构推断**:

Mesh Objects (网格对象):
```
├─ moon_02          # 月球主体表面
├─ moon_footprints_02  # 足迹细节层
├─ moon_macro_01      # 宏观地形纹理
├─ moon_meteor_01     # 陨石坑区域
└─ moon_rock_01~07    # 7 种岩石变体对象
```

Materials (材质系统):
- 使用 Principled BSDF PBR 工作流
- 支持 Displacement (位移)、Normal Map (法线)、Roughness (粗糙度) 通道
- 节点式材质编辑器配置

Textures (外部化纹理) ([moon/textures/](moon/textures/)) — **33 张 PNG**:
| 对象名称 | Diffuse (漫反射) | Normal (法线) | Roughness (粗糙度) | Displacement (位移) |
|----------|------------------|--------------|-------------------|---------------------|
| moon_02 | ✅ (77MB) | ✅ (87MB) | ✅ (22MB) | ✅ (30MB) |
| moon_footprints_02 | ✅ (170MB!) | ✅ (153MB!) | ✅ (51MB) | ✅ (47MB) + disp_mask3 |
| moon_macro_01 | ✅ (86MB) | ✅ (91MB) | ✅ (26MB) | ✅ (28MB) |
| moon_meteor_01 | ✅ (71MB) | ✅ (87MB) | ✅ (19MB) | ✅ (28MB) + disp_blur |
| moon_rock_01~07 | 各自独立 diff/nor/rough/disp (每套 14-17MB) | | | (rock_04 含 AO) |

**其他纹理资源**:
- earth_diff/norm.png - 地球天体 (漫反射 + 法线)
- Marker_07.png / VisorScratch.png - 标记和面罩划痕细节
- ph_grid.png / ph_lens_dirt.png - 网格和镜头污垢特效

### 渲染配置
- [Mobile_RPAsset.asset](My project/Assets/Settings/Mobile_RPAsset.asset): 移动端优化渲染器
- [PC_RPAsset.asset](My project/Assets/Settings/PC_RPAsset.asset): PC 高质量渲染器
- [DefaultVolumeProfile.asset](My project/Assets/Settings/DefaultVolumeProfile.asset): 默认后处理配置

---

## 📦 大文件下载与放置指南

### ⚠️ 重要说明
由于 Git 仓库不包含大型二进制资产（.glb, .blend, PNG 纹理等），你需要从共享网盘下载这些文件并放置到指定位置。

### 📋 下载清单

| 文件/目录 | 大小 | 说明 |
|---------|------|------|
| `astronaut/` | ~60 MB | 宇航员 3D 资源（含 GLB 模型 + 纹理） |
| `moon/` | ~1.8 GB | 月球场景资源（含 .blend 源文件 + 纹理） |
| `textures/` | ~1.5 GB | 月球和宇航员纹理（主目录） |
| **总计** | **~3.4 GB** | |

### 📂 放置位置

下载后请将文件放置到以下目录：

```
AR/
├── astronaut/              ← 放在项目根目录
│   ├── source/
│   │   └── Astronaut.glb
│   └── textures/
│
├── moon/                   ← 放在项目根目录
│   ├── textures/
│   └── ph_moon.blend
│
├── textures/               ← 放在项目根目录
│
└── My project/
    └── Assets/
        └── Models/
            ├── astronaut/  ← （可选，由 Unity 自动管理）
            └── moon/       ← （可选，由 Unity 自动管理）
```

### 🚀 下一步
放置完文件后：
1. 打开 Unity 编辑器
2. Unity 会自动导入这些资源
3. 等待导入完成后即可开始开发

---

## 🚀 快速开始

### 环境要求
- **Unity Editor**: 6000.4.0f1 或更高版本
- **操作系统**: Windows 10/11, macOS 10.15+
- **硬件要求**: 支持 DX11/Vulkan 的 GPU
- **IDE**: 推荐 JetBrains Rider 2023.3+
- **Git LFS**: 3.5.1+ (已安装)

### 安装步骤
1. 使用 Unity Hub 打开 `My project` 目录
2. Unity 会自动还原 Packages 中定义的依赖
3. 等待 Library 缓存重新生成（首次打开较慢，因有大型二进制资产）
4. 打开 `Assets/Scenes/SampleScene.unity` 查看示例场景
5. 按 Play 按钮运行预览

### 构建部署
```bash
# 在 Unity Editor 中:
# File -> Build Settings
# 选择目标平台 (Android/iOS/Windows Standalone/WSA for HoloLens)
# 点击 Build And Run
```

---

## 🎨 开发规范

### 代码风格
- 使用 C# 编码约定（PascalCase 命名）
- 脚本放在 `Assets/Scripts/` 目录（按功能分子目录）
- 公共类添加 XML 注释文档
- 使用 `[SerializeField]` 替代 public 字段暴露 Inspector 参数

### 资源管理
- 3D 模型统一使用 GLB/GLTF 格式导入
- 纹理遵循命名规范：`{对象}_{类型}_{通道}.png`
  - 类型：diff(漫反射), nor(法线), rough(粗糙度), disp(位移), ao(环境光遮蔽)
- Blender 源文件保留在对应资源目录
- **大型二进制文件必须通过 Git LFS 管理**

### 场景组织
- 使用 Mobile_RPAsset / PC_RPAsset 区分移动端/PC 端渲染配置
- Volume Profile 统一管理后处理
- Timeline 控制过场动画序列

---

## 🔧 常见任务

### 添加新 3D 模型
1. 准备 GLB/GLTF 格式模型文件
2. 放入对应资源目录（如 `astronaut/source/`）
3. Unity 自动导入，可在 Project 窗口查看
4. 调整 Import Settings（Scale Factor、Materials）

### 修改月球材质
1. 打开 `moon/ph_moon.blend`（需要 Blender）
2. 编辑材质节点或替换纹理
3. 导出更新后的纹理到 `moon/textures/`
4. Unity 自动检测变更并重新导入

### 配置 AR 功能 (Rokid)
1. 在 Project Settings 中启用 XR Plugin Management
2. 安装 Rokid UXR 2.0 SDK 或配置 OpenXR
3. 配置 Rokid AR Session 和 AR Origin
4. 设置 Camera 背景（Camera Background + AR Camera Background）
5. 启用手势识别（UXR 2.0: Pinch/Grip/Palm/OpenPinch）

### 配置 Git LFS（已完成）
✅ `.gitattributes` 已配置，追踪以下文件类型：
- `*.glb`, `*.gltf`, `*.blend`, `*.fbx` - 3D 模型
- `textures/*.png` - 纹理贴图
- `*.wav`, `*.mp3` - 音频文件

✅ `.gitignore` 已配置，忽略 Blender 备份文件（`*.blend1` 等）

**如需添加新文件类型到 LFS：**
```bash
git lfs track "*.你的文件格式"
git add .gitattributes
git commit -m "Add LFS tracking for new file type"
```

### 添加新脚本
1. 在 `Assets/Scripts/` 创建 `.cs` 文件
2. 继承 `MonoBehaviour` 或 ScriptableObject
3. 使用 `[RequireComponent]` 声明依赖组件
4. 挂载到 GameObject 或作为 Asset 引用

---

## 📊 当前配置状态

### 移动端渲染配置
- 使用 Mobile_RPAsset 渲染管线（已配置）
- 纹理压缩格式: ASTC (Android) / PVRTC (iOS)
- Shadow Distance 和 Cascade Count 已针对移动端调整
- Occlusion Culling 和 GPU Instancing 已启用

### 版本控制状态
- Git LFS: 已安装 v3.5.1 ✅
- .gitattributes: 已创建并配置 ✅
- .gitignore: 已配置（包含 .blend1 规则）✅
- Git 仓库: 已初始化 ✅

### Android 构建环境
- Android Build Support: 待安装 ❌
- Android SDK & NDK Tools: 待安装 ❌
- OpenJDK: 待安装 ❌

---

## 🐛 故障排除

### 常见问题
**问题**: 场景粉色/材质丢失
- **原因**: URP Shader 未正确配置
- **解决**: Graphics Settings -> URP Global Settings 确认 Renderer Data 已分配

**问题**: 输入无响应
- **原因**: Input System 与旧 Input Manager 冲突
- **解决**: Project Settings -> Player -> Other Settings -> Active Input Handling 选择 "Both" 或 "New Input System"

**问题**: XR/AR 功能不可用
- **原因**: 未安装对应平台 SDK
- **解决**: Package Manager 安装 AR Foundation 和目标平台插件

**问题**: 模型导入后尺寸异常（太大/太小）
- **原因**: GLB 模型坐标系非标准（Y轴范围 67-229 米）
- **解决**: Import Settings 中调整 Scale Factor

**问题**: GitHub 推送失败 "file too large"
- **原因**: 大文件未走 Git LFS
- **解决**: 配置 Git LFS 并重新跟踪文件（注意已提交的大文件需要 BFG Repo Cleaner 清理历史）

---

## 📚 学习资源

- [Unity 6 官方文档](https://docs.unity3d.com/6000.0/Documentation/)
- [URP 手册](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.4/manual/index.html)
- [AR Foundation 指南](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest)
- [Input System 文档](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.9/api/)
- [Visual Scripting 参考](https://docs.unity3d.com/Packages/com.unity.visualscripting@1.9/manual/)
- [Rokid AR Platform 开发文档](https://ar.rokid.com/doc)
- [Rokid UXR SDK 文档](https://developer.rokid.com/)
- [Rokid Unity OpenXR Plugin (com.rokid.xr.unity)](https://npm.rokid.com/) — 通过 Scoped Registry 安装

---

## 👥 团队协作

### 分支策略
- `main`: 生产稳定版
- `develop`: 开发集成版
- `feature/*`: 新功能分支
- `hotfix/*`: 紧急修复

### 工作流
1. 从 `develop` 创建 feature 分支
2. 开发完成后提交 Pull Request
3. Code Review 后合并回 `develop`
4. 测试通过后发布到 `main`

---

## 📝 项目状态

### 当前完成情况
- ✅ Unity 6 升级完成 (6000.4.0f1)
- ✅ URP 双渲染管线配置（Mobile/PC）
- ✅ 纹理资源就绪 (textures/ 目录)
- ✅ 场景搭建脚本就绪 (SetupMoonSurface.cs, EnhanceMoonScene.cs)
- ✅ Git LFS 安装完成 (v3.5.1)
- ✅ .gitattributes 和 .gitignore 配置完成
- ✅ Git 仓库已初始化
- ✅ 开发策略确定: 两阶段工作流（先 PC 搭建场景，后移动端优化）
- ✅ Rokid AR Lite 硬件规格调研完成
- ✅ UXR SDK 兼容性分析完成 (AR Lite = UXR 2.0)
- ⏳ 场景搭建中（第一阶段: PC 端验证）
- ⏳ 模型导入与配置
- ⏳ Android Build Support 模块待安装
- ⏳ AR 功能集成中 (UXR 2.0 / OpenXR 选型)
- ⏳ EEG 数据接口开发中
- ⏳ 资产优化待执行（第二阶段）

### 资产技术规格总结

**当前资产状态**:
- **纹理资源**: 已就绪 (textures/ 目录，包含 33+ 张 PNG 纹理)
- **场景脚本**: 已就绪 (SetupMoonSurface.cs, EnhanceMoonScene.cs)

**模型复杂度**:
- 宇航员: 约 56,200 三角形, 90 个关节
- 月球场景: 11+ 个独立 Mesh, 完整 PBR 材质系统

**资产优化目标**:
- 总大小: ~50-100 MB
- 宇航员: ~5MB / 8-12K 面
- 单纹理: 最大 4MB
- 显存: < 300MB

---

*最后更新: 2026-04-11*
