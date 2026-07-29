# Interacción swipe

Mouse y touch convergen mediante `Pointer.current`. Swipe izquierdo solicita el índice siguiente y desplaza la cámara a la derecha; swipe derecho solicita el anterior. Se exigen 90 px, predominio horizontal 1.35 y máximo 1.25 s.

El gesto iniciado sobre UI se ignora. Los límites no envuelven. Botones y swipe llaman a `AquariumNavigationCoordinator`; durante una transición el coordinador bloquea solicitudes nuevas.

El drag-follow queda fuera de esta corrección: se priorizó snap animado determinista y sin deriva.
