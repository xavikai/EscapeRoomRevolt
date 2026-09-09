# Escape Room — preparación comercial

Revisión: **9 de septiembre de 2026**. Alcance: Escape Room PC y VR con mandos. Survival Horror queda fuera de esta entrega. Véanse [la auditoría y sus límites](AUDITORIA_ESCAPE_ROOM_2026-09-09.md) y [la guía para el comprador](Documentation/ESCAPE_ROOM_QUICKSTART.md).

## Verificado en el proyecto de desarrollo

- [x] Compilación en el editor Unity 6000.4.9f1 tras las correcciones.
- [x] 20/20 pruebas Edit Mode y 23/23 Play Mode; incluyen regresiones de guardado, VR, secuencias, estados y visuales de socket.
- [x] Siete escenas inspeccionadas sin scripts ausentes ni referencias serializadas rotas detectadas; IDs de guardado sin duplicados dentro de cada escena revisada.
- [x] Los museos PC y VR contienen los mismos 12 controladores de puzle, todos con definición y pistas.
- [x] Framework Smoke Test: PASS sin avisos. Validador del museo PC: 2 UIDocuments, 12 puzles, 112 estados y 16 ítems.
- [x] Arranque en Play de ShowcaseMuseum y ShowcaseMuseumVR con GameContext inicializado y adaptador de plataforma correcto.
- [x] Guía de uso por mecánica, configuración VR, límites de guardado y matriz de aceptación disponibles desde la documentación y el menú del editor.
- [x] El constructor de release Windows ya no incluye la demo Survival Horror.
- [x] Build Windows: 0 errores y 0 avisos; arranque adicional del ejecutable sin gráficos y sin excepciones registradas.
- [x] Build Android: 0 errores. Conserva 426 avisos de shaders de `com.unity.render-pipelines.core`; su validación visual en visor sigue pendiente.

Estas comprobaciones no equivalen a una partida completa ni a una certificación de hardware. En el editor PC se observaron mensajes del controlador de audio XR con retorno al dispositivo predeterminado; deben revisarse en el ejecutable final.

## Pendiente antes de publicar como versión comercial estable

- [ ] Completar la matriz de QA en cada visor/mando anunciado, incluyendo suspensión, pérdida de tracking, UI, lanzamiento, todas las salas y rendimiento sostenido.
- [ ] Ejecutar una partida completa en los binarios finales PC y Quest y verificar guardado tras cerrar/reabrir el ejecutable.
- [ ] Importar una copia limpia de la distribución, sin Library ni herramientas privadas, y construir desde ella.
- [ ] Resolver la procedencia de los cinco audios pendientes y revisar fuentes, muestras y avisos de paquetes en [ThirdPartyNotices.md](ThirdPartyNotices.md).
- [ ] Revisar todos los textos del producto en los idiomas anunciados; el selector ES/EN no cubre automáticamente contenido de escenas ni contenido del comprador.
- [ ] Definir el arte y la presentación comercial de la demo: actualmente es una muestra funcional con geometría provisional.

## Flujo de autoría y empaquetado

1. Crear una escena propia con gestor, jugador de una sola plataforma, colliders e iluminación.
2. Configurar datos, IDs únicos, pistas y eventos; al duplicar, comprobar los IDs copiados.
3. Añadir rutas de escena y catálogos necesarios. Mantener XR instalado incluso en PC mientras Player dependa de sus tipos.
4. Ejecutar los validadores y las pruebas, y completar el recorrido con guardado/carga.
5. Preparar una copia de distribución excluyendo caches, logs, builds y MCP. No eliminar scripts Survival aisladamente: todavía hay dependencias desde sistemas compartidos.
6. Registrar versión, dispositivo, resultado y limitaciones de cada build. El informe de auditoría distingue los resultados del editor de los binarios y del hardware.

**Estado: beta en preparación comercial.** La matriz en visor físico y las comprobaciones de distribución siguen siendo requisitos abiertos.
