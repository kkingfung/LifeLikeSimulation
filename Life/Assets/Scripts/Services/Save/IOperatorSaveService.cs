#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;

namespace LifeLike.Services.Save
{
    /// <summary>
    /// オペレーターモード用セーブサービスのインターフェース
    /// 夜間進行、フラグ、エンドステートを管理
    /// </summary>
    public interface IOperatorSaveService
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
        /// 現在の夜のインデックス (0-9)
        /// </summary>
        int GetCurrentNightIndex();

        /// <summary>
        /// 完了した夜のリストを取得
        /// </summary>
        List<string> GetCompletedNights();

        /// <summary>
        /// 指定した夜のエンドステートを取得
        /// </summary>
        EndStateType? GetNightEndState(string nightId);

        /// <summary>
        /// 永続フラグを取得
        /// </summary>
        List<FlagState> GetPersistentFlags();

        /// <summary>
        /// 夜の結果を保存
        /// </summary>
        void SaveNightResult(string nightId, EndStateType endState, List<FlagState> persistentFlags);

        /// <summary>
        /// 中断セーブ（夜の途中でやめた場合）
        /// </summary>
        void SaveMidNight(string nightId, int currentTimeMinutes, List<FlagState> currentFlags);

        /// <summary>
        /// 中断セーブがあるかどうか
        /// </summary>
        bool HasMidNightSave { get; }

        /// <summary>
        /// セーブデータを削除
        /// </summary>
        void DeleteSave();

        /// <summary>
        /// オートセーブ完了時のイベント
        /// </summary>
        event Action? OnAutoSaved;

        /// <summary>
        /// ロード完了時のイベント
        /// </summary>
        event Action? OnLoaded;
    }
}
