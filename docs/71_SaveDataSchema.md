# Esquema de datos

La raíz `AcuariaSaveData` identifica el formato, schema `1`, versión del juego, SaveId,
timestamps UTC, acuario activo, progreso, slots, acuarios, estadísticas e integridad.

Cada acuario persiste IDs estables, definición, slot, runtime, agua, nitrógeno, mantenimiento,
filtro preparado, peces, decoraciones, diario, hábitat y snapshots derivados. Peces guardan
especie, saciedad, semilla, dirección y posición normalizada respecto a `AquariumSwimArea2D`;
no guardan target, velocidad por frame ni coroutine. Decoraciones guardan DefinitionId,
posición normalizada, rotación, escala, flip, orden y capa.

pH, GH y KH permanecen en el DTO para compatibilidad futura, pero el modelo actual no los
simula y por tanto no se presentan como estado restaurable real. Misiones, logros y progreso
global tienen DTOs preparados; solo deben mapearse cuando sus controladores expongan una API
de restauración idempotente.
