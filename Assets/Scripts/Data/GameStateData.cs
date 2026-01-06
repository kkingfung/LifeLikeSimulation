#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Conditions;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// ゲーム変数の定義
    /// </summary>
    [Serializable]
    public class VariableDefinition
    {
        [Tooltip("変数名")]
        public string variableName = string.Empty;

        [Tooltip("変数の型")]
        public VariableType variableType = VariableType.Integer;

        [Tooltip("初期値（整数）")]
        public int initialIntValue = 0;

        [Tooltip("初期値（真偽値）")]
        public bool initialBoolValue = false;

        [Tooltip("初期値（文字列）")]
        public string initialStringValue = string.Empty;

        /// <summary>
        /// 初期値を取得
        /// </summary>
        public object GetInitialValue()
        {
            return variableType switch
            {
                VariableType.Integer => initialIntValue,
                VariableType.Boolean => initialBoolValue,
                VariableType.String => initialStringValue,
                _ => initialIntValue
            };
        }
    }

    /// <summary>
    /// ゲームの初期状態を定義するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewGameState", menuName = "LifeLike/Game State Data")]
    public class GameStateData : ScriptableObject
    {
        [Header("初期変数")]
        [Tooltip("ゲーム開始時の変数定義")]
        public List<VariableDefinition> initialVariables = new();

        [Header("キャラクター")]
        [Tooltip("登場キャラクター一覧")]
        public List<CharacterData> characters = new();

        [Header("ストーリー")]
        [Tooltip("ゲーム開始時の最初のシーンID")]
        public string startSceneId = string.Empty;

        [Tooltip("すべてのストーリーシーン")]
        public List<StorySceneData> allScenes = new();

        /// <summary>
        /// 変数辞書を初期化して返す
        /// </summary>
        public Dictionary<string, object> CreateInitialVariables()
        {
            var variables = new Dictionary<string, object>();

            // 定義された変数を追加
            foreach (var def in initialVariables)
            {
                if (!string.IsNullOrEmpty(def.variableName))
                {
                    variables[def.variableName] = def.GetInitialValue();
                }
            }

            // キャラクターの関係性変数を追加
            foreach (var character in characters)
            {
                foreach (var axis in character.relationshipAxes)
                {
                    var variableName = character.GetRelationshipVariableName(axis.axisId);
                    variables[variableName] = axis.initialValue;
                }

                // キャラクターフラグも追加
                foreach (var flag in character.characterFlags)
                {
                    var variableName = $"{character.GetVariablePrefix()}{flag}";
                    if (!variables.ContainsKey(variableName))
                    {
                        variables[variableName] = false;
                    }
                }
            }

            return variables;
        }

        /// <summary>
        /// シーンIDからシーンデータを取得
        /// </summary>
        public StorySceneData? GetScene(string sceneId)
        {
            return allScenes.Find(s => s.sceneId == sceneId);
        }

        /// <summary>
        /// キャラクターIDからキャラクターデータを取得
        /// </summary>
        public CharacterData? GetCharacter(string characterId)
        {
            return characters.Find(c => c.characterId == characterId);
        }
    }
}
