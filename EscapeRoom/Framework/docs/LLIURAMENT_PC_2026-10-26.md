# Paquets de lliurament PC — 26/10/2026

**Carpetes reorganitzades el 26/10/2026:** consulta [l’índex actual del repositori](../../../README.md). Les rutes antigues en informes JSON i exemples històrics identifiquen l’estat de la validació; el mapa de trasllat en documenta la ubicació actual.

Revisió **0.2.0-pcfix-2026-10-26**. Unity **6000.4.9f1**. Cada ZIP conté un projecte font portable i la documentació per preparar el QA manual. No inclou Unity ni executables compilats.

| Producte | Paquet | Fitxers verificats al ZIP | Casos manuals pendents |
|---|---|---:|---:|
| Escape Room | [EscapeRoom delivery.zip](../../PaquetsFonts/EscapeRoom-0.2.0-pcfix-2026-10-26-delivery.zip) | 1.246 | 28 |
| Survival | [Survival delivery.zip](../../../Survival/PaquetsFonts/Survival-0.2.0-pcfix-2026-10-26-delivery.zip) | 1.281 | 25 |

Els ZIP, les carpetes sense comprimir i els fitxers `.zip.sha256` són a `Distributions/Deliveries/` (artefactes locals exclosos de Git). Per lliurar, enviar el ZIP i el seu `.zip.sha256`. L'informe versionat [deliveries-2026-10-26.json](validation/deliveries-2026-10-26.json) registra rutes, mida, SHA-256 de cada ZIP i hash del snapshot font.

## Contingut de cada paquet

```text
<producte>-0.2.0-pcfix-2026-10-26-delivery/
  README.md
  FILES.sha256.json
  Project/                  # exportació portable validada, sense canvis
    Assets/
    Packages/               # Common; també Survival al seu producte
    ProjectSettings/
    EXPORT.json
    ...
  docs/
    QA_VISUAL_MANUAL_PC.md
    QA_VISUAL_MANUAL_PC_REGISTRE.md
    validation/
      portable-exports-2026-10-26.json
      qa-corrections-2026-10-26.json
```

La guia cobreix tots dos recorreguts. El registre està preparat per al producte del paquet: 12 casos comuns més 16 d'Escape Room o 13 de Survival, tots PENDENT. El cas d'aïllament de partides C12 requereix disposar de tots dos productes. Cal copiar el registre abans d'omplir-lo; si falta l'altre producte, documentar el bloqueig.

## Ús per part del receptor

1. Comparar el SHA-256 del ZIP amb el fitxer adjacent o amb l'informe de lliurament. En PowerShell: `Get-FileHash -LiteralPath '<paquet>.zip' -Algorithm SHA256`.
2. Extreure tot el ZIP a una carpeta amb permisos d'escriptura i llegir el README del paquet. El directori Unity que s'ha d'importar és **Project/**.
3. Usar Unity 6000.4.9f1 amb llicència i Windows Build Support. Les dependències externes es resolen des del manifest/lock, mitjançant registre o cache global. No cal el repositori original; els paquets Revolt són embeguts.
4. Generar la build Windows seguint l'ordre batch del README. Copiar i executar la carpeta Windows completa quan es decideixi començar la sessió manual. Les builds de verificacions prèvies no s'inclouen en aquest lliurament.
5. Registrar origen, hash de la build real, maquinari i configuració. Seguir la guia, adjuntar evidències i completar la decisió d'acceptació al registre. La preparació dels paquets no dona cap cas manual per superat.

`FILES.sha256.json` conté les rutes, mides i hashes dels fitxers de contingut; exclou el mateix inventari per evitar autoreferència. El SHA-256 extern cobreix tot el ZIP, inclòs l'inventari. És una comprovació d'integritat, no una signatura digital.

## Verificació feta i límits

El script ha comprovat el hash de cada snapshot contra l'informe de portabilitat, l'ha copiat sense canvis a Project i ha comparat el resultat. Després de comprimir ha rellegit cada entrada del ZIP, comprovat els hashes i la llista exacta de fitxers, rebutjat rutes insegures o entrades duplicades i tornat a comprovar que el snapshot original no havia canviat.

Les importacions netes, proves i builds Windows són les de la [validació portable anterior](validation/portable-exports-2026-10-26.json): Escape Room 26/26 EditMode i 34/34 PlayMode; Survival 26/26 i 38/38, dues builds correctes sense preinicialització XR. Aquest empaquetatge **no ha rellançat Unity ni reprès el control de les aplicacions**. El QA visual continua pendent.

## Reproducció

`pwsh -NoProfile -File tools/package-deliveries.ps1`

L'eina empaqueta exclusivament els snapshots identificats a l'informe de portabilitat, i rebutja fonts amb hashes diferents o qualsevol sortida/informe ja existent. La revisió actual ja està generada: no la sobreescriguis. Una entrega posterior requereix una revisió i evidència de validació corresponents; no s'han de reetiquetar resultats antics com si fossin proves noves.

Per provar sense instal·lar Unity, usar els [paquets executables Windows](DEMOS_WINDOWS_2026-10-26.md), que inclouen runtime, guia i registre. Els ZIP descrits en aquest document es conserven com a lliuraments font.
