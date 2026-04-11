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
├── My project/                          # Unity 主项目
│   ├── Assets/
│   │   ├── Models/                      # 3D 资源（已导入 Unity）
│   │   │   ├── astronaut/               # 宇航员 GLB 模型 + 纹理
│   │   │   └── moon/                    # 月球纹理 + 材质 + Blender 源文件
│   │   ├── Scenes/                      # 场景文件
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
├── astronaut/                           # 宇航员原始资源
├── moon/                                # 月球原始资源（纹理集合）
├── AGENTS.md                            # AI 助手开发指南
└── .gitignore                           # Git 忽略规则
```

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

### 安装步骤

1. **克隆项目**
   ```bash
   git clone <repository-url>
   cd EEG_AI_AR/AR
   ```

2. **用 Unity Hub 打开**
   ```
   打开 "My project" 目录 → Unity 自动还原依赖包
   ```

3. **等待导入完成**（首次打开需要重建 Library 缓存）

4. **运行预览**
   ```
   打开 Assets/Scenes/SampleScene.unity → 点击 Play 按钮
   ```

---

## 📥 大文件下载与放置指南（必读！）

> ⚠️ **本项目的大体积 3D 资产（约 5 GB）不通过 Git/GitHub 分发**，需要手动下载并放到指定目录。

### 为什么不走 Git？

| 原因 | 说明 |
|------|------|
| 文件太大 | 总计 ~5 GB（BLEND 源文件 3.6GB + PNG 纹理 ~1.4GB + GLB 44MB） |
| GitHub LFS 免费配额仅 1 GB | 超出需付费 $50/月 |
| 不适合版本控制 | 二进制大文件每次修改都产生完整副本 |

---

### 第一步：获取大文件

从团队共享云盘/网盘下载以下压缩包：

| 资源包 | 包含内容 | 大小 | 放置目标 |
|--------|---------|------|---------|
| **astronaut_assets.zip** | 宇航员 GLB 模型 + 11 张 PBR 纹理 | ~55 MB | `My project/Assets/Models/astronaut/` |
| **moon_textures.zip** | 月球场景 50+ 张 PBR 纹理贴图 | ~1.4 GB | `My project/Assets/Models/moon/textures/` |
| **moon_source.blend** | 月球 Blender 源文件（可选，用于编辑材质） | ~1.8 GB | `My project/Assets/Models/moon/` |

> 💡 **没有云盘链接？** 请联系项目负责人获取共享盘地址。

---

### 第二步：解压到指定位置

#### 宇航员资源

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

#### 月球纹理

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

---

### 第三步：验证文件完整性

下载并放置完成后，在项目根目录运行以下命令检查关键文件是否存在：

**Windows (PowerShell):**
```powershell
# 检查宇航员模型
Test-Path "My project/Assets/Models/astronaut/source/Astronaut.glb"

# 检查月球纹理数量
(Get-ChildItem "My project/Assets/Models/moon/textures/*.png").Count

# 预期输出: 50+ 个 PNG 文件
```

**或者直接在文件管理器中确认：**

| 检查项 | 预期结果 |
|--------|---------|
| `My project/Assets/Models/astronaut/source/Astronaut.glb` 存在？ | ✅ 文件大小约 44 MB |
| `My project/Assets/Models/astronaut/textures/` 有 11 张 png？ | ✅ |
| `My project/Assets/Models/moon/textures/` 有 50+ 张 png？ | ✅ |
| `My project/Assets/Models/moon/ph_moon.blend` 存在？（可选） | ✅ 约 1.8 GB |

---

### 第四步：打开 Unity 项目

确认所有文件到位后：

1. 用 **Unity Hub** 打开 `My project` 目录
2. 等待 Unity 导入新资产（首次可能需要几分钟）
3. 打开 **Assets/Scenes/SampleScene.unity**
4. 点击 **Play** 按钮预览

> ⚠️ 如果看到 **粉色/紫色材质** 或 **缺失纹理**，说明文件未正确放置，请回到第二步检查路径。

---

### 常见问题

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

---

## 🎮 核心功能

### 1️⃣ 月球场景生成 ([SetupMoonSurface.cs](My project/Assets/Scripts/SetupMoonSurface.cs))

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

### 2️⃣ 场景增强 ([EnhanceMoonScene.cs](My project/Assets/Scripts/EnhanceMoonScene.cs))

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

| 文件 | 说明 |
|------|------|
| [Astronaut.glb](My%20project/Assets/Models/astronaut/source/Astronaut.glb) | 主模型（含内嵌纹理） |
| `gltf_embedded_*.png` | 导出的 PBR 纹理通道 |

### 月球纹理 (`Assets/Models/moon/textures/`)

| 类型 | 文件命名示例 | 数量 |
|------|-------------|------|
| 基础材质 | `moon_02_diff/nor/rough/disp.png` | 4 张 |
| 足迹细节 | `moon_footprints_02_*.png` | 5 张 |
| 宏观纹理 | `moon_macro_01_*.png` | 4 张 |
| 陨石坑 | `moon_meteor_01_*.png` | 5 张 |
| **岩石变体** | `moon_rock_01~07_*.png` | **28 张 (×7)** |
| 地球 | `earth_diff/norm.png` | 2 张 |
| 特效 | `ph_grid/lens_dirt.png` | 2 张 |

> 📌 **Blender 源文件**: [ph_moon.blend](My%20project/Assets/Models/moon/ph_moon.blend) （可直接编辑材质节点）

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

### Git 工作流

```bash
# 创建功能分支
git checkout -b feature/xxx

# 提交代码
git add .
git commit -m "feat: 添加新功能"

# 推送到远程
git push origin feature/xxx
```

> 📋 已配置 [.gitignore](/.gitignore)，大文件需手动放置，详见上方 **📥 大文件下载与放置指南**

---

## ❓ 常见问题

<details>
<summary><b>场景显示粉色/材质丢失？</b></summary>

**原因**: URP Shader 未正确分配  
**解决**: Edit → Project Settings → Graphics → URP Global Settings → 确认 Renderer Data 已设置
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

---

## 📚 相关文档

- [AGENTS.md](./AGENTS.md) — AI 助手开发详细指南
- [Unity 6 官方文档](https://docs.unity3d.com/6000.0/Documentation/)
- [URP 手册](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.4/manual/index.html)
- [AR Foundation 指南](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest)

---

## 📝 更新日志

### 当前版本 (v1.0)

- ✅ Unity 6 升级完成
- ✅ URP 双渲染管线配置（Mobile/PC）
- ✅ 月球场景自动生成系统
- ✅ 7 种 PBR 岩石变体
- ✅ 宇航员和月球资源就绪
- ⏳ AR 功能集成中
- ⏳ EEG 数据接口开发中

---

<div align="center">

**EEG_AI_AR Team** © 2026

*最后更新: 2026-02-04*

</div>
