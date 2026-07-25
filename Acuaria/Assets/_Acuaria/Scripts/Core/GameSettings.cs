using UnityEngine;

namespace Acuaria.Core
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Acuaria/Settings/Game")]
    public sealed class GameSettings : ScriptableObject
    {
        [SerializeField] private string initialSceneName = "Room";

        public string InitialSceneName => initialSceneName;
    }
}
