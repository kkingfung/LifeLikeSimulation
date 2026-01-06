#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace LifeLike.Services.AssetBundle
{
    /// <summary>
    /// AssetBundleの管理とダウンロードを行うサービス
    /// </summary>
    public class AssetBundleService : IAssetBundleService, IDisposable
    {
        private readonly Dictionary<string, UnityEngine.AssetBundle> _loadedBundles = new();
        private readonly Dictionary<string, BundleState> _bundleStates = new();
        private bool _isDisposed;

        /// <inheritdoc/>
        public string BaseUrl { get; set; } = string.Empty;

        /// <inheritdoc/>
        public event Action<DownloadProgress>? OnDownloadProgress;

        /// <inheritdoc/>
        public event Action<string>? OnDownloadComplete;

        /// <inheritdoc/>
        public event Action<string, string>? OnError;

        /// <inheritdoc/>
        public async Task<bool> DownloadBundleAsync(string bundleName, uint version = 0)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogError("[AssetBundleService] バンドル名が空です。");
                return false;
            }

            // 既にロード済みの場合
            if (_loadedBundles.ContainsKey(bundleName))
            {
                Debug.Log($"[AssetBundleService] バンドル {bundleName} は既にロード済みです。");
                return true;
            }

            _bundleStates[bundleName] = BundleState.Downloading;

            var url = GetBundleUrl(bundleName);
            Debug.Log($"[AssetBundleService] バンドルをダウンロード: {url}");

            try
            {
                using var request = version > 0
                    ? UnityWebRequestAssetBundle.GetAssetBundle(url, version, 0)
                    : UnityWebRequestAssetBundle.GetAssetBundle(url);

                var operation = request.SendWebRequest();

                // 進捗を監視
                while (!operation.isDone)
                {
                    var progress = new DownloadProgress
                    {
                        BundleName = bundleName,
                        Progress = request.downloadProgress,
                        DownloadedBytes = (long)request.downloadedBytes,
                        TotalBytes = 0, // ContentLengthが取得できない場合がある
                        State = BundleState.Downloading
                    };
                    OnDownloadProgress?.Invoke(progress);

                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    _bundleStates[bundleName] = BundleState.Error;
                    var error = $"ダウンロード失敗: {request.error}";
                    Debug.LogError($"[AssetBundleService] {error}");
                    OnError?.Invoke(bundleName, error);
                    return false;
                }

                _bundleStates[bundleName] = BundleState.Loading;

                var bundle = DownloadHandlerAssetBundle.GetContent(request);
                if (bundle == null)
                {
                    _bundleStates[bundleName] = BundleState.Error;
                    var error = "バンドルの取得に失敗しました。";
                    Debug.LogError($"[AssetBundleService] {error}");
                    OnError?.Invoke(bundleName, error);
                    return false;
                }

                _loadedBundles[bundleName] = bundle;
                _bundleStates[bundleName] = BundleState.Loaded;

                Debug.Log($"[AssetBundleService] バンドル {bundleName} をロードしました。");
                OnDownloadComplete?.Invoke(bundleName);
                return true;
            }
            catch (Exception ex)
            {
                _bundleStates[bundleName] = BundleState.Error;
                Debug.LogError($"[AssetBundleService] 例外発生: {ex.Message}");
                OnError?.Invoke(bundleName, ex.Message);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<VideoClip?> LoadVideoClipAsync(string bundleName, string assetName)
        {
            return await LoadAssetAsync<VideoClip>(bundleName, assetName);
        }

        /// <inheritdoc/>
        public async Task<T?> LoadAssetAsync<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            // バンドルがロードされていなければダウンロード
            if (!_loadedBundles.ContainsKey(bundleName))
            {
                var success = await DownloadBundleAsync(bundleName);
                if (!success)
                {
                    return null;
                }
            }

            if (!_loadedBundles.TryGetValue(bundleName, out var bundle))
            {
                Debug.LogError($"[AssetBundleService] バンドル {bundleName} が見つかりません。");
                return null;
            }

            try
            {
                var request = bundle.LoadAssetAsync<T>(assetName);

                while (!request.isDone)
                {
                    await Task.Yield();
                }

                var asset = request.asset as T;
                if (asset == null)
                {
                    Debug.LogError($"[AssetBundleService] アセット {assetName} のロードに失敗しました。");
                    return null;
                }

                Debug.Log($"[AssetBundleService] アセット {assetName} をロードしました。");
                return asset;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetBundleService] アセットロード例外: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public bool IsBundleCached(string bundleName, uint version = 0)
        {
            var url = GetBundleUrl(bundleName);
            return Caching.IsVersionCached(url, new Hash128(version, 0, 0, 0));
        }

        /// <inheritdoc/>
        public BundleState GetBundleState(string bundleName)
        {
            return _bundleStates.TryGetValue(bundleName, out var state) ? state : BundleState.NotLoaded;
        }

        /// <inheritdoc/>
        public void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            if (_loadedBundles.TryGetValue(bundleName, out var bundle))
            {
                bundle.Unload(unloadAllLoadedObjects);
                _loadedBundles.Remove(bundleName);
                _bundleStates[bundleName] = BundleState.NotLoaded;
                Debug.Log($"[AssetBundleService] バンドル {bundleName} をアンロードしました。");
            }
        }

        /// <inheritdoc/>
        public void UnloadAllBundles(bool unloadAllLoadedObjects = false)
        {
            foreach (var bundle in _loadedBundles.Values)
            {
                bundle.Unload(unloadAllLoadedObjects);
            }
            _loadedBundles.Clear();
            _bundleStates.Clear();
            Debug.Log("[AssetBundleService] すべてのバンドルをアンロードしました。");
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            Caching.ClearCache();
            Debug.Log("[AssetBundleService] キャッシュをクリアしました。");
        }

        /// <summary>
        /// バンドルのURLを生成する
        /// </summary>
        private string GetBundleUrl(string bundleName)
        {
            if (string.IsNullOrEmpty(BaseUrl))
            {
                // ローカルのStreamingAssetsを使用
                return System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles", bundleName);
            }

            return $"{BaseUrl.TrimEnd('/')}/{bundleName}";
        }

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            UnloadAllBundles(true);
            OnDownloadProgress = null;
            OnDownloadComplete = null;
            OnError = null;

            _isDisposed = true;
        }
    }
}
