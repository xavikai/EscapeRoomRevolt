# Escape Room

**Versió 0.2.1** · [Notes de la versió](Framework/RELEASE_NOTES_v0.2.1.md) · [0.2.0](Framework/RELEASE_NOTES_v0.2.0.md) · [Canvis](Framework/CHANGELOG.md)

**[Documentació interactiva amb Three.js](Framework/docs/guia/index.html)**: primers passos amb la plantilla amb captures de Unity, museu 3D, mecàniques jugables, menús i explicacions en CA/ES/EN. Obre l'HTML amb un navegador; necessita Internet per carregar Three.js i les tipografies.

- **Provar sense Unity:** [ZIP Windows](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.1/EscapeRoom-0.2.1-windows.zip). Extreu tot el ZIP i executa `Windows/escaperoom-revolt.exe` de la carpeta extreta.
- **Editar:** obre [Unity/](Unity) amb Unity 6000.4.9f1.
- **Unity 6000.6.8f1:** usa el ZIP font corregit de la release; s'han migrat les APIs que provocaven Safe Mode i impedien carregar el menú `Escape Room Framework`. Obre la carpeta completa amb `Assets`, `Packages` i `ProjectSettings` des de Unity Hub.
- **QA manual:** [guia](Documentacio/QA_VISUAL_MANUAL_PC.md) i [registre propi](Documentacio/QA_VISUAL_MANUAL_PC_REGISTRE.md), pendent d'executar.
- **Projecte portable:** el ZIP font porta els paquets embeguts. La carpeta local `Exportacions/EscapeRoom-0.2.1/` és un artefacte exclòs de Git.
- **Projecte font:** [ZIP del projecte](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.1/EscapeRoom-0.2.1-source.zip), amb la guia revisada i les notes de versió a `docs/release/`. Les releases anteriors continuen disponibles a GitHub.
- **Guia descarregable:** [HTML amb captures i idiomes CA/ES/EN](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.1/guia-escape-room-0.2.1.html).
- **Base comuna i eines:** [Framework/](Framework/README.md). Inclou l'autoria històrica, els paquets canònics i els informes tècnics compartits amb Survival.

El projecte de treball `Unity/` referencia els paquets locals de `Framework/SharedPackages`; conserva l'estructura del repositori. Per editar fora del repositori, extreu el ZIP font i obre la carpeta que conté `Assets`, `Packages` i `ProjectSettings`.

**[Validació del ZIP portable corregit](Framework/docs/validation/portable-release-0.2.1.json):** importació neta fora del repositori, auditoria de recursos sense incidències, 26/26 tests EditMode, 34/34 PlayMode i build Windows correcta, sense preinicialització XR nativa. L'informe identifica el SHA-256 exacte del ZIP validat. El QA visual manual i el maquinari VR continuen pendents. La [validació inicial de la release](Framework/docs/validation/release-0.2.1.json) es conserva com a evidència anterior.

La guia comuna incorpora una secció «Projectes i rutes» actualitzada. Per als sistemes específics consulta la [guia Survival](../Survival/Documentacio/GuiaInteractiva/index.html).
