#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Core.Commands;
using LifeLike.Core.MVVM;
using LifeLike.Data;
using LifeLike.Data.EndState;
using LifeLike.Data.Localization;
using LifeLike.Services.Core.Localization;
using LifeLike.Services.Core.Save;
using UnityEngine;

namespace LifeLike.ViewModels
{
    /// <summary>
    /// 結果画面のViewModel
    /// クレジットとルートサマリーを表示
    /// </summary>
    public class ResultViewModel : ViewModelBase
    {
        private readonly IOperatorSaveService _operatorSaveService;
        private readonly ILocalizationService? _localizationService;

        private string _endingTitle = string.Empty;
        private string _endingDescription = string.Empty;
        private EndStateType _finalEndState;
        private List<ChapterResultInfo> _chapterResults = new();
        private string _routeSummary = string.Empty;
        private string _playTime = string.Empty;
        private bool _isShowingCredits;

        /// <summary>
        /// エンディングタイトル
        /// </summary>
        public string EndingTitle
        {
            get => _endingTitle;
            private set => SetProperty(ref _endingTitle, value);
        }

        /// <summary>
        /// エンディング説明
        /// </summary>
        public string EndingDescription
        {
            get => _endingDescription;
            private set => SetProperty(ref _endingDescription, value);
        }

        /// <summary>
        /// 最終エンドステート
        /// </summary>
        public EndStateType FinalEndState
        {
            get => _finalEndState;
            private set => SetProperty(ref _finalEndState, value);
        }

        /// <summary>
        /// 各チャプターの結果
        /// </summary>
        public List<ChapterResultInfo> ChapterResults
        {
            get => _chapterResults;
            private set => SetProperty(ref _chapterResults, value);
        }

        /// <summary>
        /// ルートサマリー
        /// </summary>
        public string RouteSummary
        {
            get => _routeSummary;
            private set => SetProperty(ref _routeSummary, value);
        }

        /// <summary>
        /// プレイ時間
        /// </summary>
        public string PlayTime
        {
            get => _playTime;
            private set => SetProperty(ref _playTime, value);
        }

        /// <summary>
        /// クレジット表示中か
        /// </summary>
        public bool IsShowingCredits
        {
            get => _isShowingCredits;
            private set => SetProperty(ref _isShowingCredits, value);
        }

        /// <summary>
        /// クレジット表示コマンド
        /// </summary>
        public RelayCommand ShowCreditsCommand { get; }

        /// <summary>
        /// サマリー表示コマンド
        /// </summary>
        public RelayCommand ShowSummaryCommand { get; }

        /// <summary>
        /// メインメニューに戻るコマンド
        /// </summary>
        public RelayCommand ReturnToMenuCommand { get; }

        /// <summary>
        /// 新規ゲーム開始コマンド
        /// </summary>
        public RelayCommand NewGameCommand { get; }

        /// <summary>
        /// メインメニューに戻る要求イベント
        /// </summary>
        public event Action? OnReturnToMenuRequested;

        /// <summary>
        /// 新規ゲーム開始要求イベント
        /// </summary>
        public event Action? OnNewGameRequested;

        public ResultViewModel(IOperatorSaveService operatorSaveService, ILocalizationService? localizationService = null)
        {
            _operatorSaveService = operatorSaveService ?? throw new ArgumentNullException(nameof(operatorSaveService));
            _localizationService = localizationService;

            ShowCreditsCommand = new RelayCommand(() => IsShowingCredits = true);
            ShowSummaryCommand = new RelayCommand(() => IsShowingCredits = false);
            ReturnToMenuCommand = new RelayCommand(ExecuteReturnToMenu);
            NewGameCommand = new RelayCommand(ExecuteNewGame);

            LoadResults();
        }

