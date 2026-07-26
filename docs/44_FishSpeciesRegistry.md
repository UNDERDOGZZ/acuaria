# Registro de especies

`FishSpeciesRegistry` es la referencia serializada del catálogo. Expone lista de solo lectura, búsqueda ordinal por ID y filtros deterministas por dificultad, zona y tipo social. No usa `Resources.LoadAll`, búsquedas de escena ni listas globales mutables.

La validación detecta nulos, referencias repetidas, IDs vacíos o duplicados y perfiles incompletos. El orden del array es el orden estable de presentación. El registro se cachea en controladores y no se recorre por frame.
