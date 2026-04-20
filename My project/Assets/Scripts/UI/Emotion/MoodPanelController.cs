using UnityEngine;
using UnityEngine.UI;

public class MoodPanelController : MonoBehaviour
{
    public RectTransform moodWindow;
    public Button foldButton;
    private bool isFolded = false;

    // 折叠后变成正方形：宽和高都是 30
    private float foldedSize = 30f;
    private float originalWidth;
    private float originalHeight;

    void Start()
    {
        originalWidth = moodWindow.sizeDelta.x;
        originalHeight = moodWindow.sizeDelta.y;
        foldButton.onClick.AddListener(TogglePanel);
    }

    public void TogglePanel()
    {
        if (isFolded)
        {
            // 展开：还原原来的宽和高
            moodWindow.sizeDelta = new Vector2(originalWidth, originalHeight);
            foldButton.GetComponentInChildren<TMPro.TMP_Text>().text = "←";
        }
        else
        {
            // 折叠：宽、高都变成 30，直接正方形
            moodWindow.sizeDelta = new Vector2(foldedSize, foldedSize);
            foldButton.GetComponentInChildren<TMPro.TMP_Text>().text = "→";
        }
        isFolded = !isFolded;
    }
}