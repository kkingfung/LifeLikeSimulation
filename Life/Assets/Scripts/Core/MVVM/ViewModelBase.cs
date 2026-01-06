#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LifeLike.Core.MVVM
{
    /// <summary>
    /// ViewModelの基底クラス
    /// INotifyPropertyChangedを実装し、UIへのプロパティ変更通知を提供する
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// プロパティ変更時に発火するイベント
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティの値を設定し、変更があれば通知する
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">バッキングフィールドへの参照</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">プロパティ名（自動取得）</param>
        /// <returns>値が変更された場合はtrue</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// プロパティ変更を通知する
        /// </summary>
        /// <param name="propertyName">プロパティ名（自動取得）</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// すべてのプロパティ変更を通知する
        /// </summary>
        protected void OnAllPropertiesChanged()
        {
            OnPropertyChanged(string.Empty);
        }

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを解放する（派生クラスでオーバーライド可能）
        /// </summary>
        /// <param name="disposing">マネージドリソースを解放するかどうか</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // マネージドリソースの解放
                PropertyChanged = null;
            }

            _isDisposed = true;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~ViewModelBase()
        {
            Dispose(false);
        }
    }
}
