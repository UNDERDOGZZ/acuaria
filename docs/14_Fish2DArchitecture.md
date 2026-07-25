# Arquitectura de peces 2D

## Responsabilidades

- `FishSpeciesDefinition`: datos constantes y validados.
- `FishRuntimeState`: identidad, posición, dirección, velocidad, destino, semilla y tiempo; no conoce escena ni render.
- `FishMovementModel2D`: destinos deterministas, avance y separación.
- `AquariumSwimArea2D`: límites, márgenes, clamp y zonas verticales.
- `FishMovement2D`: adapta modelo, tiempo y Transform.
- `FishVisual2D`: flip, color, escala y animación procedural.
- `FishView`: coordinación ligera.
- `FishSpawner2D`: composición única desde entradas serializadas.

El flujo es `FishSpawnEntry → Species → RuntimeState → MovementModel → Movement2D → Visual2D`. La lógica no depende de `SpriteRenderer`. El prefab `Fish2D` usa piezas simples bajo `VisualRoot`; `FishPopulation` contiene el área y tres entradas con semillas diferentes.

## Limitaciones

No hay alimentación, salud, edad, reproducción, compatibilidad, química, persistencia, pooling, boids ni colisiones físicas.
## Integración con alimentación

`FishFeedingBehaviour` está separado de vista y locomoción. Consulta `AquariumFoodController`, reclama una partícula antes de fijar un objetivo prioritario, solicita feedback visual al consumir y libera el reclamo al deshabilitarse.

`FishSpawner2D` conserva además un registro pequeño de las definiciones instanciadas y emite `PopulationChanged`. `AquariumInhabitantProvider` copia ese registro bajo demanda y agrupa por especie; la UI no inspecciona GameObjects en `Update`.
El movimiento recibe un multiplicador absoluto de bienestar; nunca calcula necesidades ni modifica la velocidad base.
