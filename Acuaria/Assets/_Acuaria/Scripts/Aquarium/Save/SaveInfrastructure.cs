using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Acuaria.Save
{
    public interface ISaveSerializer
    {
        string Serialize(AcuariaSaveData value);
        AcuariaSaveData Deserialize(string json);
        bool TryDeserialize(string json, out AcuariaSaveData value);
    }
    public sealed class JsonUtilitySaveSerializer : ISaveSerializer
    {
        public string Serialize(AcuariaSaveData value) => JsonUtility.ToJson(value, true);
        public AcuariaSaveData Deserialize(string json) => JsonUtility.FromJson<AcuariaSaveData>(json);
        public bool TryDeserialize(string json, out AcuariaSaveData value)
        {
            try { value = string.IsNullOrWhiteSpace(json) ? null : Deserialize(json); return value != null; }
            catch (Exception) { value = null; return false; }
        }
    }
    public interface ISaveFileStorage
    {
        string MainPath { get; } string BackupPath { get; } string TemporaryPath { get; }
        bool Exists(string path); string Read(string path); void WriteTemporary(string content); void CommitTemporary();
        void PreserveCorrupt(string path);
    }
    public sealed class SaveFileStorage : ISaveFileStorage
    {
        readonly string directory;
        public string MainPath => Path.Combine(directory, SaveSystemDefinition.MainFileName);
        public string BackupPath => Path.Combine(directory, SaveSystemDefinition.BackupFileName);
        public string TemporaryPath => Path.Combine(directory, SaveSystemDefinition.TemporaryFileName);
        public SaveFileStorage(string path) => directory = string.IsNullOrWhiteSpace(path) ? throw new ArgumentException(nameof(path)) : path;
        public bool Exists(string path) => File.Exists(path);
        public string Read(string path) => File.ReadAllText(path, Encoding.UTF8);
        public void WriteTemporary(string content)
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(TemporaryPath, content, new UTF8Encoding(false));
        }
        public void CommitTemporary()
        {
            if (!File.Exists(TemporaryPath)) throw new FileNotFoundException("Temporary save is missing.", TemporaryPath);
            if (File.Exists(MainPath))
            {
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
                File.Move(MainPath, BackupPath);
            }
            File.Move(TemporaryPath, MainPath);
        }
        public void PreserveCorrupt(string path)
        {
            if (!File.Exists(path)) return;
            var destination = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            File.Copy(path, destination, false);
        }
    }
    public static class SaveIntegrity
    {
        public static string Calculate(string content)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
            var result = new StringBuilder(hash.Length * 2);
            foreach (var value in hash) result.Append(value.ToString("x2"));
            return result.ToString();
        }
        public static string Stamp(AcuariaSaveData data, ISaveSerializer serializer)
        {
            data.IntegrityData ??= new IntegritySaveData();
            data.IntegrityData.Checksum = string.Empty;
            data.IntegrityData.Checksum = Calculate(serializer.Serialize(data));
            return serializer.Serialize(data);
        }
        public static bool Verify(AcuariaSaveData data, ISaveSerializer serializer)
        {
            if (data?.IntegrityData == null || string.IsNullOrWhiteSpace(data.IntegrityData.Checksum)) return false;
            var expected = data.IntegrityData.Checksum;
            data.IntegrityData.Checksum = string.Empty;
            var actual = Calculate(serializer.Serialize(data));
            data.IntegrityData.Checksum = expected;
            return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        }
    }
}
