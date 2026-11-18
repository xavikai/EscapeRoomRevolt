# Actualitzar un projecte d'alumnes de la 0.2.1 a la 0.2.2

Si el grup ja ha fet escenes, models o puzles, actualitzeu només els scripts del framework. Mantingueu el projecte del grup com a projecte principal. Per començar de zero, extraieu el ZIP 0.2.2 i obriu directament la carpeta que conté `Assets`, `Packages` i `ProjectSettings`.

## Abans de copiar

1. Deseu totes les escenes i tanqueu Unity. Feu una còpia completa del projecte del grup o un commit que inclogui també els fitxers `.meta`. Comproveu que la còpia es pot recuperar.
2. Extraieu `EscapeRoom-0.2.2-source.zip` en una altra carpeta. Feu servir els fitxers extrets com a font de l'actualització.
3. Localitzeu la carpeta `Packages/com.xavikai.revolt.common` als dos projectes. Si el vostre projecte ve d'un clon del repositori, el paquet viu a `EscapeRoom/Framework/SharedPackages/com.xavikai.revolt.common`. En aquest cas, apliqueu els canvis al paquet compartit del clon o integreu el commit de la release amb Git.

## Copieu aquests scripts

Les rutes següents són relatives a `com.xavikai.revolt.common`. Copieu els fitxers de la 0.2.2 a les mateixes rutes del projecte del grup.

| Fitxer | Canvi |
| --- | --- |
| `Systems/Puzzle/ThrowTarget.cs` | Canvi de color de les dianes amb el model en un fill. |
| `Systems/Puzzle/PieceSocketReceiver.cs` | Canvi de color dels encaixos amb el model en un fill. |
| `Systems/Interaction/InteractionManager.cs` | El cos del jugador deixa d'interceptar els clics de la càmera del puzle. |
| `Core/Editor/EscapeRoomShowcaseExpansionBuilder.cs` | Nou menú per crear un circuit de llums. |
| `Core/Editor/PuzzleCreator.cs` | Connexió dels controls del circuit. Copieu-lo junt amb el fitxer anterior. |

Si també voleu que el menú de build faci servir la versió configurada a Player Settings, copieu `Core/Editor/ReleaseBuilder.cs`. Això és opcional per als puzles.

Conserveu els `.meta` del projecte del grup, les seves escenes, prefabs, models, assets i ajustos. Conserveu també el `manifest.json` i el `packages-lock.json` del grup. Si heu editat algun dels scripts de la taula, compareu els dos fitxers i fusioneu els canvis abans de copiar. No cal regenerar cap puzle existent ni canviar el seu `Persistent Id`.

## Comproveu el projecte actualitzat

1. Obriu el projecte amb la mateixa versió de Unity que ja utilitza el grup. La release 0.2.2 s'ha validat amb Unity `6000.6.8f1`. Si també decidiu actualitzar l'Editor, proveu aquest canvi primer en una còpia.
2. Espereu que Unity recompili i reviseu la Console. A `Escape Room Framework > Create > Puzzles` ha d'aparèixer `Linked Lights Puzzle`.
3. Proveu les escenes pròpies amb una partida nova. Comproveu l'impacte d'una diana, l'encaix, les fletxes de les rodetes i la recompensa de cada puzle.
4. Si afegiu un circuit nou, assigneu-li una `Puzzle Definition` amb un ID propi i connecteu les seves recompenses a `On Solved`. El circuit de mostra es resol amb els botons 1, 3 i 5.
5. Si feu servir partides guardades, comproveu una partida de prova després de carregar. Si heu afegit o reordenat elements del puzle, valideu també el resultat amb una partida nova.

Si l'actualització falla, tanqueu Unity i torneu a la còpia o al commit de seguretat. Per consultar la documentació nova, podeu obrir l'HTML 0.2.2 amb el navegador directament.
