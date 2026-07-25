# Acuaria

Acuaria es un juego mobile de simulación, colección y decoración de acuarios. Su visión es ofrecer una experiencia acogedora donde cada acuario sea un ecosistema independiente y el jugador aprenda acuarismo mientras construye la habitación de sus sueños.

## Estado

El proyecto incluye sus cimientos técnicos y la primera composición provisional de la habitación principal. `Room` presenta una habitación 2D frontal, un acuario inicial, tres slots, cámara ortográfica adaptable e iluminación URP 2D. Todavía no incluye peces, interacción, interfaz visible, economía, simulación ni guardado.

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

Los próximos hitos previstos son: interacción y navegación de habitación; modelo de acuarios; simulación gradual del agua; peces y compatibilidad; progresión; guardado versionado; y pulido mobile. Consultar [04_Roadmap.md](docs/04_Roadmap.md) y [11_RoomScene.md](docs/11_RoomScene.md).
