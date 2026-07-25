using UnityEngine;

namespace Acuaria.Audio
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Acuaria/Settings/Audio")]
    public sealed class AudioSettings : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

        public float MasterVolume => masterVolume;
    }
}
