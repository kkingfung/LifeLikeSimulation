#nullable enable
using System.Collections.Generic;
using LifeLike.Data.Conditions;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// ストーリーシーンデータ
    /// 一つの動画シーンとその選択肢を定義するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewStoryScene", menuName = "LifeLike/Story Scene")]
    public class StorySceneData : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("シーンの一意なID")]
        public string sceneId = string.Empty;

        [Tooltip("シーン名（デバッグ用）")]
        public string sceneName = string.Empty;

        [Tooltip("シーンの説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;

        [Header("動画")]
        [Tooltip("動画の参照情報（ローカル/AssetBundle/StreamingAssets対応）")]
        public VideoReference video = new();

        [Header("選択肢")]
        [Tooltip("選択肢が表示されるタイミング（動画開始からの秒数、-1で動画終了時）")]
        public float choiceAppearTime = -1f;

        [Tooltip("このシーンの選択肢リスト")]
        public List<ChoiceData> choices = new();

        [Tooltip("選択肢なしまたはタイムアウト時の次シーンID")]
        public string defaultNextSceneId = string.Empty;

        [Tooltip("デフォルト遷移時のトランジション設定")]
        public TransitionSettings defaultTransition = TransitionSettings.Default;

        [Header("シーン開始時の効果")]
        [Tooltip("このシーン開始時に適用する効果")]
        public List<StoryEffect> onEnterEffects = new();

        [Header("条件")]
        [Tooltip("このシーンを表示するための条件")]
        public List<StoryCondition> displayConditions = new();

        [Header("メタデータ")]
        [Tooltip("チャプター番号")]
        public int chapter = 1;

        [Tooltip("このシーンがエンディングかどうか")]
        public bool isEnding = false;

        [Tooltip("エンディングの種類（isEndingがtrueの場合）")]
        public string endingType = string.Empty;

        /// <summary>
        /// 動画があるかどうか
        /// </summary>
        public bool HasVideo => video != null && video.IsValid;

        /// <summary>
        /// 選択肢があるかどうか
        /// </summary>
        public bool HasChoices => choices.Count > 0;

        /// <summary>
        /// 選択肢表示タイミングが動画終了時かどうか
        /// </summary>
        public bool ShowChoicesAtEnd => choiceAppearTime < 0;
    }
}
