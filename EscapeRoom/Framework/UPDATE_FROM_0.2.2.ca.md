# Actualitzar un projecte d'alumnes de la 0.2.2 a la 0.2.3

La 0.2.3 canvia alguns fitxers del framework (portes, desar i carregar partides, i el menú). Les escenes, els models, els prefabs i la configuració del grup es mantenen tal com estan. **La manera més segura és substituir sencera la carpeta del paquet**: no conté res del grup. Si comenceu de zero, extraieu el ZIP 0.2.3 i obriu directament la carpeta que conté `Assets`, `Packages` i `ProjectSettings`.

## Abans de copiar

1. Deseu totes les escenes i tanqueu Unity.
2. Feu una còpia completa del projecte del grup, o un commit que inclogui també els fitxers `.meta`.
3. Extraieu `EscapeRoom-0.2.3-source.zip` en una altra carpeta.

## Opció recomanada: substituïu el paquet sencer

1. Al projecte del grup, esborreu la carpeta `Packages/com.xavikai.revolt.common`.
2. Copieu-hi la mateixa carpeta del ZIP 0.2.3.

Si el vostre projecte ve d'un clon del repositori, el paquet és a `EscapeRoom/Framework/SharedPackages/com.xavikai.revolt.common`: n'hi ha prou de fer pull.

Aquesta carpeta és del framework. Si algú del grup hi ha fet canvis, guardeu-los abans i torneu-los a aplicar després.

## Opció alternativa: copieu només els fitxers que canvien

Les rutes són relatives a `com.xavikai.revolt.common`. Copieu només els `.cs`, `.asset` i `.uxml`, i conserveu els `.meta` del vostre projecte.

| Fitxer | Canvi |
| --- | --- |
| `Systems/Interaction/Door.cs` | Portes, calaixos i tapes ja no salten al centre en carregar. |
| `Core/Runtime/Save/SaveManager.cs` | F5 només desa durant la partida; la partida nova reinicia bé els registres. |
| `Core/Runtime/Flow/GameFlowManager.cs`, `Core/Runtime/Flow/EndingDefinition.cs` | Textos finals d'escape room. |
| `Systems/Flow/GameOverTimer.cs`, `MovingHazard.cs`, `TimedGameOverHazard.cs` | No es reinicien en carregar. |
| `Systems/Interaction/EventTriggerZone.cs`, `InteractableNote.cs` | Recorden el seu estat en carregar. |
| `Systems/Inventory/InventoryManager.cs`, `ExamineHotspot.cs`, `ExamineHotspotRegistry.cs` | No es perden objectes en carregar; els punts d'examen es reinicien en una partida nova. |
| `Systems/Puzzle/PuzzleController.cs` | Opció `Invoke On Solved On Load`. |
| `Core/Editor/SaveSystemTools.cs` | Corregeix els Save ID duplicats. |
| `UI/Toolkit/UIToolkitMenuController.cs`, `UI/Toolkit/EscapeRoomMenu.uxml`, `Resources/DefaultLocalizationCatalog.asset` | Menú sense Survival Horror i textos nous traduïts. |

Opcional: els tests nous són a `Systems/Tests/PlayMode/SaveRestoreRegressionTests.cs`.

## Si heu fet servir la solució provisional

Si ja havíeu afegit el component `CorreccioPortaCarregar` a les portes, podeu deixar-lo: funciona bé amb la correcció. Si el voleu treure, traieu primer el component de tots els objectes (cerqueu `t:CorreccioPortaCarregar` a la Hierarchy de cada escena) i després esborreu el script.

## Comproveu-ho

1. Obriu el projecte amb la mateixa versió de Unity que ja feu servir. Espereu que acabi de compilar i reviseu que la Console no mostri errors.
2. Executeu `Escape Room Framework > Validation > Validate Save IDs` i deseu les escenes (Ctrl+S): els Save ID repetits es corregeixen sols.
3. Comproveu la correcció:
   - Feu Play des de `MainMenu` i comenceu una partida nova.
   - Obriu un calaix i deixeu-ne un altre tancat.
   - Deseu la partida, torneu al menú principal i carregueu-la.
   - Cada objecte ha de quedar al seu lloc i en el mateix estat en què el vau deixar.

Les partides desades amb la 0.2.2 es poden continuar carregant. Si un Save ID canvia en corregir duplicats, aquell objecte torna al seu estat inicial en carregar una partida antiga.

Si alguna cosa falla, tanqueu Unity i torneu a la còpia o al commit de seguretat.