        /// <summary>
        /// 結果を読み込む
        /// </summary>
        public void LoadResults()
        {
            var completedNights = _operatorSaveService.GetCompletedNights();
            var results = new List<ChapterResultInfo>();

            // 各夜の結果を取得
            for (int i = 0; i < 10; i++)
            {
                string nightId = i.ToString();  // シーンのNightDataSetと同じ形式（0, 1, 2, ...）
                var endState = _operatorSaveService.GetNightEndState(nightId);

                var result = new ChapterResultInfo
                {
                    nightIndex = i,
                    nightId = nightId,
                    title = $"Night {i + 1:D2}",
                    isCompleted = completedNights.Contains(nightId),
                    endState = endState,
                    endingTitle = GetEndingTitle(endState)
                };

                results.Add(result);
            }

            ChapterResults = results;

            // 最終エンドステートを取得
            var night10EndState = _operatorSaveService.GetNightEndState("9");
            if (night10EndState.HasValue)
            {
                FinalEndState = night10EndState.Value;
                EndingTitle = GetFinalEndingTitle(night10EndState.Value);
                EndingDescription = GetFinalEndingDescription(night10EndState.Value);
            }
            else
            {
                EndingTitle = "未完了";
                EndingDescription = "全ての夜を完了してください。";
            }

            // ルートサマリーを生成
            RouteSummary = GenerateRouteSummary(results);

            Debug.Log($"[ResultViewModel] 結果を読み込み: {completedNights.Count}/10 完了, 最終: {FinalEndState}");
        }

        /// <summary>
        /// ルートサマリーを生成（ローカライズ対応）
        /// </summary>
        private string GenerateRouteSummary(List<ChapterResultInfo> results)
        {
            int goodCount = 0;
            int neutralCount = 0;
            int badCount = 0;

            foreach (var result in results)
            {
                if (!result.isCompleted || result.endState == null) continue;

                var category = CategorizeEndState(result.endState.Value);
                switch (category)
                {
                    case "good": goodCount++; break;
                    case "neutral": neutralCount++; break;
                    case "bad": badCount++; break;
                }
            }

            string routeType = GetLocalizedRouteType(goodCount, badCount);
            string yourRoute = GetLocalizedText(UILocalizationKeys.Result.YourRoute, "あなたのルート:");
            string bestChoices = string.Format(GetLocalizedText(UILocalizationKeys.Result.BestChoices, "最善の選択: {0}回"), goodCount);
            string neutralChoices = string.Format(GetLocalizedText(UILocalizationKeys.Result.NeutralChoices, "中立の選択: {0}回"), neutralCount);
            string badChoicesText = string.Format(GetLocalizedText(UILocalizationKeys.Result.BadChoices, "過ちの選択: {0}回"), badCount);

            return $"{yourRoute} {routeType}\n{bestChoices}\n{neutralChoices}\n{badChoicesText}";
        }

        /// <summary>
        /// ルートタイプのローカライズを取得
        /// </summary>
        private string GetLocalizedRouteType(int goodCount, int badCount)
        {
            if (goodCount >= 7)
                return GetLocalizedText(UILocalizationKeys.Result.RouteTruth, "真実の道");
            else if (badCount >= 7)
                return GetLocalizedText(UILocalizationKeys.Result.RouteDarkness, "闇の道");
            else if (goodCount > badCount)
                return GetLocalizedText(UILocalizationKeys.Result.RouteLight, "光への道");
            else if (badCount > goodCount)
                return GetLocalizedText(UILocalizationKeys.Result.RouteShadow, "影への道");
            else
                return GetLocalizedText(UILocalizationKeys.Result.RouteNeutral, "中立の道");
        }

        /// <summary>
        /// ローカライズテキストを取得（フォールバック付き）
        /// </summary>
        private string GetLocalizedText(string key, string fallback)
        {
            if (_localizationService != null)
            {
                return _localizationService.GetText(key);
            }
            return fallback;
        }

