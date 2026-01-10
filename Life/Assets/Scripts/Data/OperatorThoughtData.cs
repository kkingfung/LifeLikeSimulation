#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Conditions;
using LifeLike.Data.Localization;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// オペレーターの思考が表示されるタイミング
    /// </summary>
    public enum ThoughtTiming
    {
        /// <summary>通話終了後</summary>
        AfterCall,

        /// <summary>夜の終了時</summary>
        EndOfNight,

        /// <summary>特定のフラグが設定された時</summary>
        OnFlagSet,

        /// <summary>特定の時刻</summary>
        AtTime,

        /// <summary>証拠を発見した時</summary>
        OnEvidenceDiscovered,

        /// <summary>特定のセグメント終了後</summary>
        AfterSegment
    }

    /// <summary>
    /// オペレーターの思考カテゴリ
    /// </summary>
    public enum ThoughtCategory
    {
        /// <summary>疑問・不審</summary>
        Suspicion,

        /// <summary>記憶・回想</summary>
        Memory,

        /// <summary>推理・接続</summary>
        Connection,

        /// <summary>感情・共感</summary>
        Emotion,

        /// <summary>システム・組織への疑問</summary>
        SystemDoubt,

        /// <summary>後悔・反省</summary>
        Regret,

        /// <summary>決意・覚悟</summary>
        Resolve
    }

    /// <summary>
    /// オペレーターの思考データ
    /// </summary>
    [Serializable]
    public class OperatorThought
    {
        [Header("基本情報")]
        [Tooltip("思考の一意なID")]
        public string thoughtId = string.Empty;

        [Tooltip("思考のカテゴリ")]
        public ThoughtCategory category = ThoughtCategory.Suspicion;

        [Header("内容")]
        [Tooltip("思考のテキスト（ローカライズ対応）")]
        public LocalizedString content = new();

        [Tooltip("内部メモ（開発者向け）")]
        [TextArea(1, 3)]
        public string note = string.Empty;

        [Header("表示タイミング")]
        [Tooltip("表示タイミング")]
        public ThoughtTiming timing = ThoughtTiming.AfterCall;

        [Tooltip("関連する通話ID（AfterCallの場合）")]
        public string relatedCallId = string.Empty;

        [Tooltip("関連するセグメントID（AfterSegmentの場合）")]
        public string triggerSegmentId = string.Empty;

        [Tooltip("表示時刻（AtTimeの場合、分単位）")]
        public int triggerTimeMinutes = 0;

        [Header("条件")]
        [Tooltip("この思考を表示するトリガーフラグ")]
        public string triggerFlagId = string.Empty;

        [Tooltip("追加の表示条件")]
        public List<StoryCondition> conditions = new();

        [Tooltip("この思考を抑制するフラグ（設定されていると表示しない）")]
        public List<string> suppressFlags = new();

        [Header("演出")]
        [Tooltip("表示時間（秒、0で自動計算）")]
        public float displayDuration = 0f;

        [Tooltip("フェードイン時間（秒）")]
        public float fadeInDuration = 0.5f;

        [Tooltip("フェードアウト時間（秒）")]
        public float fadeOutDuration = 0.5f;

        [Tooltip("優先度（同時に複数の思考がトリガーされた場合）")]
        public int priority = 0;

        /// <summary>
        /// 指定言語のテキストを取得
        /// </summary>
        public string GetContent(Language language) => content.GetText(language);
    }

}
