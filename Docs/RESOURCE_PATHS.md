# 项目资源路径与入口

更新：2026-10-20。所有路径相对仓库根目录；Unity Project 窗口里的 `Assets/...` 相对 `New Unity Project/`。

当前唯一交付入口是 `New Unity Project/`，不是历史文档中的 `My_2022/New Unity Project/`。Unity Hub 应选择含 Assets、Packages、ProjectSettings 的文件夹。

## 场景与脚本

| 资源 / 功能 | 路径 |
|---|---|
| 主场景 | `New Unity Project/Assets/Scenes/SampleScene.unity` |
| 月面生成 | `New Unity Project/Assets/Scripts/Core/SetupMoonSurface.cs` |
| SceneManager 岩石吸附 | `New Unity Project/Assets/Scripts/Core/EnhanceMoonScene.cs` |
| 移动、跳跃、空中惯性 | `New Unity Project/Assets/Scripts/Characters/AstronautController.cs` |
| 游戏状态、开始与重置 | `New Unity Project/Assets/Scripts/Demo/DemoGameState.cs` |
| 任务道具和氧气罐随机布置 | `New Unity Project/Assets/Scripts/Demo/RandomExpeditionLayout.cs` |
| 补氧互动 | `New Unity Project/Assets/Scripts/Demo/OxygenCanister.cs` |
| 修理、雷达、氧气警报 | `New Unity Project/Assets/Scripts/Demo/ConsoleRadarSystem.cs` |
| 任务道具互动 | `New Unity Project/Assets/Scripts/Demo/RepairNode.cs` |
| 追踪发光 | `New Unity Project/Assets/Scripts/Demo/RadarTargetGlow.cs` |
| 5×5 连线小游戏、冲刺奖励 | `New Unity Project/Assets/Scripts/Demo/EnergyLinkPuzzle.cs` |
| 火箭升空及向下喷流 | `New Unity Project/Assets/Scripts/Demo/RocketLaunchController.cs` |
| Fuel Cell 运行时材质保护 | `New Unity Project/Assets/Scripts/Demo/FuelCellRuntimeMaterialFixer.cs` |
| Fuel Cell 导入材质 | `New Unity Project/Assets/Scripts/Editor/FuelCellMaterialPostprocessor.cs` |
| 渲染配置 / Shader | `New Unity Project/Assets/Settings/` / `New Unity Project/Assets/Shaders/` |
| 包版本 / 工程版本 | `New Unity Project/Packages/` / `New Unity Project/ProjectSettings/ProjectVersion.txt` |

SceneManager 上保留已有场景工具；DemoManager 承载玩法组件。不要仅通过对象名称判断资源可以删除，场景和脚本都可能按名称查找对象。

## 模型路由

| 场景对象 / 用途 | Assets/Models 下目录 |
|---|---|
| 玩家 | `astronaut/` |
| 月面及小岩石 | `moon/`；新增小石头使用 rock 系列，不使用 large rock 系列 |
| 齿轮任务道具 SM_Gear | `gear-cog/` |
| 燃料罐任务道具 | `old_metal_jarry_can._fuel_canister/` |
| Fuel Cell1 | `sci-fi-fuel-cell-canister/`，材质在其 `Materials/` |
| Rocket 新外观 | `载人火箭/`；旧 `rocket/` 仍有保留引用，不要直接删除 |
| 控制台 | `控制台3d模型/` |
| 工具箱 | `科幻工具箱3d模型/` |
| 小游戏电源 | `能量电源3d模型/` |
| 氧气罐 | `氧气罐3d模型/` |
| 场景设备 | `货运舱3d模型/`、`太空舱3d模型/`、`卫星天线3d模型/` |

## 二进制资源交付

代码、场景、材质和 `.meta` 由 Git 分发；模型、贴图、音视频沿用原项目的独立资源包方式。
**共享盘下载链接尚未在仓库中提供**，需由项目负责人提供。不要把本机 `Resources/` 当作 Unity 的 Assets/Resources 目录；它是素材中转区。

资源包应保留 `New Unity Project/Assets/...` 相对路径，从仓库根目录解压合并；不要覆盖仓库中的 `.meta`。推荐关闭 Unity 后复制资源，完成后再首次导入。
本机中转区中的 `astronaut.zip`、`moon.zip`、`Audio.zip`、`SpaceSkies Free.zip` 可能与当前场景使用版本不同；应以当前 Assets 中的资源及下面清单为准。

