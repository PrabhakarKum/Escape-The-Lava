using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FOG.EscapeTheLava
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton = null;
        [SerializeField] private string gameSceneName = "Game Scene";

        private void Awake()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(PlayGame);
                startButton.onClick.AddListener(PlayGame);
            }
        }

        private void OnDestroy()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(PlayGame);
            }
        }

        public void PlayGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
