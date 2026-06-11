# Estat del Projecte: Escape Room Revolt

## 🟢 Sistemes Completats
- [x] **Locomoció i Càmera:** Moviment FPS, control de càmera fluït, cursor bloquejat/desbloquejat.
- [x] **Interacció Base:** Raycast pel centre de la pantalla, detecció d'objectes al layer "Interactable", i contorn (Outline) automàtic en mirar objectes gràcies a l'`InteractionManager`.
- [x] **Inventari Actiu:** Recollir objectes (`PickableItem`), guardar-los a la motxilla, i usar-los.
- [x] **Combinació d'Objectes:** Sistema de receptes (`Combinations`) per fusionar ítems arrossegant-los (Drag & Drop) a l'inventari, amb l'opció de destruir els originals.
- [x] **Examinar en 3D:** Càmera secundària i RenderTexture per poder rotar i inspeccionar els objectes recollits detalladament.
- [x] **Panys i Panells:** 
  - `ItemReceiver` genèric per demanar ítems (ex. clau a la porta).
  - Teclat numèric 3D completament interactiu amb codis de pas.
- [x] **Notes i Documents:** Sistema `InteractableNote` unificat, permetent notes fixes a la paret i notes recollibles a l'inventari amb textos extensos a la UI.
- [x] **Mecàniques de Portes/Mobles:** Calibració automàtica de frontisses per Portes, Calaixos i Armaris de forma ultra senzilla.
- [x] **Animacions Simples (`SimpleAnimator`):** Rotació i moviment per script (sense *Animator Controller*) perfecte per obrir baguls o fer girar claus.
- [x] **Botons i Actuadors (`Generic Trigger`):** Botons que disparen *UnityEvents* (encendre llums, reproduir sons, animacions), amb suport per actuar com a "Interruptors" infinitament (`IsToggle`).
- [x] **Sistema de Guardat Universal:** Serialització JSON d'Inventaris, Portes, Notes i Puzles. Restauració automàtica d'estats.
- [x] **Àudios Narratius amb Subtítols:** Caixes invisibles (Triggers) per disparar efectes de so i textos de diàleg a la pantalla amb animacions de lliscament i Typewriter.

## 🟡 En Desenvolupament (Pendent d'aprovació)
*(Cap tasca activa actualment)*

## 🔴 Roadmap i Idees Futures
- [ ] **Cinemàtiques (Timeline):** Càmeres de recompensa per mostrar una porta obrint-se a l'altra punta de l'habitació.
- [ ] **Condició de Victòria (End Game):** Mecanisme per finalitzar el joc amb una fosa a negre i estadístiques.
- [ ] **Sistema de Pistes (Hint System):** Integració de botons d'ajuda progressiva per quan el jugador es quedi encallat.
- [ ] **Mecànica de Llanterna / Llum UV:** Eina dinàmica que reveli pistes invisibles en l'entorn.
- [ ] **Portes Dinàmiques (Peeking):** Com a Outlast o Amnesia, permetre mantenir el clic sobre una porta i obrir-la a poc a poc arrossegant el ratolí cap enrere per mirar si hi ha perill abans d'entrar.
- [ ] **Interacció Física amb Objectes:** Sistema d'agafar i arrossegar caixes o cadires pel mapa.
- [ ] **Minijocs de Panell UI:** Connexió de cables, trencaclosques lliscants o canonades.
- [ ] **Transició entre Nivells:** Sistema per canviar d'habitació mantenint els ítems.
- [ ] **Moviment Avançat del Jugador:** Afegeix la capacitat de córrer (Sprint), ajupir-se (Crouch) per passar per llocs estrets, enfilar-se (Vault/Climb) a caixes o escales per arribar a trampilles, i un sistema de sons de petjades.
- [ ] **Moviment de Càmera (Camera Bobbing/Sway):** Afegir sacsejades (head bobbing) en caminar i inclinar la càmera als costats de forma suau al moure's per donar més realisme físic.
- [ ] **Sistema de Menús complet:** Menú Principal per iniciar partida/continuar, Menú de Configuració (amb ajustaments de volum, sensibilitat de ratolí i gràfics), i polir el Menú de Pausa (Escape).