| 目标目录 | 文件数 | 总大小 MiB |
|---|---:|---:|
| `New Unity Project/Assets/Audio/` | 6 | 22.1 |
| `New Unity Project/Assets/Models/ROKE.fbx/` | 1 | 40.2 |
| `New Unity Project/Assets/Models/astronaut/` | 11 | 67.3 |
| `New Unity Project/Assets/Models/gear-cog/` | 3 | 0.1 |
| `New Unity Project/Assets/Models/moon/` | 68 | 5006.5 |
| `New Unity Project/Assets/Models/old_metal_jarry_can._fuel_canister/` | 1 | 46.5 |
| `New Unity Project/Assets/Models/rocket/` | 1 | 53.0 |
| `New Unity Project/Assets/Models/sci-fi-fuel-cell-canister/` | 1 | 1.4 |
| `New Unity Project/Assets/Models/卫星天线3d模型/` | 2 | 13.7 |
| `New Unity Project/Assets/Models/太空舱3d模型/` | 2 | 11.3 |
| `New Unity Project/Assets/Models/控制台3d模型/` | 2 | 13.7 |
| `New Unity Project/Assets/Models/氧气罐3d模型/` | 2 | 10.8 |
| `New Unity Project/Assets/Models/科幻工具箱3d模型/` | 1 | 1.1 |
| `New Unity Project/Assets/Models/能量电源3d模型/` | 2 | 11.8 |
| `New Unity Project/Assets/Models/货运舱3d模型/` | 2 | 14.5 |
| `New Unity Project/Assets/Models/载人火箭/` | 5 | 16.9 |
| `New Unity Project/Assets/Scenes/` | 2 | 0.7 |
| `New Unity Project/Assets/SpaceSkies Free/` | 54 | 482.1 |
| `New Unity Project/Assets/TextMesh Pro/` | 1 | 0.1 |
| `New Unity Project/Assets/TutorialInfo/` | 1 | 0.0 |

## 完整外部资源路径清单

可用 `Tools/Verify-Resources.ps1` 检查缺失和大小差异；清单记录大小，不是内容哈希校验。

