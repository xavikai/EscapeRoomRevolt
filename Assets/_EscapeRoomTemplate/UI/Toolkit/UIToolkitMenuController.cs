using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Core.Localization;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using static EscapeRoomRevolt.Core.Localization.LocalizationService;
using EscapeRoomRevolt.Systems.Survival;
using UnityEngine;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.UI.Toolkit
{
    public enum MenuScreen { Hidden, Main, Pause, Settings, Save, Load, Credits, Results, Confirmation }

    [RequireComponent(typeof(UIDocument))]
    public sealed class UIToolkitMenuController : MonoBehaviour
    {
        public static UIToolkitMenuController Instance { get; private set; }
        public bool IsBlockingGameplay => _screen != MenuScreen.Hidden;

        [Tooltip("Optional re-skin (colors, fonts, logo). Leave empty to use EscapeRoomMenu.uss as authored.")]
        [SerializeField] private MenuThemeSettings _theme;

        private static readonly Color HighContrastBackground = Color.black;
        private static readonly Color HighContrastAccent = new Color(1f, 0.86f, 0f);
        private static readonly Color HighContrastText = Color.white;
        private static readonly Color HighContrastButtonBackground = Color.black;
        private static readonly Color HighContrastButtonBackgroundHover = new Color(0.18f, 0.18f, 0.18f);

        private bool IsHighContrastEnabled =>
            GameSettingsService.Instance != null && GameSettingsService.Instance.Data.highContrastMode;

        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _content;
        private Label _title;
        private MenuScreen _screen;
        private MenuScreen _backScreen = MenuScreen.Pause;
        private Camera _runtimeMenuCamera;
        private readonly List<Texture2D> _runtimePreviews = new List<Texture2D>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _document = GetComponent<UIDocument>();
            EnsureMenuServices();
            EnsureRenderCamera();
        }

        private void OnEnable()
        {
            _root = _document.rootVisualElement;
            _title = _root.Q<Label>("title");
            _content = _root.Q<VisualElement>("screen-content");
            ApplyStructuralTheme();
            GameFlowManager flow = GameFlowManager.EnsureInstance();
            flow.StateChanged += HandleFlowStateChanged;
            flow.GameEnded += ShowResults;
            if (flow.IsMainMenuScene) ShowMain(); else Hide();
        }

        private void OnDestroy()
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.StateChanged -= HandleFlowStateChanged;
                GameFlowManager.Instance.GameEnded -= ShowResults;
            }
            ReleasePreviews();
            InputRouter.Instance?.CancelInteractiveRebind();
            Time.timeScale = 1f;
            if (_runtimeMenuCamera != null) Destroy(_runtimeMenuCamera.gameObject);
            if (Instance == this) Instance = null;
        }

        public void TogglePause()
        {
            if (_screen == MenuScreen.Main || _screen == MenuScreen.Results) return;
            if (_screen == MenuScreen.Hidden) ShowPause(); else Hide();
        }

        public void ShowMain()
        {
            Time.timeScale = 1f;
            Show(MenuScreen.Main, "EXPEDIENTE DE INVESTIGACIÓN");
            GameFlowManager flow = GameFlowManager.EnsureInstance();
            Button continueButton = AddButton(Tr("Continuar"), flow.ContinueGame);
            continueButton.SetEnabled(flow.CanContinue());
            AddButton(Tr("Nueva partida"), () =>
            {
                if (SaveManager.Instance != null && SaveManager.Instance.GetSlots().Count > 0)
                    ShowConfirmation("¿Iniciar una nueva investigación? Las partidas guardadas no se borrarán.", flow.StartNewGame, ShowMain);
                else flow.StartNewGame();
            });
            AddButton(Tr("Cargar partida"), () => { _backScreen = MenuScreen.Main; ShowLoad(); });
            AddButton(Tr("Ajustes"), () => { _backScreen = MenuScreen.Main; ShowSettings(); });
            AddButton(Tr("Créditos"), ShowCredits, "menu-button menu-button--quiet");
            AddButton(Tr("Salir"), () => ShowConfirmation("¿Salir del juego?", flow.QuitGame, ShowMain), "menu-button menu-button--quiet");
        }

        public void ShowPause()
        {
            GameFlowManager.EnsureInstance().SetPaused(true);
            _backScreen = MenuScreen.Pause;
            Show(MenuScreen.Pause, "ARCHIVO DE INCIDENTE");
            AddButton(Tr("Reanudar investigación"), Hide);
            Button saveButton = AddButton(Tr("Guardar partida"), ShowSave);
            saveButton.SetEnabled(SurvivalDifficultyService.AllowsManualSaving);
            AddButton(Tr("Cargar partida"), ShowLoad);
            AddButton(Tr("Ajustes"), ShowSettings);
            AddButton(Tr("Menú principal…"), () => ShowConfirmation(
                "VOLVER AL MENÚ PRINCIPAL",
                "La escena del menú principal se cargará al confirmar. El progreso que no hayas guardado se perderá.",
                GameFlowManager.EnsureInstance().ReturnToMainMenu,
                ShowPause,
                "VOLVER AL MENÚ"), "menu-button menu-button--quiet");
            AddButton(Tr("Salir"), () => ShowConfirmation("¿Salir del juego?", GameFlowManager.EnsureInstance().QuitGame, ShowPause), "menu-button menu-button--quiet");
        }

        public void ShowSave()
        {
            Show(MenuScreen.Save, "ARCHIVAR INVESTIGACIÓN");
            if (GameFeatures.IsEnabled(OptionalGameFeature.PlayerVitals) && !SurvivalDifficultyService.AllowsManualSaving)
            {
                AddStatus("El guardado manual está desactivado por la dificultad activa.");
                AddButton("Volver", NavigateBack, "menu-button menu-button--quiet");
                return;
            }
            BuildSlots(saveMode: true);
            AddButton("Volver", NavigateBack, "menu-button menu-button--quiet");
        }

        public void ShowLoad()
        {
            Show(MenuScreen.Load, "CINTAS DE SEGURIDAD");
            BuildSlots(saveMode: false);
            AddButton("Volver", NavigateBack, "menu-button menu-button--quiet");
        }

        public void ShowSettings()
        {
            Show(MenuScreen.Settings, "PANEL DE MANTENIMIENTO");
            var settings = GameSettingsService.Instance?.Data ?? new GameSettingsData();
            AddLanguageSelector(settings);
            AddSlider("Volumen maestro", settings.masterVolume, value => { settings.masterVolume = value; SaveSettings(settings); });
            AddSlider("Volumen de música", settings.musicVolume, value => { settings.musicVolume = value; SaveSettings(settings); });
            AddSlider("Volumen de efectos", settings.sfxVolume, value => { settings.sfxVolume = value; SaveSettings(settings); });
            AddSlider("Sensibilidad", settings.mouseSensitivity, value => { settings.mouseSensitivity = value; SaveSettings(settings); }, 0.1f, 3f);
            AddQualitySelector(settings);
            AddToggle("Pantalla completa", settings.fullscreen, value => { settings.fullscreen = value; SaveSettings(settings); });
            AddToggle("Subtítulos", settings.subtitles, value => { settings.subtitles = value; SaveSettings(settings); });
            AddToggle("Reducir destellos", settings.reduceFlashes, value => { settings.reduceFlashes = value; SaveSettings(settings); });
            AddToggle("Reducir temblor de cámara", settings.reduceScreenShake, value => { settings.reduceScreenShake = value; SaveSettings(settings); });
            AddToggle("Reducir sonidos fuertes", settings.reduceLoudSounds, value => { settings.reduceLoudSounds = value; SaveSettings(settings); });
            AddToggle("Reducir gore", settings.reduceGore, value => { settings.reduceGore = value; SaveSettings(settings); });
            AddToggle("Reducir balanceo de cámara", settings.reduceHeadBob, value => { settings.reduceHeadBob = value; SaveSettings(settings); });
            AddToggle("Modo de alto contraste", settings.highContrastMode, value => { settings.highContrastMode = value; SaveSettings(settings); ShowSettings(); });
            if (GameFeatures.IsEnabled(OptionalGameFeature.EnemyAI))
                AddToggle("Asistencia en persecuciones", settings.chaseAssistance, value => { settings.chaseAssistance = value; SaveSettings(settings); });
            if (GameFeatures.IsEnabled(OptionalGameFeature.PlayerVitals)) AddDifficultySelector();
            AddSectionLabel("CONTROLES");
            AddRebind("Avanzar", "Move", "up", settings);
            AddRebind("Retroceder", "Move", "down", settings);
            AddRebind("Moverse a la izquierda", "Move", "left", settings);
            AddRebind("Moverse a la derecha", "Move", "right", settings);
            AddRebind("Interactuar", "Interact", null, settings);
            AddRebind("Correr", "Sprint", null, settings);
            AddRebind("Agacharse", "Crouch", null, settings);
            AddRebind("Saltar", "Jump", null, settings);
            AddRebind("Inventario", "Inventory", null, settings);
            if (GameFeatures.IsEnabled(OptionalGameFeature.AdvancedEvasion))
            {
                AddRebind("Inclinarse (mantener + A/D)", "LeanModifier", null, settings);
                AddRebind("Mirar atrás", "LookBack", null, settings);
                AddRebind("Deslizarse", "Slide", null, settings);
            }
            if (GameFeatures.IsEnabled(OptionalGameFeature.AdvancedDoors))
                AddRebind("Interacción cuidadosa (mantener)", "CarefulInteractModifier", null, settings);
            if (GameFeatures.IsEnabled(OptionalGameFeature.Flashlight))
            {
                AddRebind("Linterna", "ToggleFlashlight", null, settings);
                AddRebind("Recargar", "Reload", null, settings);
            }
            if (GameFeatures.IsEnabled(OptionalGameFeature.NightVision))
            {
                AddRebind("Subir/bajar cámara", "ToggleCamcorder", null, settings);
                AddRebind("Visión nocturna", "ToggleNightVision", null, settings);
                AddRebind("Batería de cámara", "ReloadCamcorder", null, settings);
                AddRebind("Zoom de cámara", "CamcorderZoom", null, settings);
            }
            if (GameFeatures.IsEnabled(OptionalGameFeature.EvidenceRecording))
                AddRebind("Grabar evidencia", "RecordEvidence", null, settings);
            AddRebind("Soltar equipado", "DropEquipped", null, settings);
            AddRebind("Pista", "Hint", null, settings);
            AddButton("Restablecer controles", () =>
            {
                InputRouter.Instance?.ResetBindingOverrides();
                settings.bindingOverridesJson = string.Empty;
                SaveSettings(settings);
                ShowSettings();
            }, "menu-button menu-button--quiet");
            AddButton("Volver", NavigateBack, "menu-button menu-button--quiet");
        }

        public void ShowCredits()
        {
            _backScreen = MenuScreen.Main;
            Show(MenuScreen.Credits, "CRÉDITOS");
            AddLabel("Añade aquí los créditos de tu proyecto y las licencias de terceros.");
            AddButton("Volver", ShowMain, "menu-button menu-button--quiet");
        }

        public void ShowResults(GameResult result)
        {
            Show(MenuScreen.Results, result.Title);
            var message = new Label(result.Message);
            message.AddToClassList("confirmation-message");
            _content.Add(message);
            AddButton("Reintentar", GameFlowManager.EnsureInstance().RestartCurrentScene);
            AddButton("Menú principal", GameFlowManager.EnsureInstance().ReturnToMainMenu);
            AddButton("Salir", GameFlowManager.EnsureInstance().QuitGame, "menu-button menu-button--quiet");
        }

        public void Hide()
        {
            if (GameFlowManager.Instance != null && GameFlowManager.State == GameFlowState.Paused)
                GameFlowManager.Instance.SetPaused(false);
            _screen = MenuScreen.Hidden;
            Time.timeScale = 1f;
            if (_root != null) _root.style.display = DisplayStyle.None;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        /// <summary>
        /// titleKey is looked up via LocalizationService.Tr — by this project's convention the key
        /// is the original Spanish text, so passing a literal here still displays correctly even
        /// before every call site has been migrated to a stable key.
        /// </summary>
        private void Show(MenuScreen screen, string titleKey)
        {
            _screen = screen;
            _root.style.display = DisplayStyle.Flex;
            _title.text = Tr(titleKey);
            ApplyStructuralTheme();
            ReleasePreviews();
            _content.Clear();
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        private Button AddButton(string text, Action clicked, string classes = "menu-button")
        {
            var button = new Button(clicked) { text = text };
            foreach (string className in classes.Split(' '))
                if (!string.IsNullOrWhiteSpace(className)) button.AddToClassList(className);
            ApplyButtonTheme(button);
            _content.Add(button);
            return button;
        }

        /// <summary>
        /// Applies MenuThemeSettings (if assigned) to the panel frame, title, logo and body font.
        /// Called once per document since none of those elements are rebuilt between screens.
        /// Buttons are themed individually as they're created — see ApplyButtonTheme.
        /// </summary>
        /// <summary>
        /// Applies the active theme — a fixed high-contrast palette if the player enabled it in
        /// Settings, else the optional MenuThemeSettings asset, else neither (USS defaults stand).
        /// High contrast always wins over MenuThemeSettings: accessibility overrides branding.
        /// Safe to call with _theme == null as long as highContrast is true whenever it is, which
        /// the early-out below guarantees — the ternaries then never evaluate the _theme branch.
        /// </summary>
        private void ApplyStructuralTheme()
        {
            bool highContrast = IsHighContrastEnabled;
            if (_theme == null && !highContrast) return;

            VisualElement frame = _root.Q<VisualElement>("document-frame");
            if (frame != null)
            {
                Color background = highContrast ? HighContrastBackground : _theme.panelBackground;
                Color accent = highContrast ? HighContrastAccent : _theme.accent;
                frame.style.backgroundColor = background;
                frame.style.borderTopColor = accent;
                frame.style.borderBottomColor = accent;
                frame.style.borderLeftColor = accent;
                frame.style.borderRightColor = accent;
            }

            if (_title != null)
            {
                _title.style.color = highContrast ? HighContrastText : _theme.titleText;
                Font titleFont = highContrast ? null : _theme.titleFont;
                if (titleFont != null)
                    _title.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(titleFont));
            }

            Font bodyFont = highContrast ? null : _theme?.bodyFont;
            if (bodyFont != null && _content != null)
                _content.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(bodyFont));

            // Decorative branding competes with legibility, so high contrast hides the logo.
            Image logo = _root.Q<Image>("logo");
            if (logo != null)
            {
                Sprite sprite = highContrast ? null : _theme?.logo;
                logo.style.display = sprite != null ? DisplayStyle.Flex : DisplayStyle.None;
                logo.sprite = sprite;
            }
        }

        /// <summary>
        /// Inline styles beat USS regardless of pseudo-class, so once a themed background is
        /// applied the ".menu-button:hover" rule can no longer show through — restore the hover
        /// swap manually here so a themed menu still has hover feedback.
        /// </summary>
        private void ApplyButtonTheme(Button button)
        {
            bool highContrast = IsHighContrastEnabled;
            if (_theme == null && !highContrast) return;

            Color background = highContrast ? HighContrastButtonBackground : _theme.buttonBackground;
            Color hoverBackground = highContrast ? HighContrastButtonBackgroundHover : _theme.buttonBackgroundHover;
            button.style.backgroundColor = background;
            button.style.color = highContrast ? HighContrastText : _theme.buttonText;
            Font font = highContrast ? null : _theme?.bodyFont;
            if (font != null)
                button.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(font));
            if (highContrast)
            {
                button.style.borderLeftColor = HighContrastAccent;
                button.style.borderLeftWidth = 3;
            }
            button.RegisterCallback<MouseEnterEvent>(_ => button.style.backgroundColor = hoverBackground);
            button.RegisterCallback<MouseLeaveEvent>(_ => button.style.backgroundColor = background);
        }

        private void BuildSlots(bool saveMode)
        {
            SaveManager manager = SaveManager.Instance;
            if (manager == null)
            {
                AddLabel("El sistema de archivo no está disponible.");
                return;
            }

            Dictionary<string, SaveSlotMetadata> slots = manager.GetSlots()
                .Where(slot => slot.slotId.StartsWith("slot_", StringComparison.Ordinal))
                .ToDictionary(slot => slot.slotId, slot => slot);

            for (int index = 1; index <= SaveManager.ManualSlotCount; index++)
            {
                string slotId = SaveManager.GetManualSlotId(index);
                slots.TryGetValue(slotId, out SaveSlotMetadata metadata);
                BuildSlotCard(index, slotId, metadata, saveMode);
            }
        }

        private void BuildSlotCard(int index, string slotId, SaveSlotMetadata metadata, bool saveMode)
        {
            var card = new VisualElement();
            card.AddToClassList("save-slot-card");

            Image preview = CreatePreview(slotId, metadata != null);
            card.Add(preview);

            var information = new VisualElement();
            information.AddToClassList("save-slot-info");
            var heading = new Label($"EXPEDIENTE {index:00}");
            heading.AddToClassList("save-slot-title");
            information.Add(heading);

            if (metadata == null)
            {
                var empty = new Label("SIN REGISTRO");
                empty.AddToClassList("save-slot-empty");
                information.Add(empty);
            }
            else
            {
                information.Add(CreateMetaLabel(string.IsNullOrEmpty(metadata.sceneName) ? "Escena desconocida" : metadata.sceneName));
                information.Add(CreateMetaLabel(FormatDate(metadata.savedAtUtc)));
                information.Add(CreateMetaLabel($"TIEMPO  {FormatDuration(metadata.playTimeSeconds)}"));
            }
            card.Add(information);

            var actions = new VisualElement();
            actions.AddToClassList("save-slot-actions");
            if (saveMode)
            {
                string label = metadata == null ? "GUARDAR" : "SOBRESCRIBIR";
                actions.Add(CreateActionButton(label, () =>
                {
                    if (metadata == null) SaveToSlot(slotId);
                    else ShowConfirmation("¿Sobrescribir este expediente?", () => SaveToSlot(slotId), ShowSave);
                }, false));
            }
            else
            {
                Button load = CreateActionButton("CARGAR", () => LoadSlot(slotId), false);
                load.SetEnabled(metadata != null);
                actions.Add(load);
            }

            Button delete = CreateActionButton("BORRAR", () => ShowConfirmation(
                "¿Eliminar definitivamente este expediente?", () => DeleteSlot(slotId, saveMode), saveMode ? ShowSave : ShowLoad), true);
            delete.SetEnabled(metadata != null);
            actions.Add(delete);
            card.Add(actions);
            _content.Add(card);
        }

        private Image CreatePreview(string slotId, bool hasSave)
        {
            var image = new Image { scaleMode = ScaleMode.ScaleAndCrop };
            image.AddToClassList("save-slot-preview");
            if (!hasSave || SaveManager.Instance == null) return image;

            string path = SaveManager.Instance.GetThumbnailPath(slotId);
            if (!File.Exists(path)) return image;
            try
            {
                var texture = new Texture2D(2, 2, TextureFormat.RGB24, false) { name = $"Preview_{slotId}" };
                if (texture.LoadImage(File.ReadAllBytes(path)))
                {
                    _runtimePreviews.Add(texture);
                    image.image = texture;
                }
                else Destroy(texture);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Menu] No se pudo cargar la miniatura '{slotId}': {exception.Message}");
            }
            return image;
        }

        private static Label CreateMetaLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("save-slot-meta");
            return label;
        }

        private Button CreateActionButton(string text, Action clicked, bool danger)
        {
            var button = new Button(clicked) { text = text };
            button.AddToClassList("slot-action");
            if (danger) button.AddToClassList("slot-action--danger");
            ApplyButtonTheme(button);
            return button;
        }

        private void SaveToSlot(string slotId)
        {
            StartCoroutine(SaveToSlotRoutine(slotId));
        }

        private IEnumerator SaveToSlotRoutine(string slotId)
        {
            // Hide the dossier briefly so the thumbnail records the game world, not the pause menu.
            _root.style.display = DisplayStyle.None;
            yield return new WaitForEndOfFrame();
            SaveManager.Instance?.SaveGame(slotId);
            yield return new WaitForEndOfFrame();
            ShowSave();
            AddStatus("EXPEDIENTE ARCHIVADO");
        }

        private void LoadSlot(string slotId)
        {
            Hide();
            GameFlowManager.EnsureInstance().LoadSlot(slotId);
        }

        private void DeleteSlot(string slotId, bool returnToSave)
        {
            SaveManager.Instance?.DeleteSlot(slotId);
            if (returnToSave) ShowSave(); else ShowLoad();
            AddStatus("EXPEDIENTE ELIMINADO");
        }

        private void ShowConfirmation(string message, Action confirm, Action cancel)
        {
            ShowConfirmation("CONFIRMAR OPERACIÓN", message, confirm, cancel, "CONFIRMAR");
        }

        private void ShowConfirmation(string title, string message, Action confirm, Action cancel, string confirmLabel)
        {
            Show(MenuScreen.Confirmation, title);
            var warning = new Label(message);
            warning.AddToClassList("confirmation-message");
            _content.Add(warning);
            AddButton(confirmLabel, confirm, "menu-button menu-button--danger");
            AddButton("CANCELAR", cancel, "menu-button menu-button--quiet");
        }

        private void AddStatus(string message)
        {
            var status = new Label(message);
            status.AddToClassList("operation-status");
            _content.Add(status);
        }

        private static string FormatDate(string value)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime date)
                ? date.ToLocalTime().ToString("dd/MM/yyyy  HH:mm")
                : "FECHA DESCONOCIDA";
        }

        private static string FormatDuration(float seconds)
        {
            TimeSpan duration = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
            return duration.TotalHours >= 1d ? $"{(int)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}" : $"{duration.Minutes:00}:{duration.Seconds:00}";
        }

        private void ReleasePreviews()
        {
            foreach (Texture2D texture in _runtimePreviews)
                if (texture != null) Destroy(texture);
            _runtimePreviews.Clear();
        }

        private void AddLabel(string text) => _content.Add(new Label(text));

        private void NavigateBack()
        {
            if (_backScreen == MenuScreen.Main) ShowMain(); else ShowPause();
        }

        private void HandleFlowStateChanged(GameFlowState state)
        {
            if (state == GameFlowState.MainMenu) ShowMain();
        }

        private static void EnsureMenuServices()
        {
            if (SaveManager.Instance == null)
                new GameObject("SaveManager").AddComponent<SaveManager>();
            if (GameSettingsService.Instance == null)
                new GameObject("GameSettingsService").AddComponent<GameSettingsService>();
            if (InputRouter.Instance == null)
                new GameObject("InputRouter").AddComponent<InputRouter>();
            if (GameFeatures.IsEnabled(OptionalGameFeature.PlayerVitals) && SurvivalDifficultyService.Instance == null)
                new GameObject("SurvivalDifficultyService").AddComponent<SurvivalDifficultyService>();
        }

        /// <summary>
        /// Screen-space UI Toolkit can draw without a Camera, but Unity's Game View then overlays
        /// "No cameras rendering" on top of the menu. Keep existing scene cameras untouched and
        /// create a minimal clear-only camera only for camera-less menu scenes.
        /// </summary>
        private void EnsureRenderCamera()
        {
            if (Camera.allCamerasCount > 0) return;

            var cameraObject = new GameObject("MainMenuCamera (Runtime Fallback)");
            _runtimeMenuCamera = cameraObject.AddComponent<Camera>();
            _runtimeMenuCamera.clearFlags = CameraClearFlags.SolidColor;
            _runtimeMenuCamera.backgroundColor = new Color(0.008f, 0.012f, 0.012f, 1f);
            _runtimeMenuCamera.cullingMask = 0;
            _runtimeMenuCamera.depth = -100f;
            _runtimeMenuCamera.allowHDR = false;
            _runtimeMenuCamera.allowMSAA = false;
            _runtimeMenuCamera.useOcclusionCulling = false;
        }

        private void AddSlider(string label, float value, Action<float> changed, float low = 0f, float high = 1f)
        {
            var row = new VisualElement(); row.AddToClassList("setting-row");
            var rowLabel = new Label(label); rowLabel.AddToClassList("setting-row-label"); row.Add(rowLabel);
            var slider = new Slider(low, high) { value = value }; slider.RegisterValueChangedCallback(evt => changed(evt.newValue));
            // The row already shows its own Label; collapse the Slider's own (empty) reserved label
            // gutter so the track isn't squeezed into whatever width is left over.
            slider.AddToClassList("unity-base-field--no-label");
            row.Add(slider); _content.Add(row);
        }

        private void AddToggle(string label, bool value, Action<bool> changed)
        {
            var toggle = new Toggle(label) { value = value }; toggle.RegisterValueChangedCallback(evt => changed(evt.newValue));
            _content.Add(toggle);
        }

        private static readonly Dictionary<string, string> LanguageDisplayNames = new Dictionary<string, string>
        {
            { "es", "Español" },
            { "en", "English" },
        };

        private void AddLanguageSelector(GameSettingsData settings)
        {
            LocalizationService localization = LocalizationService.Instance;
            if (localization == null) return;

            List<string> codes = localization.AvailableLanguages();
            if (codes.Count <= 1) return;

            List<string> choices = codes.ConvertAll(code => LanguageDisplayNames.TryGetValue(code, out string name) ? name : code);
            int current = codes.IndexOf(localization.CurrentLanguage);
            if (current < 0) current = 0;

            var dropdown = new DropdownField("Idioma", choices, current);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                int index = choices.IndexOf(evt.newValue);
                if (index < 0) return;
                localization.SetLanguage(codes[index]);
                ShowSettings();
            });
            _content.Add(dropdown);
        }

        private void AddQualitySelector(GameSettingsData settings)
        {
            string[] names = QualitySettings.names;
            if (names.Length == 0) return;

            int currentLevel = settings.qualityLevel >= 0 && settings.qualityLevel < names.Length
                ? settings.qualityLevel
                : QualitySettings.GetQualityLevel();
            var choices = new List<string>(names);
            var dropdown = new DropdownField("Calidad gráfica", choices, currentLevel);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                int index = choices.IndexOf(evt.newValue);
                if (index < 0) return;
                settings.qualityLevel = index;
                SaveSettings(settings);
            });
            _content.Add(dropdown);
        }

        private void AddDifficultySelector()
        {
            SurvivalDifficultyService service = SurvivalDifficultyService.Instance;
            SurvivalDifficultyProfile[] profiles = service != null ? service.AvailableProfiles : Array.Empty<SurvivalDifficultyProfile>();
            if (profiles.Length == 0) return;
            var choices = new List<string>();
            int selected = 0;
            for (int index = 0; index < profiles.Length; index++)
            {
                SurvivalDifficultyProfile profile = profiles[index];
                choices.Add(profile != null ? profile.DisplayName : "No disponible");
                if (profile != null && service.ActiveProfile == profile) selected = index;
            }
            var dropdown = new DropdownField("Dificultad", choices, selected);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                int index = choices.IndexOf(evt.newValue);
                if (index >= 0 && index < profiles.Length && profiles[index] != null)
                    service.SetDifficulty(profiles[index].DifficultyId);
            });
            _content.Add(dropdown);
        }

        private void AddSectionLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("settings-section");
            _content.Add(label);
        }

        private void AddRebind(
            string label,
            string actionName,
            string compositePart,
            GameSettingsData settings)
        {
            InputRouter router = InputRouter.Instance;
            int bindingIndex = router != null ? router.FindKeyboardBinding(actionName, compositePart) : -1;

            var row = new VisualElement();
            row.AddToClassList("binding-row");
            var description = new Label(label);
            description.AddToClassList("binding-label");
            row.Add(description);

            var bindingButton = new Button();
            bindingButton.AddToClassList("binding-button");
            bindingButton.text = router != null ? router.GetBindingDisplay(actionName, bindingIndex) : "NO DISPONIBLE";
            bindingButton.SetEnabled(router != null && bindingIndex >= 0);
            bindingButton.clicked += () =>
            {
                bindingButton.text = "PULSA UNA TECLA...  [ESC CANCELA]";
                bindingButton.SetEnabled(false);
                bool started = router != null && router.StartInteractiveRebind(
                    actionName,
                    bindingIndex,
                    display =>
                    {
                        bindingButton.text = display;
                        bindingButton.SetEnabled(true);
                        settings.bindingOverridesJson = router.SaveBindingOverrides();
                        SaveSettings(settings);
                    },
                    () =>
                    {
                        bindingButton.text = router.GetBindingDisplay(actionName, bindingIndex);
                        bindingButton.SetEnabled(true);
                    });
                if (!started)
                {
                    bindingButton.text = "NO DISPONIBLE";
                    bindingButton.SetEnabled(false);
                }
            };
            row.Add(bindingButton);
            _content.Add(row);
        }

        private static void SaveSettings(GameSettingsData settings) => GameSettingsService.Instance?.ApplyAndSave(settings);
    }
}
