using UnityEngine;
using UnityEditor;
using EscapeRoomRevolt.Systems.Survival;

namespace EscapeRoomRevolt.EditorTools
{
    public static class SurvivalInteractableCreator
    {
        [MenuItem("Escape Room Framework/Create/Survival/Hiding Spot", priority = 401)]
        public static void CreateHidingSpot()
        {
            GameObject logicObj = new GameObject("NewHidingSpot");
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) logicObj.transform.position = view.pivot;

            BoxCollider trigger = logicObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(.7f, 1.9f, .7f);
            trigger.center = new Vector3(0f, .95f, 0f);
            logicObj.AddComponent<UnityEngine.AI.NavMeshObstacle>().carving = true;

            GameObject insideAnchor = new GameObject("InsideAnchor");
            insideAnchor.transform.SetParent(logicObj.transform, false);
            insideAnchor.transform.localPosition = new Vector3(0f, 0f, .1f);

            GameObject exitAnchor = new GameObject("ExitAnchor");
            exitAnchor.transform.SetParent(logicObj.transform, false);
            exitAnchor.transform.localPosition = new Vector3(0f, 0f, 1f);

            GameObject inspectionAnchor = new GameObject("InspectionAnchor");
            inspectionAnchor.transform.SetParent(logicObj.transform, false);
            inspectionAnchor.transform.localPosition = new Vector3(0f, 1.4f, .7f);

            GameObject modelSocket = new GameObject("ModelSocket");
            modelSocket.transform.SetParent(logicObj.transform, false);

            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = "Placeholder_ReplaceMe";
            placeholder.transform.SetParent(modelSocket.transform, false);
            placeholder.transform.localPosition = new Vector3(0f, .95f, 0f);
            placeholder.transform.localScale = new Vector3(.7f, 1.9f, .7f);
            Object.DestroyImmediate(placeholder.GetComponent<Collider>());
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader != null)
            {
                Material mat = new Material(urpShader) { color = new Color(.3f, .22f, .15f) };
                placeholder.GetComponent<Renderer>().material = mat;
            }

            HidingSpot hidingSpot = logicObj.AddComponent<HidingSpot>();
            SerializedObject hidingSo = new SerializedObject(hidingSpot);
            hidingSo.FindProperty("_insideAnchor").objectReferenceValue = insideAnchor.transform;
            hidingSo.FindProperty("_exitAnchor").objectReferenceValue = exitAnchor.transform;
            hidingSo.FindProperty("_inspectionAnchor").objectReferenceValue = inspectionAnchor.transform;
            hidingSo.ApplyModifiedProperties();

            ReplaceableModelSlot modelSlot = logicObj.AddComponent<ReplaceableModelSlot>();
            SerializedObject slotSo = new SerializedObject(modelSlot);
            slotSo.FindProperty("_modelSocket").objectReferenceValue = modelSocket.transform;
            slotSo.FindProperty("_placeholderVisual").objectReferenceValue = placeholder;
            slotSo.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(logicObj, "Create " + logicObj.name);
            Selection.activeGameObject = logicObj;
            Debug.Log("[Escape Room Framework] Hiding Spot created (defaults to Kind = Locker; change it in the Inspector for a bed/container/custom spot). "
                + "Resize the trigger and reposition InsideAnchor/ExitAnchor/InspectionAnchor to match your model, then assign a replacement model prefab on ReplaceableModelSlot.");
        }

    }
}
