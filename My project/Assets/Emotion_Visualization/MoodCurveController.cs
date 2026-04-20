using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class MoodWaveGenerator : MonoBehaviour
{
    [Header("显示设置")]
    public RawImage rawImage;
    public int textureWidth = 260;
    public int textureHeight = 100;

    [Header("情绪颜色设置")]
    public Color happyColor = new Color(1f, 0.8f, 0.2f);    // 暖黄/橙
    public Color calmColor = new Color(0.3f, 0.8f, 0.5f);   // 柔和绿
    public Color sadColor = new Color(0.2f, 0.4f, 0.8f);    // 低沉蓝

    [Header("波形平滑度")]
    [Range(1, 10)] public int lineThickness = 3;
    [Range(0.05f, 0.5f)] public float smoothSpeed = 0.2f;

    [Header("当前情绪（调试用）")]
    [SerializeField]
    [Range(0, 2)]
    [Tooltip("0 = 悲伤, 1 = 平静, 2 = 开心")]
    private int currentMood = 1;

    // 只读属性（外部可读不可改）
    public int CurrentMood => currentMood;

    // 内部数据
    private Texture2D waveTexture;
    private float timeCounter;
    private float[] waveBuffer;
    private Color currentColor;

    void Start()
    {
        // 初始化纹理
        waveTexture = new Texture2D(textureWidth, textureHeight)
        {
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        waveBuffer = new float[textureWidth];
        rawImage.texture = waveTexture;
        currentColor = calmColor;

        ClearTexture();
    }

    void Update()
    {
        timeCounter += Time.deltaTime;
        DrawMoodWave();
    }

    // 清空画布
    void ClearTexture()
    {
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                waveTexture.SetPixel(x, y, Color.clear);
            }
        }
        waveTexture.Apply();
    }

    // 绘制波形
    void DrawMoodWave()
    {
        ClearTexture();
        int center = textureHeight / 2;

        for (int x = 0; x < textureWidth; x++)
        {
            float targetWave = GetWaveByMood(x);

            // 平滑
            waveBuffer[x] = Mathf.Lerp(waveBuffer[x], targetWave, smoothSpeed);

            DrawWavePixel(x, center + Mathf.RoundToInt(waveBuffer[x]), currentColor);
        }

        waveTexture.Apply();
    }

    // 根据情绪生成波形
    float GetWaveByMood(int x)
    {
        float t = timeCounter;
        float pos = x * 0.12f;

        switch (currentMood)
        {
            case 0: // 悲伤
                currentColor = Color.Lerp(currentColor, sadColor, 0.1f);
                return Mathf.Sin(pos + t * 1.2f) * 12f - 20f
                     + Mathf.PerlinNoise(pos * 0.3f, t * 0.4f) * 8f;

            case 1: // 平静
                currentColor = Color.Lerp(currentColor, calmColor, 0.1f);
                return Mathf.Sin(pos + t * 0.8f) * 5f
                     + Mathf.PerlinNoise(pos * 0.2f, t * 0.3f) * 4f;

            case 2: // 开心
                currentColor = Color.Lerp(currentColor, happyColor, 0.1f);
                return Mathf.Sin(pos + t * 4.0f) * 35f
                     + Mathf.PerlinNoise(pos * 0.8f, t * 1.2f) * 15f;

            default:
                return 0;
        }
    }

    // 绘制线条
    void DrawWavePixel(int x, int y, Color color)
    {
        for (int i = -lineThickness; i <= lineThickness; i++)
        {
            int py = y + i;
            if (py >= 0 && py < textureHeight)
            {
                waveTexture.SetPixel(x, py, color);
            }
        }
    }

    /// <summary>
    /// CNN 调用接口（直接切换）
    /// </summary>
    public void SetEmotion(int mood)
    {
        currentMood = Mathf.Clamp(mood, 0, 2);
    }

    /// <summary>
    /// 平滑过渡情绪（推荐用这个）
    /// </summary>
    public void SetEmotionSmooth(int mood)
    {
        StopAllCoroutines();
        StartCoroutine(SmoothEmotionChange(mood));
    }

    IEnumerator SmoothEmotionChange(int target)
    {
        int start = currentMood;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f; // 过渡速度（可调）
            currentMood = Mathf.RoundToInt(Mathf.Lerp(start, target, t));
            yield return null;
        }

        currentMood = target;
    }
}