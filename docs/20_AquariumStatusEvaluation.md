# Evaluación del estado del acuario

`AquariumStatusEvaluator` es lógica pura y devuelve estado, mensaje y severidad numérica. Los colores pertenecen al HUD, no al evaluador.

## Reglas provisionales

- `Excellent`: temperatura dentro del rango y población por debajo de capacidad.
- `Good`: temperatura correcta y población igual a capacidad.
- `Attention`: desviación térmica de hasta 1,5 °C o un pez sobre capacidad.
- `Critical`: desviación superior a 1,5 °C o sobrepoblación mayor a un pez.

Capacidad cero, estado sin inicializar y valores no finitos se manejan sin producir NaN ni divisiones.

## Mensajes y límites

La UI traduce los estados como Excelente, Estable, Revisar y Atención urgente. El tono evita alarmas agresivas. La evaluación no representa todavía bienestar real ni una recomendación biológica.

Una versión futura combinará temperatura con química, ciclado, filtración, oxigenación, compatibilidad y bienestar, manteniendo reglas explicables y testeables.
# Calidad del agua

La evaluación general puede recibir `WaterQualityResult`. Se conserva la mayor severidad entre temperatura/población y química; una condición menos severa nunca oculta otra más importante.
# Mantenimiento y estado

Los cambios de agua publican nuevamente química, calidad y ciclo. La pérdida bacteriana de una limpieza profunda puede empeorar el estado del ciclado.
El peor nivel relevante entre estado base, agua y bienestar puede dominar el estado general.
