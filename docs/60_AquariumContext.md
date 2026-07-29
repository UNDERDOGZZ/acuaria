# AquariumContext

`AquariumContext` publica la instancia activa mediante una referencia de solo lectura y el evento
`ActiveChanged(previous, next)`. Solo `AquariumManager` puede sustituirla.

HUD, alimentación, mantenimiento, diario, hábitat, peces, decoraciones, química y bienestar deben
resolver sus modelos desde este contexto cuando reciben el cambio. Bloquear input o abrir modales
no altera el contexto ni pausa otras instancias.

Reglas de enlace:

1. suscribirse al habilitarse;
2. renderizar inmediatamente `Context.Active`;
3. desuscribirse al deshabilitarse;
4. cancelar interacciones provisionales antes de cambiar;
5. no conservar referencias `starter-*` como fuente de verdad;
6. mantener GameObjects de instancias inactivas ocultos, no destruidos.

Las pruebas verifican creación, registro, activación única, separación de colecciones y tick
inactivo.
