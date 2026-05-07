using UnityEngine;

public class ScreenFlash : MonoBehaviour
{
    private Material flashMaterial;
    private float flashTimer;
    private float flashDuration;
    private Color flashColor;
    private bool isFlashing;

    private static readonly int ShaderColorID = Shader.PropertyToID("_FlashColor");

    private void Awake()
    {
        Shader flashShader = Shader.Find("Hidden/ScreenFlash");
        if (flashShader == null)
        {
            flashShader = Shader.Find("Unlit/Color");
        }
        flashMaterial = new Material(flashShader);
        flashMaterial.SetColor(ShaderColorID, Color.clear);
    }

    public void Flash(Color color, float duration)
    {
        flashColor = color;
        flashDuration = duration;
        flashTimer = 0f;
        isFlashing = true;
    }

    private void Update()
    {
        if (isFlashing)
        {
            flashTimer += Time.deltaTime;
            float t = flashTimer / flashDuration;
            float alpha = Mathf.Lerp(flashColor.a, 0f, t);
            
            Color currentColor = flashColor;
            currentColor.a = alpha;
            flashMaterial.SetColor(ShaderColorID, currentColor);

            if (t >= 1f)
            {
                isFlashing = false;
                flashMaterial.SetColor(ShaderColorID, Color.clear);
            }
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (flashMaterial != null && isFlashing)
        {
            Graphics.Blit(source, destination, flashMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
