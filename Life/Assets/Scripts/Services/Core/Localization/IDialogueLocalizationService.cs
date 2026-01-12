#nullable enable
using System.Collections.Generic;
using LifeLike.Data.Localization;

namespace LifeLike.Services.Core.Localization
{
    /// <summary>
    /// ダイアログ（ストーリーコンテンツ）のローカライズを管理するサービスのインターフェース
    /// </summary>
    public interface IDialogueLocalizationService
    {
        /// <summary>
        /// 現在の言語
        /// </summary>
        Language CurrentLanguage { get; }

        /// <summary>
        /// 夜の翻訳データを読み込む
        /// </summary>
        /// <param name="scenarioId">シナリオID（例: "night01"）</param>
        /// <returns>成功したかどうか</returns>
        bool LoadNightTranslation(string scenarioId);

        /// <summary>
        /// 通話の翻訳を取得
        /// </summary>
        CallTranslation? GetCallTranslation(string callId);

        /// <summary>
        /// 発信者の翻訳を取得
        /// </summary>
        CallerTranslation? GetCallerTranslation(string callerId);

        /// <summary>
        /// 証拠の翻訳を取得
        /// </summary>
        EvidenceTranslation? GetEvidenceTranslation(string evidenceId);

        /// <summary>
        /// セグメントの発信者セリフを現在の言語で取得
        /// </summary>
        /// <param name="callId">通話ID</param>
        /// <param name="segmentId">セグメントID</param>
        /// <returns>ローカライズされたセリフのリスト</returns>
        List<string> GetCallerLines(string callId, string segmentId);

        /// <summary>
        /// 応答テキストを現在の言語で取得
        /// </summary>
        /// <param name="callId">通話ID</param>
        /// <param name="segmentId">セグメントID</param>
        /// <param name="responseId">応答ID</param>
        /// <returns>ローカライズされた応答テキスト</returns>
        string GetResponseText(string callId, string segmentId, string responseId);

        /// <summary>
        /// 発信者名を現在の言語で取得
        /// </summary>
        /// <param name="callerId">発信者ID</param>
        /// <returns>ローカライズされた発信者名</returns>
        string GetCallerDisplayName(string callerId);

        /// <summary>
        /// 証拠の内容を現在の言語で取得
        /// </summary>
        /// <param name="evidenceId">証拠ID</param>
        /// <returns>ローカライズされた証拠内容</returns>
        string GetEvidenceContent(string evidenceId);

        /// <summary>
        /// シナリオタイトルを現在の言語で取得
        /// </summary>
        string GetScenarioTitle();

        /// <summary>
        /// シナリオ説明を現在の言語で取得
        /// </summary>
        string GetScenarioDescription();

        /// <summary>
        /// 翻訳データがロード済みかどうか
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// 現在ロードされているシナリオID
        /// </summary>
        string? CurrentScenarioId { get; }
    }
}
