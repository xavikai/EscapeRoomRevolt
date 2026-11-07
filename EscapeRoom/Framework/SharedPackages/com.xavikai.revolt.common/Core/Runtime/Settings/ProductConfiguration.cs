using System.IO;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Settings
{
    /// <summary>Owned by a consumer project, never shipped as shared package state.</summary>
    [CreateAssetMenu(fileName = "ProductConfiguration", menuName = "Escape Room Framework/Configuration/Product")]
    public sealed class ProductConfiguration : ScriptableObject
    {
        [SerializeField] private string _productId;
        [SerializeField] private string _displayName;
        [SerializeField] private string _applicationIdentifier;
        [SerializeField] private string[] _desktopScenes = System.Array.Empty<string>();
        [SerializeField] private string[] _questScenes = System.Array.Empty<string>();
        public string ProductId => _productId;
        public string DisplayName => _displayName;
        public string ApplicationIdentifier => _applicationIdentifier;
        public string[] DesktopScenes => _desktopScenes;
        public string[] QuestScenes => _questScenes;

        private static ProductConfiguration _current;
        private static bool _loaded;
        public static ProductConfiguration Current
        {
            get
            {
                if (!_loaded) { _current = Resources.Load<ProductConfiguration>("ProductConfiguration"); _loaded = true; }
                return _current;
            }
        }

        // Legacy authoring project retains its existing save location when no product is configured.
        public static string PersistentDataDirectory => Current != null && !string.IsNullOrWhiteSpace(Current.ProductId)
            ? Path.Combine(Application.persistentDataPath, Current.ProductId)
            : Application.persistentDataPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { _current = null; _loaded = false; }
    }
}
