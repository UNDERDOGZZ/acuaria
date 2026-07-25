# Arquitectura técnica

El código se divide por dominio mediante assembly definitions. `Acuaria.Core` contiene contratos y arranque transversal; `Acuaria.Simulation` depende de Core; Room, Aquarium, Fish y Progression representan gameplay y pueden depender de Core y Simulation; `Acuaria.UI` consume los dominios, nunca al revés.

```text
Core <- Simulation <- Gameplay <- UI
```

Data, Audio, Input y Save son límites explícitos. Sus implementaciones futuras deberán depender de abstracciones internas y evitar estado global. La composición de dependencias ocurrirá durante Bootstrap; no se introduce un contenedor hasta que exista una necesidad demostrada.

Los ScriptableObjects almacenan configuración y definiciones authoring. No son localizadores de servicios ni almacenamiento de partida. La carga futura de contenido deberá usar referencias serializadas o una estrategia explícita; `Resources.Load` no será el mecanismo de gameplay.

## Composición de Room

`Acuaria.Room` contiene componentes visuales pequeños: `RoomCameraFitter` calcula el encuadre ortográfico, `AquariumSlotView` administra una vista opcional sin datos de simulación y `RoomCompositionController` valida la composición inicial. Los prefabs se conectan mediante referencias serializadas; no se realizan búsquedas por frame.

`Acuaria.UI.SafeAreaPanel` queda disponible para futuros paneles UI y no modifica el mundo de Room. La herramienta de Editor `RoomSceneSetup` genera de forma reproducible el arte provisional, prefabs, sorting layers y escena.

`AquariumCarouselRoot` es una estructura espacial sin input, movimiento, snapping ni selección. Un sistema futuro podrá desplazar esa raíz sin modificar los prefabs visuales.

El enfoque separa `AquariumInteractable`, `AquariumFocusTarget`, `RoomViewStateMachine` y `RoomViewController`. Input emite selección; el controlador coordina cámara y UI.

## Dominio Fish

`Acuaria.Fish` separa configuración inmutable (`FishSpeciesDefinition`), estado mutable sin referencias visuales (`FishRuntimeState`), modelo determinista (`FishMovementModel2D`) y adaptadores Unity (`FishMovement2D`, `FishVisual2D`, `FishView`). `FishSpawner2D` compone instancias desde entradas serializadas y `AquariumSwimArea2D` define el espacio válido.
## Dominio de alimentación

`Acuaria.Food` contiene definición, estado runtime, vista, movimiento, área válida, input y controlador de partículas. `Acuaria.Fish` depende de ese dominio para búsqueda y consumo; `Acuaria.Room` solo coordina la UI durante el enfoque. No hay búsquedas de objetos por frame.
