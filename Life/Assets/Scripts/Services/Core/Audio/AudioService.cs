#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace LifeLike.Services.Core.Audio
{
    /// <summary>
    /// オーディオ管理サービスの実装
    /// AudioMixerを使用してボリュームを制御する
    /// </summary>
    public class AudioService : IAudioService
    {
        private const string MasterVolumeKey = "MasterVolume";
        private const string BgmVolumeKey = "BgmVolume";
        private const string SfxVolumeKey = "SfxVolume";
        private const string VoiceVolumeKey = "VoiceVolume";
        private const string MutedKey = "AudioMuted";

        // AudioMixerのパラメータ名
        private const string MasterMixerParam = "MasterVolume";
        private const string BgmMixerParam = "BgmVolume";
        private const string SfxMixerParam = "SfxVolume";
        private const string VoiceMixerParam = "VoiceVolume";

        private AudioMixer? _audioMixer;
        private AudioSource? _bgmSource;
        private AudioSource? _sfxSource;
        private AudioSource? _voiceSource;
        private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

        private float _masterVolume = 1f;
        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;
        private float _voiceVolume = 1f;
        private bool _isMuted = false;

        /// <inheritdoc/>
        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                ApplyVolume(MasterMixerParam, _masterVolume);
                OnVolumeChanged?.Invoke(MasterVolumeKey, _masterVolume);
            }
        }

        /// <inheritdoc/>
        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                ApplyVolume(BgmMixerParam, _bgmVolume);
                OnVolumeChanged?.Invoke(BgmVolumeKey, _bgmVolume);
            }
        }

        /// <inheritdoc/>
        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                ApplyVolume(SfxMixerParam, _sfxVolume);
                OnVolumeChanged?.Invoke(SfxVolumeKey, _sfxVolume);
            }
        }

        /// <inheritdoc/>
        public float VoiceVolume
        {
            get => _voiceVolume;
            set
            {
                _voiceVolume = Mathf.Clamp01(value);
                ApplyVolume(VoiceMixerParam, _voiceVolume);
                OnVolumeChanged?.Invoke(VoiceVolumeKey, _voiceVolume);
            }
        }

        /// <inheritdoc/>
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                ApplyMute();
            }
        }

        /// <inheritdoc/>
        public event Action<string, float>? OnVolumeChanged;

        /// <summary>
        /// AudioMixerとAudioSourceを初期化する
        /// </summary>
        /// <param name="audioMixer">使用するAudioMixer</param>
        /// <param name="bgmSource">BGM用AudioSource</param>
        /// <param name="sfxSource">効果音用AudioSource</param>
        /// <param name="voiceSource">ボイス用AudioSource</param>
        public void Initialize(AudioMixer audioMixer, AudioSource bgmSource, AudioSource sfxSource, AudioSource voiceSource)
        {
            _audioMixer = audioMixer;
            _bgmSource = bgmSource;
            _sfxSource = sfxSource;
            _voiceSource = voiceSource;

            LoadSettings();
            ApplyAllVolumes();

            Debug.Log("[AudioService] 初期化完了");
        }

        /// <summary>
        /// AudioClipを登録する
        /// </summary>
        /// <param name="clipName">クリップ名</param>
        /// <param name="clip">AudioClip</param>
        public void RegisterClip(string clipName, AudioClip clip)
        {
            _audioClips[clipName] = clip;
        }

        /// <inheritdoc/>
        public void LoadSettings()
        {
            _masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            _bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            _voiceVolume = PlayerPrefs.GetFloat(VoiceVolumeKey, 1f);
            _isMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;

            Debug.Log("[AudioService] 設定を読み込みました");
        }

        /// <inheritdoc/>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, _masterVolume);
            PlayerPrefs.SetFloat(BgmVolumeKey, _bgmVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
            PlayerPrefs.SetFloat(VoiceVolumeKey, _voiceVolume);
            PlayerPrefs.SetInt(MutedKey, _isMuted ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log("[AudioService] 設定を保存しました");
        }

        /// <inheritdoc/>
        public void PlayBgm(string clipName, float fadeInDuration = 0.5f)
        {
            if (_bgmSource == null)
            {
                Debug.LogWarning("[AudioService] BGM AudioSourceが設定されていません");
                return;
            }

            if (_audioClips.TryGetValue(clipName, out var clip))
            {
                _bgmSource.clip = clip;
                _bgmSource.loop = true;
                _bgmSource.Play();
                Debug.Log($"[AudioService] BGM再生: {clipName}");
            }
            else
            {
                Debug.LogWarning($"[AudioService] BGMクリップが見つかりません: {clipName}");
            }
        }

        /// <inheritdoc/>
        public void StopBgm(float fadeOutDuration = 0.5f)
        {
            if (_bgmSource == null) return;

            _bgmSource.Stop();
            Debug.Log("[AudioService] BGM停止");
        }

        /// <inheritdoc/>
        public void PlaySfx(string clipName)
        {
            if (_sfxSource == null)
            {
                Debug.LogWarning("[AudioService] SFX AudioSourceが設定されていません");
                return;
            }

            if (_audioClips.TryGetValue(clipName, out var clip))
            {
                _sfxSource.PlayOneShot(clip);
                Debug.Log($"[AudioService] SFX再生: {clipName}");
            }
            else
            {
                Debug.LogWarning($"[AudioService] SFXクリップが見つかりません: {clipName}");
            }
        }

        /// <inheritdoc/>
        public void PlayVoice(string clipName)
        {
            if (_voiceSource == null)
            {
                Debug.LogWarning("[AudioService] Voice AudioSourceが設定されていません");
                return;
            }

            if (_audioClips.TryGetValue(clipName, out var clip))
            {
                _voiceSource.clip = clip;
                _voiceSource.Play();
                Debug.Log($"[AudioService] Voice再生: {clipName}");
            }
            else
            {
                Debug.LogWarning($"[AudioService] Voiceクリップが見つかりません: {clipName}");
            }
        }

        /// <inheritdoc/>
        public void StopVoice()
        {
            if (_voiceSource == null) return;

            _voiceSource.Stop();
            Debug.Log("[AudioService] Voice停止");
        }

        /// <summary>
        /// AudioMixerにボリュームを適用する
        /// </summary>
        private void ApplyVolume(string paramName, float volume)
        {
            if (_audioMixer == null) return;

            // 0-1のボリュームを-80dB〜0dBに変換
            float db = volume > 0 ? Mathf.Log10(volume) * 20f : -80f;
            _audioMixer.SetFloat(paramName, db);
        }

        /// <summary>
        /// すべてのボリュームを適用する
        /// </summary>
        private void ApplyAllVolumes()
        {
            ApplyVolume(MasterMixerParam, _isMuted ? 0f : _masterVolume);
            ApplyVolume(BgmMixerParam, _bgmVolume);
            ApplyVolume(SfxMixerParam, _sfxVolume);
            ApplyVolume(VoiceMixerParam, _voiceVolume);
        }

        /// <summary>
        /// ミュート状態を適用する
        /// </summary>
        private void ApplyMute()
        {
            ApplyVolume(MasterMixerParam, _isMuted ? 0f : _masterVolume);
        }
    }
}
