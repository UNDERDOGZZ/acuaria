# Interacción de enfoque del acuario

## Flujo

```text
RoomOverview → FocusingAquarium → AquariumFocused
AquariumFocused → ReturningToRoom → RoomOverview
```

`RoomViewStateMachine` rechaza transiciones inválidas y duplicadas. `RoomViewController` coordina cámara, UI y bloqueo de input sin contener simulación.

## Selección e input

`AquariumInteractable` implementa `IPointerClickHandler` y requiere un `Collider2D`. La cámara usa `Physics2DRaycaster` y la escena un `EventSystem` con `InputSystemUIInputModule`, compartiendo tap y clic izquierdo. Solo el prefab ocupado contiene interactable.

## Cámara y target

`AquariumFocusTarget` aporta ID, punto mundial y tamaño ortográfico. El controlador conserva posición y tamaño overview explícitos, interpola durante 0.6 segundos y restaura esos valores exactos al regresar. No gira la cámara ni usa Cinemachine.

## UI, Safe Area y transición

Un Canvas escalable contiene `SafeArea`, un botón volver de 112×80 y un velo mediante `CanvasGroup`. La Safe Area solo afecta UI. Durante transición, interactables y botón quedan bloqueados.

## Varios acuarios

La selección se basa en referencia a `AquariumFocusTarget` y `SlotId`, no en nombre, índice o posición. Otros slots ocupados podrán registrar sus propios targets sin cambiar el flujo.

## Limitaciones y pruebas manuales

No existen swipe, navegación entre acuarios enfocados, simulación ni parámetros. Probar cinco ciclos, doble tap, slots vacíos, botón volver, 16:9, 20:9 y tablet; revisar Console y confirmar restauración exacta.

Los tres peces añadidos posteriormente permanecen activos durante enfoque y regreso; su ciclo de vida no depende de la cámara.

El HUD permanece oculto en `RoomOverview`, `FocusingAquarium` y `ReturningToRoom`. Solo se habilita al completar `AquariumFocused`. El regreso cierra inmediatamente la ficha y bloquea su input.
