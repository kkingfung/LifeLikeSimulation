#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Conditions;
using LifeLike.Data.Flag;
using UnityEngine;

namespace LifeLike.Data.EndState
{
    /// <summary>
    /// エンドステートの種類
    /// </summary>
    public enum EndStateType
    {
        // === Night01 共通 ===
        /// <summary>封じ込め — インシデントがプロトコル内で管理された</summary>
        Contained,

        /// <summary>露出 — 接続が作られ、真実が浮上</summary>
        Exposed,

        /// <summary>共犯 — オペレーターが隠蔽を可能にした</summary>
        Complicit,

        /// <summary>要注意 — オペレーターがレビュー対象としてマーク</summary>
        Flagged,

        /// <summary>吸収 — オペレーターが日常に消えた</summary>
        Absorbed,

        // === Night02 追加 ===
        /// <summary>警戒 — 危険を認識し、情報を収集した</summary>
        Vigilant,

        /// <summary>従順 — システムに従った</summary>
        Compliant,

        /// <summary>接続 — 断片を結びつけた</summary>
        Connected,

        /// <summary>孤立 — 誰も信用しなかった</summary>
        Isolated,

        /// <summary>日常 — 普通の夜として処理した</summary>
        Routine,

        // === Night03 追加 ===
        /// <summary>分かれ道 — 真理を助け、佐藤に情報を渡した</summary>
        Crossroads,

        /// <summary>介入 — 真理を助け、佐藤に情報を渡さなかった</summary>
        Intervention,

        /// <summary>開示 — 真理を助けず、佐藤に情報を渡した</summary>
        Disclosure,

        /// <summary>沈黙 — 真理を助けず、佐藤に情報を渡さなかった</summary>
        Silence,

        // === Night04 追加 ===
        /// <summary>証人と接続 — 詳細情報を得て、繋がりも伝えた</summary>
        WitnessConnected,

        /// <summary>証人 — 詳細情報を得たが、繋がりは伝えなかった</summary>
        WitnessOnly,

        /// <summary>接続のみ — 詳細情報は少ないが、繋がりは伝えた（ConnectedをNight04でも使用）</summary>
        ConnectedOnly,

        /// <summary>どちらもなし — 情報も少なく、繋がりも伝えなかった</summary>
        Neither,

        // === Night05 追加 ===
        /// <summary>届いた声 — 情報を多く集め、繋がりを見つけた</summary>
        VoiceReached,

        /// <summary>遠い声 — 部分的に情報を得た</summary>
        VoiceDistant,

        /// <summary>消えた声 — ほとんど情報を得られなかった</summary>
        VoiceLost,

        // === Night06 追加 ===
        /// <summary>嵐への備え — 多くの繋がりを見つけ、対策を講じた</summary>
        StormPrepared,

        /// <summary>嵐の予感 — 繋がりに気づいたが、完全な対策は取れなかった</summary>
        StormAware,

        /// <summary>遠い雷鳴 — 部分的な情報しか得られなかった</summary>
        StormDistant,

        /// <summary>静かな午後 — 嵐が来ることを知らない</summary>
        StormUnaware,

        // === Night07 追加 ===
        /// <summary>小さな光 — 美咲は安全、真相に迫った</summary>
        MisakiProtected,

        /// <summary>守られた秘密 — 美咲は安全だが、全容は見えない</summary>
        MisakiSafeUnaware,

        /// <summary>崩壊 — 美咲も連れ去られた</summary>
        MisakiTaken,

        /// <summary>崩壊の夜 — 真理は連れ去られ、美咲の運命は不明</summary>
        CollapseWitnessed,

        // === Night08 追加 ===
        /// <summary>真実を追う者 — 警察に全面協力し、組織に逆らった</summary>
        TruthSeeker,

        /// <summary>慎重な知識 — 多くの情報を得たが、組織には従った</summary>
        InformedCaution,

        /// <summary>沈黙の証人 — 情報を得たが、警察にも組織にも協力的ではなかった</summary>
        SilentWitness,

        /// <summary>無知な生存者 — 情報をほとんど得ずに夜を終えた</summary>
        UnawareSurvivor,

        // === Night09 追加 ===
        /// <summary>完全な同盟 — 警察と同盟を結び、告発者を保護し、全てを話すことを決意</summary>
        FullAlliance,

