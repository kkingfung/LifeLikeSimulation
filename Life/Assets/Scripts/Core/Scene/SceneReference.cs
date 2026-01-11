#nullable enable
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LifeLike.Core.Scene
{
    /// <summary>
    /// シーン参照を安全に保持するクラス
    /// エディタではSceneAssetをドラッグ＆ドロップで設定でき、
    /// ランタイムではシーン名を使用してロードする
    /// </summary>
    [Serializable]
    public class SceneReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        /// <summary>
        /// エディタ専用: SceneAssetの参照
        /// カスタムPropertyDrawerで使用
        /// </summary>
        [SerializeField]
        private UnityEditor.SceneAsset? _sceneAsset;
#endif

        /// <summary>
        /// シリアライズされるシーン名（ランタイムで使用）
        /// </summary>
        [SerializeField]
        private string _sceneName = string.Empty;

        /// <summary>
        /// シーン名を取得
        /// </summary>
        public string SceneName => _sceneName;

        /// <summary>
        /// シーン参照が有効かどうか
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(_sceneName);

        /// <summary>
        /// 暗黙的にstringに変換
        /// </summary>
        public static implicit operator string(SceneReference sceneReference)
        {
            return sceneReference._sceneName;
        }

        /// <summary>
        /// シーンをロード（シングルモード）
        /// </summary>
        public void LoadScene()
        {
            if (!IsValid)
            {
                Debug.LogError("[SceneReference] シーン名が設定されていません。");
                return;
            }
            SceneManager.LoadScene(_sceneName);
        }

        /// <summary>
        /// シーンを非同期ロード
        /// </summary>
        /// <param name="loadMode">ロードモード</param>
        /// <returns>AsyncOperation</returns>
        public AsyncOperation? LoadSceneAsync(LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if (!IsValid)
            {
                Debug.LogError("[SceneReference] シーン名が設定されていません。");
                return null;
            }
            return SceneManager.LoadSceneAsync(_sceneName, loadMode);
        }

        /// <summary>
        /// シーンを非同期アンロード
        /// </summary>
        /// <returns>AsyncOperation</returns>
        public AsyncOperation? UnloadSceneAsync()
        {
            if (!IsValid)
            {
                Debug.LogError("[SceneReference] シーン名が設定されていません。");
                return null;
            }
            return SceneManager.UnloadSceneAsync(_sceneName);
        }

        /// <summary>
        /// シリアライズ前のコールバック
        /// エディタでSceneAssetからシーン名を抽出
        /// </summary>
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (_sceneAsset != null)
            {
                _sceneName = _sceneAsset.name;
            }
            else
            {
                _sceneName = string.Empty;
            }
#endif
        }

        /// <summary>
        /// デシリアライズ後のコールバック
        /// </summary>
        public void OnAfterDeserialize()
        {
            // 特に処理なし
        }

        public override string ToString()
        {
            return _sceneName;
        }
    }
}
