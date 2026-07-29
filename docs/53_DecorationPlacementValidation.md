# Validación de colocación

La fuente de verdad usa coordenadas normalizadas `[0,1]`. El input convierte `Screen → World → local de AquariumDecorationArea2D → normalized`; la vista realiza la conversión inversa.

`DecorationPlacementValidator` es lógica pura. Aplica grid determinista, márgenes y snap al fondo para planta, roca, tronco y cueva. `DecorationFootprint` aproxima el espacio ocupado con un rectángulo escalado. `DecorationOverlapEvaluator` calcula la intersección respecto del área menor, permite solapamiento leve y bloquea el ratio configurado.

Se rechazan definiciones ausentes, escalas inválidas, límites y superposición crítica. No se usa `Random`, física global ni GameObjects. La rotación usa footprint aproximado; pivotes y polígonos específicos quedan como mejora futura.
