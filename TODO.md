# EEG_AI_AR 项目开发任务清单

**项目**: EEG_AI_AR (Rokid AR Lite 太空探索应用)
**Unity 版本**: 6000.4.0f1
**目标设备**: Rokid AR Lite (Station 2)
**最后更新**: 2026-04-11

---

## 📊 总体进度

| 阶段 | 状态 | 进度 |
|------|------|------|
| 第一阶段：资产准备与场景搭建 | 🔄 进行中 | 40% |
| 第二阶段：AR 功能集成 | ⏳ 待开始 | 0% |
| 第三阶段：性能优化与部署 | ⏳ 待开始 | 0% |

---

## ✅ 已完成

- [x] Unity 6 项目初始化 (6000.4.0f1)
- [x] URP 双渲染管线配置（Mobile/PC）
- [x] 宇航员 GLB 模型导入 (110K+ 顶点, 90 骨骼, 44MB)
- [x] 月球场景纹理导入 (33 张 PBR 纹理贴图)
- [x] Git LFS 安装配置 (v3.5.1)
- [x] .gitattributes 和 .gitignore 配置
- [x] Git 仓库初始化
- [x] Rokid AR Lite 硬件规格调研
- [x] UXR SDK 兼容性分析
- [x] AGENTS.md 项目文档编写
- [x] 资产目录结构整理（移动到 Assets/Models/）

---

## 🔧 第一阶段：资产准备与场景搭建（进行中）

### 1.1 资产导入与验证
- [ ] **[P0]** 在 Unity 中导入 Astronaut.glb，调整 Scale Factor
- [ ] **[P0]** 导入月球场景所有纹理到 Assets/Models/moon/textures/
- [ ] **[P1]** 验证宇航员模型在 Unity 中正确显示（材质、骨骼、动画）
- [ ] **[P1]** 验证月球纹理正确加载（无粉色/材质丢失问题）
- [ ] **[P2]** 检查并修复 GLB 坐标系问题（Y轴范围 67-229米 → 调整 Scale Factor）

### 1.2 月球表面场景搭建
- [ ] **[P0]** 创建月球地面 Plane/Mesh，应用 moon_02 PBR 材质
- [ ] **[P0]** 添加足迹细节层（moon_footprints_02 纹理）
- [ ] **[P1]** 分布岩石对象（moon_rock_01~07，7种变体）
- [ ] **[P1]** 添加陨石坑区域（moon_meteor_01 纹理）
- [ ] **[P2]** 添加宏观地形细节（moon_macro_01 纹理）
- [ ] **[P2]** 创建地球天体作为天空盒背景

### 1.3 宇航员角色设置
- [ ] **[P0]** 将宇航员模型放置到场景中
- [ ] **[P0]** 设置正确的 Scale（建议 0.01 或通过 Import Settings 调整）
- [ ] **[P1]** 配置 Animator Controller（待确认动画片段）
- [ ] **[P1]** 应用宇航员 PBR 材质（gltf_embedded_0~7.png）
- [ ] **[P2]** 添加面罩划痕细节（VisorScratch.png）

### 1.4 光照与环境
- [ ] **[P0]** 添加 Directional Light 模拟太阳光
- [ ] **[P0]** 配置环境光（Ambient Light）和反射探针
- [ ] **[P1]** 设置后处理效果（Bloom、Color Grading、Tonemapping）
- [ ] **[P1]** 配置 Volume Profile（使用 DefaultVolumeProfile.asset）
- [ ] **[P2]** 添加镜头污垢效果（ph_lens_dirt.png）

### 1.5 场景脚本开发
- [ ] **[P1]** 编写 SetupMoonSurface.cs — 月球表面自动配置脚本
- [ ] **[P1]** 编写 EnhanceMoonScene.cs — 场景增强工具
- [ ] **[P2]** 编写 AstronautController.cs — 宇航员基础控制器
- [ ] **[P2]** 创建场景管理器（SceneManager 或类似）

---

## 🥽 第二阶段：AR 功能集成（待开始）

### 2.1 Android 构建环境
- [ ] **[P0]** 安装 Android Build Support（Unity Hub → Add Modules）
- [ ] **[P0]** 安装 Android SDK & NDK Tools
- [ ] **[P0]** 安装 OpenJDK
- [ ] **[P1]** 配置 Player Settings（Android 平台）
  - Scripting Backend: IL2CPP
  - Target Architectures: ARM64
  - Graphics APIs: OpenGLES3 / Vulkan
  - Color Space: Linear
  - minSdk: ≥ 28
- [ ] **[P1]** 测试 Android 构建流程（Build APK）

### 2.2 Rokid AR SDK 集成
- [ ] **[P0]** 选择 SDK 方案：
  - 方案 A: UXR 2.0（官方推荐，但需 Unity 2022 LTS）
  - 方案 B: OpenXR + Rokid Plugin（兼容 Unity 6）
- [ ] **[P0]** 安装选定的 AR SDK
- [ ] **[P1]** 配置 XR Plugin Management
- [ ] **[P1]** 创建 AR Session 和 AR Session Origin
- [ ] **[P1]** 替换 Main Camera 为 AR Camera
- [ ] **[P2]** 配置相机背景（Camera Background + AR Camera Background）

