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

    [Header("岩石形状")]
    [SerializeField] private float rockIrregularity = 0.05f;

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

        // 使用球体作为岩石基础形状（更自然的月球岩石外观）
        var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = $"Rock_{rockTypeIndex + 1:D2}_{parent.childCount}";
        rock.transform.parent = parent;

        // 随机位置（圆形分布）
        float angle = (float)(rng.NextDouble() * 2 * Mathf.PI);
        float radius = (float)(rng.NextDouble() * scatterRadius);
        float posX = Mathf.Cos(angle) * radius;
        float posZ = Mathf.Sin(angle) * radius;

        // 采样地面高度，防止浮空
        float groundY = SampleGroundHeight(posX, posZ);

        rock.transform.position = new Vector3(
            posX,
            groundY,
            posZ
        );

        // 随机旋转（球形可以任意旋转）
        rock.transform.rotation = Quaternion.Euler(
            (float)(rng.NextDouble() * 360f),
            (float)(rng.NextDouble() * 360f),
            (float)(rng.NextDouble() * 360f)
        );

        // 球形岩石：均匀缩放 + 轻微不规则变形
        float baseScale = Mathf.Lerp(rockScaleRange.x, rockScaleRange.y, (float)rng.NextDouble());
        rock.transform.localScale = new Vector3(
            baseScale * (0.8f + (float)rng.NextDouble() * 0.4f),   // X: 0.8 ~ 1.2
            baseScale * (0.8f + (float)rng.NextDouble() * 0.4f),   // Y: 0.8 ~ 1.2（球形不压扁）
            baseScale * (0.8f + (float)rng.NextDouble() * 0.4f)    // Z: 0.8 ~ 1.2
        );

        // 轻微的几何顶点扰动，让形状更自然
        DistortRockMesh(rock, rng);

        // 应用对应岩石的完整 PBR 材质
        ApplyRockMaterial(rock, rockTypeIndex, rng);
    }

    /// <summary>
    /// 采样月球地面的高度，让岩石贴合地形表面
    /// </summary>
    private float SampleGroundHeight(float x, float z)
    {
        var ground = GameObject.Find("MoonSurface");
        if (ground == null) return 0f;

        var meshFilter = ground.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null) return 0f;

        // 将世界坐标转换到地面局部坐标
        Vector3 localPos = ground.transform.InverseTransformPoint(new Vector3(x, 0, z));
        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;

        // 找到最近的顶点并采样其高度
        float minDistSq = float.MaxValue;
        float height = 0f;

        for (int i = 0; i < vertices.Length; i++)
        {
            float dx = vertices[i].x - localPos.x;
            float dz = vertices[i].z - localPos.z;
            float distSq = dx * dx + dz * dz;

            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                height = vertices[i].y;
            }
        }

        // 转换回世界坐标系的高度
        return ground.transform.position.y + height * ground.transform.localScale.y;
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
                material.mainTexture = rm.diffuse;

            if (rm.normal != null)
            {
                material.SetTexture("_BumpMap", rm.normal);
                material.EnableKeyword("_NORMALMAP");
                material.SetFloat("_BumpScale", 0.3f);  // 法线强度改回 0.3，更合理！
            }

            // 暂时禁用 roughness 贴图（在 URP 里需要转成 smoothness 才能用）
            // if (rm.roughness != null)
            // {
            //     material.SetTexture("_MetallicGlossMap", rm.roughness);
            //     material.EnableKeyword("_METALLICSPECGLOSSMAP");
            // }

            // 直接设置固定的金属度和光滑度（更粗糙）
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.15f);  // 降低到 0.15，更粗糙！（数值越小越粗糙）

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

    /// <summary>
    /// 对立方体网格做轻微随机扰动（幅度很小，不会变成刺猬）
    /// </summary>
    private void DistortRockMesh(GameObject rock, System.Random rng)
    {
        var meshFilter = rock.GetComponent<MeshFilter>();
        var mesh = meshFilter.mesh;
        var vertices = mesh.vertices;

        // 扰动幅度很小（0.05-0.15），让形状稍微不规则一点，但不会变成刺猬
        float distortAmount = 0.05f + (float)rng.NextDouble() * 0.1f;

        for (int i = 0; i < vertices.Length; i++)
        {
            // 每个顶点独立的随机位移
            vertices[i] += new Vector3(
                ((float)rng.NextDouble() * 2f - 1f) * distortAmount,
                ((float)rng.NextDouble() * 2f - 1f) * distortAmount,
                ((float)rng.NextDouble() * 2f - 1f) * distortAmount
            );
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
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

    [ContextMenu("完整优化场景")]
    public void FullOptimize()
    {
        ExpandGround();
        AddRocks();
        Debug.Log("场景优化完成！");
    }
#endif
}
