using System;
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
        [SerializeField] private AudioClip[] winClips = Array.Empty<AudioClip>();
        [SerializeField] private AudioClip[] loseClips = Array.Empty<AudioClip>();
        [SerializeField] private AudioClip backgroundMusic = null;

        [Header("Settings")]
        [SerializeField] private int sfxPoolSize = 5;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;
        [SerializeField] private bool speedUpMusicUnderPressure = true;
        [SerializeField] private float pressureTimeSeconds = 10f;
        #endregion

        #region State
        private AudioSource _musicSource;
        private readonly List<AudioSource> _sfxSources = new();
        private int _sfxIndex = 0;
        #endregion

        #region Initialization
        public void Initialize()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = musicVolume;

            for (var i = 0; i < sfxPoolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;
                _sfxSources.Add(source);
            }
        }
        #endregion

        #region Music Control
        public void PlayBackgroundMusic()
        {
            if (_musicSource == null) return;

            if (backgroundMusic == null) return;
            _musicSource.clip = backgroundMusic;
            _musicSource.pitch = 1f;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }

        public void UpdateMusicPitch(float remainingTime)
        {
            if (_musicSource == null || !speedUpMusicUnderPressure || !_musicSource.isPlaying) return;

            if (remainingTime <= pressureTimeSeconds && remainingTime > 0f)
            {
                var pressureRatio = 1f - (remainingTime / pressureTimeSeconds);
                _musicSource.pitch = 1f + (pressureRatio * 0.5f);
            }
            else
            {
                _musicSource.pitch = 1f;
            }
        }
        #endregion

        #region SFX Control
        public void PlayDiamondCollect() => PlaySfx(diamondCollectClip);
        public void PlayLavaHit() => PlaySfx(lavaHitClip);
        public void PlaySafeTap() => PlaySfx(safeTapClip);
        public void PlayWin() => PlaySfx(PickRandomClip(winClips));
        public void PlayLose() => PlaySfx(PickRandomClip(loseClips));

        private static AudioClip PickRandomClip(AudioClip[] clips)
        {
            return clips is { Length: > 0 } ? clips[UnityEngine.Random.Range(0, clips.Length)] : null;
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null || _sfxSources.Count == 0) return;

            var source = _sfxSources[_sfxIndex];
            source.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            source.PlayOneShot(clip);

            _sfxIndex = (_sfxIndex + 1) % _sfxSources.Count;
        }
        #endregion
    }
}
