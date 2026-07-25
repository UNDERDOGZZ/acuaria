# Composición de Room y slots de acuario

## Problema y solución

El tanque anterior ocupaba cerca de 24% del ancho de referencia y el gabinete tenía mayor peso visual. El marco activo ahora mide 9.2 unidades dentro de una referencia de 20: aproximadamente 46%. El gabinete es más bajo y funciona como soporte.

## Jerarquía visual

1. tanque activo y base iluminada;
2. agua y glow acuático;
3. siluetas de slots futuros;
4. pared, piso y gabinete;
5. decoración secundaria.

Sombras planas, glow en pared y bandas tonales crean profundidad sin postprocesado ni sombras dinámicas.

## Carrusel estructural

```text
AquariumCarouselRoot (0, -0.15)
├── AquariumSlot_02 (-11.2, 0.1) — futuro
├── AquariumSlot_01 (0, 0) — activo y central
└── AquariumSlot_03 (+11.2, 0.1) — futuro
```

Los pivotes están centrados. Los laterales usan escala 0.72 y quedan parcialmente encuadrados. Un controlador futuro podrá mover horizontalmente la raíz; no existen swipe, drag, snapping, input ni selección.

## Cámara, agua e iluminación

La zona de diseño continúa en 20 × 11.25 unidades. El tanque central queda dentro de ±4.6 unidades y no debe recortarse en 16:9, 19.5:9, 20:9 ni tablet horizontal.

El agua combina profundidad oscura, tono medio, superficie clara, línea de superficie, sustrato contrastado y reflejos discretos. La luz acuática es Point Light 2D sin sombras; la pared recibe un glow plano barato.

## Limitaciones

Todo continúa provisional. No hay peces, simulación, colliders, input, UI funcional, movimiento ni desbloqueos. El siguiente sprint deberá validar interacción como sistema independiente.
