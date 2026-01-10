#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.EndState;

namespace LifeLike.Data
{
    /// <summary>
    /// チャプター（夜）の状態
    /// </summary>
    public enum ChapterState
    {
        Locked,      // 未解放
        Available,   // プレイ可能
        InProgress,  // 進行中（中断セーブあり）
        Completed    // 完了
    }

    /// <summary>
    /// チャプター情報
    /// </summary>
    [Serializable]
    public class ChapterInfo
    {
        public string chapterId = string.Empty;
        public string title = string.Empty;
        public string description = string.Empty;
        public int nightIndex;
        public ChapterState state = ChapterState.Locked;
        public EndStateType? endState;
        public string? endingTitle;
        public List<string> unlockedRoutes = new();
    }

    /// <summary>
    /// ルート分岐情報
    /// </summary>
    [Serializable]
    public class RouteBranch
    {
        public string fromChapterId = string.Empty;
        public string toChapterId = string.Empty;
        public string routeId = string.Empty;
        public string routeName = string.Empty;
        public bool isUnlocked;
        public EndStateType? requiredEndState;
    }

    /// <summary>
    /// プレイヤーの進行状況サマリー
    /// </summary>
    [Serializable]
    public class PlayerProgressSummary
    {
        public int totalNights = 10;
        public int completedNights;
        public string currentChapterId = string.Empty;
        public List<ChapterInfo> chapters = new();
        public List<RouteBranch> routes = new();
        public string finalEndingId = string.Empty;
        public string finalEndingTitle = string.Empty;
    }
}