        /// <summary>
        /// エンドステートをカテゴリ分け
        /// </summary>
        private string CategorizeEndState(EndStateType endState)
        {
            return endState switch
            {
                // Good endings - 最善の結果
                EndStateType.Exposed => "good",
                EndStateType.TruthDawn => "good",
                EndStateType.VoiceReached => "good",
                EndStateType.WitnessConnected => "good",
                EndStateType.Connected => "good",
                EndStateType.Crossroads => "good",
                EndStateType.Intervention => "good",
                EndStateType.StormPrepared => "good",
                EndStateType.MisakiProtected => "good",
                EndStateType.TruthSeeker => "good",
                EndStateType.FullAlliance => "good",
                EndStateType.ActiveAlliance => "good",
                EndStateType.WhistleblowerSaved => "good",
                EndStateType.TruthRevealed => "good",
                EndStateType.MisakiDiscovered => "good",

                // Neutral endings - 中立/部分的な結果
                EndStateType.Contained => "neutral",
                EndStateType.Flagged => "neutral",
                EndStateType.Vigilant => "neutral",
                EndStateType.Compliant => "neutral",
                EndStateType.Routine => "neutral",
                EndStateType.WitnessOnly => "neutral",
                EndStateType.ConnectedOnly => "neutral",
                EndStateType.VoiceDistant => "neutral",
                EndStateType.StormAware => "neutral",
                EndStateType.StormDistant => "neutral",
                EndStateType.MisakiSafeUnaware => "neutral",
                EndStateType.InformedCaution => "neutral",
                EndStateType.SilentWitness => "neutral",
                EndStateType.PassiveTruth => "neutral",
                EndStateType.InvestigationContinues => "neutral",
                EndStateType.UncertainDawn => "neutral",
                EndStateType.UncertainFuture => "neutral",
                EndStateType.Disclosure => "neutral",

                // Bad endings - 悪い結果
                EndStateType.Complicit => "bad",
                EndStateType.Absorbed => "bad",
                EndStateType.Isolated => "bad",
                EndStateType.Silence => "bad",
                EndStateType.Neither => "bad",
                EndStateType.VoiceLost => "bad",
                EndStateType.StormUnaware => "bad",
                EndStateType.MisakiTaken => "bad",
                EndStateType.CollapseWitnessed => "bad",
                EndStateType.UnawareSurvivor => "bad",
                EndStateType.WhistleblowerEndangered => "bad",
                EndStateType.IntoDarkness => "bad",

                _ => "neutral"
            };
        }

        /// <summary>
        /// エンディングタイトルを取得（ローカライズ対応）
        /// </summary>
        private string? GetEndingTitle(EndStateType? endState)
        {
            if (endState == null) return null;

            // ローカライズサービスがある場合はローカライズされた文字列を取得
            if (_localizationService != null)
            {
                string? key = GetEndingTitleKey(endState.Value);
                if (key != null)
                {
                    return _localizationService.GetText(key);
                }
            }

            // フォールバック
            return GetEndingTitleFallback(endState.Value);
        }

