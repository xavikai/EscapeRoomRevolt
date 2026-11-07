# Escape Room Revolt 0.2.0

Primera release d'Escape Room com a **projecte independent**. Fins ara, Escape Room i Survival Horror compartien un sol projecte Unity i un perfil triava quines mecàniques s'activaven. Des d'aquesta versió cada joc té el seu propi projecte Unity. El codi comú viu en paquets compartits, i no se'n manté una segona còpia.

Builds i paquets: revisió **0.2.0-pcfix-2026-10-26**. Unity **6000.4.9f1**, URP, PC Windows.

## Què has de descarregar

| Vull… | Descàrrega | Com |
|---|---|---|
| Jugar la demo sense instal·lar Unity | `EscapeRoom-0.2.0-pcfix-2026-10-26-windows.zip` | Extreu **tot** el ZIP i executa `Windows/escaperoom-revolt.exe`. No l'obris des del visor del ZIP. |
| Fer el meu escape room amb Unity | `EscapeRoom-0.2.0-pcfix-2026-10-26-delivery.zip` | Extreu-lo i obre la carpeta `Project/` amb Unity 6000.4.9f1. Els paquets del framework hi són inclosos: no cal el repositori. |
| Treballar dins del repositori | `Source code` | Obre `EscapeRoom/Unity` amb Unity 6000.4.9f1. Ha de quedar dins del repositori, perquè fa referència als paquets de `EscapeRoom/Framework/SharedPackages`. |

Guia interactiva (museu 3D, laboratori de mecàniques, receptes pas a pas, menús i jugador PC/VR, en CA/ES/EN): `EscapeRoom/Framework/docs/guia/index.html`. Obre-la amb el navegador. Necessita internet per carregar three.js i les tipografies.

## Novetats des de la 0.1.0-beta.3

**Projecte separat de Survival**
- Projecte Unity propi a `EscapeRoom/Unity`, amb la seva configuració, catàleg d'objectes, nom d'aplicació i carpeta de partides desades. Les partides d'Escape Room i de Survival no es barregen.
- Codi comú al paquet `com.xavikai.revolt.common`. El projecte Escape Room no inclou ni Survival ni AI Navigation.
- Eines per validar el producte (`tools/validate-product.ps1`) i exportar-lo com a projecte portable.

**Correccions de la demo PC**
- Els cartells i textos 3D (TextMesh Pro) ja no donen errors en una build: s'hi inclouen els recursos que faltaven.
- La build de Windows ja no intenta iniciar OpenXR en arrencar.

**Guardat, puzles i interacció** (revisió del 29/09)
- Continuar o Carregar des del menú ja no deixa el jugador bloquejat.
- Les palanques i els rodets (`StatePuzzle`) guarden la seva posició. Els puzles encadenats, de canonades, de col·locació, de codi aleatori i de llançament es restauren bé en carregar.
- Els interruptors i els triggers d'un sol ús guarden el seu estat.

**Idiomes i documentació**
- Joc complet en **català, castellà i anglès**: menús, HUD, inventari, notes, pistes, objectius, textos d'interacció i cartells del museu.
- 16 receptes pas a pas (10 puzles i 6 sistemes de sala), a la guia interactiva i en Markdown (`RECEPTES.md`, `RECETAS.md`, `RECIPES.md`).
- Manual i guia ràpida en els tres idiomes.
- El títol del menú i els crèdits es configuren des de `MenuThemeSettings`, sense tocar codi.

## Validació

- Proves automàtiques a Unity 6000.4.9f1 (batch): **26/26 EditMode i 34/34 PlayMode**, sense proves omeses.
- Importació neta del projecte portable fora del repositori i build de Windows correcta.
- Els ZIP s'han comprovat fitxer a fitxer. Cada ZIP inclou `FILES.sha256.json`.

## Integritat de les descàrregues (SHA-256)

- Demo Windows: `801629dae6a1fd670440a8f4086c9472db90cf348294712e42968c9cb5620e7f`
- Projecte portable: `c4d615564c25ccffef702e3bf5bca125b3c7eb7067bc49758a81aa93b4bb2f87`

A PowerShell: `Get-FileHash -LiteralPath '<fitxer>.zip' -Algorithm SHA256`.

## Limitacions conegudes

- **Encara no s'ha fet el QA visual manual** (recorregut complet del museu amb captures). La plantilla és a `EscapeRoom/Documentacio/QA_VISUAL_MANUAL_PC.md`.
- La VR no s'ha provat en un visor real, i `ShowcaseMuseumVR` encara no té la sala 14 (circuit de llums). Aquesta release no inclou APK de Quest.
- La versió interna de l'aplicació (`bundleVersion`) continua sent `0.1.0`, i les builds es generen a la carpeta històrica `v0.1.0-beta.5`. Es corregirà a la propera build.
- Cal confirmar la llicència de 5 àudios (`ThirdPartyNotices.md`).
- Survival té el seu projecte a `Survival/`, però no forma part d'aquesta release.
