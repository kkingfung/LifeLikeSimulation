#nullable enable
using System;
using LifeLike.Services.AssetBundle;
using UnityEngine;
using UnityEngine.Video;

namespace LifeLike.Data
{
    /// <summary>
    /// 動画の参照情報
    /// ローカルファイルとAssetBundleの両方をサポート
    /// </summary>
    [Serializable]
    public class VideoReference
    {
        [Tooltip("動画のロード元")]
        public AssetSource source = AssetSource.Local;

        [Header("Local Settings")]
        [Tooltip("ローカルの動画クリップ（Localの場合）")]
        public VideoClip? localClip;

        [Tooltip("StreamingAssetsからの相対パス（StreamingAssetsの場合）")]
        public string streamingAssetsPath = string.Empty;

        [Header("AssetBundle Settings")]
        [Tooltip("AssetBundleの名前")]
        public string bundleName = string.Empty;

        [Tooltip("バンドル内のアセット名")]
        public string assetName = string.Empty;

        [Tooltip("バンドルのバージョン（キャッシュ用）")]
        public uint bundleVersion = 1;

        [Header("URL Settings")]
        [Tooltip("直接URLから再生する場合のURL")]
        public string directUrl = string.Empty;

        /// <summary>
        /// ローカルクリップが設定されているか
        /// </summary>
        public bool HasLocalClip => localClip != null;

        /// <summary>
        /// AssetBundleから読み込むか
        /// </summary>
        public bool IsAssetBundle => source == AssetSource.AssetBundle &&
                                     !string.IsNullOrEmpty(bundleName) &&
                                     !string.IsNullOrEmpty(assetName);

        /// <summary>
        /// StreamingAssetsから読み込むか
        /// </summary>
        public bool IsStreamingAssets => source == AssetSource.StreamingAssets &&
                                         !string.IsNullOrEmpty(streamingAssetsPath);

        /// <summary>
        /// URLから読み込むか
        /// </summary>
        public bool HasDirectUrl => !string.IsNullOrEmpty(directUrl);

        /// <summary>
        /// 有効な参照かどうか
        /// </summary>
        public bool IsValid
        {
            get
            {
                return source switch
                {
                    AssetSource.Local => HasLocalClip || HasDirectUrl,
                    AssetSource.AssetBundle => IsAssetBundle,
                    AssetSource.StreamingAssets => IsStreamingAssets,
                    _ => false
                };
            }
        }

        /// <summary>
        /// StreamingAssetsの完全なパスを取得
        /// </summary>
        public string GetStreamingAssetsFullPath()
        {
            if (!IsStreamingAssets)
            {
                return string.Empty;
            }

            return System.IO.Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);
        }
    }
}
