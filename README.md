# 月球幸存者 / EEG_AI_AR

当前项目是 Unity 2022.3.62f3、URP 14.0.10 的 PC 月球探索 Demo。
Unity Hub 打开 **`New Unity Project/`**，主场景为 **`Assets/Scenes/SampleScene.unity`**。

- [团队启动与协作指南](Docs/TEAM_GUIDE.md)：克隆、环境、操作、验收、分支与 PR。
- [资源路径及完整资源清单](Docs/RESOURCE_PATHS.md)：模型、音频、脚本入口和外部资源放置路径。
- [机器可读资源清单](Docs/external-assets.json) 与 [资源检查工具](Tools/Verify-Resources.ps1)。
- [历史 README](Docs/LegacyREADME.md)：仅供背景参考，其中旧路径、玩法和版本状态不代表当前交付。

玩法：修理控制台 → 开启雷达 → 搜集三个部件 → 火箭升空；补氧与 5×5 全覆盖连线小游戏提供辅助。

**资源分开分发**：Git 包含源码、场景、配置和 `.meta`，大型模型/贴图/音频需从团队资源包恢复。目前未提供共享盘链接，请联系资源维护者。
交付分支为 `team/project-handoff-20261020`，不会自动合并 main。本地 MCP 依赖和服务密钥不包含在交付中，配置方式见团队指南。
