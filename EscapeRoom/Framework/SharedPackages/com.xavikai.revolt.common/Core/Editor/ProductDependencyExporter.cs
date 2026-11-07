#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    public static class ProductDependencyExporter
    {
        [Serializable] private sealed class Entry { public string asset; public string[] dependencies; }
        [Serializable] private sealed class Graph { public List<Entry> entries = new(); }

        // Run against the authoring project before creating new consumers; binary assets stay untouched.
        public static void Export()
        {
            var graph = new Graph();
            foreach (string asset in AssetDatabase.GetAllAssetPaths())
            {
                if (!asset.StartsWith("Assets/") || Directory.Exists(asset)) continue;
                graph.entries.Add(new Entry { asset = asset, dependencies = AssetDatabase.GetDependencies(asset, false) });
            }
            Directory.CreateDirectory("tools");
            File.WriteAllText("tools/product-asset-dependencies.json", JsonUtility.ToJson(graph, true));
            Debug.Log($"[Products] Exported dependencies for {graph.entries.Count} assets, including binary scenes.");
        }
    }
}
#endif
