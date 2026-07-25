# Estados de comportamiento del pez

- `Swimming`: nado autónomo.
- `SeekingFood`: sigue una partícula reclamada.
- `Eating`: consume, muestra un pulso y actualiza saciedad/cooldown.
- `Satisfied`: espera breve antes de volver a buscar.

Un pez disponible reclama la partícula válida más cercana dentro de su radio. Al consumir pasa por `Eating` y `Satisfied`. Si el objetivo expira, es consumido o deja de ser válido, libera el reclamo y vuelve a `Swimming`.

La búsqueda lineal es apropiada para el máximo actual de doce partículas; una estructura espacial solo sería necesaria al ampliar sustancialmente ese presupuesto.
