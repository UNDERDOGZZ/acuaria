# Layout del carrusel

`AquariumCarouselLayout` ordena por `SlotId` y asigna `origin + Vector3.right * index * spacing`. El asset `AquariumCarouselDefinition` configura separación, preview, easing y umbrales sin depender de una resolución.

La separación para un preview se puede derivar como `(tankWidth + visibleCameraWidth) / 2 - tankWidth * preview`, donde `visibleCameraWidth = 2 * orthographicSize * aspect`. El rango recomendado y validado de preview es 10–25 %.

Los slots bloqueados o vacíos se omiten antes de calcular índices. Las posiciones se recalculan solo al cambiar la colección, nunca por frame.
