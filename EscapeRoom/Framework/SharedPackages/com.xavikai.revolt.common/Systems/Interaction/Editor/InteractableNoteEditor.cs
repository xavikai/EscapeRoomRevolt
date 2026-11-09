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
        private SerializedProperty _cursorType;
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
            _cursorType = serializedObject.FindProperty("_cursorType");
            _saveId = serializedObject.FindProperty("_saveId");
            _enableOutline = serializedObject.FindProperty("_enableOutline");
            _outlineMaterial = serializedObject.FindProperty("_outlineMaterial");
            _highlightRenderers = serializedObject.FindProperty("_highlightRenderers");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Field [Header] attributes already draw the section titles.
            Draw(_interactionPrompt);
            Draw(_canInteract);
            Draw(_cursorType);
            Draw(_saveId);
            EditorGUILayout.Space(5);

            // Draw Visual Feedback (Outline)
            Draw(_enableOutline);
            if (_enableOutline != null && _enableOutline.boolValue)
            {
                // Older InteractableBase versions had these fields; skip them when absent instead of throwing.
                Draw(_outlineMaterial);
                Draw(_highlightRenderers);
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_isPickable, new GUIContent("Is Pickable?"));
            EditorGUILayout.Space(5);

            if (_isPickable.boolValue)
            {
                // Show Pickable properties
                EditorGUILayout.PropertyField(_itemData);
                EditorGUILayout.PropertyField(_quantity);
                EditorGUILayout.PropertyField(_pickupSound);
            }
            else
            {
                // Show Read In-Place properties
                EditorGUILayout.PropertyField(_noteContent);
                EditorGUILayout.PropertyField(_readPrompt);
                EditorGUILayout.PropertyField(_disappearAfterRead);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>PropertyField throws on a missing property; renamed base fields must not hide the note fields below.</summary>
        private static void Draw(SerializedProperty property)
        {
            if (property != null) EditorGUILayout.PropertyField(property);
        }
    }
}
#endif
