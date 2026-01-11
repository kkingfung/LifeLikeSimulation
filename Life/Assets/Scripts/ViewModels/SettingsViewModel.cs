#nullable enable
using System;
using LifeLike.Core.Commands;
using LifeLike.Core.MVVM;
using LifeLike.Core.Services;
using LifeLike.Services.Core.Audio;
using UnityEngine;

namespace LifeLike.ViewModels
{
    /// <summary>
    /// 設定画面のViewModel
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IAudioService? _audioService;

        // オーディオ設定
        private float _masterVolume = 1f;
        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;
        private float _voiceVolume = 1f;
        private bool _isMuted = false;

        // 表示設定
        private bool _isFullscreen = true;
        private int _resolutionIndex = 0;
        private int _qualityIndex = 2;

        // ゲーム設定
        private float _textSpeed = 1f;
        private bool _autoAdvance = false;
        private float _autoAdvanceDelay = 2f;
        private bool _skipUnread = false;
        private string _language = "Japanese";

        /// <summary>
        /// マスターボリューム (0-1)
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                if (SetProperty(ref _masterVolume, Mathf.Clamp01(value)))
                {
                    ApplyAudioVolume();
                }
            }
        }

        /// <summary>
        /// BGMボリューム (0-1)
        /// </summary>
        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                if (SetProperty(ref _bgmVolume, Mathf.Clamp01(value)))
                {
                    ApplyAudioVolume();
                }
            }
        }

        /// <summary>
        /// 効果音ボリューム (0-1)
        /// </summary>
        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                if (SetProperty(ref _sfxVolume, Mathf.Clamp01(value)))
                {
                    ApplyAudioVolume();
                }
            }
        }

        /// <summary>
        /// ボイスボリューム (0-1)
        /// </summary>
        public float VoiceVolume
        {
            get => _voiceVolume;
            set
            {
                if (SetProperty(ref _voiceVolume, Mathf.Clamp01(value)))
                {
                    ApplyAudioVolume();
                }
            }
        }

        /// <summary>
        /// ミュート状態
        /// </summary>
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (SetProperty(ref _isMuted, value))
                {
                    ApplyMute();
                }
            }
        }

        /// <summary>
        /// フルスクリーンモード
        /// </summary>
        public bool IsFullscreen
        {
            get => _isFullscreen;
            set
            {
                if (SetProperty(ref _isFullscreen, value))
                {
                    ApplyFullscreen();
                }
            }
        }

        /// <summary>
        /// 解像度インデックス
        /// </summary>
        public int ResolutionIndex
        {
            get => _resolutionIndex;
            set
            {
                if (SetProperty(ref _resolutionIndex, value))
                {
                    ApplyResolution();
                }
            }
        }

        /// <summary>
        /// 品質レベルインデックス
        /// </summary>
        public int QualityIndex
        {
            get => _qualityIndex;
            set
            {
                if (SetProperty(ref _qualityIndex, value))
                {
                    ApplyQuality();
                }
            }
        }

        /// <summary>
        /// テキスト表示速度 (0.5-2.0)
        /// </summary>
        public float TextSpeed
        {
            get => _textSpeed;
            set => SetProperty(ref _textSpeed, Mathf.Clamp(value, 0.5f, 2f));
        }

        /// <summary>
        /// オート進行
        /// </summary>
        public bool AutoAdvance
        {
            get => _autoAdvance;
            set => SetProperty(ref _autoAdvance, value);
        }

        /// <summary>
        /// オート進行の待機時間（秒）
        /// </summary>
        public float AutoAdvanceDelay
        {
            get => _autoAdvanceDelay;
            set => SetProperty(ref _autoAdvanceDelay, Mathf.Clamp(value, 0.5f, 10f));
        }

        /// <summary>
        /// 未読スキップ許可
        /// </summary>
        public bool SkipUnread
        {
            get => _skipUnread;
            set => SetProperty(ref _skipUnread, value);
        }

        /// <summary>
        /// 言語設定
        /// </summary>
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        /// <summary>
        /// 戻るコマンド
        /// </summary>
        public RelayCommand BackCommand { get; }

        /// <summary>
        /// デフォルトに戻すコマンド
        /// </summary>
        public RelayCommand ResetToDefaultCommand { get; }

        /// <summary>
        /// 戻る要求イベント
        /// </summary>
        public event Action? OnBackRequested;

        public SettingsViewModel()
        {
            // ServiceLocatorからAudioServiceを取得（存在しない場合はnull）
            _audioService = ServiceLocator.Instance.Get<IAudioService>();

            BackCommand = new RelayCommand(ExecuteBack);
            ResetToDefaultCommand = new RelayCommand(ExecuteResetToDefault);

            // 現在の設定を読み込む
            LoadSettings();
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        private void LoadSettings()
        {
            // オーディオ設定はAudioServiceから取得（重複を避ける）
            if (_audioService != null)
            {
                _masterVolume = _audioService.MasterVolume;
                _bgmVolume = _audioService.BgmVolume;
                _sfxVolume = _audioService.SfxVolume;
                _voiceVolume = _audioService.VoiceVolume;
                _isMuted = _audioService.IsMuted;
            }
            else
            {
                // AudioServiceがない場合はPlayerPrefsから読み込む
                _masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                _bgmVolume = PlayerPrefs.GetFloat("BgmVolume", 1f);
                _sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
                _voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
                _isMuted = PlayerPrefs.GetInt("AudioMuted", 0) == 1;
            }

            _isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            _resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
            _qualityIndex = PlayerPrefs.GetInt("QualityIndex", 2);
            _textSpeed = PlayerPrefs.GetFloat("TextSpeed", 1f);
            _autoAdvance = PlayerPrefs.GetInt("AutoAdvance", 0) == 1;
            _autoAdvanceDelay = PlayerPrefs.GetFloat("AutoAdvanceDelay", 2f);
            _skipUnread = PlayerPrefs.GetInt("SkipUnread", 0) == 1;
            _language = PlayerPrefs.GetString("Language", "Japanese");

            Debug.Log("[SettingsViewModel] 設定を読み込みました");
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
            PlayerPrefs.SetFloat("BgmVolume", _bgmVolume);
            PlayerPrefs.SetFloat("SfxVolume", _sfxVolume);
            PlayerPrefs.SetFloat("VoiceVolume", _voiceVolume);
            PlayerPrefs.SetInt("Fullscreen", _isFullscreen ? 1 : 0);
            PlayerPrefs.SetInt("ResolutionIndex", _resolutionIndex);
            PlayerPrefs.SetInt("QualityIndex", _qualityIndex);
            PlayerPrefs.SetFloat("TextSpeed", _textSpeed);
            PlayerPrefs.SetInt("AutoAdvance", _autoAdvance ? 1 : 0);
            PlayerPrefs.SetFloat("AutoAdvanceDelay", _autoAdvanceDelay);
            PlayerPrefs.SetInt("SkipUnread", _skipUnread ? 1 : 0);
            PlayerPrefs.SetString("Language", _language);
            PlayerPrefs.Save();

            Debug.Log("[SettingsViewModel] 設定を保存しました");
        }

        /// <summary>
        /// フルスクリーン設定を適用
        /// </summary>
        private void ApplyFullscreen()
        {
            Screen.fullScreen = _isFullscreen;
        }

        /// <summary>
        /// 解像度設定を適用
        /// </summary>
        private void ApplyResolution()
        {
            var resolutions = Screen.resolutions;
            if (_resolutionIndex >= 0 && _resolutionIndex < resolutions.Length)
            {
                var res = resolutions[_resolutionIndex];
                Screen.SetResolution(res.width, res.height, _isFullscreen);
                Debug.Log($"[SettingsViewModel] 解像度を変更: {res.width}x{res.height}");
            }
        }

        /// <summary>
        /// 品質設定を適用
        /// </summary>
        private void ApplyQuality()
        {
            QualitySettings.SetQualityLevel(_qualityIndex);
        }

        /// <summary>
        /// オーディオボリュームを適用
        /// </summary>
        private void ApplyAudioVolume()
        {
            if (_audioService == null) return;

            _audioService.MasterVolume = _masterVolume;
            _audioService.BgmVolume = _bgmVolume;
            _audioService.SfxVolume = _sfxVolume;
            _audioService.VoiceVolume = _voiceVolume;
        }

        /// <summary>
        /// ミュート状態を適用
        /// </summary>
        private void ApplyMute()
        {
            if (_audioService == null) return;

            _audioService.IsMuted = _isMuted;
        }

        /// <summary>
        /// 戻る
        /// </summary>
        private void ExecuteBack()
        {
            SaveSettings();
            _audioService?.SaveSettings();
            OnBackRequested?.Invoke();
        }

        /// <summary>
        /// デフォルトに戻す
        /// </summary>
        private void ExecuteResetToDefault()
        {
            MasterVolume = 1f;
            BgmVolume = 1f;
            SfxVolume = 1f;
            VoiceVolume = 1f;
            IsMuted = false;
            IsFullscreen = true;
            ResolutionIndex = 0;
            QualityIndex = 2;
            TextSpeed = 1f;
            AutoAdvance = false;
            AutoAdvanceDelay = 2f;
            SkipUnread = false;
            Language = "Japanese";

            Debug.Log("[SettingsViewModel] 設定をデフォルトに戻しました");
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnBackRequested = null;
            }
            base.Dispose(disposing);
        }
    }
}
