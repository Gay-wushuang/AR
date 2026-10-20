# 团队协作与启动指南

更新：2026-10-20。本次交付分支：`team/project-handoff-20261020`，仓库：https://github.com/Gay-wushuang/AR

## 第一次启动

1. 安装与 `ProjectVersion.txt` 一致的 Unity **2022.3.62f3**；URP **14.0.10**、Input System **1.7.0**，包版本由 manifest 和 lock 文件控制。
2. 获取工程：

```powershell
git clone --branch team/project-handoff-20261020 https://github.com/Gay-wushuang/AR.git
cd AR
git lfs install
git lfs pull
```

3. 根据 [资源路径](RESOURCE_PATHS.md) 获取独立资源包并合并到仓库根目录。Git LFS 只处理已有被跟踪的文件，不能替代本项目独立资源包。
4. 在仓库根目录执行 `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Verify-Resources.ps1`，确认没有缺失、大小不符的文件。
5. Unity Hub 添加 `New Unity Project/`，等待导入完成，打开 `Assets/Scenes/SampleScene.unity`。含 `.blend` 的源模型需要本机 Blender 导入环境；缺失时按 Console 提示安装，或由资产维护者提供统一导出的模型，保留 GUID 引用。
6. 检查 Console 没有编译错误，再 Play。当前验收目标是 PC Demo；本次未验证 Android/Rokid 构建。

## 可选服务

- EEG：服务来自独立的实时推理环境，仓库不包含完整训练/推理服务。默认 WebSocket 可见于项目配置；无设备时使用项目的 Demo 模拟方案。EEG 断线 1006 是已知环境问题，不能作为脚本编译失败处理。
- AI / TTS：交付版不带服务密钥。启动 Unity 前在本机环境设置 `DASHSCOPE_API_KEY`，重启 Unity Hub 和 Editor 使其生效。不要把密钥填入将要提交的场景、预制体或源码。通过运行时 SetApiKey 传入也可以，但不要保存为资产。
- Unity MCP：仅为开发辅助工具，交付版移除了本机 `file:D:/...` 依赖。需要时自行安装相容的 MCP 包；本机曾使用 10.3.1-beta.7。不要提交个人磁盘路径。正常游玩不需要 MCP。

## 游戏操作和验收

WASD / 方向键移动，鼠标转向，Space 跳跃，E 互动。修复工具箱对应的控制台后，在控制台启动雷达；按住 Tab 点击目标，一次追踪一个，点击 − / + 折叠雷达。
先修复控制台，再收集 3 个散落部件，最后靠近火箭按 E 发射。
任务道具位于初始玩家位置的 XZ 平面 25–80 单位环带；外圈补氧点随机生成，内圈保留固定补氧点。
电源 E 打开 5×5 连线关卡；三个颜色必须全连接、无交叉并覆盖 25 格。未铺满会清空当前棋盘，不发奖励；中途退出清空。
成功后 Shift 切换 1.7 倍冲刺。小游戏期间停止耗氧，游戏新一轮重置奖励。

合并前至少验证：

- 主场景可以运行，核心对象引用没有丢失，Console 无新增编译/运行错误。
- 新一轮随机布局、补氧、工具箱修复和单目标雷达正常。
- 小游戏未铺满不得奖励、六关均有完整覆盖解、重开清空奖励。
- 空中松键仍有惯性；第一人称 E 不触发挥手；火箭不旋转、喷流朝下、发射进入胜利状态。
- SceneManager → EnhanceMoonScene → 岩石吸附月面网格，在 Edit Mode 执行并保存，检查没有悬空或严重穿模。

## 日常协作

从本次交付分支建立个人功能分支，先拉取再修改。main 是稳定入口，本次没有直接更新 main。历史 AGENTS 中的 develop 策略是约定，不代表远程已经存在 develop；团队确认集成分支后统一调整。

```powershell
git switch team/project-handoff-20261020
git pull --ff-only
git switch -c feature/your-change
# 修改并完成 Unity 验证
git status --short
git add <本次明确修改的文件及对应.meta>
git diff --cached --stat
git commit -m "feat: describe the change"
git push -u origin feature/your-change
```

在 GitHub 创建 PR，先以 `team/project-handoff-20261020` 为目标；团队审核整批交付后再决定合并到 main。PR 说明问题、修改后行为、验证结果、资源包变化。
不要使用 force push 覆盖他人提交，不要盲目执行 git add .。

场景和预制体尽量同一时间由一人维护，先在群里约定；逻辑优先分散在脚本/预制体中。发生 `.unity`/`.prefab` 冲突时保留双方副本，使用 UnityYAMLMerge 或在 Editor 重新整合，不要直接接受全部一方。
Assets 内移动/重命名用 Unity Editor 操作并连同 `.meta` 提交，不能重新生成 GUID。保留 `Assets`、`Packages`、`ProjectSettings`，不提交 `Library`、`Temp`、`Logs`、`UserSettings` 和构建产物。

新增模型/贴图/音频时，由资源维护者更新独立资源包、`external-assets.json` 和 RESOURCE_PATHS.md；保持来源/授权记录。本次资源清单未附公共下载链接，资源包未到位时不能保证克隆后即可游玩。

## 本次验证边界

本机源项目已验证六关解、未铺满拒绝奖励、雷达折叠、179 块岩石接地和火箭发射；交付快照额外移除了密钥、本机 MCP 依赖及外部二进制资源。
本次没有在全新机器上恢复外部资源并重新构建。首次接入者应按上述清单完成导入与 Play Mode 验证。
