# Revolt Survival — 0.2.0

Optional runtime, UI overlay, Editor and tests package. Depends on `com.xavikai.revolt.common` 0.2.0 and Unity AI Navigation 2.0.14. The Escape Room consumer does not install it.

The consumer owns the Survival genre profile, difficulty settings/presets, objectives/evidence, enemy and equipment prefabs, catalog and demo scene. Runtime services install through `GameModules`; `SurvivalHUDMount` adds the package's Resources/SurvivalHUD template after the shared UIDocument is enabled.

The demo builder writes only into the current consumer's Assets/configuration. It cannot modify another project's assets. Existing headset and commercial QA requirements still apply; the supplied Survival demo is PC and has no validated Quest scene.
