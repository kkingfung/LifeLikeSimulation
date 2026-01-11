#nullable enable
using System;
using System.Threading.Tasks;
using LifeLike.Core.Scene;

namespace LifeLike.Services.Core.Subscene
{
    /// <summary>
    /// サブシーン（オーバーレイシーン）管理サービスのインターフェース
    /// メインシーンの上にサブシーンを重ねて表示し、閉じた時に元のシーンに戻る
    /// </summary>
    public interface ISubsceneService
    {
        /// <summary>
        /// サブシーンが現在開いているかどうか
        /// </summary>
        bool IsSubsceneOpen { get; }

        /// <summary>
        /// 現在開いているサブシーン名
        /// </summary>
        string? CurrentSubsceneName { get; }

        /// <summary>
        /// サブシーンが開かれた時のイベント
        /// </summary>
        event Action<string>? OnSubsceneOpened;

        /// <summary>
        /// サブシーンが閉じられた時のイベント
        /// </summary>
        event Action<string>? OnSubsceneClosed;

        /// <summary>
        /// サブシーンを開く（加算ロード）
        /// </summary>
        /// <param name="sceneReference">開くシーン参照</param>
        /// <returns>成功したかどうか</returns>
        Task<bool> OpenSubsceneAsync(SceneReference sceneReference);

        /// <summary>
        /// 現在のサブシーンを閉じる
        /// </summary>
        /// <returns>成功したかどうか</returns>
        Task<bool> CloseSubsceneAsync();

        /// <summary>
        /// サブシーンとして開かれたかどうかを確認
        /// SettingsViewなどが自身の表示モードを判定するために使用
        /// </summary>
        /// <param name="sceneName">確認するシーン名</param>
        /// <returns>サブシーンとして開かれた場合true</returns>
        bool IsOpenedAsSubscene(string sceneName);
    }
}
