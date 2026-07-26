# Pipeline visual de decoraciones

## Causa del hotfix

El sistema anterior copiaba `DecorationDefinition` en `AquariumHabitatController`, agregaba sus contribuciones y actualizaba UI/bienestar. `DecorationView` no era instanciada, no existía `DecorationsRoot` ni un spawner inicializado y los botones Debug solo modificaban datos.

## Flujo

`AquariumDefinition` → `DecorationPlacementData` → `AquariumHabitatController` → `DecorationSpawner2D` → `DecorationView` → `SpriteRenderer` bajo `DecorationsRoot`.

Una definición representa un tipo. Cada colocación representa una instancia, posee `InstanceId` estable, posición normalizada, rotación, escala, flip, offset de sorting, capa visual y flags de estado. `AquariumDecorationArea2D` convierte `[0,1]` a coordenadas locales dentro del cristal.

El spawner mantiene un diccionario `InstanceId → DecorationView`. Una sincronización puntual crea faltantes, actualiza existentes y elimina retiradas; repetirla no duplica objetos. Se ejecuta al inicializar, añadir, quitar o restablecer.

## Presentación

La política usa `AquariumFront`, con órdenes relativos por capa: fondo -3, sustrato -2, midground -1 y foreground 1. Así las decoraciones quedan delante del fondo y el suelo opacos sin mezclarse con el Canvas de UI. No se cambió el culling mask ni se añadieron máscaras.

Como los assets iniciales no incluyen sprites ni prefabs artísticos, `DecorationView` crea un sprite rectangular compartido y aplica un color por categoría. El sustrato es una franja, plantas son verticales, roca/cueva se apoyan abajo y el tronco se inclina. No hay materiales únicos, colliders, pathfinding ni recursos descargados.

## Eventos y depuración

`DecorationAdded`, `DecorationRemoved`, `DecorationsChanged`, `Changed` y los eventos del spawner informan creación, retiro y sincronización. Los botones Debug usan exactamente `AddById`/`RemoveById`/`ResetHabitat`; por tanto, vista, perfil, bienestar y panel cambian juntos.

## Regresión

Verificar composición inicial, añadir/quitar/restablecer, reaperturas de paneles y reenfoques. Debe existir una vista por ID, todos los puntos deben quedar dentro de `AquariumDecorationArea2D`, los peces deben nadar y la Console permanecer sin errores. EditMode cubre validación de colocación, conversión del área y sincronización idempotente.
