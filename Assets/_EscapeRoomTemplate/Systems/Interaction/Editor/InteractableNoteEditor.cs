#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction.Editor
{
    [CustomEditor(typeof(InteractableNote))]
    public class InteractableNoteEditor : UnityEditor.Editor
    {
        private SerializedProperty _isPickable;
        private SerializedProperty _noteContent;
        private SerializedProperty _readPrompt;
        private SerializedProperty _disappearAfterRead;
        private SerializedProperty _itemData;
        private SerializedProperty _quantity;
        private SerializedProperty _pickupSound;

        // Base class properties (InteractableBase)
        private SerializedProperty _interactionPrompt;
        private SerializedProperty _canInteract;
        private SerializedProperty _saveId;
        private SerializedProperty _enableOutline;
        private SerializedProperty _outlineMaterial;
        private SerializedProperty _highlightRenderers;

        private void OnEnable()
        {
            _isPickable = serializedObject.FindProperty("IsPickable");
            _noteContent = serializedObject.FindProperty("NoteContent");
            _readPrompt = serializedObject.FindProperty("ReadPrompt");
            _disappearAfterRead = serializedObject.FindProperty("DisappearAfterRead");
            
            _itemData = serializedObject.FindProperty("ItemData");
            _quantity = serializedObject.FindProperty("Quantity");
            _pickupSound = serializedObject.FindProperty("PickupSound");

            // Find base class properties
            _interactionPrompt = serializedObject.FindProperty("_interactionPrompt");
            _canInteract = serializedObject.FindProperty("_canInteract");
            _saveId = serializedObject.FindProperty("_saveId");
            _enableOutline = serializedObject.FindProperty("_enableOutline");
            _outlineMaterial = serializedObject.FindProperty("_outlineMaterial");
            _highlightRenderers = serializedObject.FindProperty("_highlightRenderers");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw Base Interaction Settings
            EditorGUILayout.LabelField("Interaction Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_interactionPrompt);
            EditorGUILayout.PropertyField(_canInteract);
            if (_saveId != null) EditorGUILayout.PropertyField(_saveId);
            EditorGUILayout.Space(5);

            // Draw Visual Feedback (Outline)
            EditorGUILayout.LabelField("Visual Feedback (Outline)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_enableOutline);
            if (_enableOutline.boolValue)
            {
                EditorGUILayout.PropertyField(_outlineMaterial);
                EditorGUILayout.PropertyField(_highlightRenderers);
            }
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Note Behaviour", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_isPickable, new GUIContent("Is Pickable?"));
            EditorGUILayout.Space(5);

            if (_isPickable.boolValue)
            {
                // Show Pickable properties
                EditorGUILayout.LabelField("Pickable Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_itemData);
                EditorGUILayout.PropertyField(_quantity);
                EditorGUILayout.PropertyField(_pickupSound);
            }
            else
            {
                // Show Read In-Place properties
                EditorGUILayout.LabelField("Read In-Place Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_noteContent);
                EditorGUILayout.PropertyField(_readPrompt);
                EditorGUILayout.PropertyField(_disappearAfterRead);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
