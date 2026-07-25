# UI de calidad del agua

El HUD reutiliza el badge superior para mostrar `Agua: Excelente/Buena/Atención/Peligrosa`; los tres parámetros completos permanecen en la ficha de Detalles.

La ficha muestra NH₃/NH₄, NO₂ y NO₃ con mg/L, flechas acompañadas por texto, estado del ciclado y consejo contextual. Los textos no dependen solo del color. La sección vive dentro del `ScrollRect`, Safe Area y Canvas existentes.

`WaterChemistryViewModel` formatea únicamente cuando llega un evento de simulación. No hay refresh de strings por frame, gráficas históricas ni colecciones crecientes. Las barras normalizadas están disponibles en el ViewModel para arte futuro; este sprint conserva presentación textual ligera y accesible.
# Feedback de mantenimiento

Después de cada acción se recalculan calidad, ciclado, tendencias y HUD mediante la publicación normal de química.
