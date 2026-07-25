# HUD y ficha del acuario

## HUD compacto

`AquariumHUDController` muestra dos bloques superiores durante `AquariumFocused`: identidad/volumen y temperatura/población/estado/detalles. Alimentar y Volver continúan en la zona inferior y lateral existente. El tanque conserva el protagonismo visual.

## Ficha de detalles

`AquariumDetailsPanel` es un overlay dentro del mismo Canvas. Presenta volumen, temperatura, rango, capacidad, estado, habitantes agrupados, explicación del volumen y consejo educativo. Su viewport usa `ScrollRect` para alturas reducidas; los peces siguen nadando detrás.

## Visibilidad e integración

- Overview y transiciones: HUD oculto y ficha cerrada.
- Enfoque completado: HUD visible e interactivo.
- Abrir detalles: cancela FeedingMode.
- Cerrar detalles: vuelve al enfoque normal sin reactivar alimentación.
- Regresar: cierra ficha, oculta HUD y bloquea input.

## Safe Area y layout

Todo cuelga del `SafeArea` existente. El CanvasScaler conserva referencia 1920×1080; anchors opuestos fijan los dos bloques superiores y el panel central limita su tamaño. La herramienta no mueve cámara ni mundo.

## Accesibilidad y localización futura

Los estados combinan badge, símbolo y texto. Botones tienen áreas táctiles amplias, contraste alto y etiquetas explícitas. `AquariumUIText` centraliza textos compartidos para facilitar una futura migración a Localization sin instalar paquetes en este sprint.
