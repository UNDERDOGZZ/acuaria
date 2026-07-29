# Acuaria

## Sprint 14 — guardado local

La partida se captura en DTOs puros y se guarda como JSON versionado en
`Application.persistentDataPath`. La escritura usa temporal, verificación SHA-256, archivo
principal y backup; una carga inválida intenta el respaldo y una versión futura nunca se
sobrescribe. `SaveCoordinator` restaura el repositorio multiacuario antes de que la UI cree
contenido por defecto y realiza autosave con dirty state y debounce.

Documentación: [arquitectura](docs/70_SaveSystemArchitecture.md),
[esquema](docs/71_SaveDataSchema.md) y [guía de pruebas](docs/77_SaveTestingGuide.md).

## Sprint 13 — arquitectura multiacuario

El dominio incorpora `AquariumInstance` como agregado independiente, creado exclusivamente por
`AquariumFactory`, almacenado en `AquariumRepository` y coordinado por un único
`AquariumManager`. `AquariumContext` publica el acuario activo sin convertirlo en estado global
mutable. Cada instancia posee runtime, agua, mantenimiento, nitrógeno simplificado, peces,
decoraciones, hábitat, diario y estadísticas propios.

Los tres espacios de habitación se modelan mediante `AquariumSlot` (`Locked`, `Empty`,
`Occupied`). La migración de los controladores visuales existentes al contexto activo es
incremental para preservar el vertical slice y evitar que IDs `starter-*` vuelvan a convertirse
en fuentes de verdad.

Documentación: [arquitectura](docs/57_MultiAquariumArchitecture.md),
[manager](docs/58_AquariumManager.md), [slots](docs/59_AquariumSlots.md) y
[contexto](docs/60_AquariumContext.md).

## Sprint 12 — editor visual de hábitat

El panel Hábitat permite abrir un editor provisional, seleccionar y arrastrar decoraciones, añadirlas desde una bandeja, quitarlas, girarlas, voltearlas y deshacer. La sesión trabaja sobre una copia: `Confirmar` aplica y recalcula el hábitat; `Cancelar` restaura la composición. Los peces y el agua continúan activos y no se usa `Time.timeScale`.

Documentación: [arquitectura](docs/52_HabitatEditorArchitecture.md), [validación](docs/53_DecorationPlacementValidation.md), [comandos](docs/54_HabitatEditCommands.md), [input](docs/55_HabitatEditorInput.md) y [UI](docs/56_HabitatEditorUI.md).

## Sprint 11 — hábitat funcional

Acuaria dispone de decoraciones y plantas data-driven mediante `DecorationDefinition` y `DecorationRegistry`. La composición inicial vive en `AquariumDefinition`, se agrega en `AquariumHabitatProfile` y afecta al bienestar únicamente cuando una especie requiere plantas, escondites o espacio abierto. El panel Hábitat y el catálogo de decoraciones son educativos y de solo lectura; no existen tienda, monedas, inventario ni colocación manual.

Documentación: [sistema de decoraciones](docs/48_DecorationSystem.md), [simulación de hábitat](docs/49_HabitatSimulation.md), [catálogo](docs/50_DecorationCatalog.md) y [pipeline visual](docs/51_DecorationVisualPipeline.md).

Acuaria es un juego mobile de simulación, colección y decoración de acuarios. Su visión es ofrecer una experiencia acogedora donde cada acuario sea un ecosistema independiente y el jugador aprenda acuarismo mientras construye la habitación de sus sueños.

## Estado

El proyecto incluye sus cimientos técnicos y un vertical slice jugable de la habitación principal. `Room` presenta una habitación 2D frontal, un acuario inicial con tres peces, enfoque interactivo, alimentación y un HUD responsive con ficha de detalles. Química, economía, mantenimiento, inventario y guardado siguen fuera de alcance.

## Estructura

- `Acuaria/`: proyecto Unity.
- `Acuaria/Assets/_Acuaria/`: código y assets propios, separados de assets de terceros y configuración del template.
- `docs/`: visión, diseño y decisiones técnicas.

La arquitectura separa `Core`, `Simulation`, dominios de gameplay y `UI` mediante assembly definitions. Las dependencias apuntan hacia capas más internas.

## Abrir el proyecto

1. Instalar Unity `6000.5.5f1`.
2. En Unity Hub, añadir la carpeta `Acuaria`.
3. Abrir `Assets/_Acuaria/Scenes/Bootstrap.unity`.
4. Confirmar que `Bootstrap`, `MainMenu` y `Room` aparecen en Build Profiles en ese orden.
5. Ejecutar desde Bootstrap para cargar Room, o abrir Room directamente para trabajo visual.

## Contribuir

Crear cambios pequeños y enfocados, respetar los namespaces y límites de assemblies, evitar dependencias desde capas internas hacia UI y acompañar cada nueva funcionalidad con validación apropiada. No agregar contenido generado a `Assets/_Acuaria/Resources` salvo una decisión arquitectónica documentada.

