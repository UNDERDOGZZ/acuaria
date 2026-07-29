# Validación y recuperación

Son críticos: formato ajeno, schema inválido, IDs de acuarios/slots duplicados, acuario activo
huérfano, referencias de slot huérfanas, timestamps raíz inválidos y números esenciales no
finitos. Peces o decoraciones individuales incompletos son recuperables y se omiten con aviso.

Cada archivo incluye SHA-256 calculado con el campo checksum vacío. Antes del commit se relee
y verifica el temporal. El principal anterior rota a backup. Si el principal falla se conserva
una copia `.corrupt-<UTC>` y se prueba el backup. Si ambos fallan, se inicia una partida segura.
Una versión futura no prueba fallback ni escribe archivos.
