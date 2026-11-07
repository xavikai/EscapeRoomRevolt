#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.Management;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Removing loaders also suppresses native XR pre-initialization in desktop builds.</summary>
    public sealed class DesktopXRBuildScope : IDisposable
    {
        private readonly XRGeneralSettings _settings;
        private readonly XRManagerSettings _manager;
        private readonly List<XRLoader> _loaders;
        private readonly bool _initialize, _automaticLoading, _automaticRunning;
        private bool _disposed;

        public DesktopXRBuildScope(XRGeneralSettings settings)
        {
            _settings = settings;
            if (settings == null) return;
            _initialize = settings.InitManagerOnStart;
            _manager = settings.Manager;
            if (_manager != null)
            {
                _loaders = new List<XRLoader>(_manager.activeLoaders);
                _automaticLoading = _manager.automaticLoading;
                _automaticRunning = _manager.automaticRunning;
                if (!_manager.TrySetLoaders(new List<XRLoader>()))
                    throw new InvalidOperationException("Could not isolate desktop build from XR loaders.");
                _manager.automaticLoading = false;
                _manager.automaticRunning = false;
                EditorUtility.SetDirty(_manager);
            }
            settings.InitManagerOnStart = false;
            EditorUtility.SetDirty(settings);
        }

        public void Dispose()
        {
            if (_disposed || _settings == null) return;
            _disposed = true;
            try
            {
                if (_manager != null)
                {
                    if (!_manager.TrySetLoaders(_loaders)) throw new InvalidOperationException("Could not restore authoring XR loaders.");
                    _manager.automaticLoading = _automaticLoading;
                    _manager.automaticRunning = _automaticRunning;
                    EditorUtility.SetDirty(_manager);
                }
            }
            finally
            {
                _settings.InitManagerOnStart = _initialize;
                EditorUtility.SetDirty(_settings);
            }
        }
    }
}
#endif
