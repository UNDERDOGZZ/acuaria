using System;
using System.Collections.Generic;
using Acuaria.Fish.Care;
using UnityEngine;

namespace Acuaria.Fish
{
    [CreateAssetMenu(menuName="Acuaria/Fish/Species Registry",fileName="FishSpeciesRegistry")]
    public sealed class FishSpeciesRegistry:ScriptableObject
    {
        [SerializeField] FishSpeciesDefinition[] species=Array.Empty<FishSpeciesDefinition>();
        public IReadOnlyList<FishSpeciesDefinition> Species=>Array.AsReadOnly(species??Array.Empty<FishSpeciesDefinition>());
        public FishSpeciesDefinition FindById(string id)
        {if(string.IsNullOrWhiteSpace(id)||species==null)return null;for(var i=0;i<species.Length;i++)
         if(species[i]!=null&&string.Equals(species[i].SpeciesId,id,StringComparison.Ordinal))return species[i];return null;}
        public IReadOnlyList<string> ValidateContent()
        {var issues=new List<string>();var ids=new HashSet<string>(StringComparer.Ordinal);var refs=new HashSet<FishSpeciesDefinition>();
         if(species==null){issues.Add("La lista de especies es nula.");return issues;}
         for(var i=0;i<species.Length;i++){var item=species[i];if(item==null){issues.Add($"Entrada {i} nula.");continue;}
          if(!refs.Add(item))issues.Add($"Referencia duplicada: {item.name}.");if(string.IsNullOrWhiteSpace(item.SpeciesId))issues.Add($"ID vacío en {item.name}.");
          else if(!ids.Add(item.SpeciesId))issues.Add($"ID duplicado: {item.SpeciesId}.");if(!item.HasCompleteContent)issues.Add($"Contenido incompleto: {item.SpeciesId}.");}
         return issues;}
        public List<FishSpeciesDefinition> Filter(FishCareDifficulty? difficulty=null,SwimmingLevel? zone=null,FishSocialType? social=null)
        {var result=new List<FishSpeciesDefinition>();if(species==null)return result;for(var i=0;i<species.Length;i++){var item=species[i];
          if(item==null||difficulty.HasValue&&item.Care.Difficulty!=difficulty.Value||zone.HasValue&&item.SwimmingLevel!=zone.Value||
             social.HasValue&&item.Social.SocialType!=social.Value)continue;result.Add(item);}return result;}
        public void Configure(params FishSpeciesDefinition[] definitions)=>species=definitions??Array.Empty<FishSpeciesDefinition>();
    }
}

