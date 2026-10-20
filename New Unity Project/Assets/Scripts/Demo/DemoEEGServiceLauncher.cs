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
        private static readonly bool UseDirectSerial = true;
        private const string DirectSerialPort = "COM8";

        private static Process connectorProcess;
        private static bool ownsConnectorProcess;
        private static Process pythonProcess;

        public static string LastStatus { get; private set; } = "EEG services not started";
        public const string ExpectedServerVersion = "2.0";

        public static string ExpectedSourcePrefix => UseDirectSerial ? "serial/" : "tcp/";
        public static string ExpectedConfigLabel => UseDirectSerial ? $"serial:{DirectSerialPort}" : "tcp";

        public static bool IsExpectedConfig(string mode, string port)
        {
            if (UseDirectSerial)
            {
                bool modeMatches = string.Equals(mode, "serial", StringComparison.OrdinalIgnoreCase);
                bool portMatches = string.IsNullOrEmpty(port) ||
                    string.Equals(port, DirectSerialPort, StringComparison.OrdinalIgnoreCase);
                return modeMatches && portMatches;
            }

            return string.Equals(mode, "tcp", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsExpectedSource(string source)
        {
            return !string.IsNullOrEmpty(source) &&
                source.StartsWith(ExpectedSourcePrefix, StringComparison.OrdinalIgnoreCase);
        }

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
            if (UseDirectSerial)
            {
                LastStatus = $"Using direct ThinkGear serial input on {DirectSerialPort}; Connector is not required.";
            }
            else
            {
                StartThinkGearConnector();
            }
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
            if (UseDirectSerial)
            {
                var emotionPanel = FindAnyObjectByType<EmotionPanel>();
                bool liveSerial = emotionPanel != null && emotionPanel.WebSocketConnected &&
                    emotionPanel.currentStreamConnected &&
                    !string.IsNullOrEmpty(emotionPanel.currentSource) &&
                    emotionPanel.currentSource.StartsWith("serial/", StringComparison.OrdinalIgnoreCase);
                LastStatus = liveSerial
                    ? $"ThinkGear is streaming directly from {DirectSerialPort}"
                    : $"No live ThinkGear stream from {DirectSerialPort}. Check headset power/Bluetooth, then restart EEG services.";
                return liveSerial;
            }

            bool connected = IsTcpPortOpen("127.0.0.1", 13854, 500);
            LastStatus = connected
                ? "ThinkGear Connector TCP 13854 is reachable"
                : "ThinkGear Connector TCP 13854 is not listening. Open the Connector, connect the headset, and enable its TCP server.";
            return connected;
        }

        public static bool TestRealtimeDataFlow()
        {
            var emotionPanel = FindAnyObjectByType<EmotionPanel>();
            if (emotionPanel == null)
            {
                LastStatus = "Unity EmotionPanel was not found";
                return false;
            }

            bool fresh = emotionPanel.currentDataAgeSeconds >= 0f && emotionPanel.currentDataAgeSeconds <= 3f;
            bool realSource = !string.Equals(emotionPanel.currentSource, "mock", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(emotionPanel.currentSource, "training", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(emotionPanel.currentSource, "unknown", StringComparison.OrdinalIgnoreCase);
            bool live = emotionPanel.WebSocketConnected && emotionPanel.MessageAgeSeconds <= 3f &&
                emotionPanel.currentDeviceConnected && emotionPanel.currentStreamConnected && fresh && realSource;

            if (live)
            {
                LastStatus = $"Realtime EEG data is live: final={emotionPanel.currentEmotion}, raw={emotionPanel.currentRawEmotion}, att={emotionPanel.currentAttention}, med={emotionPanel.currentMeditation}, eSensePackets={emotionPanel.currentESensePacketCount}";
                return true;
            }

            if (!emotionPanel.isActiveAndEnabled)
            {
                LastStatus = "EEG receiver is disabled. Enable EmotionPanel in the scene.";
                return false;
            }

            if (!emotionPanel.WebSocketConnected)
            {
                LastStatus = $"Unity EEG connection: {emotionPanel.ConnectionStatus}. Address: {emotionPanel.serverUrl}";
                return false;
            }

            if (emotionPanel.MessageAgeSeconds > 3f)
            {
                if (UseDirectSerial)
                {
                    LastStatus = $"Realtime service stopped sending messages. Restart EEG services; direct input is configured for {DirectSerialPort}.";
                }
                else if (!IsTcpPortOpen("127.0.0.1", 13854, 300))
                {
                    LastStatus = "Unity is connected to the inference service, but ThinkGear Connector TCP 13854 is not listening. Connect the headset in ThinkGear Connector and enable its TCP server.";
                }
                else
                {
                    LastStatus = $"ThinkGear TCP is open, but the inference service has sent no recent messages. Unity received: {emotionPanel.ReceivedMessageCount}";
                }
                return false;
            }

            if (!emotionPanel.currentStreamConnected && UseDirectSerial)
            {
                LastStatus = $"Realtime service is reachable, but no ThinkGear packets are arriving from {DirectSerialPort}. Check headset power/Bluetooth and restart EEG services.";
                return false;
            }

            if (!emotionPanel.currentStreamConnected && !IsTcpPortOpen("127.0.0.1", 13854, 300))
            {
                LastStatus = "Inference service is reachable, but ThinkGear Connector TCP 13854 is not listening. Connect the headset and enable the Connector TCP server.";
                return false;
            }

            LastStatus = $"Receiving data: source={emotionPanel.currentSource}, device={emotionPanel.currentDeviceConnected}, stream={emotionPanel.currentStreamConnected}, age={emotionPanel.currentDataAgeSeconds:F1}s, eSenseAge={emotionPanel.currentESenseAgeSeconds:F1}s, poorSignal={emotionPanel.currentPoorSignal}, packets={emotionPanel.currentRawPacketCount}/{emotionPanel.currentESensePacketCount}/{emotionPanel.currentPowerPacketCount}";
            return false;
        }

        public static void RestartServices()
        {
            StopRealtimePython();
            StartServices();
        }

        private static void StartThinkGearConnector()
        {
            if (IsTcpPortOpen("127.0.0.1", 13854, 250))
            {
                LastStatus = "ThinkGear Connector TCP server is already listening";
                return;
            }

            if (connectorProcess != null && !connectorProcess.HasExited)
            {
                LastStatus = "ThinkGear Connector was launched; waiting for TCP 13854. If Windows shows its publisher warning, only run it when you trust this Connector's source.";
                return;
            }

            var existingConnectors = Process.GetProcessesByName("ThinkGear Connector");
            if (existingConnectors.Length > 0)
            {
                connectorProcess = existingConnectors[0];
                ownsConnectorProcess = false;
                LastStatus = $"ThinkGear Connector process is running ({existingConnectors.Length}), but TCP 13854 is not listening. In the Connector, connect the headset and enable the TCP server. No duplicate was launched.";
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

            ownsConnectorProcess = connectorProcess != null;
            LastStatus = connectorProcess != null
                ? "ThinkGear Connector launched. Waiting for TCP 13854; complete any Windows security prompt only if you trust this Connector's source."
                : "Windows did not start ThinkGear Connector.";
            UnityEngine.Debug.Log($"[EEG] {LastStatus}");
        }

        private static void StartRealtimePython()
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                return;
            }

            if (IsPortListening(8765))
            {
                // A receiver that has not connected yet is not evidence of a stale server.
                if (string.IsNullOrEmpty(LastStatus) || LastStatus == "EEG services not started")
                    LastStatus = "Realtime inference service is already running on port 8765.";
                else
                    LastStatus += " Realtime inference is already running on port 8765.";
                return;
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

            if (string.IsNullOrEmpty(LastStatus) || LastStatus == "EEG services not started")
                LastStatus = "Realtime Python started";
            else
                LastStatus += " Realtime Python is running on port 8765.";
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
            // Do not terminate an instance that the user launched independently.
            if (ownsConnectorProcess && connectorProcess != null && !connectorProcess.HasExited)
            {
                TryCloseOrKill(connectorProcess);
            }
            connectorProcess = null;
            ownsConnectorProcess = false;
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
                    // 进程可能已自行退出，这种情况无需警告
                    if (ex.Message.Contains("exited") || ex.Message.Contains("not available"))
                    {
                        UnityEngine.Debug.Log($"[EEG] Process {pid} already exited, skipping.");
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[EEG] Failed to stop process {pid}: {ex.Message}");
                    }
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
