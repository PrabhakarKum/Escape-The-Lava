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
        [SerializeField] private DamageVignetteView damageVignette = null;
        [SerializeField] private EndScreenView endScreen = null;
        [SerializeField] private AudioManager audioManager = null;
        [SerializeField] private LevelManager levelManager = null;
        [SerializeField] private RoundController roundController = null;
        #endregion

        #region Initialization
        private void Start()
        {
            BuildGame();
        }

        private void BuildGame()
        {
            var config = gameConfig != null ? gameConfig : GameConfig.CreateRuntimeDefault();

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
            if (damageVignette != null) damageVignette.Initialize();
            if (endScreen != null) endScreen.Initialize();

            if (roundController != null)
            {
                WireEvents(config);
                roundController.Initialize(config, board);
                roundController.StartRound(levelManager.CurrentLevel);
            }
        }

        private void FetchMissingReferences()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (board == null) board = FindAnyObjectByType<BoardController>();
            if (input == null) input = FindAnyObjectByType<BoardInput>();
            if (roundController == null) roundController = FindAnyObjectByType<RoundController>();
            if (hud == null) hud = FindAnyObjectByType<HudView>();
            if (endScreen == null) endScreen = FindAnyObjectByType<EndScreenView>();
            if (floatingText == null) floatingText = FindAnyObjectByType<FloatingTextSpawner>();
            if (damageVignette == null) damageVignette = FindAnyObjectByType<DamageVignetteView>();
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
                roundController.OnTimerUpdated += hud.SetTimer;
                if (audioManager != null)
                {
                    roundController.OnTimerUpdated += audioManager.UpdateMusicPitch;
                }
                roundController.OnLivesChanged += hud.SetLives;
                roundController.OnScoreChanged += hud.SetScore;
            }

            if (endScreen != null)
            {
                roundController.OnRoundEnded += (result) => 
                {
                    if (audioManager != null)
                    {
                        audioManager.StopMusic();
                        if (result.Won) audioManager.PlayWin();
                        else audioManager.PlayLose();
                    }
                    endScreen.Show(result, levelManager.HasNextLevel());
                };
                roundController.OnRoundStarted += () =>
                {
                    if (audioManager != null) audioManager.PlayBackgroundMusic();
                    endScreen.HideImmediate();
                };
                
                endScreen.RetryRequested += () => 
                {
                    roundController.StartRound(levelManager.CurrentLevel);
                };

                endScreen.NextLevelRequested += () =>
                {
                    levelManager.AdvanceLevel(config);
                    if (hud != null) hud.SetLevelName(levelManager.GetLevelName());
                    if (cameraFraming != null && mainCamera != null) cameraFraming.Initialize(mainCamera, config, levelManager.CurrentLevel);
                    roundController.StartRound(levelManager.CurrentLevel);
                };
            }

            if (floatingText != null || fxSpawner != null || cameraShake != null || audioManager != null || damageVignette != null)
            {
                roundController.OnDiamondCollected += (worldPosition, screenPosition, pointsAwarded) =>
                {
                    if (audioManager != null) audioManager.PlayDiamondCollect();
                    if (floatingText != null) floatingText.Spawn($"+{pointsAwarded}", screenPosition, new Color(0.48f, 0.96f, 1f, 1f));
                    if (fxSpawner != null) fxSpawner.PlayDiamondCollect(worldPosition);
                };

                roundController.OnLavaHit += (worldPosition, screenPosition) =>
                {
                    if (audioManager != null) audioManager.PlayLavaHit();
                    if (floatingText != null) floatingText.Spawn("-1 Life", screenPosition, new Color(1f, 0.4f, 0.16f, 1f));
                    if (fxSpawner != null) fxSpawner.PlayLavaHit(worldPosition);
                    if (cameraShake != null) cameraShake.Shake(config.cameraShakeDuration, config.cameraShakeStrength);
                    if (damageVignette != null) damageVignette.Flash(config.lavaFlashDuration, config.lavaFlashAlpha);
                };

                roundController.OnSafeTap += (worldPosition) =>
                {
                    if (audioManager != null) audioManager.PlaySafeTap();
                    if (fxSpawner != null) fxSpawner.PlaySafeTap(worldPosition);
                };
            }
        }
        #endregion
    }
}
