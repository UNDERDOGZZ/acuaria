# Sistema de guardado

El Sprint 14 implementa persistencia local mediante DTOs de `Acuaria.Save`. Gameplay no conoce
rutas ni escribe archivos. `SaveMapper` captura/restaura el dominio; `SaveService` valida,
serializa y coordina el almacenamiento protegido. El esquema inicial es `1` y el formato es
`ACUARIA_LOCAL_SAVE`.

Los archivos `acuaria_save.json`, `acuaria_save.backup.json` y `acuaria_save.tmp.json` viven
en `Application.persistentDataPath`. No existe progreso offline, cifrado, nube ni economía.
Véanse `docs/70_SaveSystemArchitecture.md` a `docs/77_SaveTestingGuide.md`.
