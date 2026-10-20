using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EEGAI.Demo
{
    public class DemoEEGServiceLauncher : MonoBehaviour
    {
        private const string ConnectorRelativePath = "ThinkGear Connector/ThinkGear Connector.exe";
        private const string RealtimeRelativeDir = "eeg_modular/realtime_inference";
        private const string PythonArgs = "main.py -c config/thinkgear_tcp.yaml";
        private const string PreferredPythonPath = "E:/anaconda3/envs/eegcnn/python.exe";

        private static Process connectorProcess;
        private static Process pythonProcess;

        public static string LastStatus { get; private set; } = "EEG services not started";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindObjectOfType<DemoEEGServiceLauncher>() != null)
            {
                return;
            }

            var launcher = new GameObject("DemoEEGServiceLauncher");
            launcher.AddComponent<DemoEEGServiceLauncher>();
            DontDestroyOnLoad(launcher);
        }

        public static void StartServices()
        {
            StartThinkGearConnector();
            StartRealtimePython();
        }

        public static void StopServices()
        {
            StopRealtimePython();
            StopThinkGearConnector();
            LastStatus = "EEG services stopped";
        }

        public static bool TestThinkGearConnection()
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect("127.0.0.1", 13854, null, null);
                    bool connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(700));
                    if (!connected)
                    {
                        LastStatus = "ThinkGear Connector TCP 13854 is not reachable";
                        return false;
                    }

                    client.EndConnect(result);
                    LastStatus = "ThinkGear Connector TCP 13854 is reachable";
                    return true;
                }
            }
            catch (Exception ex)
            {
                LastStatus = $"Device test failed: {ex.Message}";
                return false;
            }
        }

        public static bool TestRealtimeDataFlow()
        {
            var emotionPanel = FindAnyObjectByType<EmotionPanel>();
            if (emotionPanel == null)
            {
                LastStatus = "Unity EmotionPanel was not found";
                return false;
            }

            bool websocketOpen = IsPortListening(8765);
            bool fresh = emotionPanel.currentDataAgeSeconds >= 0f && emotionPanel.currentDataAgeSeconds <= 3f;
            bool realSource = !string.Equals(emotionPanel.currentSource, "mock", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(emotionPanel.currentSource, "training", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(emotionPanel.currentSource, "unknown", StringComparison.OrdinalIgnoreCase);
            bool live = emotionPanel.currentDeviceConnected && emotionPanel.currentStreamConnected && fresh && realSource;

            if (live)
            {
                LastStatus = $"Realtime EEG data is live: final={emotionPanel.currentEmotion}, raw={emotionPanel.currentRawEmotion}, att={emotionPanel.currentAttention}, med={emotionPanel.currentMeditation}, eSensePackets={emotionPanel.currentESensePacketCount}";
                return true;
            }

            LastStatus = $"No real fresh EEG data. ws8765={(websocketOpen ? "open" : "closed")}, source={emotionPanel.currentSource}, device={emotionPanel.currentDeviceConnected}, stream={emotionPanel.currentStreamConnected}, age={emotionPanel.currentDataAgeSeconds:F1}s, eSenseAge={emotionPanel.currentESenseAgeSeconds:F1}s, poorSignal={emotionPanel.currentPoorSignal}, packets={emotionPanel.currentRawPacketCount}/{emotionPanel.currentESensePacketCount}/{emotionPanel.currentPowerPacketCount}";
            return false;
        }

        public static void RestartServices()
        {
            StopRealtimePython();
            StartServices();
        }

        private static void StartThinkGearConnector()
        {
            if (connectorProcess != null && !connectorProcess.HasExited)
            {
                return;
            }

            var existingConnectors = Process.GetProcessesByName("ThinkGear Connector");
            if (existingConnectors.Length > 0)
            {
                connectorProcess = existingConnectors[0];
                LastStatus = "ThinkGear Connector is already running";
                return;
            }

            string connectorPath = FindExistingPath(ConnectorRelativePath);
            if (!File.Exists(connectorPath))
            {
                LastStatus = $"ThinkGear Connector not found. Put it next to the exe or keep it in the workspace: {ConnectorRelativePath}";
                UnityEngine.Debug.LogWarning($"[EEG] {LastStatus}");
                return;
            }

            connectorProcess = Process.Start(new ProcessStartInfo
            {
                FileName = connectorPath,
                WorkingDirectory = Path.GetDirectoryName(connectorPath),
                UseShellExecute = true
            });

            LastStatus = "ThinkGear Connector started";
            UnityEngine.Debug.Log("[EEG] ThinkGear Connector started");
        }

        private static void StartRealtimePython()
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                return;
            }

            if (IsPortListening(8765))
            {
                if (TestRealtimeDataFlow())
                {
                    LastStatus = "Realtime EEG data is already live";
                    return;
                }

                StopPythonListeningOnPort(8765);
            }

            string realtimeDir = FindExistingPath(RealtimeRelativeDir);
            if (!Directory.Exists(realtimeDir))
            {
                LastStatus = $"Realtime inference folder not found. Put it next to the exe or keep it in the workspace: {RealtimeRelativeDir}";
                UnityEngine.Debug.LogWarning($"[EEG] {LastStatus}");
                return;
            }

            pythonProcess = Process.Start(new ProcessStartInfo
            {
                FileName = GetPythonExecutable(realtimeDir),
                Arguments = PythonArgs,
                WorkingDirectory = realtimeDir,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            LastStatus = "ThinkGear Connector and realtime Python started";
            UnityEngine.Debug.Log("[EEG] Realtime Python started");
        }

        private static void StopRealtimePython()
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill();
                pythonProcess = null;
            }

            StopPythonListeningOnPort(8765);
        }

        private static void StopThinkGearConnector()
        {
            if (connectorProcess != null && !connectorProcess.HasExited)
            {
                TryCloseOrKill(connectorProcess);
                connectorProcess = null;
            }

            foreach (var process in Process.GetProcessesByName("ThinkGear Connector"))
            {
                TryCloseOrKill(process);
            }
        }

        private static void TryCloseOrKill(Process process)
        {
            try
            {
                if (process == null || process.HasExited)
                {
                    return;
                }

                if (process.CloseMainWindow() && process.WaitForExit(2000))
                {
                    return;
                }

                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[EEG] Failed to stop process: {ex.Message}");
            }
        }

        private static void StopPythonListeningOnPort(int port)
        {
            foreach (int pid in FindListeningPids(port))
            {
                try
                {
                    var process = Process.GetProcessById(pid);
                    if (!process.ProcessName.ToLowerInvariant().Contains("python"))
                    {
                        continue;
                    }

                    UnityEngine.Debug.Log($"[EEG] Stopping stale realtime Python process {pid}");
                    process.Kill();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[EEG] Failed to stop process {pid}: {ex.Message}");
                }
            }
        }

        private static int[] FindListeningPids(int port)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-ano",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(1500);

                    var matches = Regex.Matches(output, $@":{port}\s+.*LISTENING\s+(\d+)");
                    int[] pids = new int[matches.Count];
                    for (int i = 0; i < matches.Count; i++)
                    {
                        int.TryParse(matches[i].Groups[1].Value, out pids[i]);
                    }
                    return pids;
                }
            }
            catch
            {
                return Array.Empty<int>();
            }
        }

        private static string GetPythonExecutable(string realtimeDir)
        {
            string bundledPython = Path.Combine(realtimeDir, "python", "python.exe");
            if (File.Exists(bundledPython)) return bundledPython;

            string venvPython = Path.Combine(realtimeDir, ".venv", "Scripts", "python.exe");
            if (File.Exists(venvPython)) return venvPython;

            return File.Exists(PreferredPythonPath) ? PreferredPythonPath : "python";
        }

        private static bool IsTcpPortOpen(string host, int port, int timeoutMs)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    bool connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeoutMs));
                    if (!connected)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPortListening(int port)
        {
            return FindListeningPids(port).Length > 0;
        }

        private static string FindExistingPath(string relativePath)
        {
            foreach (string root in GetCandidateRoots())
            {
                string fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return Path.GetFullPath(Path.Combine(GetCandidateRoots()[0], relativePath));
        }

        private static string[] GetCandidateRoots()
        {
            string dataPath = Application.dataPath;
            string exeDir = Path.GetFullPath(Path.Combine(dataPath, ".."));
            string workspaceRoot = Path.GetFullPath(Path.Combine(dataPath, "..", "..", ".."));
            string currentDir = Directory.GetCurrentDirectory();

            return new[]
            {
                exeDir,
                workspaceRoot,
                currentDir,
                Path.GetFullPath(Path.Combine(exeDir, "..")),
            };
        }

        private void OnApplicationQuit()
        {
            StopServices();
        }
    }
}
