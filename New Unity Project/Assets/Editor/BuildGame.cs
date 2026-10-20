using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EEGAI.Editor
{
    /// <summary>
    /// Windows build entry used to refresh the shipped Game folder.
    /// Batch mode:
    /// Unity.exe -batchmode -projectPath "AR/New Unity Project" -executeMethod EEGAI.Editor.BuildGame.BuildWindows -quit
    /// </summary>
    public static class BuildGame
    {
        private const string OutputDirectory = "../../Game";
        private const string OutputExecutable = "My project.exe";

        public static void BuildWindows()
        {
            Build(true);
        }

        public static void BuildWindowsFromEditor()
        {
            Build(false);
        }

        private static void Build(bool exitWhenDone)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputPath = Path.GetFullPath(Path.Combine(projectRoot, OutputDirectory, OutputExecutable));
            string resultPath = Path.GetFullPath(Path.Combine(projectRoot, "Logs", "BuildGameResult.txt"));

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));

            try
            {
                var options = new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Scenes/SampleScene.unity" },
                    locationPathName = outputPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;

                if (summary.result != BuildResult.Succeeded)
                {
                    File.WriteAllText(resultPath, $"FAILED: {summary.result}, errors={summary.totalErrors}, output={outputPath}");
                    Debug.LogError($"[Build] Failed: {summary.result}, errors={summary.totalErrors}");
                    if (exitWhenDone) EditorApplication.Exit(1);
                    return;
                }

                File.WriteAllText(resultPath, $"SUCCEEDED: {outputPath}, size={summary.totalSize}, time={DateTime.Now:O}");
                Debug.Log($"[Build] Succeeded: {outputPath} ({summary.totalSize} bytes)");
                if (exitWhenDone) EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                File.WriteAllText(resultPath, $"EXCEPTION: {exception}");
                Debug.LogError($"[Build] Exception: {exception}");
                if (exitWhenDone) EditorApplication.Exit(1);
            }
        }
    }
}