| 相对路径 | 字节数 |
|---|---:|
| `New Unity Project/Assets/Audio/Cry for the moon.mp3` | 4316927 |
| `New Unity Project/Assets/Audio/Look up to the Stars.mp3` | 6998922 |
| `New Unity Project/Assets/Audio/Weightless.mp3` | 11634387 |
| `New Unity Project/Assets/Audio/here39s-another-good-example.mp3` | 103279 |
| `New Unity Project/Assets/Audio/科幻电流声_电流声_兹拉声_科幻_未来_能量_能量通道_爱给网_aigei_com.mp3` | 77366 |
| `New Unity Project/Assets/Audio/警报声-嘟嘟嘟-游戏提示音_爱给网_aigei_com.mp3` | 17285 |
| `New Unity Project/Assets/Models/ROKE.fbx` | 42116268 |
| `New Unity Project/Assets/Models/astronaut/source/Astronaut.glb` | 46619328 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_0.png` | 4361734 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_1@channels=A.png` | 410100 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_1@channels=RGB.png` | 414743 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_2.png` | 1241284 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_3.png` | 4169357 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_4.png` | 5083399 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_5@channels=A.png` | 403508 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_5@channels=RGB.png` | 793131 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_6.png` | 2751530 |
| `New Unity Project/Assets/Models/astronaut/textures/gltf_embedded_7.png` | 4328056 |
| `New Unity Project/Assets/Models/gear-cog/source/SM_Gear.fbx` | 87760 |
| `New Unity Project/Assets/Models/gear-cog/textures/Gear_Diff.png` | 19351 |
| `New Unity Project/Assets/Models/gear-cog/textures/Gear_Spec.png` | 22803 |
| `New Unity Project/Assets/Models/moon/large rock-1.fbx` | 320220 |
| `New Unity Project/Assets/Models/moon/large rock-2.fbx` | 120220 |
| `New Unity Project/Assets/Models/moon/large rock-3.fbx` | 188140 |
| `New Unity Project/Assets/Models/moon/large rock-4.fbx` | 248972 |
| `New Unity Project/Assets/Models/moon/large rock-5.fbx` | 315772 |
| `New Unity Project/Assets/Models/moon/ph_moon.blend` | 1867577405 |
| `New Unity Project/Assets/Models/moon/ph_moon.blend1` | 1868699801 |
| `New Unity Project/Assets/Models/moon/rock-1.fbx` | 59388 |
| `New Unity Project/Assets/Models/moon/rock-2.fbx` | 34028 |
| `New Unity Project/Assets/Models/moon/rock-3.fbx` | 42652 |
| `New Unity Project/Assets/Models/moon/rock-4.fbx` | 50220 |
| `New Unity Project/Assets/Models/moon/rock-5.fbx` | 58684 |
| `New Unity Project/Assets/Models/moon/rock-6.fbx` | 48140 |
| `New Unity Project/Assets/Models/moon/rock-7.fbx` | 47788 |
| `New Unity Project/Assets/Models/moon/textures/Marker_07.png` | 24825 |
| `New Unity Project/Assets/Models/moon/textures/VisorScratch.png` | 3065532 |
| `New Unity Project/Assets/Models/moon/textures/earth_diff.png` | 10239097 |
| `New Unity Project/Assets/Models/moon/textures/earth_norm.png` | 2600460 |
| `New Unity Project/Assets/Models/moon/textures/moon_02_diff.png` | 80783816 |
| `New Unity Project/Assets/Models/moon/textures/moon_02_disp.png` | 31628380 |
| `New Unity Project/Assets/Models/moon/textures/moon_02_disp_blur.png` | 11839171 |
| `New Unity Project/Assets/Models/moon/textures/moon_02_nor_gl.png` | 91745714 |
| `New Unity Project/Assets/Models/moon/textures/moon_02_rough.png` | 23153507 |
| `New Unity Project/Assets/Models/moon/textures/moon_footprints_02_diff.png` | 160694530 |
| `New Unity Project/Assets/Models/moon/textures/moon_footprints_02_disp.png` | 53675586 |
| `New Unity Project/Assets/Models/moon/textures/moon_footprints_02_disp_mask3.png` | 4222957 |
| `New Unity Project/Assets/Models/moon/textures/moon_footprints_02_nor_gl.png` | 178535780 |
| `New Unity Project/Assets/Models/moon/textures/moon_footprints_02_rough.png` | 49009123 |
| `New Unity Project/Assets/Models/moon/textures/moon_macro_01_diff.png` | 90332170 |
| `New Unity Project/Assets/Models/moon/textures/moon_macro_01_disp.png` | 29681427 |
| `New Unity Project/Assets/Models/moon/textures/moon_macro_01_nor_gl.png` | 95557530 |
| `New Unity Project/Assets/Models/moon/textures/moon_macro_01_rough.png` | 27537929 |
| `New Unity Project/Assets/Models/moon/textures/moon_meteor_01_diff.png` | 74628331 |
| `New Unity Project/Assets/Models/moon/textures/moon_meteor_01_disp.png` | 29120448 |
| `New Unity Project/Assets/Models/moon/textures/moon_meteor_01_disp_blur.png` | 8815347 |
| `New Unity Project/Assets/Models/moon/textures/moon_meteor_01_nor_gl.png` | 91287313 |
| `New Unity Project/Assets/Models/moon/textures/moon_meteor_01_rough.png` | 20462014 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_01_diff.png` | 15347292 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_01_disp.png` | 7361172 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_01_nor_gl.png` | 16984167 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_01_rough.png` | 5656286 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_02_diff.png` | 17923665 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_02_disp.png` | 6682994 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_02_nor_gl.png` | 18076327 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_02_rough.png` | 16459226 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_03_diff.png` | 16076295 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_03_disp.png` | 7389481 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_03_nor_gl.png` | 17692014 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_03_rough.png` | 5098259 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_04_ao.png` | 4793884 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_04_diff.png` | 15382224 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_04_disp.png` | 6941978 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_04_nor_gl.png` | 16334957 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_04_rough.png` | 5001597 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_05_diff.png` | 15477487 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_05_disp.png` | 6677915 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_05_nor_gl.png` | 16794257 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_05_rough.png` | 4935086 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_06_diff.png` | 16303420 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_06_disp.png` | 4151774 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_06_nor_gl.png` | 17506572 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_06_rough.png` | 5692926 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_07_diff.png` | 16449786 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_07_disp.png` | 5128100 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_07_nor_gl.png` | 17350653 |
| `New Unity Project/Assets/Models/moon/textures/moon_rock_07_rough.png` | 5671449 |
| `New Unity Project/Assets/Models/moon/textures/ph_grid.png` | 4234 |
| `New Unity Project/Assets/Models/moon/textures/ph_lens_dirt.png` | 11852967 |
| `New Unity Project/Assets/Models/old_metal_jarry_can._fuel_canister/old_metal_jarry_can._fuel_canister.glb` | 48760912 |
| `New Unity Project/Assets/Models/rocket/source/Rocket.fbx` | 55573420 |
| `New Unity Project/Assets/Models/sci-fi-fuel-cell-canister/source/Fuel Cell1.blend` | 1424532 |
| `New Unity Project/Assets/Models/卫星天线3d模型/tripo_convert_c00aa5f2-3e5d-4661-b5ee-89f06e28969a.fbm/tripo_rgb_a631626a-54ef-4f92-9073-baf6cec2c3bc.jpg` | 12908832 |
| `New Unity Project/Assets/Models/卫星天线3d模型/tripo_convert_c00aa5f2-3e5d-4661-b5ee-89f06e28969a.fbx` | 1477644 |
| `New Unity Project/Assets/Models/太空舱3d模型/tripo_convert_d545df9a-84dd-4c89-b62a-8f8de6eb4326.fbm/tripo_rgb_3623a817-13f5-4207-b0e8-ead93c7ce789.jpg` | 10459083 |
| `New Unity Project/Assets/Models/太空舱3d模型/tripo_convert_d545df9a-84dd-4c89-b62a-8f8de6eb4326.fbx` | 1369532 |
| `New Unity Project/Assets/Models/控制台3d模型/tripo_convert_bbd9e3d1-1672-4cd8-9207-77415c1132e0.fbm/tripo_rgb_9bea66fc-04ad-491d-9d99-8a72d5aee9d8.jpg` | 12969257 |
| `New Unity Project/Assets/Models/控制台3d模型/tripo_convert_bbd9e3d1-1672-4cd8-9207-77415c1132e0.fbx` | 1433596 |
| `New Unity Project/Assets/Models/氧气罐3d模型/tripo_convert_b2bf8aec-8680-412c-9dcb-dfd16dcaa38c.fbm/tripo_rgb_d5fd7b37-282b-4637-b2fc-e46a4ddd412c.jpg` | 11012592 |
| `New Unity Project/Assets/Models/氧气罐3d模型/tripo_convert_b2bf8aec-8680-412c-9dcb-dfd16dcaa38c.fbx` | 346972 |
| `New Unity Project/Assets/Models/科幻工具箱3d模型/科幻工具箱3d模型.fbx` | 1106000 |
| `New Unity Project/Assets/Models/能量电源3d模型/tripo_convert_3ae30049-4edb-45f5-93a8-fd06059fbb85.fbm/tripo_rgb_aa54f273-e9c2-46a8-94df-73ffd5180f98.jpg` | 11032009 |
| `New Unity Project/Assets/Models/能量电源3d模型/tripo_convert_3ae30049-4edb-45f5-93a8-fd06059fbb85.fbx` | 1318060 |
| `New Unity Project/Assets/Models/货运舱3d模型/tripo_convert_625dbe51-cadb-4137-bc21-85e32a8823ef.fbm/tripo_rgb_75f0238a-b957-492f-a662-0a6a8901bb5c.jpg` | 14939231 |
| `New Unity Project/Assets/Models/货运舱3d模型/tripo_convert_625dbe51-cadb-4137-bc21-85e32a8823ef.fbx` | 314764 |
| `New Unity Project/Assets/Models/载人火箭/tripo_convert_eb147648-f2da-4589-b7f3-b057a6f858c4.fbm/tripo_normal_2afc304c-80d4-4082-a156-96ec185555e7.jpg` | 1491206 |
| `New Unity Project/Assets/Models/载人火箭/tripo_convert_eb147648-f2da-4589-b7f3-b057a6f858c4.fbm/tripo_rgb_2201bfbf-120b-4626-8469-38c62d0e6e8f.jpg` | 10524617 |
| `New Unity Project/Assets/Models/载人火箭/tripo_convert_eb147648-f2da-4589-b7f3-b057a6f858c4.fbm/火箭3d模型_metallic.JPEG` | 2153497 |
| `New Unity Project/Assets/Models/载人火箭/tripo_convert_eb147648-f2da-4589-b7f3-b057a6f858c4.fbm/火箭3d模型_roughness.JPEG` | 2080286 |
| `New Unity Project/Assets/Models/载人火箭/tripo_convert_eb147648-f2da-4589-b7f3-b057a6f858c4.fbx` | 1424988 |
| `New Unity Project/Assets/Scenes/SampleScene/ReflectionProbe-0.exr` | 521312 |
| `New Unity Project/Assets/Scenes/SampleScene/ReflectionProbe-1.exr` | 163865 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/1K_Resolution/Back_1K_TEX.png` | 1545889 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/1K_Resolution/Down_1K_TEX.png` | 1526361 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/1K_Resolution/Front_1K_TEX.png` | 1723268 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/1K_Resolution/Left_1K_TEX.png` | 1511704 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/1K_Resolution/Right_1K_TEX.png` | 1655502 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/1K_Resolution/Up_1K_TEX.png` | 1625489 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/2K_Resolution/Back_2K_TEX.png` | 5573339 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/2K_Resolution/Down_2K_TEX.png` | 5435029 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/2K_Resolution/Front_2K_TEX.png` | 6250763 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/2K_Resolution/Left_2K_TEX.png` | 5327812 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/2K_Resolution/Right_2K_TEX.png` | 6037190 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/2K_Resolution/Up_2K_TEX.png` | 5833579 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/4K_Resolution/Back_4K_TEX.png` | 18412117 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/4K_Resolution/Down_4K_TEX.png` | 17889452 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/4K_Resolution/Front_4K_TEX.png` | 20941927 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/4K_Resolution/Left_4K_TEX.png` | 17541855 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/4K_Resolution/Right_4K_TEX.png` | 20468598 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_1/Textures/4K_Resolution/Up_4K_TEX.png` | 19382609 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/1K_Resolution/Back_1K_TEX.png` | 2200732 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/1K_Resolution/Down_1K_TEX.png` | 2068159 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/1K_Resolution/Front_1K_TEX.png` | 1805247 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/1K_Resolution/Left_1K_TEX.png` | 1882201 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/1K_Resolution/Right_1K_TEX.png` | 2025706 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/1K_Resolution/Up_1K_TEX.png` | 2112477 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/2K_Resolution/Back_2K_TEX.png` | 8469316 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/2K_Resolution/Down_2K_TEX.png` | 8071608 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/2K_Resolution/Front_2K_TEX.png` | 6960722 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/2K_Resolution/Left_2K_TEX.png` | 7217638 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/2K_Resolution/Right_2K_TEX.png` | 7675683 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/2K_Resolution/Up_2K_TEX.png` | 8067989 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/4K_Resolution/Back_4K_TEX.png` | 31723822 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/4K_Resolution/Down_4K_TEX.png` | 30189773 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/4K_Resolution/Front_4K_TEX.png` | 24804284 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/4K_Resolution/Left_4K_TEX.png` | 25824854 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/4K_Resolution/Right_4K_TEX.png` | 29037187 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_2/Textures/4K_Resolution/Up_4K_TEX.png` | 30347883 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/1K_Resolution/Back_1K_TEX.png` | 990209 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/1K_Resolution/Down_1K_TEX.png` | 925445 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/1K_Resolution/Front_1K_TEX.png` | 1599222 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/1K_Resolution/Left_1K_TEX.png` | 904384 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/1K_Resolution/Right_1K_TEX.png` | 1415854 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/1K_Resolution/Up_1K_TEX.png` | 846882 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/2K_Resolution/Back_2K_TEX.png` | 3696200 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/2K_Resolution/Down_2K_TEX.png` | 3241437 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/2K_Resolution/Front_2K_TEX.png` | 5836727 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/2K_Resolution/Left_2K_TEX.png` | 3332619 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/2K_Resolution/Right_2K_TEX.png` | 5490341 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/2K_Resolution/Up_2K_TEX.png` | 3160614 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/4K_Resolution/Back_4K_TEX.png` | 12438309 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/4K_Resolution/Down_4K_TEX.png` | 10388058 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/4K_Resolution/Front_4K_TEX.png` | 20271503 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/4K_Resolution/Left_4K_TEX.png` | 11166202 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/4K_Resolution/Right_4K_TEX.png` | 19870018 |
| `New Unity Project/Assets/SpaceSkies Free/Skybox_3/Textures/4K_Resolution/Up_4K_TEX.png` | 10797493 |
| `New Unity Project/Assets/TextMesh Pro/Sprites/EmojiOne.png` | 112319 |
| `New Unity Project/Assets/TutorialInfo/Icons/URP.png` | 133 |
