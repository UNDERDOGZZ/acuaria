# Reloj de simulación

`AquariumSimulationClock` acumula tiempo real y emite ticks discretos. Intervalo, multiplicador y máximo de ticks por frame son configurables. Un intervalo de un segundo con multiplicador 60 representa un minuto simulado.

El reloj puede pausarse y reanudarse, ignora deltas inválidos y limita backlog para evitar una espiral de actualización. La simulación continúa en RoomOverview, Detalles, FeedingMode y transiciones. No usa `Time.timeScale` como reloj del dominio.

No existe progreso offline: cerrar la aplicación detiene la simulación y no se guardan timestamps. Los tests pueden llamar `Advance` o `SimulateTick` manualmente.
