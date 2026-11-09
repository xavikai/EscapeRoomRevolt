#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class ReleaseResourceAudit
{
    [Serializable] private sealed class Report
    {
        public string result;
        public int serializedFiles;
        public int uniqueReferencedGuids;
        public int shaders;
        public int loadedAssetObjects;
        public List<string> frameworkMenuCommands = new();
        public List<string> issues = new();
    }
    public static void Run()
    {
        var report = new Report();
        var guids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resolvedGuids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var extensions = new HashSet<string>(new[] { ".unity", ".prefab", ".asset", ".mat", ".controller", ".overrideController", ".renderTexture", ".anim", ".playable", ".meta" }, StringComparer.OrdinalIgnoreCase);
        var reference = new Regex(@"\bguid:\s*([a-fA-F0-9]{32})\b");
        foreach (string root in new[] { "Assets", "Packages/com.xavikai.revolt.common" })
        {
            foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                if (!extensions.Contains(Path.GetExtension(file))) continue;
                string text = File.ReadAllText(file);
                if (text.IndexOf('\0') >= 0) continue; // Binary scenes are checked by ProductModuleValidator.
                report.serializedFiles++;
                foreach (Match match in reference.Matches(text))
                {
                    string guid = match.Groups[1].Value;
                    if (guid.StartsWith("0000000000000000", StringComparison.Ordinal)) continue; // Unity built-in resources.
                    guids.Add(guid);
                    if (!resolvedGuids.TryGetValue(guid, out string asset))
                        resolvedGuids[guid] = asset = AssetDatabase.GUIDToAssetPath(guid);
                    string issue = file + ": unresolved GUID " + guid;
                    if (string.IsNullOrEmpty(asset) && !report.issues.Contains(issue)) report.issues.Add(issue);
                }
            }
        }
        report.uniqueReferencedGuids = guids.Count;
        foreach (string asset in AssetDatabase.GetAllAssetPaths().Where(p =>
            (p.StartsWith("Assets/", StringComparison.Ordinal) || p.StartsWith("Packages/com.xavikai.revolt.common/", StringComparison.Ordinal)) &&
            new[] { ".asset", ".mat", ".controller", ".anim" }.Contains(Path.GetExtension(p))))
        {
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(asset))
            {
                if (obj == null) { report.issues.Add("Cannot load asset object: " + asset); continue; }
                report.loadedAssetObjects++;
                using var serialized = new SerializedObject(obj);
                var property = serialized.GetIterator();
                while (property.Next(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null && property.objectReferenceEntityIdValue != EntityId.None)
                        report.issues.Add(asset + ": broken object reference " + property.propertyPath);
            }
        }
        foreach (string guid in AssetDatabase.FindAssets("t:Shader", new[] { "Assets", "Packages/com.xavikai.revolt.common" }))
        {
            string asset = AssetDatabase.GUIDToAssetPath(guid);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(asset);
            report.shaders++;
            if (shader == null) { report.issues.Add("Cannot load shader: " + asset); continue; }
            foreach (var message in ShaderUtil.GetShaderMessages(shader))
                if (message.severity.ToString() == "Error") report.issues.Add(asset + ": " + message.message);
        }
        foreach (var method in TypeCache.GetMethodsWithAttribute<MenuItem>())
            foreach (var attribute in CustomAttributeData.GetCustomAttributes(method).Where(a => a.AttributeType == typeof(MenuItem)))
            {
                string command = attribute.ConstructorArguments[0].Value as string;
                if (command != null && command.StartsWith("Escape Room Framework/", StringComparison.Ordinal) && !report.frameworkMenuCommands.Contains(command))
                    report.frameworkMenuCommands.Add(command);
            }
        if (report.frameworkMenuCommands.Count == 0) report.issues.Add("The Escape Room Framework Editor menu commands have not been loaded.");
        report.result = report.issues.Count == 0 ? "Passed" : "Failed";
        File.WriteAllText(".utmp/release-resources.json", JsonUtility.ToJson(report, true));
        if (report.issues.Count != 0) throw new InvalidOperationException(string.Join("\n", report.issues));
        Debug.Log($"[Release Resources] PASS: {report.serializedFiles} serialized files, {report.uniqueReferencedGuids} GUIDs and {report.shaders} shaders.");
    }
}
#endif
