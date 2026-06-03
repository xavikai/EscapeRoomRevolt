using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using EscapeRoomRevolt.UI.PC;
using TMPro;

namespace EscapeRoomRevolt.Core.Editor
{
    public class InventoryUISetup : MonoBehaviour
    {
        [MenuItem("EscapeRoom/Setup/Auto-Generate Inventory UI")]
        public static void CreateInventoryUI()
        {
            // 1. Find UIManager in the scene
            UIManager uiManager = FindAnyObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("No s'ha trobat cap UIManager a l'escena. Assegura't de tenir el Canvas del Player carregat.");
                return;
            }

            // 2. Get the Canvas
            Canvas canvas = uiManager.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("El UIManager no esta dins de cap Canvas.");
                return;
            }

            // 3. Check if InventoryPanel already exists
            Transform existingPanel = canvas.transform.Find("InventoryPanel");
            if (existingPanel != null)
            {
                Debug.LogWarning("Ja existeix un InventoryPanel. Esborra'l primer si el vols regenerar.");
                return;
            }

            // 4. Create InventoryPanel
            GameObject panelObj = new GameObject("InventoryPanel");
            panelObj.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            // Anchor to Right Edge, Stretch Vertically
            panelRect.anchorMin = new Vector2(1, 0);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 0.5f);
            panelRect.sizeDelta = new Vector2(300, 0); // Width 300
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f); // Dark semi-transparent

            // 5. Create ItemsContainer
            GameObject containerObj = new GameObject("ItemsContainer");
            containerObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            // Stretch inside the panel with some padding
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(-40, -40); // 20px padding
            containerRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = containerObj.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);

            // 6. Create Item Slot Prefab
            string prefabFolder = "Assets/_EscapeRoomTemplate/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(prefabFolder))
            {
                System.IO.Directory.CreateDirectory(prefabFolder);
                AssetDatabase.Refresh();
            }

            string prefabPath = prefabFolder + "/InventorySlot.prefab";
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (slotPrefab == null)
            {
                // Generate the button prefab if it doesn't exist
                GameObject btnObj = new GameObject("InventorySlot");
                RectTransform btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(0, 50); // Height 50

                Image btnImage = btnObj.AddComponent<Image>();
                btnImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

                Button btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = btnImage;
                var colors = btn.colors;
                colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
                colors.pressedColor = new Color(0.1f, 0.1f, 0.1f);
                btn.colors = colors;

                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;

                TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
                tmpText.text = "Item Name";
                tmpText.fontSize = 20;
                tmpText.alignment = TextAlignmentOptions.CenterGeoAligned;
                tmpText.color = Color.white;

                slotPrefab = PrefabUtility.SaveAsPrefabAsset(btnObj, prefabPath);
                DestroyImmediate(btnObj);
            }

            // 7. Add InventoryUI Script
            InventoryUI inventoryUI = panelObj.AddComponent<InventoryUI>();
            SerializedObject soInv = new SerializedObject(inventoryUI);
            soInv.FindProperty("_itemsContainer").objectReferenceValue = containerObj.transform;
            soInv.FindProperty("_itemSlotPrefab").objectReferenceValue = slotPrefab;
            soInv.ApplyModifiedProperties();

            // 8. Link to UIManager
            SerializedObject soUI = new SerializedObject(uiManager);
            soUI.FindProperty("_inventoryPanel").objectReferenceValue = panelObj;
            soUI.ApplyModifiedProperties();

            // 9. Hide panel by default
            panelObj.SetActive(false);

            Debug.Log("<color=green><b>Inventari Generat amb èxit!</b></color> Ja tens la pestanya al lateral dret configurada.");
        }

        [MenuItem("EscapeRoom/Setup/Update Slot with Icon")]
        public static void UpdateSlotWithIcon()
        {
            string prefabPath = "Assets/_EscapeRoomTemplate/Prefabs/UI/InventorySlot.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
            {
                Debug.LogError("No s'ha trobat el prefab a " + prefabPath);
                return;
            }

            // Check if Icon already exists
            Transform existingIcon = prefab.transform.Find("Icon");
            if (existingIcon != null)
            {
                Debug.LogWarning("El prefab ja té una Imatge anomenada 'Icon'!");
                return;
            }

            // Open Prefab for editing
            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            // Create Icon Image
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(contentsRoot.transform, false);
            
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.sizeDelta = new Vector2(40, 40);
            iconRect.anchoredPosition = new Vector2(10, 0); // 10px from left

            iconObj.AddComponent<Image>();

            // Adjust the existing text so it doesn't overlap the icon
            Transform textTransform = contentsRoot.transform.Find("Text");
            if (textTransform != null)
            {
                RectTransform textRect = textTransform.GetComponent<RectTransform>();
                textRect.offsetMin = new Vector2(60, textRect.offsetMin.y); // Push text to the right
            }

            PrefabUtility.SaveAsPrefabAsset(contentsRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(contentsRoot);

            Debug.Log("<color=green><b>Prefab actualitzat amb èxit!</b></color> Ja té la icona llesta.");
        }
    }
}
