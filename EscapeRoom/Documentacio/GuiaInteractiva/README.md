# Guia interactiva · Escape Room Revolt

Guia actualitzada el **25/11/2026** (versió 0.2.3). **[Descarrega l'HTML amb la versió 0.2.3](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.3/guia-escape-room-0.2.3.html)** o [consulta la guia en línia](https://xavikai.github.io/EscapeRoomRevolt/escape-room/). Escriu un concepte a «Cerca a la guia» per anar directament al resultat. [Notes de la 0.2.3](../../Framework/RELEASE_NOTES_v0.2.3.md).

Obre `index.html` en un navegador (doble clic). Necessita connexió a Internet per carregar three.js (cdnjs) i les tipografies. Les captures de Unity van incrustades dins del fitxer: es pot copiar sol. Clica una captura per ampliar-la.

Idiomes: català, castellà, anglès, francès i rus (selector a la barra lateral). Les versions francesa i russa parteixen de traducció automàtica amb revisió dels menús i termes principals. Les captures de Unity conserven l'idioma original.

**[Receptes pas a pas](index.html#rec):** configuració dels deu puzles i dels sistemes de la sala (incloent **Canonades (`PipePuzzle`)** amb explicació visual de `Row`, `Column`, `Open Sides`, `Starting Rotation Steps`, muntatge de les peces 3D amb `PipeTileButton` + `SteppedPositioner` i exemple 2×2, **Clau que obre una porta** i **Clavar un o diversos ganivets 3D a la diana** amb `GrabbablePiece.LockInPlace`) des de zero. Cada recepta indica què cal seleccionar, a quin panell es crea cada element i quina referència cal arrossegar a cada camp de l'Inspector. Inclou exemples per reduir els puzles, com passar de tres dianes a una.

**[Ampliar puzles](index.html#expand):** exemples per afegir llums, palanques, notes, dianes i encaixos. També indica com redimensionar o estendre els rodets, el tauler lliscant, les canonades, el teclat i els puzles encadenats.

**[Tauler lliscant en una porta que s’obre cap amunt](index.html#rec/rec_slide_door)** mostra com muntar el tauler a la fulla, desactivar la interacció amb les fitxes en resoldre’l i configurar una frontissa superior.

**[Cinemàtiques i càmeres de feedback](index.html#cinematics)** explica com enquadrar la recompensa de cada puzle, ajustar el retard i la durada, i activar un pla des d'un receptor o un encaix. Inclou captures ampliables de l'Inspector d'Unity amb el camp `Feedback Camera` assignat i els esdeveniments `On Item Accepted` i `On Item Snapped` connectats. També cobreix les intros i els finals amb imatges, vídeos i plans de càmera.

**[Objectes i inventari](index.html#inventory)** explica com crear les fitxes i els objectes recollibles, fer-los servir en portes i receptors, examinar-ne el model en 3D i combinar peces. Inclou una ràdio fixa que sona quan hi poses les piles i una ràdio portàtil que es combina a l'inventari.

**[Triggers narratius i subtítols](index.html#narrative)** explica com col·locar un volum de narració, assignar-hi un clip de veu, escriure i sincronitzar els subtítols i triar quan es repeteix el contingut.

**[Final de partida](index.html#endings)** mostra com configurar victòria i derrota, activar el final en travessar una sortida, resoldre un puzle o completar els objectius i afegir una cinemàtica abans del resultat.

**[Definition i pistes](index.html#definitions):** exemple complet d'un candau amb `Puzzle Definition` i un `Hint Data` de tres pistes. Explica cada camp, els dos enllaços entre escena i assets, IDs únics, inici del context, temporitzadors, tecla H, Hint Zone, finalització i guardat/carregat, amb comprovacions i errors habituals. Disponible en CA/ES/EN. [Referència detallada en català](../../Framework/docs/DEFINITIONS_I_PISTES.md).

**Música i efectes sonors:** nova secció amb importació de clips, música 2D en bucle, ambientació 3D, sons de portes/teclats/objectes, interaccions amb `AudioSource.Play()` i integració amb els volums d'AudioManager. Explica també les fonts independents que no queden connectades automàticament al menú de so.

**[Cursor segons l'acció](index.html#cursors):** com assignar `Cursor Type` a cada interactable, les formes reals del HUD (`Default`, `Hand`, `Eye`, `Puzzle` i `Throw`), la prioritat automàtica de llançament i el cas `Exit`, que encara no té estil propi. Inclou com personalitzar el HUD amb una còpia d'UXML/USS i com provar els canvis.

**[Canviar els models 3D](index.html#models):** jerarquia `Logic`/`Visuals`, substitució manual de la geometria, preparació de prefabs visuals i ús de `Replaceable Model Slot`. Inclou diagrames, els camps `Model Prefab`/`Model Socket`/`Placeholder Visual`, les peces animades amb `Visual Transform`, els pivots, colliders, sockets, ressaltat i comprovacions de joc i guardat. També explica què pot convertir la comanda `Split Selection Into Logic + Visuals`.

Els dos apartats són en CA/ES/EN i es poden obrir des de l'índex lateral o des dels accessos al final dels primers passos. Inclouen quatre captures reals de Unity 6000.6.8f1 amb ampliació: selector Cursor Type, nota amb Eye, components de Visuals i camps de Replaceable Model Slot/Visual Transform. Les captures es van fer en una escena de prova sense desar; no s’han modificat el codi ni les mecàniques del projecte publicat. Les taules de formes de cursor continuen sent esquemes.

Seccions: **crear el teu joc amb la plantilla** (32 passos amb captures de Unity, començant amb el projecte ja obert: jugar la demo, crear una sala amb una porta, un teclat i una nota, afegir veus amb subtítols, pistes automàtiques, zones que disparen accions, compte enrere, perills, finals i objectius, i generar el joc per a Windows; amb sala 3D que es construeix pas a pas, glossari i problemes freqüents), visita guiada amb el museu 3D, laboratori de mecàniques jugables amb Inspector, taller de menús (MenuThemeSettings) amb **disseny del menú pas a pas** (20 passos en dos nivells: tema sense UI Toolkit, amb imatge de fons, imatge del marc i imatges dels botons, i full d'estils propi amb UI Builder; esquema clicable del menú, dissenyador visual que genera l'USS i referència de selectors), jugador PC i VR, funcions (inventari, examen, guardat, pistes, perills, menú de l'editor), recepta de la primera sala i problemes freqüents.

El fitxer font és `EscapeRoom/Documentacio/GuiaInteractiva/index.html` des de l'arrel del repositori. GitHub Pages publica la guia a `/EscapeRoomRevolt/escape-room/`.

Per provar les demos PC actuals: [guia de QA manual](../QA_VISUAL_MANUAL_PC.md) i [registre de resultats](../QA_VISUAL_MANUAL_PC_REGISTRE.md). Els casos estan pendents d’executar.

Paquets de lliurament: [exportacions portables, guia de QA i registres per producte](../../Framework/docs/LLIURAMENT_PC_2026-10-26.md).

Prova directa sense Unity: [demos executables Windows amb guia i registre](../../Framework/docs/DEMOS_WINDOWS_2026-10-26.md).


[Informe de les comprovacions](../../Framework/docs/validation/interactive-guides-2026-10-26.json). La verificació de Node no substitueix la revisió del render WebGL ni de la disposició visual en un navegador.
