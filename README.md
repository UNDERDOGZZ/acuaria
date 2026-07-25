# Acuaria

Acuaria es un juego mobile de simulación, colección y decoración de acuarios. Su visión es ofrecer una experiencia acogedora donde cada acuario sea un ecosistema independiente y el jugador aprenda acuarismo mientras construye la habitación de sus sueños.

## Estado

El repositorio contiene únicamente los cimientos técnicos. Este sprint no incluye gameplay, peces, habitación, interfaz, economía, simulación ni guardado.

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

## Contribuir

Crear cambios pequeños y enfocados, respetar los namespaces y límites de assemblies, evitar dependencias desde capas internas hacia UI y acompañar cada nueva funcionalidad con validación apropiada. No agregar contenido generado a `Assets/_Acuaria/Resources` salvo una decisión arquitectónica documentada.

## Roadmap

Los próximos hitos previstos son: navegación y composición de la habitación; modelo de acuarios; simulación gradual del agua; peces y compatibilidad; progresión; guardado versionado; y pulido mobile. Consultar [04_Roadmap.md](docs/04_Roadmap.md).
