# Comandos del editor

`IHabitatEditCommand` define `Execute` y `Undo` sobre la lista provisional. `AddDecorationCommand`, `RemoveDecorationCommand` y `ReplacePlacementCommand` cubren añadir, quitar, mover, rotar y voltear.

`HabitatEditHistory` conserva un número configurable de acciones. Un drag genera un único reemplazo al soltar, no comandos por frame. Undo restaura la copia anterior en orden LIFO. Confirmar y cancelar limpian el historial; comandos duplicados o referencias inválidas fallan sin modificar la composición.
