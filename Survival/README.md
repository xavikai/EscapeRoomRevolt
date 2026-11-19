# Survival

**[Documentació interactiva compartida amb Three.js](../EscapeRoom/Framework/docs/guia/index.html)**: museu 3D, laboratori de mecàniques i explicacions en CA/ES/EN. Obre l'HTML amb un navegador; necessita Internet per carregar Three.js i les tipografies.

- **Provar sense Unity:** paquet local `Windows/Survival-0.2.0-pcfix-2026-10-26-windows.zip`, exclòs de Git i encara sense descarregable publicat a GitHub. Si ja tens el paquet, extreu-lo complet i executa `Windows/survival-revolt.exe`.
- **Editar:** obre [Unity/](Unity) amb Unity 6000.4.9f1.
- **QA manual:** [guia](Documentacio/QA_VISUAL_MANUAL_PC.md) i [registre propi](Documentacio/QA_VISUAL_MANUAL_PC_REGISTRE.md), pendent d'executar.
- **Projecte portable local:** `Exportacions/Survival-0.2.0-pcfix-2026-10-26/`, amb Common i Survival embeguts; exclòs de Git.
- **Lliurament font local amb QA:** `PaquetsFonts/Survival-0.2.0-pcfix-2026-10-26-delivery.zip`; exclòs de Git i sense adjunt publicat a GitHub.

La base canònica compartida és a [EscapeRoom/Framework/SharedPackages](../EscapeRoom/Framework/SharedPackages). `Unity/Packages/manifest.json` referencia aquella ubicació; no hi ha una segona còpia de manteniment. Per a un lliurament fora del repositori usa l'exportació portable o els ZIP. El QA visual continua pendent.

Les [darreres proves completes de Survival](../EscapeRoom/Framework/docs/QA_PC_2026-10-26.md) són del 26/10/2026: 26/26 tests EditMode, 38/38 PlayMode i build Windows correcta. La release Escape Room 0.2.1 actualitza el paquet Common compartit, però no acredita una nova validació de Survival.

**[Descarrega la guia interactiva de Survival amb cerca](https://github.com/xavikai/EscapeRoomRevolt/releases/download/docs-search-2026-11-19/guia-survival-amb-cerca.html).** Obre l'HTML amb un navegador per consultar les rutes, els controls, la IA, la càmera, els checkpoints i el recorregut de la demo. Inclou simulacions didàctiques Three.js en català.
