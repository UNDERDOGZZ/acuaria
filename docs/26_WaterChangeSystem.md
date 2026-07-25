# Sistema de cambio parcial de agua

El Sprint 7 permite cambios del 10%, 25%, 40% y 50%; 25% es la opción recomendada. La previsualización usa el mismo `WaterChangeModel` que la aplicación final y nunca modifica el estado.

La simplificación es `nuevo = actual × (1 − fracción)` para amoníaco, nitritos y nitratos. Los residuos usan un factor configurable. El agua nueva se asume acondicionada, a igual temperatura y sin concentraciones relevantes. No se modelan cloro, pH, GH ni KH.

El flujo pasa por `Preparing`, `Draining`, aplicación atómica única, `Refilling`, `Stabilizing` y `Completed`. Un cooldown real corto evita spam. El reloj químico no se reinicia y las bacterias no cambian durante un cambio normal.

La animación es estética y moderada; no cambia litros nominales ni el área lógica de nado, de modo que los peces no se reinician ni quedan fuera del agua. Las acciones incompatibles quedan bloqueadas hasta terminar.
Después de un cambio se reevalúa el agua y el bienestar evoluciona gradualmente hacia el nuevo objetivo.
