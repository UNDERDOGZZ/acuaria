# Acuaria

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