        /// <summary>積極的な協力 — 警察と協力し、全てを話すことを決意</summary>
        ActiveAlliance,

        /// <summary>真実を知った沈黙 — 多くの真実を知ったが、沈黙を選んだ</summary>
        PassiveTruth,

        /// <summary>告発者を救った — 告発者を警察に保護させることに成功</summary>
        WhistleblowerSaved,

        /// <summary>告発者の危機 — 告発者を助けられなかった</summary>
        WhistleblowerEndangered,

        /// <summary>美咲の存在を知った — オペレーターが美咲の存在と居場所を知った</summary>
        MisakiDiscovered,

        /// <summary>真実が明らかに — 誠和製薬の闘の全容が明らかになった</summary>
        TruthRevealed,

        /// <summary>不確かな未来 — 多くの情報を得たが、決定的な行動は取れなかった</summary>
        UncertainFuture,

        // === Night10 追加 ===
        /// <summary>真実の夜明け — 美咲救出、USB発見、誠和製薬壊滅</summary>
        TruthDawn,

        /// <summary>捜査は続く — 美咲救出、USB未発見、捜査継続</summary>
        InvestigationContinues,

        /// <summary>闇の中へ — 美咲死亡、隠蔽成功</summary>
        IntoDarkness,

        /// <summary>不確かな夜明け — 美咲の運命不明</summary>
        UncertainDawn
    }

    /// <summary>
    /// スコア条件
    /// </summary>
    [Serializable]
    public class ScoreCondition
    {
        [Tooltip("対象のフラグカテゴリ")]
        public FlagCategory category = FlagCategory.Escalation;

        [Tooltip("比較演算子")]
        public ComparisonOperator comparison = ComparisonOperator.GreaterThanOrEqual;

        [Tooltip("比較値")]
        public int value = 0;

        /// <summary>
        /// 条件を評価する
        /// </summary>
        public bool Evaluate(int currentScore)
        {
            return comparison switch
            {
                ComparisonOperator.Equal => currentScore == value,
                ComparisonOperator.NotEqual => currentScore != value,
                ComparisonOperator.GreaterThan => currentScore > value,
                ComparisonOperator.GreaterThanOrEqual => currentScore >= value,
                ComparisonOperator.LessThan => currentScore < value,
                ComparisonOperator.LessThanOrEqual => currentScore <= value,
                _ => false
            };
        }
    }

    /// <summary>
    /// フラグ条件（単純なboolフラグチェック用）
    /// </summary>
    [Serializable]
    public class FlagCondition
    {
        [Tooltip("フラグID")]
        public string flagId = string.Empty;

        [Tooltip("必要な値")]
        public bool requiredValue = true;
    }

    /// <summary>
    /// エンドステート条件
    /// </summary>
    [Serializable]
    public class EndStateCondition
    {
        [Tooltip("エンドステートの種類")]
        public EndStateType endStateType = EndStateType.Contained;

        [Tooltip("評価優先順位（低い方が先にチェック）")]
        public int priority = 100;

        [Tooltip("スコア条件（すべて満たす必要あり）")]
        public List<ScoreCondition> scoreConditions = new();

        [Tooltip("フラグ条件（すべて満たす必要あり）")]
        public List<FlagCondition> flagConditions = new();

        [Tooltip("説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;
    }

    /// <summary>
    /// エンディングマッピング
    /// エンドステート + 被害者生存 → エンディングID
    /// </summary>
    [Serializable]
    public class EndingMapping
    {
        [Tooltip("エンドステート")]
        public EndStateType endState = EndStateType.Contained;

        [Tooltip("被害者が生存している場合")]
        public string endingIdIfSurvived = string.Empty;

        [Tooltip("被害者が死亡している場合")]
        public string endingIdIfDied = string.Empty;

        [Tooltip("被害者の生存状態に関係なく適用（両方空の場合）")]
        public string endingIdRegardless = string.Empty;
    }

    /// <summary>
    /// 被害者生存条件
    /// </summary>
    [Serializable]
    public class VictimSurvivalCondition
    {
        [Tooltip("派遣が必要か")]
        public bool requiresDispatch = true;

        [Tooltip("派遣の最大許容時刻（分単位、これ以前なら生存）")]
        public int maxDispatchTimeMinutes = 169; // 02:49

        [Tooltip("派遣フラグID")]
        public string dispatchFlagId = "emergency_dispatched";
    }

}
