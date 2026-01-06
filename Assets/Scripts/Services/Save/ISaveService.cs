#nullable enable
using System;

namespace LifeLike.Services.Save
{
    /// <summary>
    /// セーブデータを管理するサービスのインターフェース
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// セーブデータが存在するかどうか
        /// </summary>
        bool HasSaveData { get; }

        /// <summary>
        /// 最後にセーブした日時
        /// </summary>
        DateTime? LastSaveTime { get; }

        /// <summary>
        /// オートセーブ完了時のイベント
        /// </summary>
        event Action? OnAutoSaved;

        /// <summary>
        /// ロード完了時のイベント
        /// </summary>
        event Action? OnLoaded;

        /// <summary>
        /// セーブ/ロードエラー時のイベント
        /// </summary>
        event Action<string>? OnError;

        /// <summary>
        /// オートセーブを実行する
        /// </summary>
        void AutoSave();

        /// <summary>
        /// セーブデータをロードする
        /// </summary>
        /// <returns>ロードに成功した場合はtrue</returns>
        bool Load();

        /// <summary>
        /// セーブデータを削除する
        /// </summary>
        void DeleteSave();

        /// <summary>
        /// 現在のシーンIDを取得する（ロード時に復元するため）
        /// </summary>
        string? GetSavedSceneId();
    }
}
