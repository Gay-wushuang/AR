# 🎮 月球幸存者 - Unity UI 操作全指南

> **适用版本**: Unity 6 (6000.4.0f1)
> **难度**: ⭐⭐（简单）
> **预计时间**: 30-45 分钟

---

## 📋 第一步：准备 SceneManager 对象（5 分钟）

### 1.1 找到 SceneManager

1. 打开 Unity 编辑器
2. 在 **Hierarchy** 窗口中，找到你的 `SceneManager` 对象
   - 如果没有，创建一个：右键 Hierarchy → Create Empty → 命名为 `SceneManager`

### 1.2 添加所有核心脚本（重要！）

在 **Inspector** 窗口中，点击 **"Add Component"** 按钮，**按顺序**添加以下 6 个脚本：

| 脚本名称 | 路径 | 用途 |
|---------|------|------|
| 1️⃣ **GameManager** | `Assets/Scripts/Game/GameManager.cs` | 游戏主管理器 |
| 2️⃣ **QwenService** | `Assets/Scripts/AI/LLM/QwenService.cs` | 通义千问 LLM 服务 |
| 3️⃣ **EmotionAnalyzer** | `Assets/Scripts/AI/Emotion/EmotionAnalyzer.cs` | 情绪分析引擎 |
| 4️⃣ **DialogueManager** | `Assets/Scripts/AI/Dialogue/DialogueManager.cs` | 对话管理系统 |
| 5️⃣ **MemorySystem** | `Assets/Scripts/Game/Memory/MemorySystem.cs` | 记忆系统 |
| 6️⃣ **TTSService** | `Assets/Scripts/Audio/TTS/TTSService.cs` | 语音合成服务 |

**添加方法**：
- 点击 Add Component → 在搜索框输入脚本名称 → 选择脚本
- 或者直接从 Project 窗口拖拽脚本到 SceneManager 对象上

---

## 📋 第二步：创建 Canvas 和 ChatPanel（10 分钟）

### 2.1 创建 Canvas

1. **右键 Hierarchy** → UI → Canvas
2. 在 Inspector 中设置：
   - **Render Mode**: `Screen Space - Overlay`
   - **UI Scale Mode**: `Scale With Screen Size`
   - **Reference Resolution**: `1920 x 1080`

### 2.2 创建 ChatPanel

1. **右键 Canvas** → Create Empty → 命名为 `ChatPanel`
2. 在 Inspector 中设置 **Rect Transform**：
   - 点击锚点预设图标（左上角正方形）
   - 按住 **Alt** 键，点击右下角的 **Stretch** 图标（四个箭头向外）
   - **Left**: 40
   - **Right**: 40
   - **Top**: 40
   - **Bottom**: 120
3. 添加 **Canvas Group** 组件（可选，用于淡入淡出）
4. 添加 **Image** 组件，设置背景色：
   - **Color**: `#1E1E2F`（深蓝灰）
   - **Alpha**: 255

---

## 📋 第三步：创建 Scroll View（聊天区域）（10 分钟）

### 3.1 创建 Scroll View

1. **右键 ChatPanel** → UI → Scroll View
2. 重命名为 `Scroll View`
3. 在 Inspector 中设置 **Rect Transform**：
   - 锚点：Top + Stretch（按住 Alt，点击上排中间图标）
   - **Pos Y**: 0
   - **Height**: 占 ChatPanel 高度的 80%（例如：如果 ChatPanel 高 900，就设为 720）
   - **Left**: 10
   - **Right**: 10

### 3.2 配置 Content

1. 在 Hierarchy 中展开 `Scroll View` → 找到 `Content`
2. 选中 `Content`，在 Inspector 中：
   - 添加 **Vertical Layout Group** 组件
     - **Child Force Expand**: Width ☑️, Height ☐
     - **Spacing**: 10
     - **Child Alignment**: Upper Center
   - 添加 **Content Size Fitter** 组件
     - **Vertical Fit**: `Preferred Size`

### 3.3 配置 Viewport

1. 选中 `Viewport`（在 Scroll View 下）
2. 确保已添加 **Rect Mask 2D** 组件（默认应该有）

---

## 📋 第四步：创建 InputArea（输入区域）（5 分钟）

### 4.1 创建 InputArea

