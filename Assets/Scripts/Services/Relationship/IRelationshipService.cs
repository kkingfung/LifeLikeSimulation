#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data;

namespace LifeLike.Services.Relationship
{
    /// <summary>
    /// キャラクターとの関係性を管理するサービスのインターフェース
    /// </summary>
    public interface IRelationshipService
    {
        /// <summary>
        /// 関係性変更時のイベント（キャラクターID, 軸ID, 新しい値）
        /// </summary>
        event Action<string, string, int>? OnRelationshipChanged;

        /// <summary>
        /// 関係性の閾値到達時のイベント（キャラクターID, 軸ID, 閾値名）
        /// </summary>
        event Action<string, string, string>? OnThresholdReached;

        /// <summary>
        /// キャラクターデータを登録する
        /// </summary>
        /// <param name="character">登録するキャラクター</param>
        void RegisterCharacter(CharacterData character);

        /// <summary>
        /// 複数のキャラクターデータを登録する
        /// </summary>
        /// <param name="characters">登録するキャラクターのリスト</param>
        void RegisterCharacters(IEnumerable<CharacterData> characters);

        /// <summary>
        /// 特定の関係性値を取得する
        /// </summary>
        /// <param name="characterId">キャラクターID</param>
        /// <param name="axisId">軸ID</param>
        /// <returns>関係性の値</returns>
        int GetRelationship(string characterId, string axisId);

        /// <summary>
        /// 特定の関係性値を設定する
        /// </summary>
        /// <param name="characterId">キャラクターID</param>
        /// <param name="axisId">軸ID</param>
        /// <param name="value">設定する値</param>
        void SetRelationship(string characterId, string axisId, int value);

        /// <summary>
        /// 関係性値を増減する
        /// </summary>
        /// <param name="characterId">キャラクターID</param>
        /// <param name="axisId">軸ID</param>
        /// <param name="delta">変化量（負の値で減少）</param>
        void ModifyRelationship(string characterId, string axisId, int delta);

        /// <summary>
        /// キャラクターのすべての関係性を取得する
        /// </summary>
        /// <param name="characterId">キャラクターID</param>
        /// <returns>軸IDと値のディクショナリ</returns>
        IReadOnlyDictionary<string, int> GetAllRelationships(string characterId);

        /// <summary>
        /// 特定の関係性軸の設定を取得する
        /// </summary>
        /// <param name="characterId">キャラクターID</param>
        /// <param name="axisId">軸ID</param>
        /// <returns>軸の設定（見つからない場合はnull）</returns>
        RelationshipAxis? GetRelationshipAxis(string characterId, string axisId);

        /// <summary>
        /// 関係性値を正規化した値（0.0〜1.0）で取得する
        /// </summary>
        /// <param name="characterId">キャラクターID</param>
        /// <param name="axisId">軸ID</param>
        /// <returns>正規化された値</returns>
        float GetNormalizedRelationship(string characterId, string axisId);

        /// <summary>
        /// すべての関係性をリセットする
        /// </summary>
        void ResetAllRelationships();
    }
}
