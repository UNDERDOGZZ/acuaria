# Modelo de química del agua

Se representan amoníaco total simplificado (`NH₃/NH₄`), nitritos (`NO₂`) y nitratos (`NO₃`) en mg/L. No se separan NH₃ libre y NH₄ ionizado ni se modelan pH, oxígeno, cloro o dureza.

`WaterChemistryDefinition` guarda configuración authoring: valores iniciales, bacterias, conversiones, residuos, límites, reloj y umbrales. `WaterChemistryState` guarda concentraciones, dos colonias normalizadas, residuos, tiempo, versión y tendencias. Sus snapshots son independientes.

Todos los valores no finitos se normalizan y se limitan entre cero y máximos configurables. Los residuos son masa educativa abstracta; su aporte por pez es uniforme y su conversión a concentración se divide por volumen. Futuras versiones podrán usar biomasa, especies, pH y temperatura.
# Cambios parciales

`WaterChangeModel` sustituye una fracción de las concentraciones mediante una operación atómica y conserva las poblaciones bacterianas.
