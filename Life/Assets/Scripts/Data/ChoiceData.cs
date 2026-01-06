#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Conditions;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// 選択肢のタイプ
    /// </summary>
    public enum ChoiceType
    {
        Normal,     // 通常の選択肢（時間制限なし）
        Timed,      // 時限選択（制限時間内に選択）
        StatBased   // ステータスベース（条件を満たさないと選択不可）
    }

    /// <summary>
    /// 選択肢データ
    /// ストーリー中に表示される選択肢を定義
    /// </summary>
    [Serializable]
    public class ChoiceData
    {
        [Tooltip("選択肢の一意なID")]
        public string choiceId = string.Empty;

        [Tooltip("表示するテキスト")]
        [TextArea(2, 4)]
        public string choiceText = string.Empty;

        [Tooltip("選択肢のタイプ")]
        public ChoiceType choiceType = ChoiceType.Normal;

        [Tooltip("時限選択の制限時間（秒）")]
        [Range(1f, 60f)]
        public float timeLimit = 10f;

        [Tooltip("この選択肢を表示するための条件（すべて満たす必要あり）")]
        public List<StoryCondition> requirements = new();

        [Tooltip("選択時に適用する効果")]
        public List<StoryEffect> effects = new();

        [Tooltip("選択後に遷移するシーンID")]
        public string nextSceneId = string.Empty;

        [Tooltip("選択後のトランジション設定")]
        public TransitionSettings transitionSettings = TransitionSettings.Default;

        [Tooltip("選択不可時に表示するテキスト（StatBasedの場合）")]
        [TextArea(1, 2)]
        public string lockedText = string.Empty;

        [Tooltip("選択不可時でも表示するかどうか")]
        public bool showWhenLocked = true;
    }
}
