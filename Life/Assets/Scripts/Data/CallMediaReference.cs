#nullable enable
using System;
using LifeLike.Data.Localization;
using UnityEngine;
using UnityEngine.Video;

namespace LifeLike.Data
{
    /// <summary>
    /// 通話メディアの種類
    /// 段階的な開発に対応
    /// </summary>
    public enum CallMediaType
    {
        /// <summary>シルエット画像＋テキスト（フェーズ1）</summary>
        SilhouetteText,

        /// <summary>シルエット画像＋音声（フェーズ2）</summary>
        SilhouetteVoice,

        /// <summary>動画（フェーズ3）</summary>
        Video
    }

    /// <summary>
    /// 通話メディアの参照
    /// シルエット→音声→動画の段階的開発に対応
    /// </summary>
    [Serializable]
    public class CallMediaReference
    {
        [Header("メディアタイプ")]
        [Tooltip("現在使用するメディアの種類")]
        public CallMediaType mediaType = CallMediaType.SilhouetteText;

        [Header("テキスト（フェーズ1）")]
        [Tooltip("発信者のセリフ（ローカライズ対応）")]
        public LocalizedString dialogueText = new();

        [Tooltip("テキスト表示速度（文字/秒）")]
        public float textSpeed = 30f;

        [Header("字幕")]
        [Tooltip("字幕トラックID（ScenarioLocalizationDataから取得）")]
        public string subtitleTrackId = string.Empty;

        [Tooltip("インライン字幕（外部ファイルを使わない場合）")]
        public SubtitleTrack inlineSubtitles = new();

        [Header("音声（フェーズ2）")]
        [Tooltip("音声クリップ（ローカル）")]
        public AudioClip? localAudioClip;

        [Tooltip("音声のAssetBundle名")]
        public string audioBundleName = string.Empty;

        [Tooltip("音声のアセット名")]
        public string audioAssetName = string.Empty;

        [Header("動画（フェーズ3）")]
        [Tooltip("動画の参照（既存のVideoReferenceを使用）")]
        public VideoReference videoReference = new();

        [Header("共通設定")]
        [Tooltip("このメディアの再生時間（秒）- テキストの場合は自動計算も可能")]
        public float duration = 0f;

        [Tooltip("感情状態（シルエットの色や表示に影響）")]
        public EmotionalState emotionalState = EmotionalState.Neutral;

        /// <summary>
        /// 有効なメディアがあるか
        /// </summary>
        public bool IsValid
        {
            get
            {
                return mediaType switch
                {
                    CallMediaType.SilhouetteText => dialogueText.entries.Count > 0,
                    CallMediaType.SilhouetteVoice => localAudioClip != null ||
                                                     !string.IsNullOrEmpty(audioBundleName),
                    CallMediaType.Video => videoReference.IsValid,
                    _ => false
                };
            }
        }

        /// <summary>
        /// 字幕があるか
        /// </summary>
        public bool HasSubtitles =>
            !string.IsNullOrEmpty(subtitleTrackId) || inlineSubtitles.entries.Count > 0;

        /// <summary>
        /// 指定言語のダイアログテキストを取得
        /// </summary>
        public string GetDialogueText(Language language)
        {
            return dialogueText.GetText(language);
        }

        /// <summary>
        /// 推定再生時間を取得
        /// </summary>
        public float GetEstimatedDuration(Language language = Language.Japanese)
        {
            if (duration > 0) return duration;

            return mediaType switch
            {
                CallMediaType.SilhouetteText => dialogueText.GetText(language).Length / textSpeed,
                CallMediaType.SilhouetteVoice => localAudioClip?.length ?? 0f,
                CallMediaType.Video => 0f, // 動画は非同期で取得
                _ => 0f
            };
        }
    }

    /// <summary>
    /// 感情状態
    /// シルエットの色や演出に影響
    /// </summary>
    public enum EmotionalState
    {
        Neutral,        // 通常
        Calm,           // 落ち着いている
        Nervous,        // 緊張
        Scared,         // 恐怖
        Angry,          // 怒り
        Sad,            // 悲しみ
        Panicked,       // パニック
        Confused,       // 混乱
        Suspicious,     // 疑念
        Lying,          // 嘘をついている（内部フラグ）
        Relieved,       // 安堵
        Desperate       // 絶望
    }
}
