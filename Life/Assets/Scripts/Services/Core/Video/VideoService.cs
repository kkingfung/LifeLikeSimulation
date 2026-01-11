#nullable enable
using System;
using System.Threading.Tasks;
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Services.Core.AssetBundle;
using UnityEngine;
using UnityEngine.Video;

namespace LifeLike.Services.Core.Video
{
    /// <summary>
    /// 動画再生を管理するサービス
    /// ローカルファイルとAssetBundleの両方をサポート
    /// </summary>
    public class VideoService : IVideoService, IDisposable
    {
        private VideoPlayer? _videoPlayer;
        private bool _isDisposed;
        private bool _wasPlayingBeforePause;
        private bool _isLoading;

        /// <inheritdoc/>
        public bool IsLoading => _isLoading;

        /// <inheritdoc/>
        public bool IsPlaying => _videoPlayer != null && _videoPlayer.isPlaying;

        /// <inheritdoc/>
        public bool IsPaused => _videoPlayer != null && _videoPlayer.isPaused;

        /// <inheritdoc/>
        public bool IsPrepared => _videoPlayer != null && _videoPlayer.isPrepared;

        /// <inheritdoc/>
        public double CurrentTime => _videoPlayer?.time ?? 0;

        /// <inheritdoc/>
        public double Duration => _videoPlayer?.length ?? 0;

        /// <inheritdoc/>
        public float Progress
        {
            get
            {
                if (_videoPlayer == null || Duration <= 0)
                {
                    return 0f;
                }
                return (float)(CurrentTime / Duration);
            }
        }

        /// <inheritdoc/>
        public event Action? OnVideoStarted;

        /// <inheritdoc/>
        public event Action? OnVideoCompleted;

        /// <inheritdoc/>
        public event Action<double>? OnTimeUpdated;

        /// <inheritdoc/>
        public event Action? OnVideoPrepared;

        /// <inheritdoc/>
        public event Action<string>? OnVideoError;

        /// <inheritdoc/>
        public void Initialize(VideoPlayer videoPlayer, RenderTexture? targetTexture = null)
        {
            if (videoPlayer == null)
            {
                Debug.LogError("[VideoService] VideoPlayerがnullです。");
                return;
            }

            // 既存のイベントをクリア
            if (_videoPlayer != null)
            {
                UnsubscribeEvents();
            }

            _videoPlayer = videoPlayer;

            // 基本設定
            _videoPlayer.playOnAwake = false;
            _videoPlayer.waitForFirstFrame = true;

            if (targetTexture != null)
            {
                _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                _videoPlayer.targetTexture = targetTexture;
            }

            // イベント購読
            SubscribeEvents();

            Debug.Log("[VideoService] 初期化完了");
        }

        private void SubscribeEvents()
        {
            if (_videoPlayer == null) return;

            _videoPlayer.prepareCompleted += OnPrepareCompleted;
            _videoPlayer.started += OnStarted;
            _videoPlayer.loopPointReached += OnLoopPointReached;
            _videoPlayer.errorReceived += OnErrorReceived;
        }

        private void UnsubscribeEvents()
        {
            if (_videoPlayer == null) return;

            _videoPlayer.prepareCompleted -= OnPrepareCompleted;
            _videoPlayer.started -= OnStarted;
            _videoPlayer.loopPointReached -= OnLoopPointReached;
            _videoPlayer.errorReceived -= OnErrorReceived;
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            Debug.Log($"[VideoService] 動画準備完了: {Duration:F2}秒");
            OnVideoPrepared?.Invoke();
        }

        private void OnStarted(VideoPlayer source)
        {
            Debug.Log("[VideoService] 動画再生開始");
            OnVideoStarted?.Invoke();
        }

        private void OnLoopPointReached(VideoPlayer source)
        {
            Debug.Log("[VideoService] 動画再生完了");
            OnVideoCompleted?.Invoke();
        }

        private void OnErrorReceived(VideoPlayer source, string message)
        {
            Debug.LogError($"[VideoService] 動画エラー: {message}");
            OnVideoError?.Invoke(message);
        }

        /// <inheritdoc/>
        public void Play(VideoClip clip)
        {
            if (_videoPlayer == null)
            {
                Debug.LogError("[VideoService] VideoPlayerが初期化されていません。");
                return;
            }

            if (clip == null)
            {
                Debug.LogError("[VideoService] VideoClipがnullです。");
                return;
            }

            _videoPlayer.Stop();
            _videoPlayer.source = VideoSource.VideoClip;
            _videoPlayer.clip = clip;
            _videoPlayer.Prepare();

            // 準備完了後に自動再生
            _videoPlayer.prepareCompleted += AutoPlayOnPrepared;
        }

        /// <inheritdoc/>
        public void PlayFromUrl(string url)
        {
            if (_videoPlayer == null)
            {
                Debug.LogError("[VideoService] VideoPlayerが初期化されていません。");
                return;
            }

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("[VideoService] URLが空です。");
                return;
            }

            _videoPlayer.Stop();
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = url;
            _videoPlayer.Prepare();

            // 準備完了後に自動再生
            _videoPlayer.prepareCompleted += AutoPlayOnPrepared;
        }

