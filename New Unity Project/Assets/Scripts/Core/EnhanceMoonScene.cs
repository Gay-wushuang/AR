using UnityEngine;

/// <summary>
/// 增强月球场景 - 扩大地面并添加岩石
/// </summary>
[ExecuteInEditMode]
public class EnhanceMoonScene : MonoBehaviour
{
    [Header("地面设置")]
    [SerializeField] private float groundSize = 200f;
    
    [Header("岩石材质（7种岩石各自的PBR纹理）")]
    [Tooltip("Element 0~6 对应 moon_rock_01~07，留空则使用程序化颜色")]
    [SerializeField] private RockMaterialSet[] rockMaterials = new RockMaterialSet[]
    {
        new RockMaterialSet(), new RockMaterialSet(), new RockMaterialSet(),
        new RockMaterialSet(), new RockMaterialSet(), new RockMaterialSet(),
        new RockMaterialSet()
    };

    [Header("岩石数量和分布")]
    [SerializeField] private int rockCount = 15;
    [SerializeField] private float scatterRadius = 40f;
    [SerializeField] private Vector2 rockScaleRange = new Vector2(0.5f, 3f);

    [Header("随机种子")]
    [SerializeField] private int randomSeed = 42;

    [System.Serializable]
    public class RockMaterialSet
    {
        public Texture2D diffuse;
        public Texture2D normal;
        public Texture2D roughness;
        public Texture2D displacement;
    }

#if UNITY_EDITOR
    [ContextMenu("扩大地面")]
    public void ExpandGround()
    {
        var ground = GameObject.Find("MoonSurface");
        if (ground == null)
        {
            Debug.LogWarning("未找到 MoonSurface，请先创建月球场景");
            return;
        }
        
        float scale = groundSize / 10f;
        ground.transform.localScale = new Vector3(scale, 1f, scale);
        Debug.Log($"地面已扩大到 {groundSize}x{groundSize} 米");
    }

    [ContextMenu("添加岩石")]
    public void AddRocks()
    {
        ClearRocks();
        var rocksParent = new GameObject("Rocks");
        System.Random rng = new System.Random(randomSeed);
        
        for (int i = 0; i < rockCount; i++)
        {
            CreateRock(rocksParent.transform, rng);
        }
        
        Debug.Log($"已创建 {rockCount} 个岩石");
    }

    private void CreateRock(Transform parent, System.Random rng)
    {
        int rockTypeIndex = rng.Next(0, 7);  // 0~6 对应 rock_01~07

        // 使用 Sphere 作为基础形状，然后随机非均匀缩放变成椭圆
        var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = $"Rock_{rockTypeIndex + 1:D2}_{parent.childCount}";
        rock.transform.parent = parent;

        // 随机位置（圆形分布）
        float angle = (float)(rng.NextDouble() * 2 * Mathf.PI);
        float radius = (float)(rng.NextDouble() * scatterRadius);
        rock.transform.position = new Vector3(
            Mathf.Cos(angle) * radius,
            0.5f,
            Mathf.Sin(angle) * radius
        );

        // 球形可以任意旋转，不需要限制
        rock.transform.rotation = Quaternion.Euler(
            (float)(rng.NextDouble() * 360f),  // X轴：0-360°
            (float)(rng.NextDouble() * 360f),  // Y轴：0-360°
            (float)(rng.NextDouble() * 360f)   // Z轴：0-360°
        );

        // 随机非均匀缩放，让球体变成椭圆（更自然的岩石形状）
        float baseScale = Mathf.Lerp(rockScaleRange.x, rockScaleRange.y, (float)rng.NextDouble());
        rock.transform.localScale = new Vector3(
            baseScale * (0.6f + (float)rng.NextDouble() * 0.8f),   // X: 0.6 ~ 1.4
            baseScale * (0.5f + (float)rng.NextDouble() * 0.6f),   // Y: 0.5 ~ 1.1
            baseScale * (0.6f + (float)rng.NextDouble() * 0.8f)    // Z: 0.6 ~ 1.4
        );

        // 完全禁用顶点扰动，避免面之间出现缝隙
        // MakeIrregularRock(rock, 0.02f, rng);

        // 应用对应岩石的完整 PBR 材质
        ApplyRockMaterial(rock, rockTypeIndex, rng);
    }

    /// <summary>
    /// 正确的顶点随机扰动方法（沿法线方向 + 复制 mesh + 控制强度）
    /// </summary>
    private void MakeIrregularRock(GameObject rock, float strength, System.Random rng)
    {
        var meshFilter = rock.GetComponent<MeshFilter>();
        var mesh = Instantiate(meshFilter.sharedMesh); // 必须复制 mesh，避免修改原始资源
        meshFilter.sharedMesh = mesh;

        var vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];

            // 关键1：朝"外"扰动（沿法线方向，避免炸裂）
            Vector3 dir = v.normalized;

            // 关键2：随机强度（控制在合理范围内）
            float offset = (float)(rng.NextDouble() - 0.5f) * strength;

            // 关键3：叠加一点非规则扰动（幅度更小）
            Vector3 random = new Vector3(
                (float)(rng.NextDouble() - 0.5f),
                (float)(rng.NextDouble() - 0.5f),
                (float)(rng.NextDouble() - 0.5f)
            ) * strength * 0.3f;

            vertices[i] = v + dir * offset + random;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void ApplyRockMaterial(GameObject rock, int typeIndex, System.Random rng)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.enableInstancing = true;

