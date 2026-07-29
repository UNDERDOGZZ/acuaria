# Flujo de carga

`AquariumManager` despierta primero y `SaveCoordinator` después. Se intenta el archivo
principal, luego backup. Cada candidato pasa por JSON, formato, versión, estructura,
checksum y migración. Un save válido limpia `AquariumRepository` y `AquariumContext`,
reconstruye instancias por DefinitionId y activa el ID guardado.

Después, `MultiAquariumRoomController` enlaza slots y vistas existentes; los binders normales
sincronizan HUD, química, bienestar, peces, carrusel y cámara desde el contexto activo. Si no
hay save válido, Room crea la partida segura definida por la escena. Una versión futura detiene
la restauración y el autosave para evitar sobrescribirla.
