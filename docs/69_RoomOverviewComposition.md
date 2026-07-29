# Composición de RoomOverview

Los tres `AquariumRoot` reales viven bajo `AquariumCarouselRoot`, cada uno dentro del `Content` de su slot. Permanecen activos y separados horizontalmente para que el encuadre ortográfico muestre el vecino inmediato.

`AquariumCarouselBackdrop` amplía de forma idempotente Background, Wall, Floor y FloorTrim y los centra sobre la extensión del carrusel. Esto evita zonas sin escenario al visitar los slots 2 y 3 sin duplicar cámaras ni Canvas.

Los tres displays restablecen posición local, rotación y escala desde el display original. Cada `AquariumFocusTarget` recibe el `SlotId` real; al tocar un lateral primero se centra y, al tocar el tanque ya centrado, `RoomViewController` ejecuta el zoom de AquariumFocused.

Las definiciones son independientes: Starter 50 L/25 °C, Acuario 2 80 L/27 °C y Acuario 3 35 L/23 °C. Las tarjetas y el HUD enlazado al contexto leen esas mismas definiciones.

RoomOverview permite carrusel. AquariumFocused conserva el flujo existente de `RoomViewController`; alimentación y paneles siguen reservados para la vista enfocada. El HUD permanece en `SafeArea` y no se mueve con el mundo.
