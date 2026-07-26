# Sistema de bienestar

`FishWelfareDefinition` centraliza pesos, umbrales y velocidades. `FishWelfareEvaluator` calcula un objetivo de 0–100 usando temperatura, volumen, grupo, agua, alimentación, compatibilidad y zona. El promedio es ponderado: agua y necesidades sociales tienen mayor impacto que la zona.

`FishWelfareState` conserva puntuación, objetivo, tendencia, estado y causas. `FishWelfareSimulationModel` acerca gradualmente el valor actual al objetivo; el deterioro puede ser más rápido que la recuperación. Los eventos actualizan HUD y detalles tras química, población o evaluación periódica.

El mantenimiento y la alimentación influyen indirectamente mediante química, saciedad y población. No existe daño, enfermedad ni muerte.
Los multiplicadores de bienestar alteran la magnitud de velocidad, no los bounds, el destino ni la recuperación de bordes. El mínimo visual activo impide que bienestar o UI produzcan velocidad cero.
Las especies reales reutilizan el mismo evaluador; sus perfiles sustituyen valores ficticios sin introducir daño, enfermedad o muerte.
