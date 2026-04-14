# 🌙 EEG_AI_AR - 太空探索 AR 体验

基于 **Unity 6** 的增强现实 (AR) 项目，结合脑电波 (EEG) 数据与人工智能技术，构建沉浸式月球探索体验。

![Unity](https://img.shields.io/badge/Unity-6000.4.1f1-black?logo=unity)
![URP](https://img.shields.io/badge/URP-17.4.0-blue)
![Platform](https://img.shields.io/badge/Platform-Mobile%20%7C%20VR%20%7C%20AR-orange)

---

## 📖 项目简介

本项目是一个高保真的 **月球表面 AR/VR 体验**，包含：

- 🌍 **真实感月球场景** — PBR 材质、程序化地形、陨石坑与岩石细节
- 👨‍🚀 **宇航员角色模型** — 完整的 GLB 模型 + 纹理贴图
- ☀️ **动态光照系统** — 高质量阴影、环境光反射探针
- 🔧 **可视化编辑工具** — Inspector 一键生成/优化场景

---

## 🏗️ 项目结构

```
AR/
├── My project/                          # Unity 主项目（Git 仓库核心）
│   ├── Assets/
│   │   ├── Models/                      # 📦 3D 资源（需手动放置！）
│   │   │   ├── astronaut/               # 宇航员 GLB 模型 + 纹理 (~55MB)
│   │   │   └── moon/                    # 月球纹理 + 材质 + Blender 源文件 (~3.2GB)
│   │   ├── Scenes/
│   │   │   └── SampleScene.unity        # 主场景
│   │   ├── Scripts/                     # 核心脚本
│   │   │   ├── SetupMoonSurface.cs      # 月球场景自动生成器
│   │   │   └── EnhanceMoonScene.cs      # 场景增强器（岩石、地面扩展）
│   │   ├── Settings/                    # URP 渲染配置
│   │   │   ├── Mobile_RPAsset.asset     # 移动端渲染管线
│   │   │   ├── PC_RPAsset.asset         # PC 高质量渲染管线
│   │   │   └── DefaultVolumeProfile.asset # 后处理效果
│   │   └── TutorialInfo/                # 教程信息
│   ├── Packages/
│   │   └── manifest.json                # 依赖包定义
│   └── ProjectSettings/                 # 项目配置
├── AGENTS.md                            # AI 助手开发指南
├── README.md                            # 本文档
└── .gitignore                           # Git 忽略规则（大文件已排除）
```

> ⚠️ **注意**: `astronaut/`、`moon/` 等根目录为旧版资源位置，已迁移至 `My project/Assets/Models/` 下，Git 已忽略。详见下方 **📥 大文件下载指南**

---

## 🛠️ 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| **Unity Editor** | 6000.4.1f1 | 游戏引擎 |
| **Universal Render Pipeline** | 17.4.0 | 高性能渲染 |
| **Input System** | 1.19.0 | 跨平台输入处理 |
| **GLTFast** | 6.18.0 | GLB/GLTF 模型加载 |
| **AI Navigation** | 2.0.11 | AI 寻路系统 |
| **Visual Scripting** | 1.9.11 | 可视化编程 |
| **Timeline** | 1.8.11 | 序列动画 |

---

## 🚀 快速开始

### 环境要求

- ✅ **Unity Hub** & **Unity Editor 6000.4.1f1+**
- 💻 **Windows 10/11** 或 **macOS 10.15+**
- 🎮 支持 DX11/Vulkan 的 GPU
- 💾 **磁盘空间 ≥ 10 GB**（含 Unity 缓存 + 大型 3D 资产）

### 安装步骤

```
1. 克隆仓库（不含大文件，体积小）
        ↓
2. 从云盘下载大型 3D 资产（~3.3 GB）
        ↓
3. 解压到指定目录
        ↓
4. 用 Unity Hub 打开 → 自动导入资产
        ↓
5. 打开场景 → 点击 Play 运行
```

### 第 1 步：克隆项目

```bash
git clone <repository-url>
cd EEG_AI_AR/AR
```

> 📌 由于大文件已被 `.gitignore` 排除，克隆速度很快（仅代码和配置）

---

## 📥 大文件下载与放置指南（必读！）

> ⚠️ **本项目的大体积 3D 资产（约 3.3 GB）不通过 Git/GitHub 分发**，需要手动下载并放到指定目录。

### 为什么不走 Git/LFS？

| 原因 | 说明 |
|------|------|
| 文件太大 | 总计 ~3.3 GB（BLEND 源文件 ~1.8GB + PNG 纹理 ~1.4GB + GLB ~44MB） |
| GitHub LFS 免费配额仅 1 GB | 超出需付费 $50/月 |
| 不适合版本控制 | 二进制大文件每次修改都产生完整副本 |
| 团队内网传输更快 | 云盘/网盘下载速度优于 GitHub LFS |

---

### 第 2 步：获取大文件

从团队共享云盘/网盘下载以下压缩包：

| 资源包 | 包含内容 | 大小 | 放置目标 |
|--------|---------|------|---------|
| **astronaut_assets.zip** | 宇航员 GLB 模型 + 11 张 PBR 纹理 | ~55 MB | `My project/Assets/Models/astronaut/` |
| **moon_textures.zip** | 月球场景 50+ 张 PBR 纹理贴图 | ~1.4 GB | `My project/Assets/Models/moon/textures/` |
| **moon_source.blend** | 月球 Blender 源文件（可选，用于编辑材质） | ~1.8 GB | `My project/Assets/Models/moon/` |
| **audio_assets.zip** | 背景音乐文件（musiccc.mp3） | ~10 MB | `My project/Assets/Audio/` |
| **skybox_assets.zip** | SpaceSkies Free 天空盒纹理（3 套，1K/2K/4K 分辨率） | ~200 MB | `My project/Assets/SpaceSkies Free/` |

> 💡 **没有云盘链接？** 请联系项目负责人获取共享盘地址。

---

### 第 3 步：解压到指定位置

#### ✅ 宇航员资源

```
项目根目录/
└── My project/
    └── Assets/
        └── Models/
            └── astronaut/              ← 解压 astronaut_assets.zip 到这里
                ├── source/
                │   ├── Astronaut.glb        ← 必须有此文件 (~44MB)
                │   └── Astronaut.glb.meta   ← Unity 自动生成
                └── textures/
                    ├── gltf_embedded_0.png       ← 必须有 (~11张)
                    ├── gltf_embedded_1@channels=RGB.png
                    ├── gltf_embedded_1@channels=A.png
                    ├── gltf_embedded_2.png
                    ├── gltf_embedded_3.png
                    ├── gltf_embedded_4.png
                    ├── gltf_embedded_5@channels=RGB.png
                    ├── gltf_embedded_5@channels=A.png
                    ├── gltf_embedded_6.png
                    └── gltf_embedded_7.png
```

#### ✅ 月球纹理

```
项目根目录/
└── My project/
    └── Assets/
        └── Models/
            └── moon/                  ← 解压 moon_textures.zip 的内容到这里
                ├── textures/             ← 解压 moon_textures.zip 到这里
                │   ├── moon_02_diff.png          ← 月球基础漫反射 (~77MB)
                │   ├── moon_02_nor_gl.png         ← 法线贴图 (~87MB)
                │   ├── moon_02_rough.png          ← 粗糙度 (~22MB)
                │   ├── moon_02_disp.png            ← 位移贴图 (~30MB)
                │   ├── moon_footprints_02_diff.png ← 足迹细节 (~170MB)
                │   ├── moon_footprints_02_nor_gl.png
                │   ├── moon_footprints_02_rough.png
                │   ├── moon_footprints_02_disp.png
                │   ├── moon_macro_01_diff.png      ← 宏观纹理 (~86MB)
                │   ├── moon_macro_01_nor_gl.png
                │   ├── moon_macro_01_rough.png
                │   ├── moon_macro_01_disp.png
                │   ├── moon_meteor_01_diff.png     ← 陨石坑 (~71MB)
                │   ├── moon_meteor_01_nor_gl.png
                │   ├── moon_meteor_01_rough.png
                │   ├── moon_meteor_01_disp.png
                │   ├── moon_rock_01~07_diff.png     ← 7种岩石变体 (各~15MB)
                │   ├── moon_rock_01~07_nor_gl.png
                │   ├── moon_rock_01~07_rough.png
                │   ├── moon_rock_01~07_disp.png
                │   ├── moon_rock_04_ao.png          ← 仅 rock04 有 AO
                │   ├── earth_diff.png               ← 地球天体
                │   ├── earth_norm.png
                │   ├── Marker_07.png                ← 标记细节
                │   ├── VisorScratch.png             ← 面罩划痕
                │   ├── ph_grid.png                  ← 网格特效
                │   └── ph_lens_dirt.png             ← 镜头污垢
                └── ph_moon.blend                  ← 可选: Blender 源文件 (~1.8GB)
```

#### ✅ 音频文件

```
项目根目录/
└── My project/
    └── Assets/
        └── Audio/                    ← 解压 audio_assets.zip 到这里
            └── musiccc.mp3            ← 背景音乐文件 (~10MB)
```

#### ✅ 天空盒纹理

```
项目根目录/
└── My project/
    └── Assets/
        └── SpaceSkies Free/          ← 解压 skybox_assets.zip 到这里
            ├── Skybox_1/             ← 粉色系天空盒
            │   ├── Textures/         ← 1K/2K/4K 分辨率纹理
            │   └── Pink_*_Resolution.mat
            ├── Skybox_2/             ← 绿色系天空盒
            │   ├── Textures/
            │   └── Green_*_Resolution.mat
            ├── Skybox_3/             ← 紫色系天空盒
            │   ├── Textures/
            │   └── Purple_*_Resolution.mat
            └── Demo/                 ← 演示脚本
```

---

### 第 4 步：验证文件完整性

下载并放置完成后，检查关键文件是否存在：

**Windows (PowerShell):**
```powershell
# 检查宇航员模型
Test-Path "My project/Assets/Models/astronaut/source/Astronaut.glb"

# 检查月球纹理数量
(Get-ChildItem "My project/Assets/Models/moon/textures/*.png").Count

# 检查音频文件
Test-Path "My project/Assets/Audio/musiccc.mp3"

# 检查天空盒文件
(Get-ChildItem "My project/Assets/SpaceSkies Free" -Directory).Count

# 预期输出:
# 1. True (宇航员模型)
# 2. 50+ (月球纹理)
# 3. True (音频文件)
# 4. 4+ (天空盒目录)
```

**或者直接在文件管理器中确认：**

| 检查项 | 预期结果 |
|--------|---------|
| `My project/Assets/Models/astronaut/source/Astronaut.glb` 存在？ | ✅ 文件大小约 44 MB |
| `My project/Assets/Models/astronaut/textures/` 有 11 张 png？ | ✅ |
| `My project/Assets/Models/moon/textures/` 有 50+ 张 png？ | ✅ |
| `My project/Assets/Models/moon/ph_moon.blend` 存在？（可选） | ✅ 约 1.8 GB |
| `My project/Assets/Audio/musiccc.mp3` 存在？ | ✅ 约 10 MB |
| `My project/Assets/SpaceSkies Free/` 存在？ | ✅ 包含 Skybox_1/2/3 目录 |

---

### 第 5 步：打开 Unity 项目

确认所有文件到位后：

1. 用 **Unity Hub** 打开 `My project` 目录
2. 等待 Unity 导入新资产（首次可能需要 **5~15 分钟**，取决于硬盘速度）
3. 打开 **Assets/Scenes/SampleScene.unity**
4. 点击 **Play** 按钮预览

> ⚠️ 如果看到 **粉色/紫色材质** 或 **缺失纹理**，说明文件未正确放置，请回到第 3 步检查路径。

---

### ❓ 大文件常见问题

<details>
<summary><b>下载的文件放错位置了怎么办？</b></summary>

Unity 对文件路径非常敏感。确保：
- 宇航员 GLB 在 `My project/Assets/Models/astronaut/source/` 下（不是根目录的 `astronaut/`）
- 纹理 PNG 都在对应的 `textures/` 子目录中
- 文件名**不要改名**（包括 `@channels=RGB` 这种特殊字符）

</details>

<details>
<summary><b>只有部分纹理，缺少某些岩石变体？</b></summary>

月球场景可以正常运行，只是缺少的岩石类型不会显示。7 种岩石变体（rock_01~07）并非全部必需，至少保留 `moon_02_*` 和 `moon_footprints_02_*` 即可展示基础月球表面。

</details>

<details>
<summary><b>不需要 Blender 源文件可以吗？</b></summary>

可以。`ph_moon.blend` 是编辑用源文件（1.8 GB），仅当需要修改月球材质节点或重新导出纹理时才需要。纯使用/运行项目只需纹理 PNG 文件。

</details>

<details>
<summary><b>如何确认 Unity 已正确识别新放入的文件？</b></summary>

在 Unity 的 Project 窗口中查看：
- `Assets/Models/astronaut/source/` 下应显示 Astronaut 图标
- `Assets/Models/moon/textures/` 下应显示大量纹理缩略图
- 如果显示为灰色问号图标，说明文件格式不被识别或路径错误

</details>

<details>
<summary><b>Unity 导入很慢 / 卡住怎么办？</b></summary>

首次导入大型纹理时 Unity 需要时间处理：
- 观察右下角进度条
- 1.4 GB 纹理预计需要 5~15 分钟（SSD 更快）
- 如果超过 30 分钟无响应，尝试重启 Unity Editor

</details>

---

## 🎮 核心功能

### 1️⃣ 月球场景生成 ([SetupMoonSurface.cs](My%20project/Assets/Scripts/SetupMoonSurface.cs))

在 Hierarchy 中选中挂载该脚本的 GameObject，右键 ContextMenu：

```
创建月球场景    →  一键生成完整月球环境（地面+光照+相机）
清除场景         →  删除已生成的对象
```

**功能特性：**
- ✅ 高密度网格地形（可配置细分等级 4~128）
- ✅ Perlin 噪声程序化地形起伏
- ✅ 完整 PBR 材质（漫反射 + 法线 + 粗糙度 + 位移）
- ✅ 太阳光 + 软阴影 + 反射探针

### 2️⃣ 场景增强 ([EnhanceMoonScene.cs](My%20project/Assets/Scripts/EnhanceMoonScene.cs))

在 Inspector 中调整参数后使用 ContextMenu：

```
扩大地面          →  扩展月球表面到指定尺寸
添加岩石          →  随机散布 7 种 PBR 岩石
清除岩石          →  删除所有生成的岩石
完整优化场景      →  执行以上全部操作
```

**可调参数：**
| 参数 | 默认值 | 说明 |
|------|--------|------|
| `groundSize` | 200m | 地面尺寸 |
| `rockCount` | 15 | 岩石数量 |
| `scatterRadius` | 40m | 分布半径 |
| `rockScaleRange` | 0.5~3m | 岩石大小范围 |
| `randomSeed` | 42 | 随机种子（可复现） |

---

## 📦 资源说明

### 宇航员模型 (`Assets/Models/astronaut/`)

| 文件 | 说明 | 大小 |
|------|------|------|
| [Astronaut.glb](My%20project/Assets/Models/astronaut/source/Astronaut.glb) | 主模型（含内嵌纹理） | ~44 MB |
| `gltf_embedded_*.png` (11张) | 导出的 PBR 纹理通道 | ~11 MB |

### 月球纹理 (`Assets/Models/moon/textures/`)

| 类型 | 文件命名示例 | 数量 | 总大小 |
|------|-------------|------|--------|
| 基础材质 | `moon_02_diff/nor/rough/disp.png` | 4 张 | ~216 MB |
| 足迹细节 | `moon_footprints_02_*.png` | 5 张 | ~340 MB |
| 宏观纹理 | `moon_macro_01_*.png` | 4 张 | ~260 MB |
| 陨石坑 | `moon_meteor_01_*.png` | 5 张 | ~200 MB |
| **岩石变体** | `moon_rock_01~07_*.png` | **28 张 (×7)** | ~350 MB |
| 地球 | `earth_diff/norm.png` | 2 张 | ~30 MB |
| 特效 | `ph_grid/lens_dirt.png` | 2 张 | ~5 MB |
| 其他 | `Marker_07`, `VisorScratch` | 2 张 | ~2 MB |

> 📌 **Blender 源文件**: [ph_moon.blend](My%20project/Assets/Models/moon/ph_moon.blend) （可直接编辑材质节点，~1.8 GB）

### 音频文件 (`Assets/Audio/`)

| 文件 | 说明 | 大小 |
|------|------|------|
| `musiccc.mp3` | 背景音乐 | ~10 MB |

### 天空盒纹理 (`Assets/SpaceSkies Free/`)

| 类型 | 包含内容 | 分辨率 | 总大小 |
|------|---------|--------|--------|
| Skybox_1 | 粉色系天空盒 | 1K/2K/4K | ~60 MB |
| Skybox_2 | 绿色系天空盒 | 1K/2K/4K | ~70 MB |
| Skybox_3 | 紫色系天空盒 | 1K/2K/4K | ~70 MB |
| 演示脚本 | `Demo/` 目录下的天空盒切换脚本 | - | ~1 MB |

---

## ⚙️ 构建部署

### 支持平台

| 平台 | 配置路径 |
|------|---------|
| Android | File → Build Settings → Android |
| iOS | File → Build Settings → iOS |
| Windows Standalone | File → Build Settings → PC, Mac & Linux Standalone |
| HoloLens (WSA) | File → Build Settings → Universal Windows Platform |

### 移动端优化建议

- 使用 `Mobile_RPAsset` 渲染管线（已预设）
- 纹理压缩格式: ASTC (Android) / PVRTC (iOS)
- 降低 Shadow Distance 和 Cascade Count
- 启用 GPU Instancing 和 Occlusion Culling

---

## 🤝 开发规范

### 代码风格

```csharp
// ✅ 正确的命名方式
public class SetupMoonSurface : MonoBehaviour { }
[SerializeField] private float groundSize = 200f;

// ❌ 避免
public class setupmoonsurface { }
public float GroundSize;
```

- 使用 **PascalCase** 类名和方法名
- 字段用 `[SerializeField]` + `private` / `protected`
- 公共类添加 XML 注释
- 脚本放在 `Assets/Scripts/` 目录

### Git 工作流 & 分发策略

```bash
# 创建功能分支
git checkout -b feature/xxx

# 提交代码（注意：不会提交大文件！）
git add .
git commit -m "feat: 添加新功能"

# 推送到远程
git push origin feature/xxx
```

#### 📋 .gitignore 忽略规则概览

| 类别 | 忽略内容 | 原因 |
|------|---------|------|
| **大型 3D 资产** | `astronaut/`, `moon/`, `*.glb`, `*.blend`, 纹理 PNG | 云盘分发（见上方指南） |
| **音频文件** | `*.mp3`, `*.wav`, `*.ogg` | 体积较大，云盘分发 |
| **天空盒纹理** | `SpaceSkies Free/*` | 体积较大，云盘分发 |
| **Unity 缓存** | `Library/`, `Temp/`, `Obj/`, `Build/` | 可重建，体积巨大 |
| **压缩包** | `*.zip`, `*.rar`, `*.7z` | 已解压到 Assets/ |
| **IDE 配置** | `.vs/`, `.vscode/`, `.idea/`, `*.csproj` | 个人偏好，不应共享 |
| **Blender 备份** | `*.blend1`, `*.blend2`, ... | 自动备份，无版本价值 |
| **日志** | `*.log`, `Logs/` | 运行时产物 |

> 🔗 **完整规则**: 查看 [.gitignore](/.gitignore)

---

## ❓ 常见问题

<details>
<summary><b>场景显示粉色/材质丢失？</b></summary>

**原因**: URP Shader 未正确分配  
**解决**: Edit → Project Settings → Graphics → URP Global Settings → 确认 Renderer Data 已设置  
**也可能是**: 大文件未正确放置 → 参考 [📥 大文件下载指南](#-大文件下载与放置指南必读)
</details>

<details>
<summary><b>输入无响应？</b></summary>

**原因**: Input System 与旧 Input Manager 冲突  
**解决**: Project Settings → Player → Active Input Handling → 选择 "Both" 或 "New Input System"
</details>

<details>
<summary><b>XR/AR 功能不可用？</b></summary>

**原因**: 未安装对应平台 SDK  
**解决**: Window → Package Manager → 安装 AR Foundation 和目标平台插件
</details>

<details>
<summary><b>Git 提交后同事拉取看不到模型/纹理？</b></summary>

**正常现象**！大文件已被 `.gitignore` 排除。同事需要：
1. 从云盘下载大文件（同第 2 步）
2. 放置到 `My project/Assets/Models/` 对应目录
3. 重启 Unity 让其重新导入
</details>

<details>
<summary><b>想添加新的 3D 模型但文件太大？</b></summary>

选项：
1. **推荐**: 上传到团队云盘，在 README 本文档中添加下载说明
2. **备选**: 申请 GitHub LFS（需团队付费 $50/月）
3. **临时**: 压缩后通过内部工具传输（记得加到 `.gitignore`）
</details>

---

## 📚 相关文档

- [AGENTS.md](./AGENTS.md) — AI 助手开发详细指南
- [.gitignore](./.gitignore) — Git 忽略规则完整列表
- [Unity 6 官方文档](https://docs.unity3d.com/6000.0/Documentation/)
- [URP 手册](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.4/manual/index.html)
- [AR Foundation 指南](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest)

---

## 📝 更新日志

### v1.2 (2026-04-14)

- 🎵 **新增音频资源**: 添加背景音乐文件 (`musiccc.mp3`)
- 🌌 **新增天空盒资源**: SpaceSkies Free 天空盒（3 套，1K/2K/4K 分辨率）
- 📋 **更新 .gitignore**: 忽略音频和天空盒大文件
- 📖 **更新 README.md**: 增加音频和天空盒的下载指南

### v1.1 (2026-02-04)

- 🔄 **重大变更**: 大型 3D 资产改为云盘分发模式（不再走 Git/LFS）
- 📝 新增完整的 **📥 大文件下载与放置指南**
- 🗂️ 资源目录统一迁移至 `My project/Assets/Models/`
- 📋 更新 `.gitignore` 规则（排除所有大文件、Blender 备份、压缩包等）
- 📖 README.md 全面重写，增加 FAQ 和故障排查章节

### v1.0 (初始版本)

- ✅ Unity 6000.4.1f1 升级完成
- ✅ URP 双渲染管线配置（Mobile/PC）
- ✅ 月球场景自动生成系统（SetupMoonSurface + EnhanceMoonScene）
- ✅ 7 种 PBR 岩石变体支持
- ✅ 宇航员 GLB 模型和月球纹理就绪
- ⏳ AR 功能集成中
- ⏳ EEG 数据接口开发中

---

## 📊 项目统计

| 指标 | 数值 |
|------|------|
| Unity 版本 | 6000.4.1f1 |
| URP 版本 | 17.4.0 |
| 核心脚本 | 8 个 (C#) |
| 场景文件 | 1 个 |
| 月球纹理 | 50+ 张 PBR |
| 岩石变体 | 7 种 |
| 天空盒 | 3 套（1K/2K/4K 分辨率） |
| 音频文件 | 1 个（背景音乐） |
| 大型资产总大小 | ~3.5 GB（云盘分发） |
| Git 仓库大小 | ~几 MB（仅代码+配置） |

---

<div align="center">

**EEG_AI_AR Team** © 2026

*最后更新: 2026-04-14*

*本文档随项目同步更新，如有疑问请联系团队成员*

</div>
