# Input del editor de hábitat

`HabitatEditorInputController` comparte flujo para mouse y primer touch. Ignora pointer-down sobre UI, selecciona únicamente `DecorationView`, prioriza sorting superior y usa `InstanceId` como desempate determinista.

Al comenzar un drag conserva el offset normalizado entre puntero y centro para evitar saltos. Durante el movimiento solo calcula candidato y feedback; al soltar registra un comando. Touch cancelado finaliza el gesto de forma segura. Peces, comida, agua y fondo no participan en hit testing.
