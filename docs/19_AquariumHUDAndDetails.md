# HUD y ficha del acuario

## HUD responsive

`AquariumHUDController` escribe identidad, volumen, temperatura, población y estado en cinco `TextMeshProUGUI` distintos. `AquariumHUDResponsiveLayout` presenta una fila en pantallas Wide o dos filas en Compact. Volver forma parte de la barra superior y Alimentar conserva su anclaje inferior derecho. El tanque sigue siendo el protagonista visual.

## Ficha de detalles

`AquariumDetailsPanel` es un overlay dentro del mismo Canvas. Presenta volumen, temperatura, rango, capacidad, estado, habitantes agrupados, explicación del volumen y consejo educativo. Su viewport usa `ScrollRect` para alturas reducidas; los peces siguen nadando detrás.

## Visibilidad e integración

- Overview y transiciones: HUD oculto y ficha cerrada.
- Enfoque completado: HUD visible e interactivo.
- Abrir detalles: cancela FeedingMode.
- Cerrar detalles: vuelve al enfoque normal sin reactivar alimentación.
- Regresar: cierra ficha, oculta HUD y bloquea input.

## Safe Area y layout

Todo cuelga del `SafeArea` existente. El `CanvasScaler` conserva referencia 1920×1080, modo `Scale With Screen Size`, `Match Width Or Height` y match 0.5. `TopBar` usa stretch horizontal y layouts con tamaños mínimos explícitos; el panel central limita su tamaño. La herramienta no mueve cámara ni mundo. La especificación completa está en [21_ResponsiveHUDLayout.md](21_ResponsiveHUDLayout.md).

## Accesibilidad y localización futura

Los estados combinan badge, símbolo y texto. Botones tienen áreas táctiles amplias, contraste alto y etiquetas explícitas. `AquariumUIText` centraliza textos compartidos para facilitar una futura migración a Localization sin instalar paquetes en este sprint.

La ficha incluye una sección desplazable de calidad del agua con NH₃/NH₄, NO₂, NO₃, tendencias, estado del ciclado y consejo contextual. El HUD solo muestra el indicador compacto `Agua: <estado>`.
# Acceso a mantenimiento

El HUD enfocado añade `Mantenimiento`. Detalles y Mantenimiento son mutuamente excluyentes y Alimentar se cancela al abrir mantenimiento.
Detalles incorpora bienestar, necesidades por especie y compatibilidad dentro del contenido desplazable.
