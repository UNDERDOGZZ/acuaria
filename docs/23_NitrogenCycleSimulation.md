# Simulación del ciclo del nitrógeno

En cada tick:

1. Los peces agregan `cantidad × tasa × horas` de residuos.
2. Una fracción `residuos × tasa de descomposición × horas` se convierte en amoníaco y se divide por litros.
3. Amoníaco convertido = `disponible × bacterias AOB × tasa × horas`.
4. Nitrito convertido = `disponible × bacterias NOB × tasa × horas`.
5. Cada colonia crece lentamente con su recurso y pierde población lentamente sin él.

Cada conversión se limita a la sustancia disponible. El paso es determinista, sin `Random`, y limita pasos grandes a 24 horas para estabilidad. `AquariumCycleEvaluator` distingue sin ciclar, ciclando, casi ciclado, ciclado e inestable.

La comida expirada se deduplica por ID. La consumida no expira y aporta solo la fracción mínima configurada. El modelo es educativo, no una conservación molar científica. No existen plantas, filtros ni cambios de agua.
# Capacidad del filtro

La conversión recibe un multiplicador biológico determinista derivado de la eficiencia y capacidad del filtro.
La mejora química puede elevar gradualmente el objetivo de bienestar, sin curación instantánea.