        private void AutoPlayOnPrepared(VideoPlayer source)
        {
            source.prepareCompleted -= AutoPlayOnPrepared;
            source.Play();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (_videoPlayer == null) return;

            _wasPlayingBeforePause = _videoPlayer.isPlaying;
            _videoPlayer.Pause();
            Debug.Log("[VideoService] 一時停止");
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (_videoPlayer == null) return;

            _videoPlayer.Play();
            Debug.Log("[VideoService] 再開");
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (_videoPlayer == null) return;

            _videoPlayer.Stop();
            Debug.Log("[VideoService] 停止");
        }

        /// <inheritdoc/>
        public void Skip()
        {
            if (_videoPlayer == null || !_videoPlayer.isPrepared) return;

            // 動画の最後にシーク
            _videoPlayer.time = _videoPlayer.length - 0.1;
            Debug.Log("[VideoService] スキップ");
        }

        /// <inheritdoc/>
        public void Seek(double time)
        {
            if (_videoPlayer == null || !_videoPlayer.isPrepared) return;

            time = Math.Clamp(time, 0, _videoPlayer.length);
            _videoPlayer.time = time;
            Debug.Log($"[VideoService] シーク: {time:F2}秒");
        }

        /// <inheritdoc/>
        public void SetVolume(float volume)
        {
            if (_videoPlayer == null) return;

            volume = Mathf.Clamp01(volume);

            // すべてのオーディオトラックに音量を設定
            for (ushort i = 0; i < _videoPlayer.audioTrackCount; i++)
            {
                _videoPlayer.SetDirectAudioVolume(i, volume);
            }
        }

        /// <summary>
        /// 毎フレーム呼び出して時間更新イベントを発火する
        /// MonoBehaviourのUpdateから呼び出す
        /// </summary>
        public void Update()
        {
            if (_videoPlayer != null && _videoPlayer.isPlaying)
            {
                OnTimeUpdated?.Invoke(_videoPlayer.time);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PlayAsync(VideoReference videoReference)
        {
            if (_videoPlayer == null)
            {
                Debug.LogError("[VideoService] VideoPlayerが初期化されていません。");
                return false;
            }

            if (videoReference == null || !videoReference.IsValid)
            {
                Debug.LogError("[VideoService] VideoReferenceが無効です。");
                return false;
            }

            _isLoading = true;

            try
            {
                VideoClip? clip = null;

                switch (videoReference.source)
                {
                    case AssetSource.Local:
                        // ローカルクリップを使用
                        if (videoReference.HasLocalClip)
                        {
                            clip = videoReference.localClip;
                        }
                        else if (videoReference.HasDirectUrl)
                        {
                            // URLから再生
                            _isLoading = false;
                            PlayFromUrl(videoReference.directUrl);
                            return true;
                        }
                        break;

                    case AssetSource.AssetBundle:
                        // AssetBundleからロード
                        clip = await LoadVideoFromAssetBundleAsync(videoReference);
                        break;

                    case AssetSource.StreamingAssets:
                        // StreamingAssetsからURL再生
                        var path = videoReference.GetStreamingAssetsFullPath();
                        _isLoading = false;
                        PlayFromUrl(path);
                        return true;
                }

                _isLoading = false;

                if (clip != null)
                {
                    Play(clip);
                    return true;
                }

                Debug.LogError("[VideoService] 動画のロードに失敗しました。");
                return false;
            }
            catch (Exception ex)
            {
                _isLoading = false;
                Debug.LogError($"[VideoService] 動画再生エラー: {ex.Message}");
                OnVideoError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PreloadAsync(VideoReference videoReference)
        {
            if (videoReference == null || !videoReference.IsValid)
            {
                return false;
            }

            // AssetBundleの場合のみプリロードが意味を持つ
            if (videoReference.source != AssetSource.AssetBundle)
            {
                return true;
            }

            var assetBundleService = ServiceLocator.Instance.Get<IAssetBundleService>();
            if (assetBundleService == null)
            {
                Debug.LogWarning("[VideoService] AssetBundleServiceが見つかりません。");
                return false;
            }

            try
            {
                var success = await assetBundleService.DownloadBundleAsync(
                    videoReference.bundleName,
                    videoReference.bundleVersion);

                Debug.Log($"[VideoService] プリロード完了: {videoReference.bundleName} = {success}");
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VideoService] プリロードエラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// AssetBundleから動画をロードする
        /// </summary>
        private async Task<VideoClip?> LoadVideoFromAssetBundleAsync(VideoReference videoReference)
        {
            var assetBundleService = ServiceLocator.Instance.Get<IAssetBundleService>();
            if (assetBundleService == null)
            {
                Debug.LogError("[VideoService] AssetBundleServiceが登録されていません。");
                return null;
            }

            Debug.Log($"[VideoService] AssetBundleから動画をロード: {videoReference.bundleName}/{videoReference.assetName}");

            return await assetBundleService.LoadVideoClipAsync(
                videoReference.bundleName,
                videoReference.assetName);
        }

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            UnsubscribeEvents();
            _videoPlayer = null;

            OnVideoStarted = null;
            OnVideoCompleted = null;
            OnTimeUpdated = null;
            OnVideoPrepared = null;
            OnVideoError = null;

            _isDisposed = true;
        }
    }
}
