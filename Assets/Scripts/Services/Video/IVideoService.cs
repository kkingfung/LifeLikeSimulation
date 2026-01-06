#nullable enable
using System;
using System.Threading.Tasks;
using LifeLike.Data;
using UnityEngine;
using UnityEngine.Video;

namespace LifeLike.Services.Video
{
    /// <summary>
    /// 動画再生を管理するサービスのインターフェース
    /// ローカルファイルとAssetBundleの両方をサポート
    /// </summary>
    public interface IVideoService
    {
        /// <summary>
        /// 動画をロード中かどうか
        /// </summary>
        bool IsLoading { get; }
        /// <summary>
        /// 動画が再生中かどうか
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// 動画が一時停止中かどうか
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// 動画が準備完了かどうか
        /// </summary>
        bool IsPrepared { get; }

        /// <summary>
        /// 現在の再生時間（秒）
        /// </summary>
        double CurrentTime { get; }

        /// <summary>
        /// 動画の総再生時間（秒）
        /// </summary>
        double Duration { get; }

        /// <summary>
        /// 再生進捗（0.0〜1.0）
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// 動画再生開始時のイベント
        /// </summary>
        event Action? OnVideoStarted;

        /// <summary>
        /// 動画再生完了時のイベント
        /// </summary>
        event Action? OnVideoCompleted;

        /// <summary>
        /// 再生時間更新時のイベント（秒）
        /// </summary>
        event Action<double>? OnTimeUpdated;

        /// <summary>
        /// 動画準備完了時のイベント
        /// </summary>
        event Action? OnVideoPrepared;

        /// <summary>
        /// エラー発生時のイベント
        /// </summary>
        event Action<string>? OnVideoError;

        /// <summary>
        /// VideoPlayerを初期化する
        /// </summary>
        /// <param name="videoPlayer">使用するVideoPlayer</param>
        /// <param name="targetTexture">描画先のRenderTexture（nullの場合はカメラ描画）</param>
        void Initialize(VideoPlayer videoPlayer, RenderTexture? targetTexture = null);

        /// <summary>
        /// 動画クリップを再生する
        /// </summary>
        /// <param name="clip">再生する動画クリップ</param>
        void Play(VideoClip clip);

        /// <summary>
        /// URLから動画を再生する
        /// </summary>
        /// <param name="url">動画のURL</param>
        void PlayFromUrl(string url);

        /// <summary>
        /// 再生を一時停止する
        /// </summary>
        void Pause();

        /// <summary>
        /// 一時停止から再開する
        /// </summary>
        void Resume();

        /// <summary>
        /// 再生を停止する
        /// </summary>
        void Stop();

        /// <summary>
        /// 現在の動画をスキップする（最後までシーク）
        /// </summary>
        void Skip();

        /// <summary>
        /// 指定した時間にシークする
        /// </summary>
        /// <param name="time">シーク先の時間（秒）</param>
        void Seek(double time);

        /// <summary>
        /// 音量を設定する
        /// </summary>
        /// <param name="volume">音量（0.0〜1.0）</param>
        void SetVolume(float volume);

        /// <summary>
        /// VideoReferenceから動画を再生する（AssetBundle対応）
        /// </summary>
        /// <param name="videoReference">動画参照情報</param>
        /// <returns>再生準備が完了した場合はtrue</returns>
        Task<bool> PlayAsync(VideoReference videoReference);

        /// <summary>
        /// 動画をプリロードする（事前ダウンロード）
        /// </summary>
        /// <param name="videoReference">動画参照情報</param>
        /// <returns>プリロードが完了した場合はtrue</returns>
        Task<bool> PreloadAsync(VideoReference videoReference);
    }
}
