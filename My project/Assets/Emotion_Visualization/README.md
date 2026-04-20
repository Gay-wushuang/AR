# 情绪可视化面板

## 📋 项目概述

Unity 项目，使用 RawImage 实时绘制情绪变化波形图。

---

## 🎯 功能特性

### ✅ 已实现功能

1. **情绪波形可视化**
   - 开心（黄色）
   - 平静（绿色）
   - 悲伤（蓝色）

2. **可折叠面板**
   - 展开状态：显示完整波形图
   - 折叠状态：仅显示小按钮

3. **平滑过渡**
   - 情绪切换有平滑动画
   - 波形曲线实时更新

---

## 📁 文件结构

```
Emotion_Visualization/
├── MoodWaveGenerator.cs    # 情绪波形生成器
├── MoodPanelController.cs  # 面板控制器
└── ui.unity              # UI场景
```

---

## 🚀 使用方法

### 1️⃣ 打开场景

在 Unity 中打开：
```
Assets/Emotion_Visualization/ui.unity
```

### 2️⃣ 面板设置

**MoodWaveGenerator 组件：

| 参数 | 说明 |
|------|------|
| RawImage | 用于显示波形的 RawImage |
| 三种颜色 | 分别设置开心、平静、悲伤的颜色 |
| 线条粗细 | 曲线宽度 |
| 平滑速度 | 波形平滑度 |

**MoodPanelController 组件：

| 参数 | 说明 |
|------|------|
| moodWindow | 面板 RectTransform |
| foldButton | 折叠按钮 |

---

## 🔌 API 接口

### 设置情绪

```csharp
// 直接设置
moodWaveGenerator.SetEmotion(mood);
// mood: 0=悲伤, 1=平静, 2=开心
```

```csharp
// 平滑过渡
moodWaveGenerator.SetEmotionSmooth(mood);
```

---

## 🎨 情绪效果

| 情绪 | 数值 | 颜色 | 波形特点 |
|------|------|------|----------|
| 开心 | 2 | 黄色 | 高振幅、高频 |
| 平静 | 1 | 绿色 | 中振幅、中频 |
| 悲伤 | 0 | 蓝色 | 低振幅、低频 |

---

## 📝 技术说明

使用 Texture2D 逐像素绘制，性能优化的实时波形。