        /// <summary>
        /// エンドステートに対応するローカライズキーを取得
        /// </summary>
        private string? GetEndingTitleKey(EndStateType endState)
        {
            return endState switch
            {
                EndStateType.Contained => UILocalizationKeys.Result.Contained,
                EndStateType.Exposed => UILocalizationKeys.Result.Exposed,
                EndStateType.Complicit => UILocalizationKeys.Result.Complicit,
                EndStateType.Flagged => UILocalizationKeys.Result.Flagged,
                EndStateType.Absorbed => UILocalizationKeys.Result.Absorbed,
                EndStateType.Vigilant => UILocalizationKeys.Result.Vigilant,
                EndStateType.Compliant => UILocalizationKeys.Result.Compliant,
                EndStateType.Connected => UILocalizationKeys.Result.Connected,
                EndStateType.Isolated => UILocalizationKeys.Result.Isolated,
                EndStateType.Routine => UILocalizationKeys.Result.Routine,
                EndStateType.Crossroads => UILocalizationKeys.Result.Crossroads,
                EndStateType.Intervention => UILocalizationKeys.Result.Intervention,
                EndStateType.Disclosure => UILocalizationKeys.Result.Disclosure,
                EndStateType.Silence => UILocalizationKeys.Result.Silence,
                EndStateType.WitnessConnected => UILocalizationKeys.Result.WitnessConnected,
                EndStateType.WitnessOnly => UILocalizationKeys.Result.WitnessOnly,
                EndStateType.ConnectedOnly => UILocalizationKeys.Result.ConnectedOnly,
                EndStateType.Neither => UILocalizationKeys.Result.Neither,
                EndStateType.VoiceReached => UILocalizationKeys.Result.VoiceReached,
                EndStateType.VoiceDistant => UILocalizationKeys.Result.VoiceDistant,
                EndStateType.VoiceLost => UILocalizationKeys.Result.VoiceLost,
                EndStateType.StormPrepared => UILocalizationKeys.Result.StormPrepared,
                EndStateType.StormAware => UILocalizationKeys.Result.StormAware,
                EndStateType.StormDistant => UILocalizationKeys.Result.StormDistant,
                EndStateType.StormUnaware => UILocalizationKeys.Result.StormUnaware,
                EndStateType.MisakiProtected => UILocalizationKeys.Result.MisakiProtected,
                EndStateType.MisakiSafeUnaware => UILocalizationKeys.Result.MisakiSafeUnaware,
                EndStateType.MisakiTaken => UILocalizationKeys.Result.MisakiTaken,
                EndStateType.CollapseWitnessed => UILocalizationKeys.Result.CollapseWitnessed,
                EndStateType.TruthSeeker => UILocalizationKeys.Result.TruthSeeker,
                EndStateType.InformedCaution => UILocalizationKeys.Result.InformedCaution,
                EndStateType.SilentWitness => UILocalizationKeys.Result.SilentWitness,
                EndStateType.UnawareSurvivor => UILocalizationKeys.Result.UnawareSurvivor,
                EndStateType.FullAlliance => UILocalizationKeys.Result.FullAlliance,
                EndStateType.ActiveAlliance => UILocalizationKeys.Result.ActiveAlliance,
                EndStateType.PassiveTruth => UILocalizationKeys.Result.PassiveTruth,
                EndStateType.WhistleblowerSaved => UILocalizationKeys.Result.WhistleblowerSaved,
                EndStateType.WhistleblowerEndangered => UILocalizationKeys.Result.WhistleblowerEndangered,
                EndStateType.MisakiDiscovered => UILocalizationKeys.Result.MisakiDiscovered,
                EndStateType.TruthRevealed => UILocalizationKeys.Result.TruthRevealed,
                EndStateType.UncertainFuture => UILocalizationKeys.Result.UncertainFuture,
                EndStateType.TruthDawn => UILocalizationKeys.Result.TruthDawn,
                EndStateType.InvestigationContinues => UILocalizationKeys.Result.InvestigationContinues,
                EndStateType.IntoDarkness => UILocalizationKeys.Result.IntoDarkness,
                EndStateType.UncertainDawn => UILocalizationKeys.Result.UncertainDawn,
                _ => null
            };
        }

