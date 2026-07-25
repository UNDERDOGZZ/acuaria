# UI de mantenimiento

El botón `Mantenimiento` solo aparece con el acuario enfocado. Al abrirlo cancela Alimentar, cierra interacciones incompatibles y presenta porcentajes, previsualización antes/después, confirmación y cancelación.

Durante mantenimiento se bloquean Confirmar, Volver, Alimentar, Detalles y otro mantenimiento. El panel informa `Preparando`, `Drenando agua`, `Llenando` y `Estabilizando el acuario`. Después recupera el input y actualiza química, calidad y HUD por eventos.

La sección del filtro muestra eficiencia, suciedad y recomendación, con `Enjuague suave` y `Limpieza profunda`. El panel vive bajo `SafeArea`, usa un único Canvas y está diseñado para horizontal responsive.
La información de bienestar permanece en Detalles y no se superpone con Mantenimiento.
