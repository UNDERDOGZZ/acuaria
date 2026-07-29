# Bucle de juego

La hipótesis de alto nivel es: observar la habitación, elegir un acuario, comprender sus necesidades, realizar una acción significativa, apreciar el resultado y avanzar en colección o expresión personal.

Las sesiones deben admitir tanto visitas breves como periodos de decoración tranquila. El progreso nunca debe recompensar el abandono deliberado del bienestar del ecosistema.

Este bucle es una dirección de diseño, no una implementación. Se validará antes de definir temporizadores, recompensas o economía.

El tramo implementado actualmente es observar Room, seleccionar el tanque activo, entrar en vista enfocada y regresar.

Los peces continúan nadando y conservan su estado durante enfoque y regreso.
## Bucle de cuidado disponible

1. Enfocar el acuario.
2. Activar `Alimentar`.
3. Tocar la superficie.
4. Observar la caída y reacción de los peces.
5. Cancelar o volver.

La saciedad y el cooldown regulan solo la respuesta inmediata.

La observación enfocada incorpora ahora una capa opcional de comprensión: revisar resumen, abrir detalles, identificar habitantes y leer un consejo educativo. Cerrar la ficha devuelve al estado enfocado sin reactivar automáticamente la alimentación.

La química continúa avanzando durante RoomOverview, enfoque, Detalles y FeedingMode. Alimentar añade una consecuencia gradual observable; todavía no existe una acción jugable para corregir nitratos o cambiar agua.
# Bucle de mantenimiento

Observar calidad → abrir Mantenimiento → elegir porcentaje → previsualizar → confirmar → observar drenado/llenado → revisar resultado. El filtro añade decisiones entre limpieza mecánica y conservación bacteriana.
Observar bienestar → revisar causas → alimentar o mantener el agua → observar recuperación gradual.
Descubrir especie → estudiar ficha → comparar suitability → comprender límites del acuario, sin compra ni modificación de población.
Desde el acuario enfocado, el jugador puede abrir Hábitat, editar una composición provisional y confirmarla para actualizar el perfil, o cancelarla sin consecuencias.