        /// <summary>
        /// エンディングタイトルのフォールバック（日本語）
        /// </summary>
        private string GetEndingTitleFallback(EndStateType endState)
        {
            return endState switch
            {
                EndStateType.Contained => "封じ込め",
                EndStateType.Exposed => "露出",
                EndStateType.Complicit => "共犯",
                EndStateType.Flagged => "要注意",
                EndStateType.Absorbed => "吸収",
                EndStateType.Vigilant => "警戒",
                EndStateType.Compliant => "従順",
                EndStateType.Connected => "接続",
                EndStateType.Isolated => "孤立",
                EndStateType.Routine => "日常",
                EndStateType.Crossroads => "分かれ道",
                EndStateType.Intervention => "介入",
                EndStateType.Disclosure => "開示",
                EndStateType.Silence => "沈黙",
                EndStateType.WitnessConnected => "証人と接続",
                EndStateType.WitnessOnly => "証人",
                EndStateType.ConnectedOnly => "接続のみ",
                EndStateType.Neither => "どちらもなし",
                EndStateType.VoiceReached => "届いた声",
                EndStateType.VoiceDistant => "遠い声",
                EndStateType.VoiceLost => "消えた声",
                EndStateType.StormPrepared => "嵐への備え",
                EndStateType.StormAware => "嵐の予感",
                EndStateType.StormDistant => "遠い雷鳴",
                EndStateType.StormUnaware => "静かな午後",
                EndStateType.MisakiProtected => "小さな光",
                EndStateType.MisakiSafeUnaware => "守られた秘密",
                EndStateType.MisakiTaken => "崩壊",
                EndStateType.CollapseWitnessed => "崩壊の夜",
                EndStateType.TruthSeeker => "真実を追う者",
                EndStateType.InformedCaution => "慎重な知識",
                EndStateType.SilentWitness => "沈黙の証人",
                EndStateType.UnawareSurvivor => "無知な生存者",
                EndStateType.FullAlliance => "完全な同盟",
                EndStateType.ActiveAlliance => "積極的な協力",
                EndStateType.PassiveTruth => "真実を知った沈黙",
                EndStateType.WhistleblowerSaved => "告発者を救った",
                EndStateType.WhistleblowerEndangered => "告発者の危機",
                EndStateType.MisakiDiscovered => "美咲の存在を知った",
                EndStateType.TruthRevealed => "真実が明らかに",
                EndStateType.UncertainFuture => "不確かな未来",
                EndStateType.TruthDawn => "真実の夜明け",
                EndStateType.InvestigationContinues => "調査は続く",
                EndStateType.IntoDarkness => "闇の中へ",
                EndStateType.UncertainDawn => "不確かな夜明け",
                _ => endState.ToString()
            };
        }

        /// <summary>
        /// 最終エンディングタイトルを取得
        /// </summary>
        private string GetFinalEndingTitle(EndStateType endState)
        {
            return endState switch
            {
                EndStateType.TruthDawn => "真実の夜明け",
                EndStateType.InvestigationContinues => "調査は続く",
                EndStateType.IntoDarkness => "闇の中へ",
                EndStateType.UncertainDawn => "不確かな夜明け",
                _ => "終幕"
            };
        }

        /// <summary>
        /// 最終エンディング説明を取得
        /// </summary>
        private string GetFinalEndingDescription(EndStateType endState)
        {
            return endState switch
            {
                EndStateType.TruthDawn =>
                    "全ての真実が明らかになった。\n" +
                    "あなたの選択は、闇に光を灯した。\n" +
                    "夜明けは、新たな始まりを告げる。",

                EndStateType.InvestigationContinues =>
                    "真実の一部は明らかになった。\n" +
                    "しかし、まだ多くの謎が残されている。\n" +
                    "調査は続く——いつか全てが明らかになる日まで。",

                EndStateType.IntoDarkness =>
                    "真実は闇に葬られた。\n" +
                    "あなたの選択は、影を深くした。\n" +
                    "夜は、まだ終わらない。",

                EndStateType.UncertainDawn =>
                    "夜は明けた。\n" +
                    "しかし、何が正しかったのかは分からない。\n" +
                    "不確かな夜明けの中、日常は続く。",

                _ => "物語は終わりを迎えた。"
            };
        }

        private void ExecuteReturnToMenu()
        {
            Debug.Log("[ResultViewModel] メインメニューに戻る");
            OnReturnToMenuRequested?.Invoke();
        }

        private void ExecuteNewGame()
        {
            Debug.Log("[ResultViewModel] 新規ゲーム開始");
            _operatorSaveService.DeleteSave();
            OnNewGameRequested?.Invoke();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnReturnToMenuRequested = null;
                OnNewGameRequested = null;
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// チャプター結果情報
    /// </summary>
    [Serializable]
    public class ChapterResultInfo
    {
        public int nightIndex;
        public string nightId = string.Empty;
        public string title = string.Empty;
        public bool isCompleted;
        public EndStateType? endState;
        public string? endingTitle;
    }
}
