# Pipeline de contenido de especies

Cada especie se crea como `FishSpeciesDefinition` con un ID estable `fish.*`, perfiles biológico, cuidado, social, compatibilidad, educativo y visual. Los nombres de asset, prefab y textos localizables no forman parte de su identidad.

El autor registra fuentes, fecha de consulta, campos respaldados y estado de revisión. `Verified` exige al menos una referencia válida; sin ella el asset se degrada a `NeedsReview`. Tras validar rangos finitos y perfiles, la especie se incorpora a `FishSpeciesRegistry`, a su futura entrada del Codex y, si corresponde, a un `AquariumPopulationDefinition`.

El prefab solo contiene presentación y runtime. Sin arte final se reutiliza `Fish2D.prefab` con color, escala y zona configurables. Antes de integrar contenido se ejecutan los tests de definición, registry, suitability, población, bienestar, compatibilidad y movimiento.
