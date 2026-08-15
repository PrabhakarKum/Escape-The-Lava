using System.Collections.Generic;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    /// <summary>
    /// Handles all audio playback for the game.
    /// Utilizes a small object pool of AudioSources for sound effects to prevent clip cutoff during rapid interactions,
    /// and dynamically scales background music pitch based on time pressure.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        #region Settings
        [Header("Audio Clips")]
        [SerializeField] private AudioClip diamondCollectClip = null;
        [SerializeField] private AudioClip lavaHitClip = null;
        [SerializeField] private AudioClip safeTapClip = null;
        [SerializeField] private AudioClip winClip = null;
        [SerializeField] private AudioClip loseClip = null;
        [SerializeField] private AudioClip backgroundMusic = null;

        [Header("Settings")]
        [SerializeField] private int sfxPoolSize = 5;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;
        [SerializeField] private bool speedUpMusicUnderPressure = true;
        [SerializeField] private float pressureTimeSeconds = 10f;
        #endregion

        #region State
        private AudioSource musicSource;
        private readonly List<AudioSource> sfxSources = new();
        private int sfxIndex = 0;
        #endregion

        #region Initialization
        public void Initialize()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            for (int i = 0; i < sfxPoolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;
                sfxSources.Add(source);
            }
        }
        #endregion

        #region Music Control
        public void PlayBackgroundMusic()
        {
            if (musicSource == null) return;
            
            if (backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.pitch = 1f;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void UpdateMusicPitch(float remainingTime)
        {
            if (musicSource == null || !speedUpMusicUnderPressure || !musicSource.isPlaying) return;

            if (remainingTime <= pressureTimeSeconds && remainingTime > 0f)
            {
                float pressureRatio = 1f - (remainingTime / pressureTimeSeconds);
                musicSource.pitch = 1f + (pressureRatio * 0.5f);
            }
            else
            {
                musicSource.pitch = 1f;
            }
        }
        #endregion

        #region SFX Control
        public void PlayDiamondCollect() => PlaySfx(diamondCollectClip);
        public void PlayLavaHit() => PlaySfx(lavaHitClip);
        public void PlaySafeTap() => PlaySfx(safeTapClip);
        public void PlayWin() => PlaySfx(winClip);
        public void PlayLose() => PlaySfx(loseClip);

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null || sfxSources.Count == 0) return;

            AudioSource source = sfxSources[sfxIndex];
            source.clip = clip;
            source.pitch = Random.Range(0.95f, 1.05f);
            source.Play();

            sfxIndex = (sfxIndex + 1) % sfxSources.Count;
        }
        #endregion
    }
}
