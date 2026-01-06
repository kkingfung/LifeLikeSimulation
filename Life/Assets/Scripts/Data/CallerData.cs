#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// 発信者の性格特性
    /// </summary>
    [Serializable]
    public class CallerPersonality
    {
        [Tooltip("正直さ（0-100）- 低いと嘘をつきやすい")]
        [Range(0, 100)]
        public int honesty = 50;

        [Tooltip("感情的安定性（0-100）- 低いとパニックしやすい")]
        [Range(0, 100)]
        public int stability = 50;

        [Tooltip("協力性（0-100）- 低いと情報を隠しやすい")]
        [Range(0, 100)]
        public int cooperation = 50;

        [Tooltip("攻撃性（0-100）- 高いと敵対的になりやすい")]
        [Range(0, 100)]
        public int aggression = 20;
    }

    /// <summary>
    /// 発信者（Caller）のデータ
    /// 緊急通報システムに電話をかけてくる人物の情報
    /// </summary>
    [CreateAssetMenu(fileName = "NewCaller", menuName = "LifeLike/Operator/Caller Data")]
    public class CallerData : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("発信者の一意なID")]
        public string callerId = string.Empty;

        [Tooltip("表示名（最初は「不明」の場合も）")]
        public string displayName = "不明";

        [Tooltip("本名（ゲーム内で判明する可能性）")]
        public string realName = string.Empty;

        [Tooltip("発信者の説明（内部用）")]
        [TextArea(2, 4)]
        public string description = string.Empty;

        [Header("ビジュアル")]
        [Tooltip("シルエット画像（フェーズ1用）")]
        public Sprite? silhouetteImage;

        [Tooltip("顔写真（判明後に表示）")]
        public Sprite? revealedImage;

        [Tooltip("シルエットの色（感情や状況で変化可能）")]
        public Color silhouetteColor = Color.white;

        [Header("音声")]
        [Tooltip("声の特徴（テキスト表示用）")]
        public string voiceDescription = string.Empty;

        [Tooltip("音声ピッチ（1.0が標準）")]
        [Range(0.5f, 2.0f)]
        public float voicePitch = 1.0f;

        [Header("性格")]
        [Tooltip("発信者の性格特性")]
        public CallerPersonality personality = new();

        [Header("関係性")]
        [Tooltip("他の発信者との関係（CallerID → 関係タイプ）")]
        public List<CallerRelation> relations = new();

        [Header("秘密")]
        [Tooltip("この発信者が隠している情報")]
        [TextArea(2, 4)]
        public string hiddenInfo = string.Empty;

        [Tooltip("この発信者の本当の目的")]
        [TextArea(2, 4)]
        public string trueMotivation = string.Empty;

        /// <summary>
        /// 特定の発信者との関係を取得
        /// </summary>
        public CallerRelation? GetRelationWith(string otherCallerId)
        {
            return relations.Find(r => r.targetCallerId == otherCallerId);
        }
    }

    /// <summary>
    /// 発信者間の関係
    /// </summary>
    [Serializable]
    public class CallerRelation
    {
        [Tooltip("対象の発信者ID")]
        public string targetCallerId = string.Empty;

        [Tooltip("関係タイプ")]
        public RelationType relationType = RelationType.Stranger;

        [Tooltip("関係の詳細説明")]
        public string description = string.Empty;

        [Tooltip("この関係を知っているか（プレイヤーが発見する必要があるか）")]
        public bool isKnown = false;
    }

    /// <summary>
    /// 関係タイプ
    /// </summary>
    public enum RelationType
    {
        Stranger,       // 他人
        Acquaintance,   // 知人
        Friend,         // 友人
        Family,         // 家族
        Colleague,      // 同僚
        Lover,          // 恋人
        ExLover,        // 元恋人
        Rival,          // ライバル
        Enemy,          // 敵
        Accomplice,     // 共犯者
        Victim,         // 被害者
        Suspect         // 容疑者
    }
}
