#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.EndState;
using UnityEngine;

namespace LifeLike.Data.Flag
{
    /// <summary>
    /// 夜ごとの結果サマリー
    /// </summary>
    [Serializable]
    public class NightResultSummary
    {
        [Tooltip("夜のID")]
        public string nightId = string.Empty;

        [Tooltip("エンドステート")]
        public EndStateType endState = EndStateType.Contained;

        [Tooltip("エンディングID")]
        public string endingId = string.Empty;

        [Tooltip("被害者が生存したか")]
        public bool victimSurvived = false;

        [Tooltip("派遣タイミング（分、派遣しなかった場合は-1）")]
        public int dispatchTimeMinutes = -1;

        [Tooltip("永続フラグのスナップショット")]
        public List<FlagState> persistentFlags = new();

        [Tooltip("重要な選択の記録")]
        public List<KeyDecision> keyDecisions = new();

        [Tooltip("取得した証拠ID")]
        public List<string> collectedEvidenceIds = new();

        [Tooltip("プレイ終了時刻")]
        public DateTime completedAt = DateTime.MinValue;
    }

    /// <summary>
    /// 重要な選択の記録
    /// </summary>
    [Serializable]
    public class KeyDecision
    {
        [Tooltip("決定のID")]
        public string decisionId = string.Empty;

        [Tooltip("選択したオプション")]
        public string chosenOption = string.Empty;

        [Tooltip("関連する通話ID")]
        public string relatedCallId = string.Empty;

        [Tooltip("決定時のゲーム内時刻")]
        public int timeMinutes = 0;

        [Tooltip("説明")]
        public string description = string.Empty;
    }

    /// <summary>
    /// ゲーム全体を通じた永続的なフラグ
    /// 夜をまたいで保持される情報
    /// </summary>
    [Serializable]
    public class PersistentFlag
    {
        [Tooltip("フラグID")]
        public string flagId = string.Empty;

        [Tooltip("元の夜ID")]
        public string originNightId = string.Empty;

        [Tooltip("設定された時刻")]
        public int setTimeMinutes = 0;

        [Tooltip("設定されているか")]
        public bool isSet = false;

        [Tooltip("追加データ（JSON形式）")]
        public string additionalData = string.Empty;
    }

    /// <summary>
    /// 複数の夜をまたぐ状態管理データ
    /// </summary>
    [Serializable]
    public class CrossNightState
    {
        [Header("基本情報")]
        [Tooltip("セーブスロットID")]
        public string saveSlotId = string.Empty;

        [Tooltip("現在の夜")]
        public string currentNightId = "night_01";

        [Tooltip("作成日時")]
        public DateTime createdAt = DateTime.Now;

        [Tooltip("最終更新日時")]
        public DateTime lastUpdatedAt = DateTime.Now;

        [Header("夜ごとの結果")]
        [Tooltip("完了した夜の結果リスト")]
        public List<NightResultSummary> nightResults = new();

        [Header("永続フラグ")]
        [Tooltip("夜をまたいで保持されるフラグ")]
        public List<PersistentFlag> persistentFlags = new();

        [Header("証拠")]
        [Tooltip("収集した全証拠ID")]
        public List<string> allCollectedEvidence = new();

        [Header("統計")]
        [Tooltip("総プレイ時間（秒）")]
        public float totalPlayTimeSeconds = 0f;

        [Tooltip("救った人数")]
        public int peopleSaved = 0;

        [Tooltip("見捨てた人数")]
        public int peopleAbandoned = 0;

        /// <summary>
        /// 指定した夜の結果を取得
        /// </summary>
        public NightResultSummary? GetNightResult(string nightId)
        {
            return nightResults.Find(r => r.nightId == nightId);
        }

