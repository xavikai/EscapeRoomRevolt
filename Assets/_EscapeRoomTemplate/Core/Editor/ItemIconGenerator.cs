using System.Collections.Generic;
using System.IO;
using EscapeRoomRevolt.Systems.Inventory;
using UnityEditor;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>
    /// Renders an inventory icon for every InventoryItemData from its own World Prefab, so the icon
    /// always matches the object the player actually picked up. Writes transparent PNGs and assigns
    /// them back to the asset's Icon field.
    /// </summary>
    public static class ItemIconGenerator
    {
        private const string OutputFolder = "Assets/_EscapeRoomTemplate/Art/ItemIcons";
        private const int IconSize = 256;

        [MenuItem("Escape Room Framework/Maintenance/Generate Missing Item Icons", priority = 420)]
        private static void GenerateMissing() => Generate(false);

        [MenuItem("Escape Room Framework/Maintenance/Regenerate All Item Icons", priority = 421)]
        private static void RegenerateAll()
        {
            bool confirmed = EditorUtility.DisplayDialog("Regenerar totes les icones",
                "Això sobreescriurà les icones ja assignades a tots els objectes d'inventari. Vols continuar?",
                "Sí, regenerar", "Cancel·la");
            if (confirmed) Generate(true);
        }

        private static void Generate(bool overwriteExisting)
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(InventoryItemData));
            if (guids.Length == 0)
            {
                Debug.LogWarning("[Item Icons] No s'ha trobat cap InventoryItemData al projecte.");
                return;
            }

            EnsureFolder(OutputFolder);

            var generated = new List<string>();
            var skippedNoPrefab = new List<string>();
            var skippedHasIcon = new List<string>();

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var data = AssetDatabase.LoadAssetAtPath<InventoryItemData>(path);
                    if (data == null) continue;

                    EditorUtility.DisplayProgressBar("Generant icones d'inventari", data.name, (float)i / guids.Length);

                    // A Unity built-in sprite is a leftover placeholder, not a real icon, so it never
                    // counts as "already has one".
                    bool hasRealIcon = data.Icon != null
                        && !AssetDatabase.GetAssetPath(data.Icon).Contains("unity_builtin");
                    if (!overwriteExisting && hasRealIcon) { skippedHasIcon.Add(data.name); continue; }

                    // Photograph the real model when there is one; otherwise draw a flat icon.
                    Texture2D texture = data.WorldPrefab != null ? RenderPrefab(data.WorldPrefab) : null;
                    if (texture == null) texture = ProceduralItemIcons.Create(data, IconSize);
                    if (texture == null) { skippedNoPrefab.Add(data.name); continue; }

                    string pngPath = $"{OutputFolder}/Icon_{data.name}.png";
                    File.WriteAllBytes(pngPath, texture.EncodeToPNG());
                    Object.DestroyImmediate(texture);

                    AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
                    ConfigureAsSprite(pngPath);

                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
                    if (sprite == null) continue;

                    var so = new SerializedObject(data);
                    so.FindProperty("_icon").objectReferenceValue = sprite;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(data);
                    generated.Add(data.name);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Item Icons] Generades {generated.Count} icones a {OutputFolder}."
                + (generated.Count > 0 ? $" ({string.Join(", ", generated)})" : string.Empty)
                + (skippedHasIcon.Count > 0 ? $"\nJa en tenien ({skippedHasIcon.Count}): {string.Join(", ", skippedHasIcon)}" : string.Empty)
                + (skippedNoPrefab.Count > 0 ? $"\nSense World Prefab utilitzable ({skippedNoPrefab.Count}): {string.Join(", ", skippedNoPrefab)} — assigna'ls una icona a mà." : string.Empty));
        }

        /// <summary>Renders the prefab on a transparent background, framed to its own bounds from a three-quarter angle.</summary>
        private static Texture2D RenderPrefab(GameObject prefab)
        {
            GameObject instance = Object.Instantiate(prefab);
            instance.hideFlags = HideFlags.HideAndDontSave;

            // Strip behaviours so nothing gameplay-related runs while we are only taking a picture.
            foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
                Object.DestroyImmediate(behaviour);

            if (!TryGetBounds(instance, out Bounds bounds))
            {
                Object.DestroyImmediate(instance);
                return null;
            }

            var preview = new PreviewRenderUtility();
            try
            {
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                // Overwritten per pass by RenderOverBackground; the alpha comes from compositing.
                preview.camera.backgroundColor = Color.black;
                preview.camera.orthographic = true;
                preview.camera.nearClipPlane = 0.01f;
                preview.camera.farClipPlane = 100f;

                // 10% margin so the silhouette never touches the icon edge.
                float radius = bounds.extents.magnitude;
                preview.camera.orthographicSize = Mathf.Max(radius * 1.1f, 0.05f);

                Quaternion viewAngle = Quaternion.Euler(25f, 135f, 0f);
                preview.camera.transform.rotation = viewAngle;
                preview.camera.transform.position = bounds.center - viewAngle * Vector3.forward * (radius + 2f);

                preview.lights[0].intensity = 1.4f;
                preview.lights[0].transform.rotation = Quaternion.Euler(35f, 120f, 0f);
                preview.lights[1].intensity = 0.7f;
                preview.lights[1].transform.rotation = Quaternion.Euler(-15f, -70f, 0f);
                preview.ambientColor = new Color(.35f, .35f, .38f, 1f);

                preview.AddSingleGO(instance);

                // BeginStaticPreview returns a texture with no usable alpha, so a single pass would
                // bake an opaque box around every icon. Rendering the same frame over black and over
                // white lets us recover the real coverage: with c = fg*a + bg*(1-a), the difference
                // between the two backgrounds is exactly (1 - a).
                Texture2D overBlack = RenderOverBackground(preview, Color.black);
                Texture2D overWhite = RenderOverBackground(preview, Color.white);
                Texture2D composited = Composite(overBlack, overWhite);
                Object.DestroyImmediate(overBlack);
                Object.DestroyImmediate(overWhite);
                return composited;
            }
            finally
            {
                preview.Cleanup();
                Object.DestroyImmediate(instance);
            }
        }

        private static Texture2D RenderOverBackground(PreviewRenderUtility preview, Color background)
        {
            preview.camera.backgroundColor = background;
            preview.BeginStaticPreview(new Rect(0, 0, IconSize, IconSize));
            preview.camera.Render();
            return preview.EndStaticPreview();
        }

        /// <summary>Recovers per-pixel alpha from the same frame rendered over black and over white, then un-premultiplies the colour so the icon keeps its real tint at the edges.</summary>
        private static Texture2D Composite(Texture2D overBlack, Texture2D overWhite)
        {
            Color[] black = overBlack.GetPixels();
            Color[] white = overWhite.GetPixels();
            var output = new Color[black.Length];

            for (int i = 0; i < black.Length; i++)
            {
                float alpha = Mathf.Clamp01(1f - (white[i].r - black[i].r));
                if (alpha <= 0.002f) { output[i] = Color.clear; continue; }
                Color colour = black[i] / alpha;
                output[i] = new Color(Mathf.Clamp01(colour.r), Mathf.Clamp01(colour.g), Mathf.Clamp01(colour.b), alpha);
            }

            var result = new Texture2D(overBlack.width, overBlack.height, TextureFormat.RGBA32, false);
            result.SetPixels(output);
            result.Apply();
            return result;
        }

        private static bool TryGetBounds(GameObject instance, out Bounds bounds)
        {
            bounds = default;
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(false);
            bool found = false;
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.enabled) continue;
                if (!found) { bounds = renderer.bounds; found = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return found;
        }

        private static void ConfigureAsSprite(string pngPath)
        {
            var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
