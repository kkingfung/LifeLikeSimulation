#nullable enable
using System;
using System.Windows.Input;

namespace LifeLike.Core.Commands
{
    /// <summary>
    /// パラメータなしのRelayCommandの実装
    /// ボタンクリックなどのUIアクションをViewModelにバインドする
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// CanExecuteの結果が変更された時に発火するイベント
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// RelayCommandを初期化する
        /// </summary>
        /// <param name="execute">実行するアクション</param>
        /// <param name="canExecute">実行可能かどうかを判定する関数（省略可）</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定する
        /// </summary>
        /// <param name="parameter">未使用</param>
        /// <returns>実行可能な場合はtrue</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        /// <summary>
        /// コマンドを実行する
        /// </summary>
        /// <param name="parameter">未使用</param>
        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                _execute();
            }
        }

        /// <summary>
        /// CanExecuteの再評価を要求する
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// パラメータ付きRelayCommandの実装
    /// </summary>
    /// <typeparam name="T">パラメータの型</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        /// <summary>
        /// CanExecuteの結果が変更された時に発火するイベント
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// RelayCommandを初期化する
        /// </summary>
        /// <param name="execute">実行するアクション</param>
        /// <param name="canExecute">実行可能かどうかを判定する関数（省略可）</param>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定する
        /// </summary>
        /// <param name="parameter">コマンドパラメータ</param>
        /// <returns>実行可能な場合はtrue</returns>
        public bool CanExecute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
            {
                return _canExecute?.Invoke(default) ?? true;
            }

            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        /// <summary>
        /// コマンドを実行する
        /// </summary>
        /// <param name="parameter">コマンドパラメータ</param>
        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                T? typedParameter = default;
                if (parameter != null)
                {
                    typedParameter = (T)parameter;
                }
                _execute(typedParameter);
            }
        }

        /// <summary>
        /// CanExecuteの再評価を要求する
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
