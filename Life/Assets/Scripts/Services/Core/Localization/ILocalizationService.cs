#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Localization;

namespace LifeLike.Services.Core.Localization
{
    /// <summary>
    /// ローカライズサービスのインターフェース
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// 現在の言語
        /// </summary>
        Language CurrentLanguage { get; }

        /// <summary>
        /// 利用可能な言語リスト
        /// </summary>
        IReadOnlyList<Language> AvailableLanguages { get; }

        /// <summary>
        /// 言語変更時のイベント
        /// </summary>
        event Action<Language>? OnLanguageChanged;

        /// <summary>
        /// 言語を設定
        /// </summary>
        void SetLanguage(Language language);

        /// <summary>
        /// ローカライズテーブルを読み込む
        /// </summary>
        void LoadTable(LocalizationTable table);

        /// <summary>
        /// シナリオ固有のローカライズデータを読み込む
        /// </summary>
        void LoadScenarioLocalization(ScenarioLocalizationData data);

        /// <summary>
        /// キーからテキストを取得（現在の言語）
        /// </summary>
        string GetText(string key);

        /// <summary>
        /// キーからテキストを取得（指定言語）
        /// </summary>
        string GetText(string key, Language language);

        /// <summary>
        /// LocalizedStringから現在の言語のテキストを取得
        /// </summary>
        string GetText(LocalizedString localizedString);

        /// <summary>
        /// 字幕トラックを取得
        /// </summary>
        SubtitleTrack? GetSubtitleTrack(string trackId);

        /// <summary>
        /// 指定時間の字幕テキストを取得
        /// </summary>
        string? GetSubtitleAt(string trackId, float time);

        /// <summary>
        /// テーブルをクリア
        /// </summary>
        void ClearTables();

        /// <summary>
        /// シナリオローカライズデータをクリア
        /// </summary>
        void ClearScenarioData();

        /// <summary>
        /// 言語の表示名を取得
        /// </summary>
        string GetLanguageDisplayName(Language language);
    }
}
