#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Localization
{
    /// <summary>
    /// ローカライズテーブルのエントリ
    /// </summary>
    [Serializable]
    public class LocalizationTableEntry
    {
        [Tooltip("ローカライズキー")]
        public string key = string.Empty;

        [Tooltip("ローカライズされたテキスト")]
        public LocalizedString value = new();
    }

    /// <summary>
    /// ローカライズテーブル（ScriptableObject）
    /// 共通UIテキストや汎用テキストを管理
    /// </summary>
    [CreateAssetMenu(fileName = "NewLocalizationTable", menuName = "LifeLike/Localization/Localization Table")]
    public class LocalizationTable : ScriptableObject
    {
        [Header("テーブル情報")]
        [Tooltip("テーブル名")]
        public string tableName = string.Empty;

        [Tooltip("説明")]
        [TextArea(1, 3)]
        public string description = string.Empty;

        [Header("エントリ")]
        [Tooltip("ローカライズエントリのリスト")]
        public List<LocalizationTableEntry> entries = new();

        // 高速検索用のキャッシュ
        private Dictionary<string, LocalizedString>? _cache;

        /// <summary>
        /// キーからローカライズされた文字列を取得
        /// </summary>
        public LocalizedString? GetLocalizedString(string key)
        {
            BuildCacheIfNeeded();
            return _cache!.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// キーと言語からテキストを取得
        /// </summary>
        public string GetText(string key, Language language)
        {
            var localized = GetLocalizedString(key);
            return localized?.GetText(language) ?? key;
        }

        /// <summary>
        /// キーが存在するか確認
        /// </summary>
        public bool HasKey(string key)
        {
            BuildCacheIfNeeded();
            return _cache!.ContainsKey(key);
        }

        /// <summary>
        /// 全てのキーを取得
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            BuildCacheIfNeeded();
            return _cache!.Keys;
        }

        /// <summary>
        /// キャッシュを構築
        /// </summary>
        private void BuildCacheIfNeeded()
        {
            if (_cache != null) return;

            _cache = new Dictionary<string, LocalizedString>();
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                {
                    _cache[entry.key] = entry.value;
                }
            }
        }

        /// <summary>
        /// キャッシュをクリア（エディタでの変更時など）
        /// </summary>
        public void ClearCache()
        {
            _cache = null;
        }

        private void OnValidate()
        {
            ClearCache();
        }
    }

}
