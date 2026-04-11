using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 自动创建月球表面场景（优化版）
/// 修复：平滑地形、自然光照、高密度网格
/// </summary>
[ExecuteInEditMode]
public class SetupMoonSurface : MonoBehaviour
{
    [Header("月球纹理（从 Assets/Models/moon 拖入）")]
    [SerializeField] private Texture2D moonDiffuse;
    [SerializeField] private Texture2D moonNormal;
    [SerializeField] private Texture2D moonRoughness;
    [SerializeField] private Texture2D moonDisplacement;

    [Header("地面设置")]
    [SerializeField] private float groundSize = 200f;
    [SerializeField] private Color groundColor = Color.white;
    [SerializeField] [Range(4, 128)] private int groundSubdivisions = 64;

    [Header("地形噪声设置")]
    [SerializeField] private float terrainHeight = 0.3f;
    [SerializeField] private float terrainScale = 8f;
    [SerializeField] private int terrainOctaves = 4;
    [SerializeField] private float terrainPersistence = 0.5f;
    [SerializeField] private float terrainLacunarity = 2f;

    [Header("光照设置")]
    [SerializeField] private float sunIntensity = 1.5f;
    [SerializeField] private Vector3 sunRotation = new Vector3(50f, -30f, 0f);
    [SerializeField] private Color ambientColor = new Color(0.15f, 0.15f, 0.18f);
    [SerializeField] private float ambientIntensity = 0.8f;

    [Header("相机设置")]
    [SerializeField] private Vector3 cameraPosition = new Vector3(0f, 2f, -5f);

#if UNITY_EDITOR
    [ContextMenu("创建月球场景")]
    public void CreateMoonScene()
    {
        CreateGround();
        CreateSunlight();
        SetupAmbientLight();
        SetupCamera();
        Debug.Log("✅ 月球场景创建完成！（优化版）");
    }

    private void CreateGround()
    {
        var existing = GameObject.Find("MoonSurface");
        if (existing != null)
        {
            Debug.LogWarning("月球表面已存在，跳过创建");
            return;
        }

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "MoonSurface";
        ground.transform.localScale = new Vector3(groundSize / 10f, 1f, groundSize / 10f);

        SubdivideMesh(ground);
        ApplySmoothTerrain(ground);

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = "MoonMaterial_Regolith";

        if (moonDiffuse != null)
        {
            material.mainTexture = moonDiffuse;
            material.color = new Color(0.75f, 0.73f, 0.70f);
            Debug.Log($"应用漫反射贴图: {moonDiffuse.name}");
        }
        else
        {
            material.color = new Color(0.45f, 0.43f, 0.40f);
        }

        if (moonNormal != null)
        {
            material.SetTexture("_BumpMap", moonNormal);
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_BumpScale", 2.0f);
        }

        if (moonRoughness != null)
        {
            material.SetTexture("_MetallicGlossMap", moonRoughness);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_GlossMapScale", 0.0f);
            material.SetFloat("_Smoothness", 0.0f);
        }
        else
        {
            material.SetFloat("_Smoothness", 0.0f);
        }

        if (moonDisplacement != null)
        {
            material.SetTexture("_ParallaxMap", moonDisplacement);
            material.EnableKeyword("_PARALLAXMAP");
            material.SetFloat("_Parallax", 0.005f);
        }

        material.SetFloat("_Metallic", 0.0f);
        material.renderPriority = 1;
        material.doubleSidedGI = true;

        var renderer = ground.GetComponent<Renderer>();
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;

        Debug.Log($"✅ 月球表面创建完成 | 月壤质感模式 | 网格细分: {groundSubdivisions}x{groundSubdivisions} | 顶点数: {(groundSubdivisions+1)*(groundSubdivisions+1)}");
    }

    private void SubdivideMesh(GameObject ground)
    {
        var meshFilter = ground.GetComponent<MeshFilter>();
        var originalMesh = meshFilter.sharedMesh;

        var mesh = new Mesh();
        mesh.name = "MoonSurface_HighPoly";

        int resolution = groundSubdivisions + 1;
        var vertices = new Vector3[resolution * resolution];
        var uvs = new Vector2[resolution * resolution];
        var triangles = new int[groundSubdivisions * groundSubdivisions * 6];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = z * resolution + x;
                float px = (float)x / groundSubdivisions * 10f - 5f;
                float pz = (float)z / groundSubdivisions * 10f - 5f;

                vertices[index] = new Vector3(px, 0, pz);
                uvs[index] = new Vector2((float)x / groundSubdivisions, (float)z / groundSubdivisions);
            }
        }

        int triIndex = 0;
        for (int z = 0; z < groundSubdivisions; z++)
        {
            for (int x = 0; x < groundSubdivisions; x++)
            {
                int topLeft = z * resolution + x;
                int topRight = topLeft + 1;
                int bottomLeft = (z + 1) * resolution + x;
                int bottomRight = bottomLeft + 1;

                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topRight;

                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = bottomRight;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
    }

    private void ApplySmoothTerrain(GameObject ground)
    {
        var meshFilter = ground.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        var mesh = meshFilter.sharedMesh;
        var vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            float x = vertices[i].x;
            float z = vertices[i].z;

            float height = 0f;
            float amplitude = terrainHeight;
            float frequency = 1f / terrainScale;

            for (int octave = 0; octave < terrainOctaves; octave++)
            {
                height += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
                amplitude *= terrainPersistence;
                frequency *= terrainLacunarity;
            }

            vertices[i].y += height;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        Debug.Log($"✅ 平滑地形已应用 | 噪声八度: {terrainOctaves} | 最大高度: ±{terrainHeight}m");
    }

    private void CreateSunlight()
    {
        var existing = GameObject.Find("SunLight");
        if (existing != null)
        {
            Debug.LogWarning("光源已存在，跳过创建");
            return;
        }

        var lightGO = new GameObject("SunLight");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.99f, 0.98f);
        light.intensity = sunIntensity;
        light.shadows = LightShadows.Soft;
        light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
        light.shadowStrength = 0.9f;

        lightGO.transform.rotation = Quaternion.Euler(sunRotation);

        Debug.Log("✅ 太阳光创建完成（月球高对比度模式）");
    }

    private void SetupAmbientLight()
    {
        RenderSettings.ambientSkyColor = new Color(0.02f, 0.02f, 0.025f);
        RenderSettings.ambientIntensity = 0.3f;

        var probeGO = GameObject.Find("SceneReflectionProbe");
        if (probeGO == null)
        {
            probeGO = new GameObject("SceneReflectionProbe");
            var probe = probeGO.AddComponent<ReflectionProbe>();
            probe.resolution = 256;
            probe.backgroundColor = new Color(0.01f, 0.01f, 0.015f, 1f);
            probe.size = new Vector3(50f, 20f, 50f);
            probe.blendDistance = 5f;
        }

        Debug.Log("✅ 环境光和反射探针已配置（低强度太空模式）");
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = cameraPosition;
            cam.transform.LookAt(Vector3.zero);

            var camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null)
            {
                camData.renderPostProcessing = true;
            }

            Debug.Log("✅ 相机位置已设置");
        }
        else
        {
            Debug.LogWarning("⚠️ 未找到主相机，请手动调整");
        }
    }

    [ContextMenu("清除场景")]
    public void ClearScene()
    {
        var ground = GameObject.Find("MoonSurface");
        var light = GameObject.Find("SunLight");
        var probe = GameObject.Find("SceneReflectionProbe");

        if (ground != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(ground);
        if (light != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(light);
        if (probe != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(probe);

        Debug.Log("🗑️ 场景已清除");
    }
#endif
}
