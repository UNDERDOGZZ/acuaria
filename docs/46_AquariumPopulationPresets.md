# Presets de población

`AquariumPopulationDefinition` contiene ID, etiqueta, estado y entradas de especie, cantidad y semilla. El total se calcula desde datos. `FishSpawner2D` consume el preset y ya no limita la población a tres. Los IDs runtime combinan especie, entrada e índice local.

`starter-real-species` contiene un betta y dos corydoras como escenario educativo; el evaluador debe advertir que dos corydoras no forman el grupo recomendado. No es una población ideal. Los presets Debug cubren cada especie y una combinación incompatible.

La ruta legacy de `FishSpawnEntry` se conserva para migración. Los placeholders usan el prefab compartido y no crean materiales por instancia.
