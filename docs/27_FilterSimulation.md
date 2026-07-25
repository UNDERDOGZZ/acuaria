# Simulación del filtro biológico

`FilterDefinition` contiene configuración constante y `FilterRuntimeState` conserva actividad, eficiencia, suciedad, capacidad y mantenimiento. `FilterSimulationModel` es puro, determinista y avanza con los ticks existentes.

La suciedad crece con tiempo y carga orgánica, reduciendo la eficiencia. El filtro limpio aporta capacidad biológica a la conversión de amoníaco y nitritos; apagado no aporta capacidad.

El enjuague suave elimina menos suciedad pero conserva aproximadamente 90% de la capacidad bacteriana. La limpieza profunda elimina más suciedad y conserva aproximadamente 45%, por lo que puede desestabilizar temporalmente el ciclo.

Solo existe `StarterInternalFilter`. No hay compra, reemplazo, caudal real, consumo eléctrico ni marcas comerciales.
