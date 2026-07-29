# AquariumSlots

`AquariumSlot` representa un espacio lógico de la habitación y contiene `SlotId`, `State` y la
instancia asignada.

- `Locked`: no acepta asignación.
- `Empty`: puede recibir una instancia creada por el manager.
- `Occupied`: presenta resumen y permite activar su acuario.

Vaciar un slot no elimina automáticamente el acuario. El controlador de habitación debe decidir
explícitamente si solo desasigna o solicita eliminación al manager. Esto evita pérdidas
accidentales y mantiene separadas presentación y ciclo de vida.

La habitación dispone conceptualmente de tres slots. El acabado visual y desbloqueo permanecen
fuera de alcance.
