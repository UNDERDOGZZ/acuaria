# Sincronización de estado multiacuario

La fuente de verdad es `AquariumInstance`: runtime, agua, nitrógeno, mantenimiento, hábitat, peces, decoraciones, diario y estadísticas. `AquariumRepository` solo almacena; `AquariumManager` coordina; `AquariumContext` conserva exactamente un activo.

`OnActiveAquariumChanging` precede al cambio. Después se actualizan `IsActive` y `OnActiveAquariumChanged`. Los controladores se enlazan una vez por cambio, no por frame. La simulación simplificada de inactivos avanza separadamente y no copia estados.

La presentación de peces se sincroniza por `FishInstanceId`. Cada colección rechaza duplicados y cada vista visible recibe un único estado en el binding activo.

## Política de datos mostrados

Cuando `AquariumHUDController` tiene un `AquariumInstance` enlazado, no acepta conteos del `AquariumInhabitantProvider` legacy. Nombre, definición, litros, temperatura, capacidad, habitantes y cantidad de peces se derivan del agregado activo. `FishCollection.Count` es la autoridad compartida por HUD y tarjetas.

`AquariumSimulationController` calcula residuos usando la colección enlazada. `FishWelfareController` recibe el `FishSpawner2D` del `AquariumViewBinding` activo y usa el volumen de su definición. Al cambiar de contexto se limpian snapshots visuales anteriores y se publican química y bienestar del destino, evitando mezclar datos durante la transición.
