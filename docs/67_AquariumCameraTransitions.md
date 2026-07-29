# Transiciones de cámara

`AquariumCameraCarouselController` interpola la cámara real entre `CameraFocusPoint` con `Time.unscaledDeltaTime` y `AnimationCurve`. Conserva Z, finaliza asignando la posición exacta y rechaza transiciones simultáneas.

La duración base es 0.48 s, suma 0.16 s por slot adicional y se limita a 0.8 s. `AquariumNavigationCoordinator` mantiene el destino pendiente y cambia `AquariumContext` únicamente al completar; así HUD e interacción nunca se adelantan a la cámara.
