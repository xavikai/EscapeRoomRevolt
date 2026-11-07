using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.Systems.Survival;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>
    /// Converts objects built the old way — mesh, collider and gameplay scripts all on one node —
    /// into the framework's Logic/Visuals split, in place. The creators only shape new objects, so
    /// without this every room authored before the split keeps a placeholder cube that cannot be
    /// swapped for a model without dismantling the object by hand.
    ///
    /// The conversion is written to be invisible: the object keeps its position, its scripts, its
    /// references and its exact on-screen size. What changes is where the mesh lives. The logic node
    /// is normalised to scale 1 so that the collider and any board driving it can work in real
    /// units, and everything that hung off the old scale — the mesh, the collider, sibling children —
    /// is compensated so nothing appears to move.
    /// </summary>
    public static class LogicVisualsMigrator
    {
        [MenuItem("Escape Room Framework/Maintenance/Split Selection Into Logic + Visuals", priority = 303)]
        public static void SplitSelection()
        {
            GameObject[] selection = Selection.gameObjects;
            if (selection.Length == 0)
            {
                EditorUtility.DisplayDialog("Split Into Logic + Visuals",
                    "Select the objects to convert first.\n\n"
                    + "Each one keeps its scripts and its place in the scene; its mesh moves to a new "
                    + "'<name>_Visuals' child and it gains a ReplaceableModelSlot, so a modelled mesh "
                    + "can replace the placeholder from the Inspector.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Split Into Logic + Visuals",
                    $"Restructure {selection.Length} selected object(s)?\n\n"
                    + "The mesh moves to a '_Visuals' child, the collider stays on the root and is resized "
                    + "to compensate, and the root is normalised to scale 1.\n\n"
                    + "Objects already split, without a mesh, or part of a prefab instance are skipped. "
                    + "One Undo reverts the whole batch.",
                    "Convert", "Cancel"))
                return;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Split into Logic + Visuals");
            int group = Undo.GetCurrentGroup();

            var report = new StringBuilder();
            int converted = 0, skipped = 0;

            foreach (GameObject target in selection)
            {
                if (Split(target, out string message))
                {
                    converted++;
                    report.Append($"\n  · {target.name}: {message}");
                }
                else
                {
                    skipped++;
                    report.Append($"\n  · {target.name}: skipped — {message}");
                }
            }

            // Boards cache which transform carries the art; that answer just changed.
            foreach (SlidingBoardView view in Object.FindObjectsByType<SlidingBoardView>(FindObjectsInactive.Include))
            {
                view.InvalidatePieces();
                view.ApplyImage();
                view.SnapAll();
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log($"[Escape Room Framework] Split into Logic + Visuals — {converted} converted, {skipped} skipped:{report}");
        }

        /// <summary>
        /// Converts one object in place, preserving its world size, position, scripts and children.
        /// Self-skipping — an object already split, without a mesh, or inside a prefab instance is
        /// reported and left alone — so callers can run it over a whole board without checking first.
        /// </summary>
        internal static bool Split(GameObject target, out string message)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(target))
            {
                message = "part of a prefab instance; unpack it, or convert the prefab itself";
                return false;
            }

            string baseName = target.name.EndsWith("_Logic")
                ? target.name.Substring(0, target.name.Length - "_Logic".Length)
                : target.name;

            // Checked before the mesh, so converting an already-converted object says so plainly
            // instead of reporting the missing mesh it no longer has.
            if (target.transform.Find(baseName + "_Visuals") != null)
            {
                message = "already split";
                return false;
            }

            var filter = target.GetComponent<MeshFilter>();
            var renderer = target.GetComponent<MeshRenderer>();
            if (filter == null || renderer == null)
            {
                message = "no mesh on the root, nothing to move";
                return false;
            }

            Transform root = target.transform;
            Vector3 oldScale = root.localScale;

            // ── The new art node ─────────────────────────────────────────────
            var visuals = new GameObject(baseName + "_Visuals");
            Undo.RegisterCreatedObjectUndo(visuals, "Split into Logic + Visuals");
            visuals.layer = target.layer;
            visuals.transform.SetParent(root, false);

            var newFilter = visuals.AddComponent<MeshFilter>();
            newFilter.sharedMesh = filter.sharedMesh;
            var newRenderer = visuals.AddComponent<MeshRenderer>();
            newRenderer.sharedMaterials = renderer.sharedMaterials;
            newRenderer.shadowCastingMode = renderer.shadowCastingMode;
            newRenderer.receiveShadows = renderer.receiveShadows;
            newRenderer.lightProbeUsage = renderer.lightProbeUsage;
            newRenderer.reflectionProbeUsage = renderer.reflectionProbeUsage;
            newRenderer.motionVectorGenerationMode = renderer.motionVectorGenerationMode;
            newRenderer.allowOcclusionWhenDynamic = renderer.allowOcclusionWhenDynamic;
            GameObjectUtility.SetStaticEditorFlags(visuals, GameObjectUtility.GetStaticEditorFlags(target));

            // ── Normalise the root, compensating everything that rode on its scale ──
            // Captured in world space and restored afterwards, which stays correct even when the old
            // scale was non-uniform and the children were rotated.
            var siblings = new List<Transform>();
            var worldPositions = new List<Vector3>();
            var worldRotations = new List<Quaternion>();
            var worldScales = new List<Vector3>();
            foreach (Transform child in root)
            {
                if (child == visuals.transform) continue;
                siblings.Add(child);
                worldPositions.Add(child.position);
                worldRotations.Add(child.rotation);
                worldScales.Add(child.lossyScale);
            }

            Undo.RecordObject(root, "Split into Logic + Visuals");
            root.localScale = Vector3.one;
            visuals.transform.localScale = oldScale;

            for (int i = 0; i < siblings.Count; i++)
            {
                Undo.RecordObject(siblings[i], "Split into Logic + Visuals");
                siblings[i].SetPositionAndRotation(worldPositions[i], worldRotations[i]);
                Vector3 parentScale = siblings[i].parent.lossyScale;
                siblings[i].localScale = new Vector3(
                    SafeDivide(worldScales[i].x, parentScale.x),
                    SafeDivide(worldScales[i].y, parentScale.y),
                    SafeDivide(worldScales[i].z, parentScale.z));
            }

            string colliderNote = CompensateColliders(target, oldScale);

            // ── Retire the old mesh and declare the piece replaceable ────────
            Undo.DestroyObjectImmediate(filter);
            Undo.DestroyObjectImmediate(renderer);

            if (target.name != baseName + "_Logic")
            {
                Undo.RegisterCompleteObjectUndo(target, "Split into Logic + Visuals");
                target.name = baseName + "_Logic";
            }

            var slot = target.GetComponent<ReplaceableModelSlot>();
            if (slot == null) slot = Undo.AddComponent<ReplaceableModelSlot>(target);
            var slotSo = new SerializedObject(slot);
            slotSo.FindProperty("_placeholderVisual").objectReferenceValue = visuals;
            slotSo.ApplyModifiedProperties();

            message = $"mesh moved to {visuals.name}, root normalised from {oldScale} to 1{colliderNote}";
            return true;
        }

        /// <summary>
        /// Resizes the root's colliders by the scale the root just gave up, so the interaction volume
        /// covers exactly what it covered before. A collider that silently shrank to a third of its
        /// size would make the object almost unclickable, and nothing about the scene would look wrong.
        /// </summary>
        private static string CompensateColliders(GameObject target, Vector3 oldScale)
        {
            float uniform = Mathf.Max(Mathf.Abs(oldScale.x), Mathf.Abs(oldScale.y), Mathf.Abs(oldScale.z));
            var warnings = new List<string>();

            foreach (Collider collider in target.GetComponents<Collider>())
            {
                Undo.RecordObject(collider, "Split into Logic + Visuals");
                switch (collider)
                {
                    case BoxCollider box:
                        box.size = Vector3.Scale(box.size, oldScale);
                        box.center = Vector3.Scale(box.center, oldScale);
                        break;
                    case SphereCollider sphere:
                        // Unity sizes a sphere by the largest scale axis, so that is what it gives up.
                        sphere.radius *= uniform;
                        sphere.center = Vector3.Scale(sphere.center, oldScale);
                        break;
                    case CapsuleCollider capsule:
                        int axis = capsule.direction;
                        float heightScale = Mathf.Abs(axis == 0 ? oldScale.x : axis == 1 ? oldScale.y : oldScale.z);
                        float radiusScale = axis == 0 ? Mathf.Max(Mathf.Abs(oldScale.y), Mathf.Abs(oldScale.z))
                            : axis == 1 ? Mathf.Max(Mathf.Abs(oldScale.x), Mathf.Abs(oldScale.z))
                            : Mathf.Max(Mathf.Abs(oldScale.x), Mathf.Abs(oldScale.y));
                        capsule.height *= heightScale;
                        capsule.radius *= radiusScale;
                        capsule.center = Vector3.Scale(capsule.center, oldScale);
                        break;
                    default:
                        warnings.Add(collider.GetType().Name);
                        break;
                }
            }

            return warnings.Count == 0
                ? string.Empty
                : $" — CHECK: {string.Join(", ", warnings)} cannot be resized in code and is now {oldScale} smaller";
        }

        private static float SafeDivide(float value, float divisor) => Mathf.Approximately(divisor, 0f) ? value : value / divisor;
    }
}
