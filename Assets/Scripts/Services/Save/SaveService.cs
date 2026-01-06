#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Services.Story;
using UnityEngine;

namespace LifeLike.Services.Save
{
    /// <summary>
    /// セーブデータの構造
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public string currentSceneId = string.Empty;
        public string saveTime = string.Empty;
        public List<SaveVariable> variables = new();
    }

    /// <summary>
    /// 保存する変数のデータ
    /// </summary>
    [Serializable]
    public class SaveVariable
    {
        public string name = string.Empty;
        public string type = string.Empty;
        public string value = string.Empty;
    }

    /// <summary>
    /// セーブデータを管理するサービス
    /// </summary>
    public class SaveService : ISaveService, IDisposable
    {
        private const string SAVE_KEY = "LifeLike_SaveData";

        private readonly IStoryService _storyService;
        private SaveData? _cachedSaveData;
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
        public event Action? OnAutoSaved;

        /// <inheritdoc/>
        public event Action? OnLoaded;

        /// <inheritdoc/>
        public event Action<string>? OnError;

        /// <summary>
        /// SaveServiceを初期化する
        /// </summary>
        /// <param name="storyService">ストーリーサービス</param>
        public SaveService(IStoryService storyService)
        {
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
        }

        /// <inheritdoc/>
        public void AutoSave()
        {
            try
            {
                var currentScene = _storyService.CurrentScene;
                if (currentScene == null)
                {
                    Debug.LogWarning("[SaveService] 保存するシーンがありません。");
                    return;
                }

                var saveData = new SaveData
                {
                    currentSceneId = currentScene.sceneId,
                    saveTime = DateTime.Now.ToString("o")
                };

                // 変数を保存
                var variables = _storyService.GetAllVariables();
                foreach (var kvp in variables)
                {
                    var saveVar = new SaveVariable
                    {
                        name = kvp.Key,
                        type = kvp.Value.GetType().Name,
                        value = kvp.Value.ToString() ?? string.Empty
                    };
                    saveData.variables.Add(saveVar);
                }

                // JSONにシリアライズ
                var json = JsonUtility.ToJson(saveData, true);
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();

                _cachedSaveData = saveData;

                Debug.Log($"[SaveService] オートセーブ完了: {currentScene.sceneId}");
                OnAutoSaved?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] セーブエラー: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }

        /// <inheritdoc/>
        public bool Load()
        {
            try
            {
                var saveData = GetSaveData();
                if (saveData == null)
                {
                    Debug.LogWarning("[SaveService] セーブデータがありません。");
                    return false;
                }

                // 変数を復元
                foreach (var saveVar in saveData.variables)
                {
                    var value = ConvertValue(saveVar.type, saveVar.value);
                    if (value != null)
                    {
                        _storyService.SetVariable(saveVar.name, value);
                    }
                }

                Debug.Log($"[SaveService] ロード完了: {saveData.currentSceneId}");
                OnLoaded?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] ロードエラー: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <inheritdoc/>
        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            _cachedSaveData = null;
            Debug.Log("[SaveService] セーブデータを削除しました。");
        }

        /// <inheritdoc/>
        public string? GetSavedSceneId()
        {
            var saveData = GetSaveData();
            return saveData?.currentSceneId;
        }

        /// <summary>
        /// セーブデータを取得する
        /// </summary>
        private SaveData? GetSaveData()
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
                _cachedSaveData = JsonUtility.FromJson<SaveData>(json);
                return _cachedSaveData;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 文字列から適切な型に変換する
        /// </summary>
        private object? ConvertValue(string typeName, string value)
        {
            return typeName switch
            {
                "Int32" or "int" => int.TryParse(value, out var i) ? i : 0,
                "Boolean" or "bool" => bool.TryParse(value, out var b) && b,
                "String" or "string" => value,
                "Single" or "float" => float.TryParse(value, out var f) ? f : 0f,
                "Double" or "double" => double.TryParse(value, out var d) ? d : 0.0,
                _ => value
            };
        }

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _cachedSaveData = null;
            OnAutoSaved = null;
            OnLoaded = null;
            OnError = null;

            _isDisposed = true;
        }
    }
}
