# Sistema de decoraciones

Sprint 11 introduce decoraciones funcionales definidas por `DecorationDefinition`. Cada asset posee un ID estable, categoría, presentación, ocupación, contribuciones al hábitat, especies favorecidas y contenido educativo.

Las categorías soportadas son plantas, rocas, madera, cuevas, sustrato, elementos artificiales y área abierta. `DecorationRegistry` es la fuente única del catálogo y valida referencias nulas e IDs duplicados.

`AquariumDefinition.InstalledDecorations` contiene la composición inicial. En ejecución, `AquariumHabitatController` mantiene una copia mutable para depuración; no modifica el asset, no representa inventario y no implementa compra ni colocación manual.

`DecorationView` puede presentar un asset mediante sprite o servir de raíz para un prefab. El sprint no incorpora arrastre, tienda, monedas, crecimiento, poda ni química derivada de decoraciones.

Los controles debug permiten añadir o quitar plantas y rocas, añadir cuevas y restablecer la composición inicial.

Una definición describe un tipo; `DecorationPlacementData` describe cada instancia instalada con ID estable, posición normalizada, rotación, escala, flip, orden y visibilidad. `DecorationSpawner2D` sincroniza esas instancias dentro del mundo.