## Roadmap

Room permite seleccionar el acuario ocupado, enfocarlo suavemente y volver mediante una UI mínima respetando Safe Area.

Room cuenta ahora con una composición refinada: acuario central dominante, dos slots futuros y una raíz estructural preparada para un carrusel futuro sin interacción.

El acuario inicial contiene tres peces 2D provisionales. Cada uno usa una especie, semilla, nivel de nado, velocidad y fase visual diferentes; alimentación, química, economía y guardado siguen fuera de alcance.

Los próximos hitos previstos son: interacción y navegación de habitación; modelo de acuarios; simulación gradual del agua; peces y compatibilidad; progresión; guardado versionado; y pulido mobile. Consultar [04_Roadmap.md](docs/04_Roadmap.md) y [11_RoomScene.md](docs/11_RoomScene.md).

## Alimentación interactiva

El vertical slice permite enfocar el acuario, activar `Alimentar`, tocar la superficie y observar cómo los peces reclaman y consumen escamas antes de reanudar su nado. Incluye límite de comida activa y feedback educativo. Consultar [16_FeedingSystem.md](docs/16_FeedingSystem.md) y [17_FishBehaviourStates.md](docs/17_FishBehaviourStates.md).

## HUD del acuario

La vista enfocada muestra nombre, volumen nominal, temperatura, población y estado general mediante campos TextMeshPro independientes. La barra superior cambia entre una fila Wide y dos filas Compact según el ancho disponible, siempre dentro del Safe Area. Una ficha móvil presenta rango recomendado, capacidad provisional, habitantes agrupados y un consejo educativo sin introducir química dinámica. Consultar [18_AquariumDataModel.md](docs/18_AquariumDataModel.md), [19_AquariumHUDAndDetails.md](docs/19_AquariumHUDAndDetails.md), [20_AquariumStatusEvaluation.md](docs/20_AquariumStatusEvaluation.md) y [21_ResponsiveHUDLayout.md](docs/21_ResponsiveHUDLayout.md).

## Calidad del agua

El acuario inicial incorpora una simulación educativa determinista del ciclo del nitrógeno: residuos, amoníaco total simplificado, nitritos, nitratos y dos colonias bacterianas. Funciona mediante ticks discretos y eventos, sin progreso offline, daño a peces ni cambios de agua jugables. Consultar [22_WaterChemistryModel.md](docs/22_WaterChemistryModel.md), [23_NitrogenCycleSimulation.md](docs/23_NitrogenCycleSimulation.md), [24_AquariumSimulationClock.md](docs/24_AquariumSimulationClock.md) y [25_WaterQualityUI.md](docs/25_WaterQualityUI.md).
Acuaria incluye mantenimiento básico: cambios parciales de agua con previsualización y un filtro biológico simplificado con suciedad, eficiencia, enjuague suave y limpieza profunda. Consulta `docs/26_WaterChangeSystem.md` a `docs/29_MaintenanceEducationalDesign.md`.
Acuaria también evalúa necesidades ficticias de especie, bienestar gradual, ocupación y compatibilidad básica sin daño ni muerte. Consulta `docs/30_FishCareRequirements.md` a `docs/34_FishWelfareUI.md`.
Acuaria incorpora un Diario del Acuarista con XP educativa, niveles, misiones, códice, logros y estadísticas de sesión. No utiliza monedas, energía, anuncios ni guardado. Consulta `docs/35_PlayerProgression.md` a `docs/39_PlayerStatistics.md`.
Los paneles modales bloquean únicamente input: peces, animación y búsqueda de alimento continúan en segundo plano. Consulta `docs/40_UIAndSimulationSeparation.md`.
La navegación de peces selecciona destinos interiores en el lado opuesto, detecta clamps y recupera movimiento horizontal sin teletransporte. Consulta `docs/41_FishNavigationAndBoundaryRecovery.md`.
Acuaria incorpora cinco especies reales mediante assets data-driven, un registro validado y presets de población, sin compra ni economía. Las recomendaciones están revisadas, no verificadas; consulta `docs/42_FishSpeciesContentPipeline.md` a `docs/47_SpeciesResearchSources.md`.
# Hotfix Sprint 13

La navegación multiacuario usa `AquariumContext` como única fuente del acuario activo. El acuario inicial migra sus peces visibles a `FishCollection`; tarjetas, HUD y presentación se enlazan al mismo agregado. Véanse `docs/57_MultiAquariumArchitecture.md` a `docs/65_LegacyAquariumMigration.md`.

RoomOverview usa ahora un carrusel espacial: raíces reales por slot, preview lateral y transición ortográfica animada compartida por botones y swipe. Véanse `docs/66_AquariumCarouselLayout.md` a `docs/69_RoomOverviewComposition.md`.

El escenario cubre todo el recorrido horizontal; los tanques conservan escala uniforme, foco propio y parámetros independientes. El HUD, las tarjetas y la población visual se sincronizan con el `AquariumInstance` activo.
