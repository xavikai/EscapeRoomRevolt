# Guia interactiva · Escape Room Revolt

**Versió publicada 0.2.2:** [descarrega l'HTML amb captures](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.2/guia-escape-room-0.2.2.html). A GitHub, `index.html` es mostra com a codi; desa'l o descarrega l'adjunt de la release abans d'obrir-lo al navegador. [Notes i comprovacions de la 0.2.2](../../RELEASE_NOTES_v0.2.2.md).

Obre `index.html` en un navegador (doble clic). Necessita connexió a Internet per carregar three.js (cdnjs) i les tipografies. Les captures de Unity van incrustades dins del fitxer: es pot copiar sol. Clica una captura per ampliar-la.

Idiomes: català, castellà, anglès, francès i rus (selector a la barra lateral). Les versions francesa i russa parteixen de traducció automàtica amb revisió dels menús i termes principals. Les captures de Unity conserven l'idioma original.

**[Receptes pas a pas](index.html#rec):** configuració dels deu puzles i dels sistemes de la sala (incloent **Clau que obre una porta** i **Clavar un o diversos ganivets 3D a la diana** amb `GrabbablePiece.LockInPlace`) des de zero. Cada recepta indica què cal seleccionar, a quin panell es crea cada element i quina referència cal arrossegar a cada camp de l'Inspector. Inclou exemples per reduir els puzles, com passar de tres dianes a una.

**[Ampliar puzles](index.html#expand):** exemples per afegir llums, palanques, notes, dianes i encaixos. També indica com redimensionar o estendre els rodets, el tauler lliscant, les canonades, el teclat i els puzles encadenats.

**[Cinemàtiques](index.html#cinematics):** pas a pas per mostrar una porta quan es resol un puzle i per muntar una intro o un final amb imatges, vídeos i plans de càmera.

**[Definition i pistes](index.html#definitions):** exemple complet d'un candau amb `Puzzle Definition` i un `Hint Data` de tres pistes. Explica cada camp, els dos enllaços entre escena i assets, IDs únics, inici del context, temporitzadors, tecla H, Hint Zone, finalització i guardat/carregat, amb comprovacions i errors habituals. Disponible en CA/ES/EN. [Referència detallada en català](../DEFINITIONS_I_PISTES.md).

**Música i efectes sonors:** nova secció amb importació de clips, música 2D en bucle, ambientació 3D, sons de portes/teclats/objectes, interaccions amb `AudioSource.Play()` i integració amb els volums d'AudioManager. Explica també les fonts independents que no queden connectades automàticament al menú de so.

**[Cursor segons l'acció](index.html#cursors):** com assignar `Cursor Type` a cada interactable, les formes reals del HUD (`Default`, `Hand`, `Eye`, `Puzzle` i `Throw`), la prioritat automàtica de llançament i el cas `Exit`, que encara no té estil propi. Inclou com personalitzar el HUD amb una còpia d'UXML/USS i com provar els canvis.

**[Canviar els models 3D](index.html#models):** jerarquia `Logic`/`Visuals`, substitució manual de la geometria, preparació de prefabs visuals i ús de `Replaceable Model Slot`. Inclou diagrames, els camps `Model Prefab`/`Model Socket`/`Placeholder Visual`, les peces animades amb `Visual Transform`, els pivots, colliders, sockets, ressaltat i comprovacions de joc i guardat. També explica què pot convertir la comanda `Split Selection Into Logic + Visuals`.

Els dos apartats són en CA/ES/EN i es poden obrir des de l'índex lateral o des dels accessos al final dels primers passos. Inclouen quatre captures reals de Unity 6000.6.8f1 amb ampliació: selector Cursor Type, nota amb Eye, components de Visuals i camps de Replaceable Model Slot/Visual Transform. Les captures es van fer en una escena de prova sense desar; no s’han modificat el codi ni les mecàniques del projecte publicat. Les taules de formes de cursor continuen sent esquemes.

Seccions: **crear el teu joc amb la plantilla** (32 passos amb captures de Unity, començant amb el projecte ja obert: jugar la demo, crear una sala amb una porta, un teclat i una nota, afegir veus amb subtítols, pistes automàtiques, zones que disparen accions, compte enrere, perills, finals i objectius, i generar el joc per a Windows; amb sala 3D que es construeix pas a pas, glossari i problemes freqüents), visita guiada amb el museu 3D, laboratori de mecàniques jugables amb Inspector, taller de menús (MenuThemeSettings) amb **disseny del menú pas a pas** (20 passos en dos nivells: tema sense UI Toolkit, amb imatge de fons, imatge del marc i imatges dels botons, i full d'estils propi amb UI Builder; esquema clicable del menú, dissenyador visual que genera l'USS i referència de selectors), jugador PC i VR, funcions (inventari, examen, guardat, pistes, perills, menú de l'editor), recepta de la primera sala i problemes freqüents.

Ubicació després de la reorganització: `EscapeRoom/Framework/docs/guia/index.html`, des de l'arrel del repositori. Els índexs principals d'EscapeRoom i Survival hi enllacen. La ruta antiga `docs/guia/` s'ha traslladat; per publicar-la cal configurar el desplegament amb aquesta ubicació nova. No s'ha publicat ni modificat cap lloc web en aquesta revisió.

Per provar les demos PC actuals: [guia de QA manual](../QA_VISUAL_MANUAL_PC.md) i [registre de resultats](../QA_VISUAL_MANUAL_PC_REGISTRE.md). Els casos estan pendents d’executar.

Paquets de lliurament: [exportacions portables, guia de QA i registres per producte](../LLIURAMENT_PC_2026-10-26.md).

Prova directa sense Unity: [demos executables Windows amb guia i registre](../DEMOS_WINDOWS_2026-10-26.md).

Actualització 26/10/2026: nova secció de projectes i rutes en CA/ES/EN, estat de validació actual i distinció entre perfils i paquets. Les implementacions Three.js originals es conserven. [Guia pròpia de Survival](../../../../Survival/Documentacio/GuiaInteractiva/index.html), en català, amb simulacions didàctiques de detecció, càmera i checkpoints. Verificació estàtica i del model amb Node; cap control d’aplicacions ni revisió visual executada.

[Informe de les comprovacions](../validation/interactive-guides-2026-10-26.json). La verificació de Node no substitueix la revisió del render WebGL ni de la disposició visual en un navegador.
