# Sistema de alimentación

## Alcance

Interacción de alimentación 2D modular y provisional. No modela inventario, costes, hambre persistente, química ni nutrición compleja.

## Flujo

`FeedingUIController` activa el modo. `FeedingInputController` proyecta y valida el toque con `AquariumFeedingArea2D`. `AquariumFoodController` crea tres partículas hasta un máximo de doce.

Cada partícula combina `FoodDefinition`, `FoodRuntimeState`, `FoodView2D` y `FoodMovement2D`.

## Reglas

- Input solo con acuario enfocado y modo activo.
- El toque se ajusta a la superficie.
- Una partícula solo puede estar reclamada por un pez.
- El controlador crea, reclama, consume y retira.
- El máximo produce feedback sin crear objetos.
- Volver a la vista general cancela el modo.

## Configuración y validación

`Acuaria/Setup Feeding System` reconstruye assets y enlaces. Las pruebas EditMode cubren definición, estados, reclamos, área válida, máximo y consumo.

Abrir `AquariumDetailsPanel` cancela el modo de alimentación y oculta sus instrucciones, sin destruir comida existente. Cerrar detalles no reactiva el modo automáticamente.
# Integración con química

`AquariumFoodController` emite `FoodExpired` y `FoodConsumed` usando el ID de instancia. La simulación deduplica expiraciones; alimento consumido nunca se procesa como expirado y solo aporta una fracción metabólica mínima configurable.
La saciedad es una entrada simplificada del bienestar. Comida expirada no cuenta como consumo.