        /// <summary>
        /// 夜の結果を追加または更新
        /// </summary>
        public void SetNightResult(NightResultSummary result)
        {
            var existing = nightResults.FindIndex(r => r.nightId == result.nightId);
            if (existing >= 0)
            {
                nightResults[existing] = result;
            }
            else
            {
                nightResults.Add(result);
            }
            lastUpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 永続フラグを設定
        /// </summary>
        public void SetPersistentFlag(string flagId, string originNightId, int timeMinutes)
        {
            var existing = persistentFlags.Find(f => f.flagId == flagId);
            if (existing != null)
            {
                existing.isSet = true;
                existing.setTimeMinutes = timeMinutes;
            }
            else
            {
                persistentFlags.Add(new PersistentFlag
                {
                    flagId = flagId,
                    originNightId = originNightId,
                    setTimeMinutes = timeMinutes,
                    isSet = true
                });
            }
            lastUpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 永続フラグをクリア
        /// </summary>
        public void ClearPersistentFlag(string flagId)
        {
            var existing = persistentFlags.Find(f => f.flagId == flagId);
            if (existing != null)
            {
                existing.isSet = false;
            }
            lastUpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 永続フラグが設定されているか確認
        /// </summary>
        public bool IsPersistentFlagSet(string flagId)
        {
            var flag = persistentFlags.Find(f => f.flagId == flagId);
            return flag?.isSet ?? false;
        }

        /// <summary>
        /// 指定した夜のエンドステートを取得
        /// </summary>
        public EndStateType? GetNightEndState(string nightId)
        {
            var result = GetNightResult(nightId);
            return result?.endState;
        }

        /// <summary>
        /// 証拠を追加
        /// </summary>
        public void AddEvidence(string evidenceId)
        {
            if (!allCollectedEvidence.Contains(evidenceId))
            {
                allCollectedEvidence.Add(evidenceId);
                lastUpdatedAt = DateTime.Now;
            }
        }

        /// <summary>
        /// 証拠を持っているか確認
        /// </summary>
        public bool HasEvidence(string evidenceId)
        {
            return allCollectedEvidence.Contains(evidenceId);
        }

        /// <summary>
        /// 前の夜が完了しているか確認
        /// </summary>
        public bool IsNightCompleted(string nightId)
        {
            return nightResults.Exists(r => r.nightId == nightId);
        }

        /// <summary>
        /// 次の夜に進む
        /// </summary>
        public void AdvanceToNight(string nextNightId)
        {
            currentNightId = nextNightId;
            lastUpdatedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// 夜間のエフェクト（前の夜の結果が次の夜に与える影響）
    /// </summary>
    [Serializable]
    public class NightEffect
    {
        [Tooltip("エフェクトID")]
        public string effectId = string.Empty;

        [Tooltip("元の夜ID")]
        public string sourceNightId = string.Empty;

        [Tooltip("影響を受ける夜ID")]
        public string targetNightId = string.Empty;

        [Tooltip("必要なエンドステート")]
        public EndStateType requiredEndState = EndStateType.Contained;

        [Tooltip("追加の必要フラグ")]
        public List<string> requiredFlags = new();

        [Tooltip("効果の説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;

        [Tooltip("設定するフラグ")]
        public List<string> setFlags = new();

        [Tooltip("利用可能にする通話ID")]
        public List<string> enableCallIds = new();

        [Tooltip("無効にする通話ID")]
        public List<string> disableCallIds = new();
    }

    /// <summary>
    /// 夜間エフェクト定義
    /// </summary>
    [CreateAssetMenu(fileName = "NightEffects", menuName = "LifeLike/Operator/Night Effects")]
    public class NightEffectsDefinition : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("対象の夜ID")]
        public string targetNightId = string.Empty;

        [Tooltip("説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;

        [Header("エフェクトリスト")]
        [Tooltip("この夜に適用されるエフェクト")]
        public List<NightEffect> effects = new();

        /// <summary>
        /// 指定した夜の結果に基づいて適用されるエフェクトを取得
        /// </summary>
        public List<NightEffect> GetApplicableEffects(CrossNightState state)
        {
            var applicable = new List<NightEffect>();

            foreach (var effect in effects)
            {
                var nightResult = state.GetNightResult(effect.sourceNightId);
                if (nightResult == null) continue;

                // エンドステートが一致するか確認
                if (nightResult.endState != effect.requiredEndState) continue;

                // 必要フラグがすべて設定されているか確認
                bool allFlagsSet = true;
                foreach (var flagId in effect.requiredFlags)
                {
                    if (!state.IsPersistentFlagSet(flagId))
                    {
                        allFlagsSet = false;
                        break;
                    }
                }

                if (allFlagsSet)
                {
                    applicable.Add(effect);
                }
            }

            return applicable;
        }
    }
}
