# Unity MCP 接入与验证

验证日期：2026-10-12。

## 当前配置

- 工程：`E:\ERE\Documents\Work\EEG_AI_AR\AR\New Unity Project`
- Unity：`2022.3.62f3c1`
- Unity 包及 Python 服务端：MCP for Unity `10.0.0`
- UPM 固定版本：`https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.0.0`
- 锁定提交：`d49ae2953580f3481beb1e084a1da2682f0b5610`
- MCP 地址：`http://127.0.0.1:8080/mcp`
- 实例：`New Unity Project@2a8f720c4543ba1f`
- uvx：`C:\Users\ERE.DESKTOP-JD48OPB\.local\bin\uvx.exe`

Codex 用户配置 `C:\Users\ERE.DESKTOP-JD48OPB\.codex\config.toml` 已加入：

```toml
[mcp_servers.unityMCP]
url = "http://127.0.0.1:8080/mcp"
```

Unity 的 MCP 设置已启用 HTTP Local 和 Auto-Start on Editor Load，服务监听本机回环地址。设置保存在本机 Unity EditorPrefs 中；其他电脑需要重新配置。打开工程后，插件负责后台启动服务、连接会话和脚本重编译后的重连。

## 如何使用

1. 保持这个 Unity 工程打开，等待编译和导入完成。
2. 本次接入前已打开的 Codex 聊天尚未加载新增工具。到 Codex 设置中的 MCP servers 对 `unityMCP` 执行 Restart；如果仍未出现，重启 Codex 后继续原聊天。
3. 可以直接请求：“用 unityMCP 读取当前场景层级”“检查 Unity Console 错误”“检查三个 RepairNode 的配置”。
4. 同时打开多个工程时，先用 `set_active_instance` 选择上述实例，再执行操作。

如果服务断开，在 Unity 的 `Window > MCP for Unity` 面板检查 HTTP Local 地址，启动 Server 和 Session。已启用的 Auto-Start 会在后续编辑器启动时尝试自动连接。

## 已完成验证

- `/health` 返回 healthy，应用版本为 `10.0.0`。
- `/api/instances` 返回目标工程及正确 Unity 版本。
- 正式 MCP 协议初始化、工具发现和资源读取成功。
- 独立 Codex app-server 实际发现 47 个工具、19 个资源，`toolsError=null`。
- `manage_scene` 读取 `Assets/Scenes/SampleScene.unity`，根对象数量为 13。
- 在临时附加场景中创建立方体、修改坐标、读回验证、删除对象并关闭临时场景，全部成功。
- 原场景验证前后均为 `isDirty=false`，根对象数量不变。
- `read_console` 返回 0 条错误。
- 一次性安装脚本已移除；强制资源刷新引发域重载后，MCP 自动重连，场景读取和代码执行再次通过，Console 仍为 0 条错误。

详细本机验证记录：`New Unity Project/Library/MCPForUnity/codex-verification-20261012.json`。
后台启动日志：`New Unity Project/Library/MCPForUnity/Logs/server-launch-8080.log`。
安装前备份位于 `%TEMP%\unity-mcp-setup-backup-20261012`。

只读快速检查（PowerShell）：

```powershell
codex mcp get unityMCP
Invoke-RestMethod http://127.0.0.1:8080/health
Invoke-RestMethod http://127.0.0.1:8080/api/instances
```

`health` 只证明服务存在；必须同时确认目标实例已连接且场景工具能返回数据。

## 参考

- [MCP for Unity 上游](https://github.com/CoplayDev/unity-mcp)
- [Codex 官方 MCP 配置说明](https://learn.chatgpt.com/docs/extend/mcp?surface=cli)
