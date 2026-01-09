#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Flag
{
    /// <summary>
    /// フラグのカテゴリ
    /// </summary>
    public enum FlagCategory
    {
        /// <summary>安心フラグ — 偽りの安心を与えたか</summary>
        Reassurance,

        /// <summary>開示フラグ — 通話間で情報を共有したか</summary>
        Disclosure,

        /// <summary>エスカレーションフラグ — 行動したか、先送りしたか</summary>
        Escalation,

        /// <summary>アラインメントフラグ — システム信頼度</summary>
        Alignment,

        /// <summary>証拠フラグ — 情報の収集・接続</summary>
        Evidence,

        /// <summary>矛盾検出フラグ — 話の変化を追跡</summary>
        Contradiction,

        /// <summary>伏線フラグ — 後の夜で意味を持つ</summary>
        Foreshadowing,

        /// <summary>イベントフラグ — 状態追跡</summary>
        Event,

        /// <summary>派遣タイミングフラグ</summary>
        Dispatch,

        // === Night03 追加 ===
        /// <summary>脅威フラグ — 組織からの脅迫</summary>
        Threat,

        /// <summary>Night01の影響フラグ</summary>
        Night01Effect,

        /// <summary>Night02の影響フラグ</summary>
        Night02Effect,

        // === Night04 追加 ===
        /// <summary>Night03の影響フラグ</summary>
        Night03Effect,

        // === Night05 追加 ===
        /// <summary>Night04の影響フラグ</summary>
        Night04Effect
    }

    /// <summary>
    /// フラグ定義
    /// </summary>
    [Serializable]
    public class FlagDefinition
    {
        [Tooltip("フラグの一意なID")]
        public string flagId = string.Empty;

        [Tooltip("フラグのカテゴリ")]
        public FlagCategory category = FlagCategory.Event;

        [Tooltip("フラグの説明")]
        [TextArea(1, 3)]
        public string description = string.Empty;

        [Tooltip("スコア計算時の重み")]
        public int weight = 1;

        [Tooltip("夜をまたいで永続化するか")]
        public bool persistsAcrossNights = false;

        [Tooltip("このフラグが設定されたときにクリアされるフラグID")]
        public List<string> cancelsFlags = new();
    }

    /// <summary>
    /// 相互排他ルール
    /// </summary>
    [Serializable]
    public class MutualExclusionRule
    {
        [Tooltip("このフラグが設定されたとき")]
        public string whenFlagSet = string.Empty;

        [Tooltip("これらのフラグをクリアする")]
        public List<string> cancelFlags = new();
    }

    /// <summary>
    /// フラグの実行時状態
    /// </summary>
    [Serializable]
    public class FlagState
    {
        public string flagId = string.Empty;
        public bool isSet = false;
        public int setTime = 0; // 設定されたゲーム内時刻（分）
    }

    /// <summary>
    /// 夜のフラグスナップショット（セーブ/ロード用）
    /// </summary>
    [Serializable]
    public class NightFlagSnapshot
    {
        public string nightId = string.Empty;
        public List<FlagState> flagStates = new();
        public Dictionary<FlagCategory, int> calculatedScores = new();

        /// <summary>
        /// スナップショットを作成
        /// </summary>
        public static NightFlagSnapshot Create(string nightId, Dictionary<string, FlagState> flags, NightFlagsDefinition? definition)
        {
            var snapshot = new NightFlagSnapshot
            {
                nightId = nightId,
                flagStates = new List<FlagState>(flags.Values)
            };

            // スコアを計算して保存
            if (definition != null)
            {
                foreach (FlagCategory category in Enum.GetValues(typeof(FlagCategory)))
                {
                    int score = 0;
                    var categoryFlags = definition.GetFlagsByCategory(category);
                    foreach (var flagDef in categoryFlags)
                    {
                        if (flags.TryGetValue(flagDef.flagId, out var state) && state.isSet)
                        {
                            score += flagDef.weight;
                        }
                    }
                    snapshot.calculatedScores[category] = score;
                }
            }

            return snapshot;
        }
    }
}
