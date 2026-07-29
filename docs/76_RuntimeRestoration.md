# Restauración runtime

La restauración comienza con limpieza del repositorio y contexto, evitando duplicados al cargar
varias veces. Definiciones se resuelven por IDs serializados. Peces se reconstruyen como
`FishRuntimeState` y su posición normalizada se convierte y clampa al área de nado. Decoraciones
se resuelven en `DecorationRegistry`; faltantes se omiten.

Agua y nitrógeno recuperan valores y tiempo simulado sin avanzar tiempo offline. Mantenimiento
se normaliza a estado estable. El hábitat se recalcula desde decoraciones, y bienestar y
compatibilidad se recalculan por sus controladores al enlazar el acuario activo. Slots, HUD,
carrusel y cámara consumen `AquariumContext`; no se persisten vistas ni transforms.
# Restauración tras progreso offline

`SaveMapper` restaura necesidades de cada pez y estados propios de cada acuario después de la simulación. No instancia peces adicionales ni altera el acuario activo, carrusel o cámara fuera del flujo existente.
