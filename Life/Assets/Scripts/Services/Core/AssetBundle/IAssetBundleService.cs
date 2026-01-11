#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace LifeLike.Services.Core.AssetBundle
{
    /// <summary>
    /// アセットのロード元
    /// </summary>
    public enum AssetSource
    {
        Local,          // ローカルファイル（Resources または直接参照）
        AssetBundle,    // AssetBundleからダウンロード
        StreamingAssets // StreamingAssetsフォルダ
    }

    /// <summary>
    /// アセットバンドルの状態
    /// </summary>
    public enum BundleState
    {
        NotLoaded,
        Downloading,
        Downloaded,
        Loading,
        Loaded,
        Error
    }

    /// <summary>
    /// ダウンロード進捗情報
    /// </summary>
    public class DownloadProgress
    {
        public string BundleName { get; set; } = string.Empty;
        public float Progress { get; set; }
        public long DownloadedBytes { get; set; }
        public long TotalBytes { get; set; }
        public BundleState State { get; set; }
    }

    /// <summary>
    /// AssetBundleの管理とダウンロードを行うサービスのインターフェース
    /// </summary>
    public interface IAssetBundleService
    {
        /// <summary>
        /// AssetBundleのベースURL
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// ダウンロード進捗イベント
        /// </summary>
        event Action<DownloadProgress>? OnDownloadProgress;

        /// <summary>
        /// ダウンロード完了イベント
        /// </summary>
        event Action<string>? OnDownloadComplete;

        /// <summary>
        /// エラー発生イベント
        /// </summary>
        event Action<string, string>? OnError;

        /// <summary>
        /// AssetBundleをダウンロードする
        /// </summary>
        /// <param name="bundleName">バンドル名</param>
        /// <param name="version">バージョン（キャッシュ用）</param>
        /// <returns>成功した場合はtrue</returns>
        Task<bool> DownloadBundleAsync(string bundleName, uint version = 0);

        /// <summary>
        /// AssetBundleから動画クリップをロードする
        /// </summary>
        /// <param name="bundleName">バンドル名</param>
        /// <param name="assetName">アセット名</param>
        /// <returns>VideoClip（失敗時はnull）</returns>
        Task<VideoClip?> LoadVideoClipAsync(string bundleName, string assetName);

        /// <summary>
        /// AssetBundleから任意のアセットをロードする
        /// </summary>
        /// <typeparam name="T">アセットの型</typeparam>
        /// <param name="bundleName">バンドル名</param>
        /// <param name="assetName">アセット名</param>
        /// <returns>アセット（失敗時はnull）</returns>
        Task<T?> LoadAssetAsync<T>(string bundleName, string assetName) where T : UnityEngine.Object;

        /// <summary>
        /// バンドルがキャッシュされているかを確認
        /// </summary>
        /// <param name="bundleName">バンドル名</param>
        /// <param name="version">バージョン</param>
        /// <returns>キャッシュされている場合はtrue</returns>
        bool IsBundleCached(string bundleName, uint version = 0);

        /// <summary>
        /// バンドルの状態を取得
        /// </summary>
        /// <param name="bundleName">バンドル名</param>
        /// <returns>バンドルの状態</returns>
        BundleState GetBundleState(string bundleName);

        /// <summary>
        /// ロード済みのバンドルをアンロードする
        /// </summary>
        /// <param name="bundleName">バンドル名</param>
        /// <param name="unloadAllLoadedObjects">ロード済みオブジェクトもアンロードするか</param>
        void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false);

        /// <summary>
        /// すべてのバンドルをアンロードする
        /// </summary>
        /// <param name="unloadAllLoadedObjects">ロード済みオブジェクトもアンロードするか</param>
        void UnloadAllBundles(bool unloadAllLoadedObjects = false);

        /// <summary>
        /// キャッシュをクリアする
        /// </summary>
        void ClearCache();
    }
}
