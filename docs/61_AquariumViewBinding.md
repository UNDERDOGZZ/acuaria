# Aquarium view binding

Sprint 13 usa una presentación reutilizable enlazada explícitamente al `AquariumContext`. `AquariumContextBinder` descarga el contexto lógico anterior y vuelve a enlazar HUD, simulación, mantenimiento, diario, hábitat y peces al agregado activo. La UI nunca es fuente de verdad.

`FishSpawner2D` migra una sola vez los peces iniciales a `FishCollection` y, en cambios posteriores, reutiliza las vistas disponibles: activa solo las necesarias y las enlaza a los `FishRuntimeState` del acuario actual. Esto evita contar GameObjects o duplicar vistas.

Esta variante no mantiene tres copias del tanque en memoria. `VisualRootId` documenta el binding lógico y deja preparado el reemplazo futuro por un registro de vistas persistentes.
