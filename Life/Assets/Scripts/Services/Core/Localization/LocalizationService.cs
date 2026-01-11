#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Localization;
using UnityEngine;

namespace LifeLike.Services.Core.Localization
{
    /// <summary>
    /// ローカライズサービスの実装
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private Language _currentLanguage = Language.Japanese;
        private readonly List<LocalizationTable> _tables = new();
        private ScenarioLocalizationData? _scenarioData;

        private readonly List<Language> _availableLanguages = new()
        {
            Language.Japanese,
            Language.English,
            Language.ChineseSimplified,
            Language.ChineseTraditional,
            Language.Korean
        };

        // 言語表示名のマッピング
        private static readonly Dictionary<Language, string> LanguageDisplayNames = new()
        {
            { Language.Japanese, "日本語" },
            { Language.English, "English" },
            { Language.ChineseSimplified, "简体中文" },
            { Language.ChineseTraditional, "繁體中文" },
            { Language.Korean, "한국어" },
            { Language.Spanish, "Español" },
            { Language.French, "Français" },
            { Language.German, "Deutsch" },
            { Language.Portuguese, "Português" },
            { Language.Russian, "Русский" },
            { Language.Italian, "Italiano" },
            { Language.Thai, "ไทย" },
            { Language.Vietnamese, "Tiếng Việt" },
            { Language.Indonesian, "Bahasa Indonesia" }
        };

        public Language CurrentLanguage => _currentLanguage;
        public IReadOnlyList<Language> AvailableLanguages => _availableLanguages.AsReadOnly();

        public event Action<Language>? OnLanguageChanged;

        public LocalizationService()
        {
            // システム言語から初期言語を決定
            _currentLanguage = GetSystemLanguage();
            Debug.Log($"[LocalizationService] 初期言語: {GetLanguageDisplayName(_currentLanguage)}");
        }

        public void SetLanguage(Language language)
        {
            if (_currentLanguage == language)
            {
                return;
            }

            _currentLanguage = language;
            Debug.Log($"[LocalizationService] 言語を変更: {GetLanguageDisplayName(language)}");
            OnLanguageChanged?.Invoke(language);

            // 言語設定を保存
            PlayerPrefs.SetInt("LifeLike_Language", (int)language);
            PlayerPrefs.Save();
        }

        public void LoadTable(LocalizationTable table)
        {
            if (!_tables.Contains(table))
            {
                _tables.Add(table);
                Debug.Log($"[LocalizationService] テーブルを読み込み: {table.tableName}");
            }
        }

        public void LoadScenarioLocalization(ScenarioLocalizationData data)
        {
            _scenarioData = data;
            Debug.Log($"[LocalizationService] シナリオローカライズデータを読み込み: {data.scenarioId}");
        }

        public string GetText(string key)
        {
            return GetText(key, _currentLanguage);
        }

        public string GetText(string key, Language language)
        {
            // シナリオデータを優先
            if (_scenarioData != null)
            {
                var text = _scenarioData.GetText(key, language);
                if (text != key) // キーそのものが返ってきた場合は見つからなかった
                {
                    return text;
                }
            }

            // テーブルを検索
            foreach (var table in _tables)
            {
                if (table.HasKey(key))
                {
                    return table.GetText(key, language);
                }
            }

            // 見つからない場合はキーをそのまま返す
            Debug.LogWarning($"[LocalizationService] キーが見つかりません: {key}");
            return key;
        }

        public string GetText(LocalizedString localizedString)
        {
            // キーが設定されていればテーブルから取得
            if (!string.IsNullOrEmpty(localizedString.key))
            {
                var text = GetText(localizedString.key);
                if (text != localizedString.key)
                {
                    return text;
                }
            }

            // 直接のエントリから取得
            return localizedString.GetText(_currentLanguage);
        }

        public SubtitleTrack? GetSubtitleTrack(string trackId)
        {
            return _scenarioData?.GetSubtitleTrack(trackId);
        }

        public string? GetSubtitleAt(string trackId, float time)
        {
            var track = GetSubtitleTrack(trackId);
            var entry = track?.GetSubtitleAt(time);

            if (entry == null)
            {
                return null;
            }

            return entry.text.GetText(_currentLanguage);
        }

        public void ClearTables()
        {
            _tables.Clear();
            Debug.Log("[LocalizationService] テーブルをクリア");
        }

        public void ClearScenarioData()
        {
            _scenarioData = null;
            Debug.Log("[LocalizationService] シナリオデータをクリア");
        }

        public string GetLanguageDisplayName(Language language)
        {
            return LanguageDisplayNames.TryGetValue(language, out var name) ? name : language.ToString();
        }

        /// <summary>
        /// システム言語から対応する言語を取得
        /// </summary>
        private Language GetSystemLanguage()
        {
            // 保存された言語設定があればそれを使用
            if (PlayerPrefs.HasKey("LifeLike_Language"))
            {
                return (Language)PlayerPrefs.GetInt("LifeLike_Language");
            }

            // システム言語から判定
            return Application.systemLanguage switch
            {
                SystemLanguage.Japanese => Language.Japanese,
                SystemLanguage.English => Language.English,
                SystemLanguage.ChineseSimplified => Language.ChineseSimplified,
                SystemLanguage.ChineseTraditional => Language.ChineseTraditional,
                SystemLanguage.Chinese => Language.ChineseSimplified,
                SystemLanguage.Korean => Language.Korean,
                SystemLanguage.Spanish => Language.Spanish,
                SystemLanguage.French => Language.French,
                SystemLanguage.German => Language.German,
                SystemLanguage.Portuguese => Language.Portuguese,
                SystemLanguage.Russian => Language.Russian,
                SystemLanguage.Italian => Language.Italian,
                SystemLanguage.Thai => Language.Thai,
                SystemLanguage.Vietnamese => Language.Vietnamese,
                SystemLanguage.Indonesian => Language.Indonesian,
                _ => Language.English // デフォルトは英語
            };
        }

        /// <summary>
        /// 利用可能な言語を追加
        /// </summary>
        public void AddAvailableLanguage(Language language)
        {
            if (!_availableLanguages.Contains(language))
            {
                _availableLanguages.Add(language);
            }
        }

        /// <summary>
        /// 利用可能な言語を設定
        /// </summary>
        public void SetAvailableLanguages(IEnumerable<Language> languages)
        {
            _availableLanguages.Clear();
            _availableLanguages.AddRange(languages);
        }
    }
}
