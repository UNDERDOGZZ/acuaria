# Estrategia de autosave

Cambios de creación, eliminación, activación y runtime marcan dirty. Un fingerprint de bajo
costo detecta cambios en peces, decoraciones, agua, mantenimiento y diario sin guardar por
frame. El debounce es de 2 segundos y el intervalo mínimo entre escrituras es de 5 segundos.

Pausa, pérdida de foco y salida solicitan guardado inmediato. `SaveService` rechaza una segunda
operación mientras otra está activa. Las animaciones de mantenimiento no se reanudan a mitad:
se persiste y restaura el último estado estable `Idle`.
# Persistencia offline

El estado offline se guarda inmediatamente por defecto. Si falla, el coordinador queda `Dirty` y el autosave vuelve a intentarlo; el intervalo no se vuelve a ejecutar dentro de la sesión.
