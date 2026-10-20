# 月球幸存者：情绪信标 - Demo 设置指南

## 审核状态

当前脚本是“情绪切换 + 记忆碎片收集”的原型版本，尚未达到项目文档中定义的 **《月球幸存者：飞船修复协议》** 最小验收标准。

仍需补齐：

- `attention` 集中度输入与显示
- `meditation` 冥想度输入与显示
- 氧气系统
- 飞船修理进度系统
- 靠近节点后按 `E` 修理，而不是自动收集
- 集中度影响修理速度
- 冥想度影响氧气消耗速度
- 情绪影响角色移动速度

## 🚀 快速开始

### 1. 创建 Demo 管理器对象

在场景中创建一个空 GameObject，命名为 `DemoManager`，添加以下组件：

- `DemoManager`
- `DemoEmotionSimulator`
- `DemoGameState`
- `EmotionGameAdapter`

### 2. 创建 HUD 对象

创建一个 Canvas，命名为 `DemoHUD`，添加以下 UI 元素：

| 元素 | 类型 | 名称 | 作用 |
|------|------|------|------|
| EmotionText | Text | emotionText | 显示当前情绪 |
| ShardCount | Text | shardCountText | 显示碎片收集进度 |
| ProgressBar | Slider | progressSlider | 进度条 |
| EmotionIndicator | Image | emotionIndicator | 情绪颜色指示器 |
| VictoryPanel | Panel | victoryPanel | 胜利面板 |
| VictoryText | Text | victoryText | 胜利文字 |
| RestartButton | Button | restartButton | 重新开始按钮 |

然后给 Canvas 添加 `DemoHUD` 组件，并将上述元素拖拽到对应的 Inspector 字段中。

### 3. 创建记忆碎片

在场景中创建 3 个 GameObject，命名为 `MemoryShard_01`、`MemoryShard_02`、`MemoryShard_03`，放置在不同位置。

每个碎片添加以下组件：
- `MemoryShard`
- (可选) `ParticleSystem` - 作为 idleParticles 和 collectParticles

### 4. 配置 EmotionGameAdapter

在 `DemoManager` 的 `EmotionGameAdapter` 组件中配置：

| 字段 | 推荐配置 |
|------|---------|
| Happy Light Color | 黄色 (1, 0.8, 0.2) |
| Sad Light Color | 蓝色 (0.3, 0.4, 0.8) |
| Normal Light Color | 绿色 (0.7, 0.9, 0.5) |
| Happy Music | Look up to the Stars.mp3 |
| Sad Music | Cry for the moon.mp3 |
| Normal Music | Weightless.mp3 |

### 5. 运行游戏

按 Play 按钮运行：

- **按键 [1]** - 切换到 Happy（快乐）状态
- **按键 [2]** - 切换到 Sad（悲伤）状态
- **按键 [3]** - 切换到 Normal（平静）状态

走近记忆碎片即可收集，收集 3 个后显示胜利面板。

## 🎮 游戏流程

```
开始游戏 → 月球场景 → 切换情绪(1/2/3) → 场景变化(灯光/音乐/天空盒)
    ↓
收集记忆碎片(3个) → 显示胜利面板 → 点击重新开始
```

## 📁 新增文件

```
Assets/Scripts/Demo/
├── DemoEmotionSimulator.cs    # 情绪模拟器（键盘切换）
├── DemoGameState.cs           # 游戏状态管理
├── EmotionGameAdapter.cs      # 情绪-游戏适配器
├── MemoryShard.cs             # 记忆碎片脚本
├── DemoHUD.cs                 # UI界面和胜利面板
├── DemoManager.cs             # Demo总管理器
└── DEMO_SETUP_GUIDE.md        # 本指南
```

## 🛠️ 修复的问题

1. **SceneManager 命名冲突**
   - 文件：`Assets/Scripts/Core/SceneManager.cs`
   - 问题：类名与 `UnityEngine.SceneManagement.SceneManager` 冲突
   - 修复：重命名为 `MoonSceneManager`

2. **SceneInputController 引用更新**
   - 文件：`Assets/Scripts/Core/SceneInputController.cs`
   - 修复：将 `SceneManager` 引用改为 `MoonSceneManager`

## ✅ 最小测试清单

- [ ] 无红色编译错误
- [ ] Play 可运行
- [ ] 按键 1/2/3 可切换情绪
- [ ] 灯光/音乐随情绪变化
- [ ] 3 个记忆碎片可收集
- [ ] 收集完成后胜利面板出现
- [ ] 重新开始按钮可用

## 🎨 情绪效果

| 情绪 | 灯光颜色 | 灯光强度 | 音乐 | 天空盒 |
|------|---------|---------|------|--------|
| Happy | 金色 | 1.3x | Look up to the Stars | Pink |
| Sad | 蓝色 | 0.5x | Cry for the moon | Blue |
| Normal | 绿色 | 1.0x | Weightless | Purple |

## ⚠️ 注意事项

1. 确保场景中有一个 Directional Light
2. 确保音频文件已导入并设置为 AudioClip
3. 如果不需要 EmotionEffectManager，可以在 Inspector 中留空
4. 记忆碎片的收集距离默认为 3 米
