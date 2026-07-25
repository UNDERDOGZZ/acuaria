using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Acuaria.Core
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private GameSettings gameSettings;

        private IEnumerator Start()
        {
            if (gameSettings == null)
            {
                Debug.LogError("Bootstrap requires GameSettings.", this);
                yield break;
            }

            yield return SceneManager.LoadSceneAsync(gameSettings.InitialSceneName, LoadSceneMode.Single);
        }
    }
}
