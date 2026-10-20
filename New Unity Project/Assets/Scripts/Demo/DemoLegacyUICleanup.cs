using UnityEngine;

namespace EEGAI.Demo
{
    public class DemoLegacyUICleanup : MonoBehaviour
    {
        private static readonly string[] LegacyObjectNames =
        {
            "Btn_Fold",
            "MoodWindow",
            "MoodCurve_Raw"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Cleanup()
        {
            DestroyLegacyPanel();

            foreach (string objectName in LegacyObjectNames)
            {
                DestroyByName(objectName);
            }
        }

        private static void DestroyByName(string objectName)
        {
            foreach (var transform in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (transform == null || transform.gameObject == null || transform.name != objectName)
                {
                    continue;
                }

                if (!transform.gameObject.scene.IsValid())
                {
                    continue;
                }

                Destroy(transform.gameObject);
            }
        }

        private static void DestroyLegacyPanel()
        {
            foreach (var transform in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (transform == null || transform.gameObject == null || transform.name != "Panel")
                {
                    continue;
                }

                bool looksLikeLegacyFoldPanel =
                    transform.childCount == 1 &&
                    transform.GetChild(0).name == "Btn_Fold";

                if (looksLikeLegacyFoldPanel && transform.gameObject.scene.IsValid())
                {
                    Destroy(transform.gameObject);
                }
            }
        }
    }
}
