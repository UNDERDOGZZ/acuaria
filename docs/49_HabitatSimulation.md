# Simulación de hábitat

El flujo de datos es:

`AquariumDefinition` → decoraciones instaladas → `AquariumHabitatCalculator` → `AquariumHabitatProfile` → `FishWelfareEvaluator`.

El perfil agrega cobertura vegetal, escondites, resistencia a la corriente, cobertura lumínica y complejidad. El espacio abierto se calcula desde una base de 100 % menos el espacio consumido, siempre limitado a valores válidos.

El bienestar solo aplica una necesidad cuando la especie la declara:

- `NeedsPlants` compara cobertura actual con `PlantCoverageRecommended`.
- `NeedsHidingPlaces` evalúa el número de refugios.
- `OpenSpaceRequired` protege el espacio libre necesario.
- La zona de nado continúa evaluándose de forma independiente.

Las decoraciones no cambian compatibilidad, pH, oxígeno, CO₂ ni fórmulas químicas. El perfil puede recalcularse en runtime sin pausar peces ni alterar `Time.timeScale`.

La calificación del panel usa cuatro estados educativos: Excellent, Good, Attention y Poor, acompañados por las carencias detectadas.

El mismo cambio que recalcula el perfil emite `DecorationsChanged` y sincroniza las vistas. La presentación consume las mismas instancias y no calcula contribuciones.
