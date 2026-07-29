# Guía de pruebas

Los EditMode tests usan un directorio temporal único y lo eliminan al terminar. Cubren JSON,
escritura temporal/principal/backup, corrupción del principal, recuperación del backup,
rechazo inmutable de versión futura, IDs duplicados, huérfanos y restauración idempotente de
tres acuarios.

Validación manual: iniciar Room, cambiar temperatura/activo, mover una decoración, observar
peces, salir de Play Mode y volver a entrar. Confirmar mismos acuarios, conteos, posiciones,
química y HUD. Repetir con pausa y pérdida de foco. Para corrupción, copiar el save, dañar el
principal y comprobar recuperación; no usar partidas reales sin respaldo.

Android e iOS requieren prueba en dispositivo de suspensión, kill del proceso, espacio lleno y
permisos. En este sprint solo se valida Editor/Windows; no se afirma certificación móvil.
