#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;
using UnityEngine;

namespace LifeLike.Services.Core.Save
{
    /// <summary>
    /// オペレーターモード用セーブデータ構造
    /// </summary>
    [Serializable]
    public class OperatorSaveData
    {
        public int currentNightIndex = 0;
        public string saveTime = string.Empty;
        public List<NightResultData> nightResults = new();
        public List<FlagStateData> persistentFlags = new();
        public MidNightSaveData? midNightSave;
    }

    /// <summary>
    /// 夜の結果データ
    /// </summary>
    [Serializable]
    public class NightResultData
    {
        public string nightId = string.Empty;
        public string endStateType = string.Empty;
        public string completedTime = string.Empty;
    }

    /// <summary>
    /// フラグ状態データ（シリアライズ用）
    /// </summary>
    [Serializable]
    public class FlagStateData
    {
        public string flagId = string.Empty;
        public bool isSet = false;
        public int setTime = 0;
    }

    /// <summary>
    /// 中断セーブデータ
    /// </summary>
    [Serializable]
    public class MidNightSaveData
    {
        public string nightId = string.Empty;
        public int currentTimeMinutes = 0;
        public List<FlagStateData> currentFlags = new();
    }

    /// <summary>
    /// オペレーターモード用セーブサービス
    /// </summary>
    public class OperatorSaveService : IOperatorSaveService, IDisposable
    {
        private const string SAVE_KEY = "LifeLike_OperatorSave";

        private OperatorSaveData? _cachedSaveData;
        private bool _isDisposed;

        /// <inheritdoc/>
        public bool HasSaveData
        {
            get
            {
                if (_cachedSaveData != null)
                {
                    return true;
                }
                return PlayerPrefs.HasKey(SAVE_KEY);
            }
        }

        /// <inheritdoc/>
        public DateTime? LastSaveTime
        {
            get
            {
                var data = GetSaveData();
                if (data == null || string.IsNullOrEmpty(data.saveTime))
                {
                    return null;
                }
                if (DateTime.TryParse(data.saveTime, out var time))
                {
                    return time;
                }
                return null;
            }
        }

        /// <inheritdoc/>
        public bool HasMidNightSave
        {
            get
            {
                var data = GetSaveData();
                return data?.midNightSave != null;
            }
        }

        /// <inheritdoc/>
        public event Action? OnAutoSaved;

        /// <inheritdoc/>
        public event Action? OnLoaded;

        /// <inheritdoc/>
        public int GetCurrentNightIndex()
        {
            var data = GetSaveData();
            return data?.currentNightIndex ?? 0;
        }

        /// <inheritdoc/>
        public List<string> GetCompletedNights()
        {
            var data = GetSaveData();
            if (data == null) return new List<string>();

            return data.nightResults.Select(r => r.nightId).ToList();
        }

        /// <inheritdoc/>
        public EndStateType? GetNightEndState(string nightId)
        {
            var data = GetSaveData();
            if (data == null) return null;

            var result = data.nightResults.FirstOrDefault(r => r.nightId == nightId);
            if (result == null) return null;

            if (Enum.TryParse<EndStateType>(result.endStateType, out var endState))
            {
                return endState;
            }
            return null;
        }

        /// <inheritdoc/>
        public List<FlagState> GetPersistentFlags()
        {
            var data = GetSaveData();
            if (data == null) return new List<FlagState>();

            return data.persistentFlags.Select(f => new FlagState
            {
                flagId = f.flagId,
                isSet = f.isSet,
                setTime = f.setTime
            }).ToList();
        }

        /// <inheritdoc/>
        public void SaveNightResult(string nightId, EndStateType endState, List<FlagState> persistentFlags)
        {
            var data = GetSaveData() ?? new OperatorSaveData();

            // 夜の結果を追加または更新
            var existingResult = data.nightResults.FirstOrDefault(r => r.nightId == nightId);
            if (existingResult != null)
            {
                existingResult.endStateType = endState.ToString();
                existingResult.completedTime = DateTime.Now.ToString("o");
            }
            else
            {
                data.nightResults.Add(new NightResultData
                {
                    nightId = nightId,
                    endStateType = endState.ToString(),
                    completedTime = DateTime.Now.ToString("o")
                });
            }

            // 次の夜のインデックスを設定
            data.currentNightIndex = data.nightResults.Count;

            // 永続フラグを更新
            data.persistentFlags = persistentFlags.Select(f => new FlagStateData
            {
                flagId = f.flagId,
                isSet = f.isSet,
                setTime = f.setTime
            }).ToList();

            // 中断セーブをクリア
            data.midNightSave = null;

            // 保存
            SaveData(data);

            Debug.Log($"[OperatorSaveService] 夜の結果を保存: {nightId} -> {endState}");
            OnAutoSaved?.Invoke();
        }

        /// <inheritdoc/>
        public void SaveMidNight(string nightId, int currentTimeMinutes, List<FlagState> currentFlags)
        {
            var data = GetSaveData() ?? new OperatorSaveData();

            data.midNightSave = new MidNightSaveData
            {
                nightId = nightId,
                currentTimeMinutes = currentTimeMinutes,
                currentFlags = currentFlags.Select(f => new FlagStateData
                {
                    flagId = f.flagId,
                    isSet = f.isSet,
                    setTime = f.setTime
                }).ToList()
            };

            SaveData(data);

            Debug.Log($"[OperatorSaveService] 中断セーブ: {nightId} at {currentTimeMinutes}分");
            OnAutoSaved?.Invoke();
        }

        /// <inheritdoc/>
        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            _cachedSaveData = null;
            Debug.Log("[OperatorSaveService] セーブデータを削除しました。");
        }

        /// <summary>
        /// セーブデータを取得
        /// </summary>
        private OperatorSaveData? GetSaveData()
        {
            if (_cachedSaveData != null)
            {
                return _cachedSaveData;
            }

            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                return null;
            }

            var json = PlayerPrefs.GetString(SAVE_KEY);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                _cachedSaveData = JsonUtility.FromJson<OperatorSaveData>(json);
                OnLoaded?.Invoke();
                return _cachedSaveData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OperatorSaveService] セーブデータのパースに失敗: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// セーブデータを保存
        /// </summary>
        private void SaveData(OperatorSaveData data)
        {
            data.saveTime = DateTime.Now.ToString("o");
            var json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            _cachedSaveData = data;
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _cachedSaveData = null;
            OnAutoSaved = null;
            OnLoaded = null;
            _isDisposed = true;
        }
    }
}
