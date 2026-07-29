using System;

namespace Acuaria.Save
{
    public enum SaveLoadSource { None, Main, Backup, NewGame }
    public sealed class SaveOperationResult
    {
        public bool Success; public string Message; public AcuariaSaveData Data;
        public SaveLoadSource Source; public SaveValidationResult Validation; public bool WasMigrated;
    }
    public sealed class SaveService
    {
        readonly ISaveSerializer serializer; readonly ISaveFileStorage storage;
        readonly SaveValidator validator; readonly SaveMigrationPipeline migrations;
        bool busy;
        public SaveService(ISaveSerializer saveSerializer, ISaveFileStorage fileStorage, SaveValidator saveValidator, SaveMigrationPipeline pipeline)
        { serializer = saveSerializer; storage = fileStorage; validator = saveValidator; migrations = pipeline; }
        public SaveOperationResult Save(AcuariaSaveData data)
        {
            if (busy) return new SaveOperationResult { Message = "A save operation is already running." };
            busy = true;
            try
            {
                var validation = validator.Validate(data);
                if (!validation.IsValid) return new SaveOperationResult { Message = "Save data is invalid.", Validation = validation };
                var json = SaveIntegrity.Stamp(data, serializer);
                storage.WriteTemporary(json);
                if (!serializer.TryDeserialize(storage.Read(storage.TemporaryPath), out var roundTrip) ||
                    !validator.Validate(roundTrip).IsValid || !SaveIntegrity.Verify(roundTrip, serializer))
                    return new SaveOperationResult { Message = "Temporary save verification failed." };
                storage.CommitTemporary();
                return new SaveOperationResult { Success = true, Message = "Saved.", Data = data, Validation = validation };
            }
            catch (Exception exception) { return new SaveOperationResult { Message = exception.Message }; }
            finally { busy = false; }
        }
        public SaveOperationResult Load()
        {
            var main = TryLoad(storage.MainPath, SaveLoadSource.Main);
            if (main.Success || main.Validation?.IsFutureVersion == true) return main;
            var backup = TryLoad(storage.BackupPath, SaveLoadSource.Backup);
            if (backup.Success) return backup;
            return new SaveOperationResult { Message = $"No valid local save. Main: {main.Message}; backup: {backup.Message}" };
        }
        SaveOperationResult TryLoad(string path, SaveLoadSource source)
        {
            if (!storage.Exists(path)) return new SaveOperationResult { Message = "File not found.", Source = source };
            try
            {
                var json = storage.Read(path);
                if (!serializer.TryDeserialize(json, out var data)) { storage.PreserveCorrupt(path); return new SaveOperationResult { Message = "Invalid JSON.", Source = source }; }
                var validation = validator.Validate(data);
                if (validation.IsFutureVersion) return new SaveOperationResult { Message = "Future schema was not modified.", Source = source, Validation = validation };
                if (!validation.IsValid || !SaveIntegrity.Verify(data, serializer))
                { storage.PreserveCorrupt(path); return new SaveOperationResult { Message = "Validation or checksum failed.", Source = source, Validation = validation }; }
                var sourceSchemaVersion = data.SchemaVersion;
                data = migrations.Migrate(data);
                return new SaveOperationResult { Success = true, Message = "Loaded.", Source = source, Data = data,
                    Validation = validation, WasMigrated = data.SchemaVersion != sourceSchemaVersion };
            }
            catch (Exception exception) { storage.PreserveCorrupt(path); return new SaveOperationResult { Message = exception.Message, Source = source }; }
        }
    }
}
