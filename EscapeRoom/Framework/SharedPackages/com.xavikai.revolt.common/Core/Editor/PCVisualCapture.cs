#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Graphics-enabled batch diagnostic: render real runtime scenes at reproducible viewpoints.</summary>
    public static class PCVisualCapture
    {
        [Serializable] private sealed class View { public string scene; public string subject; public string image; public Vector3 position; public Vector3 target; }
        [Serializable] private sealed class Report { public string graphicsDevice; public List<View> views = new(); public List<string> errors = new(); }
        private static Report _report;
        private static string _folder;
        private static string[] _scenes;
        private static int _sceneIndex, _wait;
        private static Queue<MonoBehaviour> _subjects;
        private static bool _spawnCaptured, _changing;
        private static bool _oldOptionsEnabled;
        private static EnterPlayModeOptions _oldOptions;
        private static UnityEngine.XR.Management.XRGeneralSettings _xrSettings;
        private static bool _oldXRInitialize;

        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Run this diagnostic in a separate graphics-enabled Unity batch process.");
            _report = new Report { graphicsDevice = SystemInfo.graphicsDeviceType.ToString() };
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) throw new InvalidOperationException("Visual QA requires a graphics device; omit -nographics.");
            var product = ProductConfiguration.Current;
            _folder = Path.GetFullPath(".utmp/visual-captures");
            Directory.CreateDirectory(_folder);
            _scenes = product.DesktopScenes.Where(scene => !new[] { "Intro", "MainMenu" }.Contains(Path.GetFileNameWithoutExtension(scene))).ToArray();
            _sceneIndex = 0;
            _oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            _oldOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            _xrSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (_xrSettings != null) { _oldXRInitialize = _xrSettings.InitManagerOnStart; _xrSettings.InitManagerOnStart = false; }
            Application.logMessageReceived += HandleLog;
            EditorSceneManager.OpenScene(_scenes[0], OpenSceneMode.Single);
            _wait = 20;
            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        private static void HandleLog(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
                if (!_report.errors.Contains(message)) _report.errors.Add(message);
        }

        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying) return;
                if (SceneManager.GetActiveScene().path != _scenes[_sceneIndex]) return;
                if (_wait-- > 0) return;
                Time.timeScale = 0f;
                if (!_spawnCaptured)
                {
                    Camera camera = Camera.main;
                    if (camera == null) throw new InvalidOperationException("No main camera in " + _scenes[_sceneIndex]);
                    Capture(camera, "spawn", camera.transform.position + camera.transform.forward * 5f);
                    string[] names = { "HorrorEnemyController", "HidingSpot", "TraversalObstacle", "SurvivalPowerConsole", "RecordableEvidence", "SurvivalCheckpoint" };
                    _subjects = new Queue<MonoBehaviour>(Object.FindObjectsByType<MonoBehaviour>()
                        .Where(component => component.gameObject.scene == SceneManager.GetActiveScene() && component.gameObject.activeInHierarchy
                            && (component is PuzzleController || names.Contains(component.GetType().Name)))
                        .OrderBy(component => component.transform.position.z).ThenBy(component => component.name));
                    _spawnCaptured = true;
                    _wait = 3;
                    return;
                }
                if (_subjects.Count > 0)
                {
                    var subject = _subjects.Dequeue();
                    if (subject == null) return;
                    Camera camera = Camera.main;
                    Vector3 target = subject.GetComponent<Collider>() != null ? subject.GetComponent<Collider>().bounds.center : subject.transform.position + Vector3.up;
                    // Museum subjects face the central aisle. Other demos use a nearby overview.
                    Vector3 position = SceneManager.GetActiveScene().name.StartsWith("Showcase")
                        ? new Vector3(0f, 1.7f, target.z - 2f)
                        : target + new Vector3(0f, 1f, -3f);
                    camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
                    Capture(camera, subject.name, target);
                    _wait = 3;
                    return;
                }
                if (_sceneIndex + 1 < _scenes.Length)
                {
                    _sceneIndex++;
                    _spawnCaptured = false;
                    _wait = 20;
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(_scenes[_sceneIndex]);
                    return;
                }
                Finish();
            }
            catch (Exception exception) { _report.errors.Add(exception.ToString()); Finish(); }
        }

        private static void Capture(Camera camera, string subject, Vector3 target)
        {
            string filename = SceneManager.GetActiveScene().name + "-" + _report.views.Count.ToString("00") + "-" + string.Concat(subject.Select(c => char.IsLetterOrDigit(c) ? c : '_')) + ".png";
            RenderTexture texture = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = texture });
                RenderTexture.active = texture;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(Path.Combine(_folder, filename), pixels.EncodeToPNG());
                _report.views.Add(new View { scene = SceneManager.GetActiveScene().name, subject = subject, image = filename, position = camera.transform.position, target = target });
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(texture); Object.DestroyImmediate(pixels); }
        }

        private static void Finish()
        {
            if (_changing) return;
            _changing = true;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= HandleLog;
            Time.timeScale = 1f;
            File.WriteAllText(Path.Combine(_folder, "report.json"), JsonUtility.ToJson(_report, true));
            EditorApplication.isPlaying = false;
            EditorApplication.update += ExitAfterPlayMode;
        }

        private static void ExitAfterPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying) return;
            EditorApplication.update -= ExitAfterPlayMode;
            if (_xrSettings != null) _xrSettings.InitManagerOnStart = _oldXRInitialize;
            EditorSettings.enterPlayModeOptionsEnabled = _oldOptionsEnabled;
            EditorSettings.enterPlayModeOptions = _oldOptions;
            EditorApplication.Exit(_report.errors.Count == 0 ? 0 : 1);
        }
    }
}
#endif
