🎯 Unity 聊天系统生成提示词（适用于 Trae CN）
🧩 一、项目目标

请帮我使用 Unity（C# + UGUI + TextMeshPro）生成一个完整的聊天系统 UI + 逻辑脚本。

该系统将用于后续 EEG 情绪识别结果的可视化反馈。

🏗 二、UI结构（必须严格按照此结构生成）

Canvas（Screen Space - Overlay）
└── ChatPanel（全屏或居中面板）
├── Scroll View（聊天区域）
│ ├── Viewport（带 Rect Mask 2D）
│ │ └── Content（聊天内容容器）
│ │ - Vertical Layout Group
│ │ - Content Size Fitter（Vertical = Preferred Size）
│ │
│ └── Scrollbar Vertical
│
├── InputArea（底部输入区）
│ ├── TMP\_InputField
│ └── SendButton（Button + Text）
│
└── MessagePrefab（不要放在Hierarchy，需说明结构）

📐 三、UI尺寸与布局（重要）
ChatPanel
锚点：Stretch（全屏）
Padding：左右 40，上 40，下 120
Scroll View（聊天区）
高度占比：80%
锚点：Top Stretch
Margin：10
InputArea（输入区）
高度：100
锚点：Bottom Stretch
背景颜色：浅灰
Message气泡
最大宽度：屏幕宽度的 60%
内边距：10\~16
圆角：20
🎨 四、配色方案（现代UI风格）
背景
ChatPanel：#1E1E2F（深蓝灰）
用户消息（右侧）
背景：#4CAF50（绿色）
文字：白色
系统消息（左侧）
背景：#2D8CFF（蓝色）
文字：白色
输入框
背景：#2A2A3D
文字：白色
按钮
背景：#4CAF50
Hover：稍亮
