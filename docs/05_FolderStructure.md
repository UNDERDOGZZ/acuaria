# Estructura de carpetas

`Assets/_Acuaria` es la raíz de contenido propio:

- `Art`, `Audio`, `Materials`, `Shaders`: producción audiovisual.
- `Data/Definitions`: definiciones authoring por tipo.
- `Editor`: herramientas exclusivas del editor.
- `Prefabs`, `Scenes`, `Settings`: assets de ejecución y configuración.
- `Resources`: reservado; no se usa para carga de gameplay.
- `Scripts/<Domain>`: código y assembly definitions por dominio.
- `Tests/EditMode`: pruebas rápidas de lógica y authoring.
- `Docs`: documentación que deba distribuirse dentro del proyecto.

La documentación principal vive en `/docs`. No se crean carpetas por anticipado fuera de esta taxonomía.

Room utiliza únicamente estas extensiones:

- `Art/Prototype/Room`: sprite neutro reutilizable para geometría provisional.
- `Materials/Room`: reservado para materiales compartidos de Room; actualmente no requiere materiales propios.
- `Prefabs/Room`: `AquariumRoomDisplay`, `AquariumSlotView`, `RoomLamp` y `DecorativePlant`.
- `Scripts/Room`: composición, slots y cámara.

Fish añade `Data/FishSpecies`, `Prefabs/Fish`, `Scripts/Fish` y `Tests/EditMode/Fish` para separar authoring, representación, runtime y pruebas deterministas.
