#nullable enable
using System;
using System.Threading.Tasks;
using LifeLike.Core.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LifeLike.Services.Core.Subscene
{
    /// <summary>
    /// サブシーン（オーバーレイシーン）管理サービスの実装
    /// </summary>
    public class SubsceneService : ISubsceneService
    {
        private string? _currentSubsceneName;
        private bool _isLoading;

        /// <inheritdoc/>
        public bool IsSubsceneOpen => _currentSubsceneName != null;

        /// <inheritdoc/>
        public string? CurrentSubsceneName => _currentSubsceneName;

        /// <inheritdoc/>
        public event Action<string>? OnSubsceneOpened;

        /// <inheritdoc/>
        public event Action<string>? OnSubsceneClosed;

        /// <inheritdoc/>
        public async Task<bool> OpenSubsceneAsync(SceneReference sceneReference)
        {
            if (sceneReference == null || !sceneReference.IsValid)
            {
                Debug.LogError("[SubsceneService] シーン参照が無効です。");
                return false;
            }

            string sceneName = sceneReference.SceneName;

            if (_isLoading)
            {
                Debug.LogWarning("[SubsceneService] 既にシーンをロード中です。");
                return false;
            }

            if (IsSubsceneOpen)
            {
                Debug.LogWarning($"[SubsceneService] 既にサブシーンが開いています: {_currentSubsceneName}");
                return false;
            }

            _isLoading = true;

            try
            {
                // 加算モードでシーンをロード
                var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                if (asyncOperation == null)
                {
                    Debug.LogError($"[SubsceneService] シーンのロードに失敗: {sceneName}");
                    _isLoading = false;
                    return false;
                }

                // ロード完了を待機
                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                _currentSubsceneName = sceneName;
                _isLoading = false;

                Debug.Log($"[SubsceneService] サブシーンを開きました: {sceneName}");
                OnSubsceneOpened?.Invoke(sceneName);

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SubsceneService] サブシーンのロード中にエラー: {ex.Message}");
                _isLoading = false;
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CloseSubsceneAsync()
        {
            if (!IsSubsceneOpen || _currentSubsceneName == null)
            {
                Debug.LogWarning("[SubsceneService] 閉じるサブシーンがありません。");
                return false;
            }

            if (_isLoading)
            {
                Debug.LogWarning("[SubsceneService] シーンのロード/アンロード中です。");
                return false;
            }

            _isLoading = true;
            var sceneName = _currentSubsceneName;

            try
            {
                // シーンをアンロード
                var asyncOperation = SceneManager.UnloadSceneAsync(sceneName);
                if (asyncOperation == null)
                {
                    Debug.LogError($"[SubsceneService] シーンのアンロードに失敗: {sceneName}");
                    // アンロード失敗でも状態をリセット（既にアンロード済みの可能性）
                    _currentSubsceneName = null;
                    _isLoading = false;
                    return false;
                }

                // アンロード完了を待機
                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                _currentSubsceneName = null;
                _isLoading = false;

                Debug.Log($"[SubsceneService] サブシーンを閉じました: {sceneName}");
                OnSubsceneClosed?.Invoke(sceneName);

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SubsceneService] サブシーンのアンロード中にエラー: {ex.Message}");
                // 例外発生時も状態をリセット
                _currentSubsceneName = null;
                _isLoading = false;
                return false;
            }
        }

        /// <inheritdoc/>
        public bool IsOpenedAsSubscene(string sceneName)
        {
            return _currentSubsceneName == sceneName;
        }
    }
}
