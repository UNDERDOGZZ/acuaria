# Arquitectura del editor de hábitat

`HabitatEditorController` coordina una sesión sin convertirse en fuente de simulación. Al abrir captura un `HabitatLayoutSnapshot`, crea una copia de trabajo y habilita input especializado. `HabitatEditorState` contiene fase, selección, validez, cambios y mensajes sin referencias Unity.

Las acciones modifican exclusivamente la copia y sincronizan sus vistas por `InstanceId`. Confirmar valida toda la composición, la aplica una vez mediante `AquariumHabitatController`, recalcula el perfil y emite eventos. Cancelar vuelve a presentar el snapshot sin progresión. Ambos caminos limpian selección, historial y bloqueo de alimentación.

El ciclo de vida contempla `OnDisable`: una sesión activa se cancela de manera segura. El editor no pausa peces, agua o reloj visual.
