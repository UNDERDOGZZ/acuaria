# Modelo de datos del acuario

## Datos estáticos

`AquariumDefinition` es un ScriptableObject de authoring. Define ID estable, nombre, volumen nominal, rango e inicio de temperatura, capacidad provisional, descripción, consejo educativo, icono y color opcionales. `StarterAquarium.asset` representa el tanque inicial de 50 L a 25 °C.

## Estado mutable

`AquariumRuntimeState` es una clase C# sin referencias a escena o UI. Contiene ID de instancia y definición, temperatura actual, conteo de peces, inicialización, timestamp lógico, disponibilidad y enfoque. Sus cambios emiten un evento para invalidar presentación.

## Slots y peces

El ID de definición describe el tipo de tanque; el ID de instancia identifica el ecosistema colocado en un slot. En este sprint solo existe `slot-01`. `FishSpawner2D` registra las especies instanciadas y `AquariumInhabitantProvider` las agrupa para presentación.

## Limitaciones

El estado no contiene química, suciedad, filtración, salud, historial ni persistencia. La capacidad es únicamente una señal provisional. Una regla futura deberá considerar especie, tamaño adulto, comportamiento, filtración, oxigenación, zona y compatibilidad.
