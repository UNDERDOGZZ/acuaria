# Navegación y recuperación de bordes

## Causa del atasco

`FishMovementModel2D` clampleaba la posición, pero conservaba una dirección suavizada que todavía apuntaba fuera del acuario. Al cambiar a un objetivo interior, la interpolación atravesaba una dirección X cercana a cero y el pez permanecía varios segundos moviéndose casi verticalmente contra el cristal. Los destinos Wander tampoco exigían distancia horizontal ni preferían el lado contrario.

Una segunda ruta conservaba el objetivo prioritario de una comida destruida: la semántica `null` de Unity impedía liberar el objetivo después de desaparecer el componente.

## Coordenadas y bounds

Movimiento, posiciones, destinos y comida usan coordenadas locales del `AquariumSwimArea2D`. La conversión visual usa `TransformPoint` y `InverseTransformPoint`.

`LocalBounds` descuenta los márgenes estructurales del tanque. `NavigationBounds` aplica además `horizontalBoundaryPadding` y `verticalBoundaryPadding`. `SwimBounds2D.Inset` limita márgenes excesivos al 49 % para mantener ancho y alto positivos.

## Destinos Wander

`ChooseWanderTarget`:

- trabaja dentro de la zona vertical de la especie;
- prefiere la mitad contraria;
- exige `minimumHorizontalTravelFraction` del ancho útil;
- limita los intentos aleatorios;
- usa un fallback determinista a 5 % del extremo contrario;
- devuelve siempre un punto finito y clampleado.

La llegada acepta distancia total dentro de `targetArrivalThreshold`. Cerca de un borde también acepta distancia horizontal suficiente para que la variación Y no bloquee el cambio.

## Recuperación

Si el pez está cerca de un borde y su dirección u objetivo continúan hacia afuera, el destino se invalida una sola vez, se elige otro interior y la dirección X se orienta inmediatamente hacia el tanque. No hay teletransporte ni aumento extraordinario de velocidad.

Un detector de atasco muestrea progreso horizontal en intervalos configurables. Solo actúa en Wander; SeekFood vertical válido no se considera atasco.

## Flip visual

`FishVisual2D` conserva un dead zone y cambia únicamente la escala de `VisualRoot`. `FishRoot`, bounds y coordenadas no se espejan. La recuperación cambia la dirección efectiva, por lo que el flip ocurre sin depender directamente del target.

## SeekFood a Wander

`FishFeedingBehaviour` registra `hasTarget` independientemente de la referencia Unity. Consumo, expiración, destrucción, pérdida del reclamo y `OnDisable` limpian el objetivo prioritario. `ClearPriorityTarget` genera un nuevo Wander válido.

## Debug

`showGizmos` permanece desactivado por defecto. Al activarlo muestra objetivo, línea de navegación, dirección y recuperación en rojo. No crea objetos ni logs en builds.

## Regresión

EditMode cubre bounds positivos, inset excesivo, destino contrario, distancia mínima, fallback, llegada, detección de borde y dirección interior. Manualmente se deben observar los tres peces durante cinco minutos y repetir Diario, Mantenimiento, Detalles, FeedingMode y cambio de agua.
Los perfiles reales conservan `SwimmingLevel`; Upper, Middle y Lower sesgan destinos sin alterar bounds ni recuperación horizontal.
