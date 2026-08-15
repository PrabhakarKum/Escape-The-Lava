using UnityEngine;

namespace FOG.EscapeTheLava
{
    [DefaultExecutionOrder(-1000)]
    /// <summary>
    /// Acts as the main entry point and mediator for the game.
    /// It auto-fetches all necessary systems, wires them together via events, 
    /// and initializes the core game loop to ensure components remain decoupled.
    /// </summary>
    public sealed class GameBootstrapper : MonoBehaviour
    {
        #region Dependencies
        [SerializeField] private GameConfig gameConfig = null;
        [SerializeField] private Camera mainCamera = null;
        [SerializeField] private BoardInput input = null;
        [SerializeField] private BoardController board = null;
        [SerializeField] private WorldFxSpawner fxSpawner = null;
        [SerializeField] private CameraFramingController cameraFraming = null;
        [SerializeField] private CameraShake cameraShake = null;
        
        [SerializeField] private HudView hud = null;
        [SerializeField] private FloatingTextSpawner floatingText = null;
        [SerializeField] private EndScreenView endScreen = null;
        [SerializeField] private AudioManager audioManager = null;
        [SerializeField] private LevelManager levelManager = null;
        [SerializeField] private GameController gameController = null;
        #endregion

        #region Initialization
        private void Start()
        {
            BuildGame();
        }

        private void BuildGame()
        {
            GameConfig config = gameConfig != null ? gameConfig : GameConfig.CreateRuntimeDefault();

            FetchMissingReferences();

            if (fxSpawner != null) fxSpawner.Initialize();
            if (input != null && mainCamera != null && board != null) input.Initialize(mainCamera, board);
            
            if (audioManager != null) audioManager.Initialize();
            levelManager.Initialize(config);
            if (cameraFraming != null && mainCamera != null) cameraFraming.Initialize(mainCamera, config, levelManager.CurrentLevel);

            if (hud != null)
            {
                hud.Initialize(config);
                hud.SetLevelName(levelManager.GetLevelName());
            }
            if (floatingText != null) floatingText.Initialize(config);
            if (endScreen != null) endScreen.Initialize();

            if (gameController != null)
            {
                WireEvents(config);
                gameController.Initialize(config, board);
                gameController.StartRound(levelManager.CurrentLevel);
            }
        }

        private void FetchMissingReferences()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (board == null) board = FindAnyObjectByType<BoardController>();
            if (input == null) input = FindAnyObjectByType<BoardInput>();
            if (gameController == null) gameController = FindAnyObjectByType<GameController>();
            if (hud == null) hud = FindAnyObjectByType<HudView>();
            if (endScreen == null) endScreen = FindAnyObjectByType<EndScreenView>();
            if (floatingText == null) floatingText = FindAnyObjectByType<FloatingTextSpawner>();
            if (fxSpawner == null) fxSpawner = FindAnyObjectByType<WorldFxSpawner>();
            if (cameraFraming == null) cameraFraming = FindAnyObjectByType<CameraFramingController>();
            if (cameraShake == null) cameraShake = FindAnyObjectByType<CameraShake>();
            
            if (audioManager == null)
            {
                audioManager = FindAnyObjectByType<AudioManager>();
                if (audioManager == null)
                {
                    audioManager = new GameObject("Audio Manager").AddComponent<AudioManager>();
                }
            }
            if (levelManager == null)
            {
                levelManager = FindAnyObjectByType<LevelManager>();
                if (levelManager == null)
                {
                    levelManager = new GameObject("Level Manager").AddComponent<LevelManager>();
                }
            }
        }
        #endregion

        #region Event Wiring
        private void WireEvents(GameConfig config)
        {
            if (hud != null)
            {
                gameController.OnTimerUpdated += hud.SetTimer;
                if (audioManager != null)
                {
                    gameController.OnTimerUpdated += audioManager.UpdateMusicPitch;
                }
                gameController.OnLivesChanged += hud.SetLives;
                gameController.OnScoreChanged += hud.SetScore;
            }

            if (endScreen != null)
            {
                gameController.OnRoundEnded += (result) => 
                {
                    if (audioManager != null)
                    {
                        audioManager.StopMusic();
                        if (result.Won) audioManager.PlayWin();
                        else audioManager.PlayLose();
                    }
                    endScreen.Show(result, levelManager.HasNextLevel());
                };
                gameController.OnRoundStarted += () =>
                {
                    if (audioManager != null) audioManager.PlayBackgroundMusic();
                    endScreen.HideImmediate();
                };
                
                endScreen.RetryRequested += () => 
                {
                    gameController.StartRound(levelManager.CurrentLevel);
                };

                endScreen.NextLevelRequested += () =>
                {
                    levelManager.AdvanceLevel(config);
                    if (hud != null) hud.SetLevelName(levelManager.GetLevelName());
                    if (cameraFraming != null && mainCamera != null) cameraFraming.Initialize(mainCamera, config, levelManager.CurrentLevel);
                    gameController.StartRound(levelManager.CurrentLevel);
                };
            }

            if (floatingText != null || fxSpawner != null || cameraShake != null || audioManager != null)
            {
                gameController.OnDiamondCollected += (worldPos, screenPos, score) =>
                {
                    if (audioManager != null) audioManager.PlayDiamondCollect();
                    if (floatingText != null) floatingText.Spawn($"+{score}", screenPos, new Color(0.48f, 0.96f, 1f, 1f));
                    if (fxSpawner != null) fxSpawner.PlayDiamondCollect(worldPos);
                };

                gameController.OnLavaHit += (worldPos, screenPos) =>
                {
                    if (audioManager != null) audioManager.PlayLavaHit();
                    if (floatingText != null) floatingText.Spawn("-1 Life", screenPos, new Color(1f, 0.4f, 0.16f, 1f));
                    if (fxSpawner != null) fxSpawner.PlayLavaHit(worldPos);
                    if (cameraShake != null) cameraShake.Shake(config.CameraShakeDuration, config.CameraShakeStrength);
                };

                gameController.OnSafeTap += (worldPos) =>
                {
                    if (audioManager != null) audioManager.PlaySafeTap();
                    if (fxSpawner != null) fxSpawner.PlaySafeTap(worldPos);
                };
            }
        }
        #endregion
    }
}