1. **右键 ChatPanel** → Create Empty → 命名为 `InputArea`
2. 在 Inspector 中设置 **Rect Transform**：
   - 锚点：Bottom + Stretch（按住 Alt，点击下排中间图标）
   - **Pos Y**: 0
   - **Height**: 100
   - **Left**: 10
   - **Right**: 10
3. 添加 **Image** 组件，设置背景色：
   - **Color**: `#2A2A3D`（深灰）

### 4.2 添加 TMP_InputField

1. **右键 InputArea** → UI → Text - TextMeshPro → Input Field (TMP)
2. 重命名为 `InputField`
3. 在 Inspector 中设置 **Rect Transform**：
   - 锚点：Left + Stretch（按住 Alt，点击左排中间图标）
   - **Pos X**: 10
   - **Width**: 占 InputArea 宽度的 80%
4. 设置 **TextMeshPro - Input Field**：
   - **Placeholder**: 输入消息...
   - **Text Color**: White
   - **Font Size**: 24

### 4.3 添加 SendButton

1. **右键 InputArea** → UI → Button - TextMeshPro
2. 重命名为 `SendButton`
3. 在 Inspector 中设置 **Rect Transform**：
   - 锚点：Right + Stretch（按住 Alt，点击右排中间图标）
   - **Pos X**: -10
   - **Width**: 100
4. 设置 **Button**：
   - **Normal Color**: `#4CAF50`（绿色）
   - **Highlighted Color**: 稍亮一点的绿色
5. 设置 **Text (TMP)**（在 Button 下）：
   - **Text**: 发送
   - **Font Size**: 20
   - **Color**: White

---

## 📋 第五步：创建消息预制体（10 分钟）

### 5.1 创建消息气泡预制体基础结构

我们需要创建 3 个预制体：用户消息、AI 消息、系统消息。

#### 通用结构（所有消息都需要）：

1. **在 Project 窗口的 Assets 文件夹中**，创建文件夹：`Prefabs/UI/Chat`
2. **右键 Hierarchy** → Create Empty → 命名为 `MessageBubble`
3. 添加 **Rect Transform**：
   - **Width**: 设为 600（屏幕宽度的 60% 左右）
   - **Height**: 设为 50（会根据内容自动调整）
4. 添加 **Image** 组件：
   - **Color**: 暂时设为白色
5. 添加 **Vertical Layout Group**：
   - **Padding**: Left=16, Right=16, Top=12, Bottom=12
   - **Child Force Expand**: Width ☑️, Height ☐
6. 添加 **Content Size Fitter**：
   - **Vertical Fit**: `Preferred Size`
7. **右键 MessageBubble** → UI → Text - TextMeshPro
8. 重命名 Text 为 `MessageText`
9. 选中 `MessageText`，设置：
   - **Text**: 测试消息
   - **Font Size**: 18
   - **Color**: White
   - **Alignment**: Left
   - **Wrap Mode**: Word Wrap
10. 添加 **ChatMessageBubble** 脚本（从 Project 窗口拖拽）

---

#### 5.2 创建 UserMessagePrefab（用户消息）

1. 复制上面的 `MessageBubble`，重命名为 `UserMessageBubble`
2. 选中它，在 Inspector 中：
   - **Image** 的 **Color**: `#4CAF50`（绿色）
   - 在 **ChatMessageBubble** 脚本中，确认颜色设置正确
3. 拖拽到 `Assets/Prefabs/UI/Chat` 文件夹，成为预制体
4. 删除 Hierarchy 中的 `UserMessageBubble`

---

#### 5.3 创建 AIMessagePrefab（AI 消息）

1. 复制 `MessageBubble`，重命名为 `AIMessageBubble`
2. 选中它，在 Inspector 中：
   - **Image** 的 **Color**: `#2D8CFF`（蓝色）
3. 拖拽到 `Assets/Prefabs/UI/Chat` 文件夹，成为预制体
4. 删除 Hierarchy 中的 `AIMessageBubble`

---

#### 5.4 创建 SystemMessagePrefab（系统消息）

1. 复制 `MessageBubble`，重命名为 `SystemMessageBubble`
2. 选中它，在 Inspector 中：
   - **Image** 的 **Color**: `#999999`（灰色）
3. 拖拽到 `Assets/Prefabs/UI/Chat` 文件夹，成为预制体
4. 删除 Hierarchy 中的 `MessageBubble` 和 `SystemMessageBubble`

---

## 📋 第六步：配置 EnhancedChatUI（5 分钟）

### 6.1 添加脚本

