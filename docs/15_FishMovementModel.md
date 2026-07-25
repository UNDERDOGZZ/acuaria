# Modelo de movimiento de peces

Cada pez posee un `System.Random` propio. La semilla determina destino, velocidad, duración y escala de forma reproducible sin modificar `UnityEngine.Random`.

## Movimiento

Los destinos se generan dentro de `SwimBounds2D` y su franja Upper, Middle o Lower. El avance limita la componente vertical, suaviza dirección y aplica clamp tras movimiento y separación. Al llegar cerca del destino o superar su duración se elige otro.

`FishVisual2D` cambia orientación solo al superar una zona muerta horizontal. Cola y aleta oscilan con fases independientes sin alterar la posición lógica. Una corrección O(n²) suave separa peces cercanos; con tres instancias su coste es mínimo.

No existen LINQ, búsquedas ni allocations deliberadas en `Update`. Los Gizmos opcionales muestran área y destino únicamente en Editor.

## Validación manual

Observar dos minutos, enfocar y regresar cinco veces, comprobar exactamente tres instancias, ausencia de vibración y límites correctos. Repetir en 16:9, 20:9 y tablet horizontal y revisar Console.
