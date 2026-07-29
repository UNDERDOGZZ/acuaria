# Arquitectura multiacuario

`AquariumInstance` es el agregado raíz de un acuario. Dos instancias pueden compartir
`AquariumDefinition`, que es configuración inmutable, pero nunca comparten sus estados runtime.
Cada agregado posee colecciones de peces y decoraciones, perfil de hábitat, química, nitrógeno,
mantenimiento, diario y estadísticas.

Flujo de creación:

`AquariumFactory → AquariumInstance → AquariumRepository → AquariumManager → AquariumContext`

La fábrica construye el grafo completo. El repositorio garantiza identidad ordinal y orden
estable. El manager coordina ciclo de vida y activación. Los consumidores observan el contexto.
Los acuarios inactivos reciben ticks simplificados; el activo conserva la simulación completa.

La configuración ScriptableObject nunca almacena progreso. No hay economía, persistencia,
desbloqueos ni simulación offline en este sprint.

## Independencia

- Alimentación añade residuos y eventos únicamente al agregado activo.
- Mantenimiento opera sobre su `MaintenanceState` y `WaterState`.
- Diario y estadísticas pertenecen a la instancia.
- Peces y decoraciones tienen colecciones distintas aunque usen las mismas definiciones.
- Cambiar de acuario no destruye el anterior ni recarga la escena.

Los controladores legacy deben migrarse mediante adaptadores de enlace al contexto; no deben
cachear para siempre referencias del acuario inicial.
