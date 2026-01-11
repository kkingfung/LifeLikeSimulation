#nullable enable
using System;

namespace LifeLike.Services.Core.Audio
{
    /// <summary>
    /// オーディオ管理サービスのインターフェース
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// マスターボリューム (0-1)
        /// </summary>
        float MasterVolume { get; set; }

        /// <summary>
        /// BGMボリューム (0-1)
        /// </summary>
        float BgmVolume { get; set; }

        /// <summary>
        /// 効果音ボリューム (0-1)
        /// </summary>
        float SfxVolume { get; set; }

        /// <summary>
        /// ボイスボリューム (0-1)
        /// </summary>
        float VoiceVolume { get; set; }

        /// <summary>
        /// ミュート状態
        /// </summary>
        bool IsMuted { get; set; }

        /// <summary>
        /// ボリューム変更時のイベント
        /// </summary>
        event Action<string, float>? OnVolumeChanged;

        /// <summary>
        /// 設定をPlayerPrefsから読み込む
        /// </summary>
        void LoadSettings();

        /// <summary>
        /// 設定をPlayerPrefsに保存する
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// BGMを再生する
        /// </summary>
        /// <param name="clipName">クリップ名</param>
        /// <param name="fadeInDuration">フェードイン時間（秒）</param>
        void PlayBgm(string clipName, float fadeInDuration = 0.5f);

        /// <summary>
        /// BGMを停止する
        /// </summary>
        /// <param name="fadeOutDuration">フェードアウト時間（秒）</param>
        void StopBgm(float fadeOutDuration = 0.5f);

        /// <summary>
        /// 効果音を再生する
        /// </summary>
        /// <param name="clipName">クリップ名</param>
        void PlaySfx(string clipName);

        /// <summary>
        /// ボイスを再生する
        /// </summary>
        /// <param name="clipName">クリップ名</param>
        void PlayVoice(string clipName);

        /// <summary>
        /// ボイスを停止する
        /// </summary>
        void StopVoice();
    }
}
