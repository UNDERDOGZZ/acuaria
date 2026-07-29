# Arquitectura del guardado

El flujo es dominio → `SaveMapper` → DTO → `ISaveSerializer` → `SaveFileStorage`. En carga se
invierte después de deserializar, validar y migrar. Los DTOs no contienen `GameObject`,
`MonoBehaviour`, `ScriptableObject`, eventos ni referencias visuales.

`SaveCoordinator` vive en Room y carga antes de los controladores normales. `SaveService`
impide operaciones simultáneas; `SaveValidator` clasifica problemas; `SaveMigrationPipeline`
encadena futuras migraciones. `SaveFileStorage` administra temporal, principal y backup, y
preserva copias de archivos corruptos. El autosave está localizado en el coordinador.

Límites del sprint: JSON legible sin cifrado o compresión, sin progreso offline, nube, cuentas,
economía ni inventario comercial.