### 2.3 手势与交互
- [ ] **[P1]** 启用手势识别（UXR 2.0: Pinch/Grip/Palm/OpenPinch）
- [ ] **[P1]** 配置射线交互系统（Raycast / Gaze）
- [ ] **[P2]** 实现远近场切换逻辑
- [ ] **[P2]** 添加 UI 交互元素（按钮、面板等）

### 2.4 EEG 数据接口（预留）
- [ ] **[P2]** 设计 EEG 数据接收架构
- [ ] **[P2]** 定义数据协议（JSON/Binary/UDP/TCP）
- [ ] **[P2]** 实现 EEG 数据解析模块
- [ ] **[P2]** 将 EEG 数据映射到宇航员动画状态

---

## ⚡ 第三阶段：性能优化与部署（待开始）

### 3.1 模型优化
- [ ] **[P0]** 宇航员模型减面：56K → 8-12K 三角形（Blender Decimate Modifier）
- [ ] **[P0]** 骨骼精简：90根 → 25-30根
- [ ] **[P1]** 生成 LOD 分级（LOD0/LOD1/LOD2）
- [ ] **[P1]** 月球网格优化（合并静态物体）
- [ ] **[P2]** 启用 GPU Instancing（重复物件如星星/粒子）

### 3.2 纹理优化
- [ ] **[P0]** 纹理降分辨率：8K/16K → 1024 或 2048
- [ ] **[P0]** 纹理压缩：PNG → ASTC 4×4 (Android) / ETC2
- [ ] **[P1]** 图集合并：30+ 张独立纹理 → 几张大图集（Atlas）
- [ ] **[P1]** Mipmap 配置优化
- [ ] **[P2]** 使用 Streaming Mip Maps（按需加载）

### 3.3 渲染优化
- [ ] **[P0]** 切换到 Mobile_RPAsset 渲染管线
- [ ] **[P0]** 关闭实时光影（使用烘焙光照）
- [ ] **[P1]** 启用 SRP Batching
- [ ] **[P1]** 启用 Occlusion Culling
- [ ] **[P1]** 减少后处理效果（仅保留必要项）
- [ ] **[P2]** Shader 精简（使用 Unlit/Lite 版本）
- [ ] **[P2]** 降低 Shadow Distance 和 Cascade Count

### 3.4 内存与加载优化
- [ ] **[P1]** 配置 Addressables / AssetBundle 异步加载
- [ ] **[P1]** 对象池复用（粒子等短生命周期对象）
- [ ] **[P2]** 场景分块加载
- [ ] **[P2]** 资源卸载策略

### 3.5 设备测试与调优
- [ ] **[P0]** 构建 APK/ABB 并部署到 Rokid Lite
- [ ] **[P0]** 运行 Unity Profiler（远程连接或 ADB）
- [ ] **[P1]** 监测帧率目标：60 FPS（最低 30 FPS）
- [ ] **[P1]** 监测内存占用：< 300 MB（运行时峰值）
- [ ] **[P1]** 监测启动/加载时间：< 5 秒
- [ ] **[P2]** 连续运行稳定性测试（10 分钟+ 无卡顿/过热降频）
- [ ] **[P2]** 根据数据迭代优化（减面/降质/关特效）

---

## 📦 第四阶段：版本控制与发布（待开始）

### 4.1 Git 提交规范
- [ ] **[P1]** 建立分支策略（main / develop / feature/* / hotfix/*）
- [ ] **[P1]** 规范 Commit Message 格式
- [ ] **[P2]** 配置 CI/CD（可选：GitHub Actions 自动构建）

### 4.2 发布准备
- [ ] **[P1]** 编写 README.md（项目介绍、安装步骤、使用说明）
- [ ] **[P1]** 编写 CHANGELOG.md（版本更新记录）
- [ ] **[P2]** 准备演示视频/截图
- [ ] **[P2]** 打包最终 APK/ABB 用于分发

---

## 🎯 性能目标汇总

| 指标 | 当前值 | 目标值 |
|------|--------|--------|
| 宇航员三角面数 | 56,200 | 8,000 - 15,000 |
| 单张纹理大小 | 最大 170 MB | ≤ 4 MB |
| 纹理分辨率 | 可能 8K/16K | 1024 - 2048 |
| 骨骼数量 | 90 根 | 25 - 30 根 |
| 运行时显存 | > 2 GB | < 300 MB |
| 总资源包体 | ~5 GB | 50 - 100 MB |
| 帧率 | < 10 FPS | 45 - 60 FPS |

---

## 📌 优先级说明

- **[P0]**: 必须完成，阻塞后续工作
- **[P1]**: 重要，影响核心功能或性能
- **[P2]**: 锦上添花，可延后处理

---

## 🔗 相关文档

- **AGENTS.md**: 项目完整指南（技术栈、架构、规范）
- **chatgpt建议.md**: ChatGPT 生成的优化方案参考（本地保留，不上传）

---

*此文件会随着项目进展持续更新*
