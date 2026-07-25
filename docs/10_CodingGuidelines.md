# Guías de desarrollo

- Mantener dependencias en dirección `Core <- Simulation <- Gameplay <- UI`.
- Aplicar SOLID donde reduzca acoplamiento real; evitar abstracciones especulativas.
- No usar singletons como acceso global por defecto.
- No buscar objetos repetidamente en `Update`.
- No cargar gameplay mediante `Resources.Load`.
- No mezclar datos de authoring, estado runtime y datos guardados.
- No dejar código muerto, duplicado ni marcadores pendientes.
- Validar referencias serializadas en los límites de composición.
- Añadir pruebas EditMode para lógica pura y PlayMode solo cuando la integración lo exija.
- Revisar compilación, escenas, referencias, diff y estado Git antes de entregar.
