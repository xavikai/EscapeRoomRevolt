# Escape Room

**Versió 0.2.2** · [Notes de la versió](Framework/RELEASE_NOTES_v0.2.2.md) · [Actualitzar des de 0.2.1](Framework/UPDATE_FROM_0.2.1.ca.md) · [Canvis](Framework/CHANGELOG.md)

**[Consulta la guia interactiva amb cerca](https://xavikai.github.io/EscapeRoomRevolt/escape-room/)** per veure els primers passos amb la plantilla, captures de Unity, museu 3D, mecàniques jugables i menús en CA/ES/EN/FR/RU.

El fitxer font i les instruccions són a [Documentacio/GuiaInteractiva](Documentacio/GuiaInteractiva/README.md).

- **Provar sense Unity:** [ZIP Windows](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.2/EscapeRoom-0.2.2-windows.zip). Extreu tot el ZIP i executa `Windows/escaperoom-revolt.exe` de la carpeta extreta.
- **Editar:** obre [Unity/](Unity) amb Unity 6000.4.9f1.
- **ZIP 0.2.2:** validat amb Unity 6000.6.8f1. Obre la carpeta extreta amb Unity Hub.
- **QA manual:** [guia](Documentacio/QA_VISUAL_MANUAL_PC.md) i [registre propi](Documentacio/QA_VISUAL_MANUAL_PC_REGISTRE.md), pendent d'executar.
- **Projecte portable:** el ZIP font porta el paquet del framework embegut.
- **Projecte font:** [ZIP del projecte](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.2/EscapeRoom-0.2.2-source.zip), amb la guia revisada i les notes de versió a `docs/release/`. Les releases anteriors continuen disponibles a GitHub.
- **Guia descarregable:** [HTML amb cerca, captures i idiomes CA/ES/EN/FR/RU](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.2/guia-escape-room-0.2.2.html).
- **Base comuna i eines:** [Framework/](Framework/README.md). Inclou l'autoria històrica, els paquets canònics i els informes tècnics compartits amb Survival.

El projecte de treball `Unity/` referencia els paquets locals de `Framework/SharedPackages`; conserva l'estructura del repositori. Per editar fora del repositori, extreu el ZIP font i obre la carpeta que conté `Assets`, `Packages` i `ProjectSettings`.

**[Validació històrica del ZIP portable 0.2.1](Framework/docs/validation/portable-release-0.2.1.json):** importació neta fora del repositori, auditoria de recursos sense incidències, 26/26 tests EditMode, 34/34 PlayMode i build Windows correcta, sense preinicialització XR nativa. L'informe identifica el SHA-256 exacte del ZIP validat. El QA visual manual i el maquinari VR continuen pendents. La [validació inicial de la release](Framework/docs/validation/release-0.2.1.json) es conserva com a evidència anterior.
