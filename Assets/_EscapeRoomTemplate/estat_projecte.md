# Estat del Projecte: Escape Room Revolt

## 🟢 Sistemes Completats
- [x] **Locomoció i Càmera:** Moviment FPS, control de càmera fluït, cursor bloquejat/desbloquejat.
- [x] **Interacció Base Multiplataforma:** Raycast PC i ponts XRI comparteixen `InteractionDispatcher`; l'outline usa una Renderer Feature d'URP sense duplicar materials.
- [x] **Inventari Contextual:** Emmagatzematge, barra d'accés ràpid independent, equipament, lectura, consum, examen i ús contextual amb confirmació.
- [x] **Combinació d'Objectes:** Sistema de receptes guiat des d'UI Toolkit, amb feedback de compatibilitat i consum configurable dels ingredients.
- [x] **Examinar en 3D:** Càmera secundària i RenderTexture per poder rotar i inspeccionar els objectes recollits detalladament.
- [x] **Panys i Panells:** 
  - `ItemReceiver` genèric per demanar ítems (ex. clau a la porta).
  - Teclat numèric 3D completament interactiu amb codis de pas.
- [x] **Notes i Documents:** Sistema `InteractableNote` unificat, permetent notes fixes a la paret i notes recollibles a l'inventari amb textos extensos a la UI.
- [x] **Mecàniques de Portes/Mobles:** Calibració automàtica de frontisses per Portes, Calaixos i Armaris de forma ultra senzilla.
- [x] **Animacions Simples (`SimpleAnimator`):** Rotació i moviment per script (sense *Animator Controller*) perfecte per obrir baguls o fer girar claus.
- [x] **Botons i Actuadors (`Generic Trigger`):** Botons que disparen *UnityEvents* (encendre llums, reproduir sons, animacions), amb suport per actuar com a "Interruptors" infinitament (`IsToggle`).
- [x] **Sistema de Guardat Universal:** Tres ranures manuals, guardat ràpid, miniatures, metadades i serialització versionada d'inventari, portes, notes i puzles.
- [x] **Àudios Narratius amb Subtítols:** Caixes invisibles (Triggers) per disparar efectes de so i textos de diàleg a la pantalla amb animacions de lliscament i Typewriter.
- [x] **UI Toolkit:** Menú principal, pausa, Save/Load, HUD, inventari, examinador, notes, keypad, resultats i rebinding de controls sense Canvas heretat.
- [x] **Flux i Finals:** Objectius configurables, condicions de victòria/derrota, resultats, reintent i retorn al menú principal.
- [x] **Survival Horror:** Llanterna modular, piles, cordura i esdeveniments de terror configurables.
- [x] **Pistes Progressives:** Pistes manuals i automàtiques associades a definicions de puzle.
- [x] **Preparació VR:** `Player_VR`, adaptadors de plataforma, UI Toolkit en món, mans/hàptics i ponts XRI per a les escenes de demostració.
- [x] **Interacció Física:** Objectes agafables, sockets físics i equipament reemplaçable mitjançant `ModelSocket`.

## 🟡 En Desenvolupament (Pendent d'aprovació)
*(Cap tasca activa actualment)*

## 🔴 Roadmap i Idees Futures
- [ ] **Cinemàtiques (Timeline):** Càmeres de recompensa per mostrar una porta obrint-se a l'altra punta de l'habitació.
- [ ] **Portes Dinàmiques (Peeking):** Com a Outlast o Amnesia, permetre mantenir el clic sobre una porta i obrir-la a poc a poc arrossegant el ratolí cap enrere per mirar si hi ha perill abans d'entrar.
- [ ] **Minijocs de Panell UI:** Connexió de cables, trencaclosques lliscants o canonades.
- [ ] **Moviment Avançat Addicional:** Vault/Climb, escales i pas per trampilles més enllà del sprint, salt i crouch actuals.
- [ ] **Moviment de Càmera (Camera Bobbing/Sway):** Afegir sacsejades (head bobbing) en caminar i inclinar la càmera als costats de forma suau al moure's per donar més realisme físic.
