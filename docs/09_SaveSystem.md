# Sistema de guardado

El guardado futuro será independiente de Unity scenes y ScriptableObjects. El estado persistente usará modelos de datos explícitos, versión de esquema, migraciones deterministas y escritura segura con posibilidad de recuperación.

La capa `Acuaria.Save` expondrá contratos hacia el dominio; los detalles de almacenamiento dependerán de esos contratos. Ningún sistema de gameplay conocerá rutas, serializadores o APIs de plataforma.

Este sprint no implementa persistencia. Antes de hacerlo se definirán identidad de entidades, política de autosave, compatibilidad entre versiones y estrategia de pruebas.
