# Escena Room

## Objetivo

Room es la primera representación funcional del hogar de Acuaria. Presenta una habitación 2D frontal y acogedora, con un acuario inicial como foco visual. No contiene gameplay, peces, interacción ni UI visible.

## Composición y jerarquía

```text
Room
├── Systems
├── Environment
│   ├── Background
│   ├── Wall
│   ├── Floor
│   ├── Window
│   ├── Furniture
│   ├── Shelves
│   ├── DecorativePlants
│   └── Lamps
├── AquariumArea
│   ├── AquariumSlot_01
│   ├── AquariumSlot_02
│   └── AquariumSlot_03
├── AmbientEffects
└── Cameras
    └── MainCamera
```

La ventana ocupa el lateral izquierdo, el acuario y su mueble dominan el centro, y la repisa y planta equilibran el lateral derecho. Los elementos son objetos independientes reemplazables.

## Cámara y resoluciones

La cámara es ortográfica y usa una zona de referencia de 20 × 11.25 unidades, equivalente a 1920 × 1080. `RoomCameraFitter` conserva toda esa zona en 16:9. En pantallas más anchas, como 20:9, muestra contenido adicional en los bordes. En tablets 4:3 aumenta el tamaño ortográfico para conservar el ancho sin deformar sprites.

La cámara no se mueve, no permite zoom y no ejecuta lógica por frame. Si la resolución cambia en runtime, el consumidor debe llamar `Refresh`; la orientación prevista es horizontal fija.

## Sorting Layers

Orden posterior a frontal:

1. `RoomBackground`
2. `RoomEnvironment`
3. `RoomFurniture`
4. `AquariumBack`
5. `AquariumContents`
6. `AquariumFront`
7. `RoomForeground`
8. `Effects`
9. `UI`

El fondo y agua del acuario quedan detrás de su contenido; vidrio, reflejo y marco quedan delante. Los efectos tienen baja opacidad para no ocultar el foco.

## Iluminación

La escena usa URP 2D sin cambiar el pipeline:

- luz global azul-violeta para ambiente nocturno;
- luz Point 2D cálida integrada en `RoomLamp`;
- luz Point 2D fría junto a la ventana;
- brillo Point 2D tenue integrado en `AquariumRoomDisplay`.

Las luces no proyectan sombras y se evita postprocesado para mantener un coste moderado en mobile.

## Prefabs y slots

- `AquariumRoomDisplay`: marco, fondo, agua, sustrato, roca, planta, vidrio, reflejo y brillo; no tiene peces, colliders ni lógica.
- `AquariumSlotView`: raíz reutilizable y contenedor `Content`.
- `RoomLamp`: geometría provisional y luz cálida.
- `DecorativePlant`: maceta, tallo y hojas independientes.

Existen tres slots. Solo `AquariumSlot_01` contiene el acuario. Los otros dos muestran una reserva visual tenue; no existe sistema de desbloqueo.

## Safe Area

`SafeAreaPanel` está preparado para futuros `RectTransform` de interfaz. No existe Canvas visible y la safe area no altera la composición del mundo.

## Limitaciones y reemplazo de arte

Todo el arte es provisional y parte de `PrototypeWhite.png`, tintado y escalado mediante `SpriteRenderer`. No hay partículas de polvo ni animaciones ambientales. Para reemplazar arte:

1. conservar las raíces y pivotes de cada prefab;
2. sustituir SpriteRenderers por sprites finales respetando sorting layers;
3. mantener el acuario frontal dentro de sus dimensiones actuales;
4. ajustar luces compartidas antes de crear materiales únicos;
5. verificar 16:9, 20:9 y 4:3 en Game View.
