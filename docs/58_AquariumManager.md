# AquariumManager

Debe existir un único `AquariumManager` por escena de juego. Su singleton sirve para detectar
duplicados de componente; el estado del dominio permanece en `AquariumRepository` y
`AquariumContext`.

Responsabilidades:

- crear mediante `AquariumFactory`;
- registrar, buscar y enumerar;
- activar exactamente una instancia;
- impedir eliminar el último acuario;
- emitir `OnAquariumCreated`, `OnAquariumRemoved`, `OnAquariumActivated`,
  `OnAquariumDeactivated` y `OnActiveAquariumChanged`;
- enviar ticks simplificados a instancias inactivas.

Una activación desmarca el runtime anterior, marca el nuevo, incrementa sus estadísticas y
actualiza el contexto antes de notificar consumidores. No destruye GameObjects ni cambia escena.
