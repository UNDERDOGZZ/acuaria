# Arquitectura técnica

El código se divide por dominio mediante assembly definitions. `Acuaria.Core` contiene contratos y arranque transversal; `Acuaria.Simulation` depende de Core; Room, Aquarium, Fish y Progression representan gameplay y pueden depender de Core y Simulation; `Acuaria.UI` consume los dominios, nunca al revés.

```text
Core <- Simulation <- Gameplay <- UI
```

Data, Audio, Input y Save son límites explícitos. Sus implementaciones futuras deberán depender de abstracciones internas y evitar estado global. La composición de dependencias ocurrirá durante Bootstrap; no se introduce un contenedor hasta que exista una necesidad demostrada.

Los ScriptableObjects almacenan configuración y definiciones authoring. No son localizadores de servicios ni almacenamiento de partida. La carga futura de contenido deberá usar referencias serializadas o una estrategia explícita; `Resources.Load` no será el mecanismo de gameplay.
