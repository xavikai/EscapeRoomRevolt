# Escape Room i Survival: projectes independents

**Carpetes reorganitzades el 26/10/2026:** consulta [l’índex actual del repositori](../../../README.md). Les rutes antigues en informes JSON i exemples històrics identifiquen l’estat de la validació; el mapa de trasllat en documenta la ubicació actual.

Actualització: 9 de novembre de 2026. [Escape Room 0.2.1 publicada](https://github.com/xavikai/EscapeRoomRevolt/releases/tag/escaperoom-v0.2.1), amb [validació batch actual](validation/release-0.2.1.json). Les darreres proves completes de Survival i de portabilitat externa són les del [26 d'octubre](QA_PC_2026-10-26.md). La verificació visual final continua pendent.

| Projecte Unity | Perfil i entrada | Escenes | Codi |
|---|---|---|---|
| `EscapeRoom/Unity` | EscapeRoom → ShowcaseMuseum | Intro, MainMenu, ShowcaseMuseum, LockedOffice, VRTemplate, ShowcaseMuseumVR, LockedOfficeVR | Common 0.2.0 |
| `Survival/Unity` | SurvivalHorror → SurvivalHorrorDemo | Intro, MainMenu, SurvivalHorrorDemo, VRTemplate | Common 0.2.0 + Survival 0.2.0 |

Obre directament la carpeta del projecte amb Unity **6000.4.9f1**. Cadascuna té Assets, manifests i lock de paquets, ProjectSettings i caches pròpies. L'autoria històrica és a `EscapeRoom/Framework`; l'arrel del repositori ja no és un projecte Unity. Les versions UPM dels paquets continuen sent 0.2.0; la release del producte Escape Room és la 0.2.1.

## Una sola base de codi

`SharedPackages/com.xavikai.revolt.common` conté Core, Player PC/VR, Systems, opcions reutilitzables, UI i plantilles, Bootstrap, Editor i tests. L'Editor compartit ara té assembly pròpia. `SharedPackages/com.xavikai.revolt.survival` conté runtime, UI, Editor i tests Survival. La base comuna no referencia les assemblies Survival.

El manifest d'Escape Room referencia Common amb `file:../../Framework/SharedPackages/com.xavikai.revolt.common`; el de Survival usa `file:../../../EscapeRoom/Framework/SharedPackages/com.xavikai.revolt.common` i també referencia el paquet Survival. Les versions declarades són 0.2.0 i Survival exigeix Common 0.2.0. Les versions externes estan resoltes als locks. Les dependències locals comparteixen la font del mateix repositori: una correcció comuna no s'ha de copiar a dos llocs. No hi ha scripts del framework duplicats dins dels Assets dels consumidors.

Les rutes `file:` són relatives a la carpeta Packages, segons [la documentació de Unity](https://docs.unity.com/en-us/engine/6000.7/manual/packages-list/dependencies-lp/upm-localpath). Conserva les carpetes `EscapeRoom` i `Survival` en aquesta disposició; per moure un projecte fora del repositori, usa l'exportació autocontinguda.

Els noms públics, namespaces i GUID dels scripts moguts es conserven. Alguns tipus compartits encara es diuen `Systems.Survival` per compatibilitat; això no és una dependència de l'assembly Survival. `SharedPackages/source-map.json` registra els trasllats.

## Dades i configuració pròpies

Cada consumidor té `ProductConfiguration`, `GenreFeatureSettings`, `GameFlowSettings`, `VRComfortSettings` i catàleg d'inventari propis. Només Survival inclou dificultat, presets, evidències, objectius i prefabs específics del seu gènere. El catàleg actual d'Escape Room conté **10 ítems**; el de la demo Survival, **2**. `product-inventory.json` registra les dades de cada conjunt de dependències.

`ProductConfiguration` defineix ID, títol, identificador d'aplicació i escenes de release. El menú usa el títol del producte si no hi ha un tema que el substitueixi. Guardats i settings es desen sota `Application.persistentDataPath/<productId>`, amb noms de producte i identificadors d'aplicació diferents. No hi ha migració ni compatibilitat de partides entre productes. L'autoria sense ProductConfiguration conserva la ubicació antiga.

Entrada i localització són defaults compartits del paquet Common, sense configuració específica de producte. L'entrada conté un superset d'accions; els noms d'acció Survival no carreguen el seu codi. Les mostres XR necessàries i els seus avisos es conserven amb els rigs. XR continua sent necessari també a PC.

La llanterna continua disponible a Escape Room. El seu prefab manté la ruta històrica `Prefabs/Survival/Flashlight_Modular.prefab`; el nom de carpeta no implica dependència del paquet Survival.

## Separació visual i referències

GameplayHUD i els estils comuns ja no contenen cordura, vida/estamina ni càmera nocturna. Aquests elements són a `Survival/UI/Resources/SurvivalHUD`. `SurvivalHUDMount` els munta després que el UIDocument estigui activat i llavors vincula els presentadors. Escape Room no importa aquest recurs.

Les plantilles, PanelSettings i shaders compartits són al paquet Common. `FrameworkAssetPaths` resol primer possibles overrides del consumidor i després les plantilles del paquet. Les eines escriuen a Assets del projecte actual, sense reescriure els paquets ni l'altre producte. Els shaders consultats per nom s'inclouen explícitament als ajustos gràfics dels consumidors.

Les còpies dels museus d'Escape Room exclouen tres components que el perfil no activava: SanityController al museu PC i HorrorEventTrigger als museus PC/VR. Això elimina les seves dependències Survival; el NarrativeTrigger d'àudio i subtítols compartit es conserva. L'autoria original conserva aquests components.

La demo Survival original és binària: s'ha copiat byte a byte, amb el mateix GUID, sense regenerar-ne la geometria. Les dependències s'han obtingut amb AssetDatabase, incloent formats binaris.

## Crear i validar

Els dos projectes ja estan creats. `tools/create-products.cjs` és una eina d'inicialització, no de sincronització: **rebutja sobreescriure projectes existents**. No la tornis a executar sobre dades editades.

Per preparar una composició nova en una carpeta buida, usa `EscapeRoom/Framework` com a projecte d'autoria i executa les ordres Node des de l'arrel del repositori:

1. Executar Unity al projecte d'autoria amb `-executeMethod EscapeRoomRevolt.EditorTools.ProductDependencyExporter.Export`. Actualitza `tools/product-asset-dependencies.json`.
2. Executar `node EscapeRoom/Framework/tools/create-products.cjs --output-root=<carpeta-buida>`; opcionalment indicar EscapeRoom o Survival. La generació usa només les escenes de cada producte i bloqueja dependències Survival inesperades a Escape Room.
3. Importar i validar el projecte. Les referències locals es calculen respecte de la ubicació de generació.

Per validar els projectes existents, executa des de l'arrel del repositori:

```powershell
pwsh -File EscapeRoom/Framework/tools/validate-product.ps1 -Product EscapeRoom -BuildWindows
pwsh -File EscapeRoom/Framework/tools/validate-product.ps1 -Product Survival -BuildWindows
```

Comprova assemblies, perfil, escena inicial, dades, recursos, escenes i prefabs; executa EditMode i PlayMode i opcionalment construeix Windows. Els logs i XML són a `.utmp` de cada consumidor. El validador també és al menú `Escape Room Framework > Validation > Validate Product Module Boundaries`.

ReleaseBuilder llegeix la configuració del consumidor. Windows només inclou les seves escenes i desactiva temporalment XR, restaurant l'ajust en acabar. Escape Room disposa de ShowcaseMuseumVR com a demo Quest. Survival no té demo Quest validada i el builder rebutja aquella configuració; VRTemplate és una mostra d'autoria.

## Exportar sense el repositori

```powershell
node EscapeRoom/Framework/tools/export-product.cjs EscapeRoom EscapeRoom/Exportacions/EscapeRoom-nova-revisio
node EscapeRoom/Framework/tools/export-product.cjs Survival Survival/Exportacions/Survival-nova-revisio
```

Executa aquestes ordres des de l'arrel i tria una destinació nova per a cada revisió. Crea dades, ajustos, manifests i snapshots dels paquets embeguts; no inclou Library, logs, builds ni MCP. L'exportació és una entrega versionada i rebutja sobreescriure entregues existents. Sense destinació explícita, l'eina encara selecciona el nom històric `0.2.0-pcfix-2026-10-26` i falla si ja existeix.

## Validació i límits

La taula resumeix els resultats del 26/10/2026. Escape Room ha repetit la validació de mòduls, les suites 26/26 i 34/34 i la build Windows per a la [release 0.2.1](validation/release-0.2.1.json). Survival i la importació externa dels ZIP no s'han tornat a validar per a aquesta release.

Els dos consumidors s'han importat i compilat amb Unity 6000.4.9f1 sense MCP. Escape Room no carrega l'assembly Survival; Survival sí. Els perfils, rutes inicials i ubicacions de dades s'han comprovat per separat.

| Suite | Escape Room | Survival |
|---|---:|---:|
| EditMode | 26/26 | 26/26 |
| PlayMode | 34/34 | 38/38 |
| Build Windows | Correcta | Correcta |
| Escenes revisades | 7 | 4 |
| Prefabs revisats | 5 | 10 |

Sense fallades ni omissions. PlayMode inclou la càrrega de la primera escena real i comprovacions de Bootstrap, inventari, jugador, dificultat, HUD propi i espai de dades. Les quatre regressions Survival només es distribueixen amb aquell paquet. El validador no ha detectat scripts absents ni referències serialitzades trencades en les escenes i prefabs revisats. Resultats actuals: [correccions i reconstruccions del 26 d'octubre](validation/qa-corrections-2026-10-26.json). L'[informe de productes del 25 d'octubre](validation/products-2026-10-25.json) es conserva com a històric. Es conserva l'[informe de la primera fase](validation/separation-2026-10-25.json).

La build publicada d'Escape Room 0.2.1 es genera a `EscapeRoom/Unity/Builds/Release/v0.2.1/Windows/`. El builder compartit actual usa la versió 0.2.1; això no acredita una reconstrucció ni una release nova de Survival. Els informes del 26 d'octubre conserven les rutes històriques `Projects/<producte>/Builds/Release/v0.1.0-beta.5/Windows/`. No s'ha generat un APK per a la release 0.2.1.

Els snapshots autocontinguts `Distributions/EscapeRoom-0.2.0` i `Distributions/Survival-0.2.0` també han passat importació neta amb Unity fora del repositori original, les comprovacions de referències i les suites completes. Escape Room passa 25/25 EditMode i 34/34 PlayMode; Survival, 25/25 i 38/38, sense fallades ni omissions. Les exportacions originals s'han conservat intactes, comprovat amb SHA-256 abans i després. [Resultats de les exportacions](validation/portable-exports-2026-10-25.json).

Les proves i builds són batch sense gràfics. Continuen pendents el recorregut humà dels executables, QA i rendiment en hardware, paritat completa del museu VR, procedència dels àudios i preparació comercial. La separació no dona aquests punts per tancats.

## Correccions PC del 26 d'octubre

Recursos implícits de TextMesh Pro restaurats als dos consumidors; builds Windows sense preinicialització nativa XR; NavMesh de Survival desat com a asset persistent i comprovat en carregar la demo. L'escena reparada conserva la seva identitat, però el seu contingut ja no és byte a byte igual a la còpia binària d'autoria: inclou la referència al NavMesh. La geometria de la demo no s'ha regenerat. Les proves i les dues reconstruccions han passat en mode batch. [Abast i evidència](QA_PC_2026-10-26.md).

Les exportacions originals sense sufix 0.2.0 són snapshots històrics: han passat portabilitat, però no inclouen les correccions PC. Es conserven intactes; la revisió actual amb sufix pcfix-2026-10-26 sí que les incorpora.

## Repetir la verificació de portabilitat

```powershell
pwsh -File EscapeRoom/Framework/tools/validate-portable.ps1 -Product EscapeRoom -BuildWindows
pwsh -File EscapeRoom/Framework/tools/validate-portable.ps1 -Product Survival -BuildWindows
```

Executa des de l'arrel. Aquestes ordres seleccionen els snapshots històrics `0.2.0-pcfix-2026-10-26`. Per a la 0.2.1, selecciona `-ExportPath EscapeRoom/Exportacions/EscapeRoom-0.2.1` i omet `-BuildWindows`: la comprovació de binaris d'aquest validador portable encara busca `v0.1.0-beta.5`. El validador del projecte de treball sí que resol la versió actual.

El script copia l'exportació existent a una carpeta temporal única del sistema, fora del repositori, i comprova que comença sense Library, Temp, Logs ni objectes compilats copiats. No regenera l'exportació a partir de SharedPackages.

Afegeix un comprovador d'Editor només a la còpia de prova. Unity resol els paquets Revolt dins de Packages d'aquella còpia i compila les assemblies dins de la seva Library. Es rebutja qualsevol paquet que es resolgui dins del repositori original i la presència de MCP. Després s'executen la validació de producte i EditMode/PlayMode, inclosa l'arrencada de la primera escena i del HUD.

Els logs i XML es conserven a la carpeta temporal; .utmp/portable-EscapeRoom.json i .utmp/portable-Survival.json registren la seva ubicació i els resultats locals. L'informe actual és docs/validation/portable-exports-2026-10-26.json, amb les rutes normalitzades; el del 25 d'octubre es conserva com a històric. Les exportacions no s'editen: el hash de tot l'arbre ha de coincidir abans i després.

Aquesta verificació usa la instal·lació de Unity i les seves dependències externes normals, que poden provenir de la cache global o del registre. No depèn del codi, Assets ni Library del repositori original. La passada del 25 d'octubre no repetia builds dins de les còpies temporals; la revisió actual sí que reconstrueix Windows a les dues còpies aïllades. Quest i QA visual o en visor continuen pendents.

## Exportacions corregides del 26 d'octubre

Snapshots històrics validats: `EscapeRoom/Exportacions/EscapeRoom-0.2.0-pcfix-2026-10-26` i `Survival/Exportacions/Survival-0.2.0-pcfix-2026-10-26`. Inclouen TMP, aïllament XR de Windows i navegació Survival persistent. Les versions dels paquets continuen sent 0.2.0; el sufix identifica la revisió de l'exportació. Els exportadors i validadors encara els usen per defecte; `-ExportPath` permet seleccionar un altre snapshot al validador. Els resultats següents corresponen exclusivament al 26 d'octubre.

Importació des de zero fora del repositori i auditoria de paquets/assemblies: PASS. Escape Room: 26/26 EditMode i 34/34 PlayMode. Survival: 26/26 i 38/38. Cap fallada ni omissió. Dues builds Windows Succeeded, amb boot.config sense preinicialització nativa XR. No s'ha reparat ni regenerat el NavMesh abans de les proves: s'ha comprovat l'asset lliurat. Hash de cada exportació idèntic abans i després. [Informe actual](validation/portable-exports-2026-10-26.json).

No s'ha reprès el control de les aplicacions. El QA visual continua pendent. Les exportacions contenen fonts i recursos; les builds, logs i Library de validació es conserven només a les còpies temporals.

QA visual manual preparat: [passos i resultats esperats per a les dues demos](QA_VISUAL_MANUAL_PC.md) i [plantilla de sessió i incidències](QA_VISUAL_MANUAL_PC_REGISTRE.md). Tots els casos manuals continuen pendents. No s’ha reprès el control de les aplicacions ni modificat els snapshots validats.

Lliurament preparat: [un ZIP per projecte amb exportació, guia i registre propi](LLIURAMENT_PC_2026-10-26.md). Integritat del projecte i de totes les entrades ZIP comprovada. Snapshots originals intactes; sense rellançar Unity ni reprendre el control d’aplicacions. QA manual pendent.

Demos Windows preparades per provar sense Unity: [ZIPs executables i instruccions](DEMOS_WINDOWS_2026-10-26.md), amb runtime complet, guia i registre propi. Integritat verificada contra les builds prèvies; no s’han executat les demos ni reprès el control de les aplicacions.

Guies interactives actualitzades: [projectes i mecàniques comunes](guia/index.html#projects) i [guia Survival pròpia](../../../Survival/Documentacio/GuiaInteractiva/index.html). Es distingeixen simulacions web, funcionament real i QA pendent. No s’han modificat els ZIPs ni snapshots existents.
