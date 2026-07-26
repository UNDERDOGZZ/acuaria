# Compatibilidad básica

Los perfiles usan reglas generales de temperatura, tamaño, actividad, territorialidad y zona, además de overrides opcionales por ID. El resultado es `Compatible`, `Caution` o `Incompatible`.

`AquariumCompatibilityReport` compara pares únicos `i < j`, por lo que su complejidad es O(n²) sin duplicar pares inversos. No genera combate, persecución, depredación física ni daño.
El catálogo presenta compatibilidad contextual. Los overrides por ID estable no sustituyen temperatura, territorialidad, actividad, zona ni volumen.