1. 选中 **ChatPanel** 对象
2. 点击 **Add Component**
3. 搜索并添加 **EnhancedChatUI** 脚本

### 6.2 分配引用（重要！）

在 **EnhancedChatUI** 组件的 Inspector 中，逐个分配：

| 字段 | 拖入对象 |
|------|---------|
| **Message Content** | `Scroll View` → `Content` |
| **Scroll Rect** | `Scroll View` |
| **Input Field** | `InputArea` → `InputField` |
| **Send Button** | `InputArea` → `SendButton` |
| **User Message Prefab** | `Assets/Prefabs/UI/Chat/UserMessagePrefab` |
| **AI Message Prefab** | `Assets/Prefabs/UI/Chat/AIMessagePrefab` |
| **System Message Prefab** | `Assets/Prefabs/UI/Chat/SystemMessagePrefab` |
| **Dialogue Manager** | Hierarchy 中的 `SceneManager` |
| **Emotion Analyzer** | Hierarchy 中的 `SceneManager` |
| **TTS Service** | Hierarchy 中的 `SceneManager` |

---

## 📋 第七步：连接按钮事件（2 分钟）

### 7.1 连接 SendButton

1. 选中 **SendButton**
2. 在 Inspector 中找到 **Button** 组件的 **On Click ()** 事件
3. 点击 **+** 号添加事件
4. 将 **ChatPanel** 对象拖入对象槽
5. 在函数下拉菜单中选择：
   - **EnhancedChatUI** → **SendMessage ()**

---

## 📋 第八步：（可选）创建 MemoryPanel（10 分钟）

### 8.1 创建 MemoryPanel

1. **右键 Canvas** → Create Empty → 命名为 `MemoryPanel`
2. 设置 **Rect Transform**：
   - 锚点：Stretch（Alt + 右下角）
   - **Left**: 20
   - **Right**: 20
   - **Top**: 20
   - **Bottom**: 20
3. 添加 **Image** 组件，设为半透明背景

### 8.2 添加 MemoryUI 脚本

1. 选中 **MemoryPanel**
2. 添加 **MemoryUI** 脚本
3. 按照脚本中的要求创建 UI 元素并分配引用

---

## 📋 第九步：测试运行！（5 分钟）

### 9.1 保存场景

1. **File** → **Save**（或 Ctrl+S）
2. 确保保存的是你的场景

### 9.2 点击 Play！

1. 点击 Unity 顶部的 **Play** 按钮 ▶️
2. 你应该会看到：
   - ChatPanel 显示
   - Lunar 的欢迎消息自动出现
3. 在输入框输入消息，例如：
   - "你好"
   - "我在哪里？"
4. 点击发送或按回车
5. 观察 AI 的回复！

---

## 🎯 常见问题排查

### Q1: 脚本找不到？
**A**: 确保所有脚本都在正确的文件夹中，并且 Unity 已经编译完成（右下角没有转圈的加载图标）。

### Q2: 引用丢失？
**A**: 仔细检查每个引用字段，确保拖入的对象类型正确。

### Q3: 点击发送没反应？
**A**: 检查 SendButton 的 On Click 事件是否正确连接到 EnhancedChatUI.SendMessage()。

### Q4: API 调用失败？
**A**: 
- 确保网络连接正常
- 检查 QwenService 中的 API Key 是否正确
- 查看 Console 窗口的错误信息

### Q5: TextMeshPro 缺失？
**A**: 
- 如果提示 TMP 缺失，点击 **Window** → **TextMeshPro** → **Import TMP Essential Resources**
- 导入后重新打开场景

---

## ✅ 完成检查清单

在开始测试前，请确认：

- [ ] SceneManager 上添加了 6 个核心脚本
- [ ] Canvas 已创建并配置
- [ ] ChatPanel 已创建并配置
- [ ] Scroll View 和 Content 已配置
- [ ] InputArea、InputField、SendButton 已创建
- [ ] 3 个消息预制体已创建
- [ ] EnhancedChatUI 脚本已添加
- [ ] 所有引用都已分配
- [ ] SendButton 事件已连接
- [ ] 场景已保存

---

## 🎉 恭喜！

如果你完成了以上所有步骤，那么你已经成功搭建了完整的 **ARS+LLM+TTS 集成聊天系统**！

现在点击 Play，开始和 Lunar 聊天吧！🚀

---

*如有问题，查看 Console 窗口的错误信息，或参考快速开始文档！*
