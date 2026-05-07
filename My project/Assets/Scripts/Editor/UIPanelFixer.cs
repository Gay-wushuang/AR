using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace EEGAI.UI.Editor
{
    public class UIPanelFixer : EditorWindow
    {
        [MenuItem("Tools/UI Panel Fixer")]
        public static void ShowWindow()
        {
            GetWindow<UIPanelFixer>("UI Panel Fixer");
        }

        private void OnGUI()
        {
            GUILayout.Label("UI Panel 修复工具", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("修复当前场景中的所有 UI 面板"))
            {
                FixAllUIPanels();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("修复内容:", EditorStyles.wordWrappedLabel);
            GUILayout.Label("- 设置 TMP_InputField 为多行模式", EditorStyles.wordWrappedLabel);
            GUILayout.Label("- 启用 TextMeshProUGUI 的自动换行", EditorStyles.wordWrappedLabel);
            GUILayout.Label("- 确保所有 UI 元素可交互", EditorStyles.wordWrappedLabel);
        }

        private static void FixAllUIPanels()
        {
            int fixCount = 0;
            
            TMP_InputField[] inputFields = FindObjectsOfType<TMP_InputField>(true);
            foreach (var inputField in inputFields)
            {
                inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
                inputField.interactable = true;
                inputField.shouldHideMobileInput = false;
                EditorUtility.SetDirty(inputField);
                fixCount++;
            }
            
            TextMeshProUGUI[] textComponents = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var textComp in textComponents)
            {
                textComp.enableWordWrapping = true;
                textComp.textWrappingMode = TextWrappingModes.Normal;
                textComp.overflowMode = TextOverflowModes.Overflow;
                EditorUtility.SetDirty(textComp);
                fixCount++;
            }
            
            InputField[] legacyInputFields = FindObjectsOfType<InputField>(true);
            foreach (var inputField in legacyInputFields)
            {
                inputField.lineType = InputField.LineType.MultiLineNewline;
                inputField.interactable = true;
                EditorUtility.SetDirty(inputField);
                fixCount++;
            }
            
            CanvasGroup[] canvasGroups = FindObjectsOfType<CanvasGroup>(true);
            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                EditorUtility.SetDirty(canvasGroup);
                fixCount++;
            }
            
            EditorUtility.DisplayDialog("修复完成", $"已修复 {fixCount} 个 UI 组件！", "确定");
        }
    }
}
