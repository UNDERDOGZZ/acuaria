# Migración del acuario legacy

La desincronización original tenía dos fuentes: el HUD consultaba el `FishSpawner2D` visible y las tarjetas consultaban una `FishCollection` vacía. Al iniciar, el binder fuerza el spawn una sola vez y registra sus tres `FishRuntimeState` en el `AquariumInstance` inicial.

Los paneles principales reciben el acuario activo mediante `Bind(AquariumInstance)`. Persisten riesgos legacy en controladores auxiliares que aún consultan componentes de escena; deben migrarse al contexto cuando entren en alcance funcional, sin introducir una segunda colección.
