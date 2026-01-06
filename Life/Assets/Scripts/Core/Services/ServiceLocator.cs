#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Core.Services
{
    /// <summary>
    /// サービスロケーターパターンの実装
    /// アプリケーション全体でサービスの登録と取得を管理する
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// ServiceLocatorのシングルトンインスタンス
        /// </summary>
        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ServiceLocator();
                    }
                }
                return _instance;
            }
        }

        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<Type, Func<object>> _factories = new();

        /// <summary>
        /// プライベートコンストラクタ（シングルトンパターン）
        /// </summary>
        private ServiceLocator() { }

        /// <summary>
        /// サービスを登録する
        /// </summary>
        /// <typeparam name="T">サービスのインターフェース型</typeparam>
        /// <param name="service">サービスのインスタンス</param>
        public void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var type = typeof(T);
            lock (_lock)
            {
                if (_services.ContainsKey(type))
                {
                    Debug.LogWarning($"[ServiceLocator] サービス {type.Name} は既に登録されています。上書きします。");
                }
                _services[type] = service;
            }
            Debug.Log($"[ServiceLocator] サービス {type.Name} を登録しました。");
        }

        /// <summary>
        /// サービスファクトリを登録する（遅延初期化用）
        /// </summary>
        /// <typeparam name="T">サービスのインターフェース型</typeparam>
        /// <param name="factory">サービスを生成するファクトリ関数</param>
        public void RegisterFactory<T>(Func<T> factory) where T : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var type = typeof(T);
            lock (_lock)
            {
                _factories[type] = () => factory();
            }
            Debug.Log($"[ServiceLocator] サービスファクトリ {type.Name} を登録しました。");
        }

        /// <summary>
        /// サービスを取得する
        /// </summary>
        /// <typeparam name="T">サービスのインターフェース型</typeparam>
        /// <returns>サービスのインスタンス（見つからない場合はnull）</returns>
        public T? Get<T>() where T : class
        {
            var type = typeof(T);
            lock (_lock)
            {
                // まず登録済みサービスを探す
                if (_services.TryGetValue(type, out var service))
                {
                    return (T)service;
                }

                // ファクトリがあれば生成して登録
                if (_factories.TryGetValue(type, out var factory))
                {
                    var newService = (T)factory();
                    _services[type] = newService;
                    _factories.Remove(type);
                    Debug.Log($"[ServiceLocator] サービス {type.Name} をファクトリから生成しました。");
                    return newService;
                }
            }

            Debug.LogWarning($"[ServiceLocator] サービス {type.Name} が見つかりません。");
            return null;
        }

        /// <summary>
        /// サービスを取得する（見つからない場合は例外をスロー）
        /// </summary>
        /// <typeparam name="T">サービスのインターフェース型</typeparam>
        /// <returns>サービスのインスタンス</returns>
        /// <exception cref="InvalidOperationException">サービスが見つからない場合</exception>
        public T GetRequired<T>() where T : class
        {
            var service = Get<T>();
            if (service == null)
            {
                throw new InvalidOperationException($"必須サービス {typeof(T).Name} が登録されていません。");
            }
            return service;
        }

        /// <summary>
        /// サービスが登録されているかどうかを確認する
        /// </summary>
        /// <typeparam name="T">サービスのインターフェース型</typeparam>
        /// <returns>登録されている場合はtrue</returns>
        public bool IsRegistered<T>() where T : class
        {
            var type = typeof(T);
            lock (_lock)
            {
                return _services.ContainsKey(type) || _factories.ContainsKey(type);
            }
        }

        /// <summary>
        /// サービスの登録を解除する
        /// </summary>
        /// <typeparam name="T">サービスのインターフェース型</typeparam>
        public void Unregister<T>() where T : class
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (_services.Remove(type))
                {
                    Debug.Log($"[ServiceLocator] サービス {type.Name} の登録を解除しました。");
                }
                _factories.Remove(type);
            }
        }

        /// <summary>
        /// すべてのサービスをクリアする
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                // IDisposableを実装しているサービスはDisposeする
                foreach (var service in _services.Values)
                {
                    if (service is IDisposable disposable)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[ServiceLocator] サービスのDispose中にエラー: {ex.Message}");
                        }
                    }
                }

                _services.Clear();
                _factories.Clear();
            }
            Debug.Log("[ServiceLocator] すべてのサービスをクリアしました。");
        }

        /// <summary>
        /// インスタンスをリセットする（主にテスト用）
        /// </summary>
        public static void ResetInstance()
        {
            lock (_lock)
            {
                _instance?.Clear();
                _instance = null;
            }
        }
    }
}
