# Separación entre UI y simulación visual

## Causa del hotfix

La auditoría descartó pausas directas desde Diario, Mantenimiento, Detalles y Alimentar: no existían asignaciones a `Time.timeScale`, desactivaciones de peces, cambios de Rigidbody2D ni velocidades cero desde esos controladores.

La dependencia problemática estaba en la fuente temporal. Los modales y el mantenimiento funcionan con tiempo no escalado, mientras `FishMovement2D`, `FishView`, `FishVisual2D` y `FishFeedingBehaviour` dependían de `Time.deltaTime`/`Time.time`. Ante cualquier pausa o alteración del tiempo escalado, la UI continuaba funcionando y los peces parecían congelados.

El movimiento visual y la respuesta alimentaria ahora usan tiempo no escalado. Una pausa visual solo puede ocurrir mediante `SetExplicitVisualPause`.

## Input bloqueado no significa simulación pausada

`AquariumInteractionState` mantiene estados independientes:

- `InteractionBlocked`: un modal intercepta los taps.
- `SimulationPaused`: pausa explícita de modelos.
- `FishVisualMovementPaused`: pausa visual excepcional.

Abrir un modal solo implica bloqueo de interacción.

## Velocidad

La velocidad final siempre se calcula desde la velocidad base:

`base × bienestar × comportamiento × mantenimiento visual`

Los multiplicadores no se acumulan y se limitan entre `0.35` y `2`. Solo una pausa explícita devuelve cero. Valores `NaN`, infinitos o negativos se sustituyen por valores seguros.

## Modales y jerarquía

Los peces permanecen en el mundo del acuario. Diario, Detalles y Mantenimiento están bajo `UIRoot/SafeArea` y utilizan imágenes o CanvasGroups exclusivamente para bloquear raycasts. Ocultar un panel no desactiva `FishPopulation` ni `AquariumSwimArea2D`.

## FeedingMode

Alimentar solo cambia el input disponible. El movimiento, adquisición de objetivos, persecución de comida, consumo y animación de mordida continúan usando tiempo visual no escalado.

## Mantenimiento

Preparación, drenado, llenado y estabilización no pausan peces. Un futuro multiplicador visual de mantenimiento dispone de un canal independiente y un mínimo activo de `0.35`.

## Política de Time.timeScale

Los paneles nunca modifican `Time.timeScale`. El movimiento visual permanece observable incluso si una pausa explícita afecta otros modelos. La química conserva su reloj y política de pausa propios.

## Regresión

Las pruebas verifican modales sin pausa, recuperación de input, multiplicadores seguros, valores no finitos y pausa explícita reversible. La prueba manual debe observar posiciones durante cinco segundos en estado normal, Diario, Mantenimiento, Alimentar, Detalles y cambio de agua.

La selección y recuperación de destinos se mantienen en la capa de movimiento. Ningún modal modifica el objetivo, los bounds ni la orientación; consultar `41_FishNavigationAndBoundaryRecovery.md`.
## Decoraciones del mundo

Las decoraciones pertenecen al mundo bajo `DecorationsRoot`, nunca a `SafeArea` ni a un `CanvasGroup`. Abrir Diario, Mantenimiento, Detalles, Hábitat o Alimentación no desactiva sus vistas ni solicita una nueva sincronización.
El editor de hábitat es otro consumidor del bloqueo localizado de interacción. Redirige taps del tanque a `HabitatEditorInputController`, cancela FeedingMode y bloquea acciones incompatibles, pero no modifica `Time.timeScale`, `FishMovement2D` ni `DecorationsRoot`.