        // 如果该类型配置了纹理，使用完整的 PBR 材质
        if (rockMaterials != null && typeIndex < rockMaterials.Length && rockMaterials[typeIndex] != null)
        {
            var rm = rockMaterials[typeIndex];

            if (rm.diffuse != null)
            {
                material.mainTexture = rm.diffuse;
                // 移除颜色 tint，让纹理本身的颜色自然显示
            }

            if (rm.normal != null)
            {
                material.SetTexture("_BumpMap", rm.normal);
                material.EnableKeyword("_NORMALMAP");
                material.SetFloat("_BumpScale", 0.3f);
            }

            // 暂时禁用 roughness 贴图（在 URP 里需要转成 smoothness 才能用）
            // if (rm.roughness != null)
            // {
            //     material.SetTexture("_MetallicGlossMap", rm.roughness);
            //     material.EnableKeyword("_METALLICSPECGLOSSMAP");
            // }

            // 直接设置固定的金属度和光滑度（更粗糙）
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.15f);

            // 禁用位移/视差贴图
            // if (rm.displacement != null)
            // {
            //     material.SetTexture("_ParallaxMap", rm.displacement);
            //     material.EnableKeyword("_PARALLAXMAP");
            //     material.SetFloat("_Parallax", 0.05f);
            // }
        }
        else
        {
            // 回退到程序化颜色
            float gray = 0.3f + (float)rng.NextDouble() * 0.3f;
            material.color = new Color(gray, gray * 0.95f, gray * 0.9f);
        }

        rock.GetComponent<Renderer>().material = material;
    }

    [ContextMenu("清除岩石")]
    public void ClearRocks()
    {
        var rocksParent = GameObject.Find("Rocks");
        if (rocksParent != null)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(rocksParent);
                Debug.Log("岩石已清除");
            };
        }
        else
        {
            Debug.Log("没有找到岩石");
        }
    }

    private static Vector3[] ReadEditorVertices(Mesh mesh)
    {
        using (var data = UnityEditor.MeshUtility.AcquireReadOnlyMeshData(mesh))
        using (var vertices = new Unity.Collections.NativeArray<Vector3>(data[0].vertexCount, Unity.Collections.Allocator.Temp))
        {
            data[0].GetVertices(vertices);
            return vertices.ToArray();
        }
    }
    [ContextMenu("Snap Rocks To Moon Mesh / 岩石吸附月面网格")]
    public void SnapRocksToMoonMesh()
    {
        var ground = GameObject.Find("MoonSurface");
        var surface = ground != null ? ground.GetComponent<MeshCollider>() : null;
        if (surface == null) { Debug.LogWarning("MoonSurface MeshCollider is required."); return; }
        Physics.SyncTransforms();
        int count = 0;
        foreach (string groupName in new[] { "Rocks", "MoonRockScatter", "SmallMoonRockScatter" })
        {
            var group = GameObject.Find(groupName);
            if (group == null) continue;
            foreach (Transform rock in group.transform)
            {
                var renderers = rock.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;
                Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                // Editor mesh access is valid even when the runtime Read/Write flag is off.
                bool hasVertex = false;
                foreach (var filter in rock.GetComponentsInChildren<MeshFilter>())
                {
                    if (filter.sharedMesh == null) continue;
                    foreach (var vertex in ReadEditorVertices(filter.sharedMesh))
                    {
                        Vector3 point = filter.transform.TransformPoint(vertex);
                        if (!hasVertex) { bounds = new Bounds(point, Vector3.zero); hasVertex = true; }
                        else bounds.Encapsulate(point);
                    }
                }
                // Sample the actual transformed rock mesh, not just its pivot or box bottom.
                float contactOffset = float.NegativeInfinity;
                var supports = new System.Collections.Generic.List<float>();
                foreach (var filter in rock.GetComponentsInChildren<MeshFilter>())
                {
                    if (filter.sharedMesh == null) continue;
                    foreach (Vector3 vertex in ReadEditorVertices(filter.sharedMesh))
                    {
                        Vector3 world = filter.transform.TransformPoint(vertex);
                        if (world.y > bounds.min.y + bounds.size.y * .25f) continue;
                        RaycastHit hit;
                        if (surface.Raycast(new Ray(new Vector3(world.x, surface.bounds.max.y + 10f, world.z), Vector3.down), out hit, surface.bounds.size.y + 20f))
                            supports.Add(hit.point.y - world.y);
                    }
                }
                if (supports.Count > 0)
                {
                    supports.Sort();
                    // Support the lower footprint, rather than balancing on a single tip.
                    contactOffset = supports[Mathf.FloorToInt((supports.Count - 1) * .75f)];
                }
                if (float.IsNegativeInfinity(contactOffset))
                {
                    RaycastHit hit;
                    if (!surface.Raycast(new Ray(new Vector3(bounds.center.x, surface.bounds.max.y + 10f, bounds.center.z), Vector3.down), out hit, surface.bounds.size.y + 20f)) continue;
                    contactOffset = hit.point.y - bounds.min.y;
                }
                UnityEditor.Undo.RecordObject(rock, "Snap rock to moon mesh");
                // Slight burial conceals small gaps under irregular boulders.
                rock.position += Vector3.up * (contactOffset - Mathf.Min(0.35f, bounds.size.y * 0.18f));
                UnityEditor.EditorUtility.SetDirty(rock);
                count++;
            }
        }
        Physics.SyncTransforms();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log("Snapped " + count + " rocks to the moon mesh.");
    }

    [ContextMenu("完整优化场景")]
    public void FullOptimize()
    {
        ExpandGround();
        AddRocks();
        SnapRocksToMoonMesh();
        Debug.Log("场景优化完成！");
    }
#endif
}
