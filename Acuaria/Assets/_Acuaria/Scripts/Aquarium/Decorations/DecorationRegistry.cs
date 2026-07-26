using System;
using System.Collections.Generic;
using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    [CreateAssetMenu(menuName = "Acuaria/Aquarium/Decoration Registry", fileName = "DecorationRegistry")]
    public sealed class DecorationRegistry : ScriptableObject
    {
        [SerializeField] DecorationDefinition[] decorations = Array.Empty<DecorationDefinition>();
        public IReadOnlyList<DecorationDefinition> Decorations => decorations;
        public void Configure(params DecorationDefinition[] values) => decorations = values ?? Array.Empty<DecorationDefinition>();
        public DecorationDefinition FindById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            for (var i = 0; i < decorations.Length; i++)
                if (decorations[i] != null && string.Equals(decorations[i].DecorationId, id, StringComparison.Ordinal))
                    return decorations[i];
            return null;
        }
        public List<string> ValidateContent()
        {
            var issues = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < decorations.Length; i++)
            {
                var item = decorations[i];
                if (item == null || !item.IsValid) { issues.Add($"Decoración inválida en índice {i}."); continue; }
                if (!ids.Add(item.DecorationId)) issues.Add($"ID de decoración duplicada: {item.DecorationId}.");
            }
            return issues;
        }
    }
}
