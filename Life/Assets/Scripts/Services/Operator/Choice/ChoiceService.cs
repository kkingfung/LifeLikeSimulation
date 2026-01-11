#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Data;
using LifeLike.Services.Core.Story;
using UnityEngine;

namespace LifeLike.Services.Operator.Choice
{
    /// <summary>
    /// 選択肢の表示と選択を管理するサービス
    /// </summary>
    public class ChoiceService : IChoiceService, IDisposable
    {
        private readonly IStoryService _storyService;
        private readonly List<ChoiceData> _currentChoices = new();
        private float _remainingTime;
        private bool _isTimerActive;
        private bool _isDisposed;

        /// <inheritdoc/>
        public bool IsShowingChoices => _currentChoices.Count > 0;

        /// <inheritdoc/>
        public float RemainingTime => _remainingTime;

        /// <inheritdoc/>
        public bool IsTimerActive => _isTimerActive;

        /// <inheritdoc/>
        public event Action<IReadOnlyList<ChoiceData>>? OnChoicesPresented;

        /// <inheritdoc/>
        public event Action<ChoiceData>? OnChoiceSelected;

        /// <inheritdoc/>
        public event Action<float>? OnTimerUpdated;

        /// <inheritdoc/>
        public event Action? OnTimedOut;

        /// <inheritdoc/>
        public event Action? OnChoicesHidden;

        /// <summary>
        /// ChoiceServiceを初期化する
        /// </summary>
        /// <param name="storyService">ストーリーサービス</param>
        public ChoiceService(IStoryService storyService)
        {
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
        }

        /// <inheritdoc/>
        public void PresentChoices(IEnumerable<ChoiceData> choices, bool filterUnavailable = false)
        {
            _currentChoices.Clear();

            var choiceList = choices.ToList();

            foreach (var choice in choiceList)
            {
                var isAvailable = IsChoiceAvailable(choice);

                // フィルタリング設定に応じて追加
                if (filterUnavailable)
                {
                    if (isAvailable)
                    {
                        _currentChoices.Add(choice);
                    }
                }
                else
                {
                    // showWhenLockedがtrueか、利用可能な場合は追加
                    if (choice.showWhenLocked || isAvailable)
                    {
                        _currentChoices.Add(choice);
                    }
                }
            }

            Debug.Log($"[ChoiceService] 選択肢を表示: {_currentChoices.Count}個");

            // 時限選択のチェック
            var timedChoice = _currentChoices.FirstOrDefault(c => c.choiceType == ChoiceType.Timed);
            if (timedChoice != null)
            {
                StartTimer(timedChoice.timeLimit);
            }

            OnChoicesPresented?.Invoke(_currentChoices.AsReadOnly());
        }

        /// <inheritdoc/>
        public void SelectChoice(ChoiceData choice)
        {
            if (choice == null)
            {
                Debug.LogWarning("[ChoiceService] 選択肢がnullです。");
                return;
            }

            if (!IsChoiceAvailable(choice))
            {
                Debug.LogWarning($"[ChoiceService] 選択肢 {choice.choiceId} は選択できません。");
                return;
            }

            StopTimer();

            // 効果を適用
            if (choice.effects.Count > 0)
            {
                _storyService.ApplyEffects(choice.effects);
            }

            Debug.Log($"[ChoiceService] 選択肢を選択: {choice.choiceId}");

            // 選択肢をクリア
            _currentChoices.Clear();

            OnChoiceSelected?.Invoke(choice);

            // 次のシーンへ遷移
            if (!string.IsNullOrEmpty(choice.nextSceneId))
            {
                _storyService.LoadScene(choice.nextSceneId);
            }
        }

        /// <inheritdoc/>
        public void HideChoices()
        {
            _currentChoices.Clear();
            StopTimer();
            OnChoicesHidden?.Invoke();
            Debug.Log("[ChoiceService] 選択肢を非表示");
        }

        /// <inheritdoc/>
        public bool IsChoiceAvailable(ChoiceData choice)
        {
            if (choice == null)
            {
                return false;
            }

            // 条件がない場合は常に利用可能
            if (choice.requirements.Count == 0)
            {
                return true;
            }

            // すべての条件を評価
            return _storyService.EvaluateConditions(choice.requirements);
        }

        /// <inheritdoc/>
        public IReadOnlyList<ChoiceData> FilterAvailableChoices(IEnumerable<ChoiceData> choices)
        {
            return choices.Where(IsChoiceAvailable).ToList().AsReadOnly();
        }

        /// <inheritdoc/>
        public void StartTimer(float duration)
        {
            _remainingTime = duration;
            _isTimerActive = true;
            Debug.Log($"[ChoiceService] タイマー開始: {duration}秒");
        }

        /// <inheritdoc/>
        public void StopTimer()
        {
            _isTimerActive = false;
            _remainingTime = 0;
        }

        /// <summary>
        /// 毎フレーム呼び出してタイマーを更新する
        /// MonoBehaviourのUpdateから呼び出す
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        public void Update(float deltaTime)
        {
            if (!_isTimerActive)
            {
                return;
            }

            _remainingTime -= deltaTime;
            OnTimerUpdated?.Invoke(_remainingTime);

            if (_remainingTime <= 0)
            {
                _remainingTime = 0;
                _isTimerActive = false;
                Debug.Log("[ChoiceService] タイムアウト");
                OnTimedOut?.Invoke();
            }
        }

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _currentChoices.Clear();
            OnChoicesPresented = null;
            OnChoiceSelected = null;
            OnTimerUpdated = null;
            OnTimedOut = null;
            OnChoicesHidden = null;

            _isDisposed = true;
        }
    }
}
