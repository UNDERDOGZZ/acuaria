# UI del catálogo de peces

`FishCatalogController` transforma el registro y descubrimiento en `FishSpeciesViewModel`. `Hidden` no aparece, `Silhouette` oculta datos, `Discovered` muestra identidad básica y `Studied` permite la ficha completa. El avance es monotónico y no duplica recompensas.

`FishCatalogPanel` y `FishSpeciesDetailPanel` son modales bajo Safe Area. Sus `CanvasGroup` bloquean raycasts del mundo sin pausar peces. El catálogo filtra por dificultad, zona y vida social; no compra, instancia ni modifica peces.

La ficha presenta rangos con unidades, vida social, dieta, advertencias y `SpeciesTankSuitabilityResult`. El layout visual final debe usar scroll, objetivos táctiles amplios, landscape amplio/compacto y texto además de color.
