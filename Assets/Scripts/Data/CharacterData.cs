#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// キャラクターの関係性軸
    /// 好感度、信頼度など複数の軸で関係性を追跡できる
    /// </summary>
    [Serializable]
    public class RelationshipAxis
    {
        [Tooltip("軸の名前（例: Love, Trust, Friendship）")]
        public string axisName = string.Empty;

        [Tooltip("軸のID（内部で使用）")]
        public string axisId = string.Empty;

        [Tooltip("初期値")]
        public int initialValue = 0;

        [Tooltip("最小値")]
        public int minValue = 0;

        [Tooltip("最大値")]
        public int maxValue = 100;

        [Tooltip("この軸のアイコン（UI表示用）")]
        public Sprite? icon;
    }

    /// <summary>
    /// キャラクターデータ
    /// ストーリーに登場するキャラクターの情報を定義
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "LifeLike/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("キャラクターの一意なID")]
        public string characterId = string.Empty;

        [Tooltip("キャラクター名")]
        public string characterName = string.Empty;

        [Tooltip("キャラクターの説明")]
        [TextArea(3, 5)]
        public string description = string.Empty;

        [Header("ビジュアル")]
        [Tooltip("キャラクターのポートレート画像")]
        public Sprite? portrait;

        [Tooltip("キャラクターのサムネイル画像")]
        public Sprite? thumbnail;

        [Header("関係性")]
        [Tooltip("このキャラクターとの関係性軸")]
        public List<RelationshipAxis> relationshipAxes = new();

        [Header("追加データ")]
        [Tooltip("このキャラクター固有のフラグ変数名（自動生成される）")]
        public List<string> characterFlags = new();

        /// <summary>
        /// 関係性変数のプレフィックスを取得
        /// 例: "char_sakura_love" のような形式で変数名を生成
        /// </summary>
        public string GetVariablePrefix()
        {
            return $"char_{characterId}_";
        }

        /// <summary>
        /// 特定の軸の変数名を取得
        /// </summary>
        /// <param name="axisId">軸のID</param>
        /// <returns>変数名</returns>
        public string GetRelationshipVariableName(string axisId)
        {
            return $"{GetVariablePrefix()}{axisId}";
        }
    }
}
