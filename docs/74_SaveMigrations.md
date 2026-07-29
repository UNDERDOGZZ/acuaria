# Migraciones

`SchemaVersion` empieza en `1`. `ISaveMigration` declara versión origen/destino y
`SaveMigrationPipeline` aplica pasos secuenciales, registrándolos en `MigrationHistory`.
Actualmente no hay versiones antiguas productivas y por ello no hay transformaciones
registradas.

Toda versión nueva debe añadir una migración determinista, tests con fixture antiguo y
validación posterior. Nunca se migra ni sobrescribe una versión mayor que la soportada.
