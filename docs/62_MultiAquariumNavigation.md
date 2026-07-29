# Navegación multiacuario

Botones y swipe terminan en el mismo flujo: `AquariumManager.Activate`, seguido por el cambio atómico de `AquariumContext`.

El swipe usa `Pointer.current`, por lo que admite touch y arrastre de mouse con el Input System existente. El gesto exige 90 px, predominio horizontal 1.35, duración máxima 1.25 s y cooldown de 0.25 s. Se ignora si comienza sobre UI, si es corto o vertical, o si excede los límites.

Orden: validar destino, desactivar runtime anterior, cambiar contexto, activar runtime nuevo, publicar eventos y reenlazar presentación.
