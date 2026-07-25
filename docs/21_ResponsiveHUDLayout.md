# Layout responsive del HUD

## Problema y causa

El HUD anterior colocaba nombre/volumen y temperatura/población mediante `RectTransform` stretch con offsets manuales coincidentes. Los pares ocupaban la misma región, y estado y Detalles compartían el poco ancho restante. La auditoría confirmó que no había dos HUD ni textos antiguos duplicados: el defecto estaba en una única jerarquía con anchors y offsets incompatibles.

## Jerarquía final

`SafeArea/AquariumHUDSystem/AquariumHUD/TopBar` contiene dos contenedores alternativos:

- `WideRow`: Volver, identidad, spacer flexible y estadísticas con Detalles.
- `CompactStack`: una fila primaria con Volver, identidad, spacer y Detalles; una segunda fila de estadísticas.

`AquariumIdentityGroup` contiene `AquariumNameText` y `VolumeText`. `AquariumStatsGroup` contiene `TemperatureItem`, `FishCountItem` y `StatusBadge`. Cada valor tiene un `TextMeshProUGUI` independiente. Solo una variante está activa y los mismos controles se reparentan de forma controlada; no existen copias funcionales.

## RectTransform, anchors y pivots

- `AquariumHUD`: stretch completo dentro del Safe Area.
- `TopBar`: stretch horizontal, anclado arriba, pivot superior central y márgenes de 16 unidades.
- Filas Wide/Compact: stretch dentro de TopBar, escala 1.
- Volver e identidad: ordenados desde la izquierda por el layout.
- Spacer: `flexibleWidth = 1`.
- Estadísticas: tamaño mínimo/preferido explícito.
- Alimentar: mantiene el anchor inferior derecho y sus márgenes existentes.

## LayoutGroups y LayoutElements

La fila Wide, la fila primaria Compact, estadísticas y cada item usan `HorizontalLayoutGroup`. La identidad y el stack Compact usan `VerticalLayoutGroup`. `LayoutElement` define mínimos, preferidos y el spacer flexible. No se usa `ContentSizeFitter`, reconstrucción por frame ni posiciones libres para separar datos.

## Modos y breakpoint

`AquariumHUDResponsiveLayout` compara el ancho disponible con un breakpoint configurable de 2100 unidades de referencia:

- `Wide` (≥2100): una fila, identidad a la izquierda y estadísticas a la derecha.
- `Compact` (<2100): identidad/acciones arriba y temperatura/peces/estado debajo.

La evaluación ocurre al habilitarse, inicializarse o cambiar las dimensiones del `RectTransform`; no existe `Update`.

## Canvas, Safe Area y tipografía

Se reutilizan el Canvas y el componente Safe Area existentes. `CanvasScaler` usa 1920×1080, `Scale With Screen Size`, `Match Width Or Height` y match 0.5. No se desplaza ningún objeto del mundo.

Los campos TMP usan una línea, wrapping desactivado, ellipsis, alineación consistente y Auto Size limitado. Nombre, volumen, temperatura, peces, estado y texto de Detalles son objetos distintos. Se importaron los recursos esenciales de TMP incluidos con Unity; no se instaló ningún paquete.

## Resoluciones objetivo

El criterio asigna 1920×1080, 1280×800 y 2732×2048 horizontal al modo Compact; 2340×1080 y 2400×1080 al modo Wide. La selección de modo está cubierta por EditMode tests. La inspección manual se realizó en el Game View disponible; las cinco resoluciones exactas deben repetirse con presets de dispositivo antes de una entrega de tienda.

## Integraciones

Detalles conserva el mismo botón y controlador: al abrir cancela FeedingMode, puede cerrarse sin alterar la composición y se cierra al volver a RoomOverview. Alimentar conserva su botón, posición y flujo; el refactor no modifica peces, comida ni reglas de consumo.

## Validación y limitaciones

`AquariumHUDController` valida referencias faltantes y campos TMP duplicados una vez, con mensajes que incluyen el nombre del campo. El generador destruye el sistema HUD anterior antes de crear exactamente uno. La principal limitación pendiente es validar físicamente notch/cutouts y densidades en dispositivos iOS/Android reales.

El indicador químico reutiliza `StatusBadge` y `StatusText`; no añade tarjetas permanentes ni altera los mínimos del layout responsive.
