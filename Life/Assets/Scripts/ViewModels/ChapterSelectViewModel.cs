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
    /// チャプター選択画面のViewModel
    /// フローチャート形式で夜の進行状況を表示
    /// </summary>
    public class ChapterSelectViewModel : ViewModelBase
    {
        private readonly IOperatorSaveService _operatorSaveService;
        private readonly IDialogueLocalizationService _dialogueLocalizationService;
        private readonly ILocalizationService? _localizationService;

        private PlayerProgressSummary _progressSummary = new();
        private ChapterInfo? _selectedChapter;
        private bool _canStartChapter;

        /// <summary>
        /// 進行状況サマリー
        /// </summary>
        public PlayerProgressSummary ProgressSummary
        {
            get => _progressSummary;
            private set => SetProperty(ref _progressSummary, value);
        }

        /// <summary>
        /// 選択中のチャプター
        /// </summary>
        public ChapterInfo? SelectedChapter
        {
            get => _selectedChapter;
            private set
            {
                if (SetProperty(ref _selectedChapter, value))
                {
                    UpdateCanStartChapter();
                }
            }
        }

        /// <summary>
        /// チャプターを開始できるか
        /// </summary>
        public bool CanStartChapter
        {
            get => _canStartChapter;
            private set => SetProperty(ref _canStartChapter, value);
        }

        /// <summary>
        /// チャプター選択コマンド
        /// </summary>
        public RelayCommand<string> SelectChapterCommand { get; }

        /// <summary>
        /// チャプター開始コマンド
        /// </summary>
        public RelayCommand StartChapterCommand { get; }

        /// <summary>
        /// 戻るコマンド
        /// </summary>
        public RelayCommand BackCommand { get; }

        /// <summary>
        /// チャプター開始要求イベント
        /// </summary>
        public event Action<int>? OnChapterStartRequested;

        /// <summary>
        /// メインメニューに戻る要求イベント
        /// </summary>
        public event Action? OnBackToMenuRequested;

        public ChapterSelectViewModel(IOperatorSaveService operatorSaveService, IDialogueLocalizationService dialogueLocalizationService, ILocalizationService? localizationService = null)
        {
            _operatorSaveService = operatorSaveService ?? throw new ArgumentNullException(nameof(operatorSaveService));
            _dialogueLocalizationService = dialogueLocalizationService ?? throw new ArgumentNullException(nameof(dialogueLocalizationService));
            _localizationService = localizationService;

            SelectChapterCommand = new RelayCommand<string>(ExecuteSelectChapter);
            StartChapterCommand = new RelayCommand(ExecuteStartChapter, () => CanStartChapter);
            BackCommand = new RelayCommand(ExecuteBack);

            LoadProgressSummary();
        }

        /// <summary>
        /// 進行状況を読み込む
        /// </summary>
        public void LoadProgressSummary()
        {
            var summary = new PlayerProgressSummary();
            var completedNights = _operatorSaveService.GetCompletedNights();
            int currentNightIndex = _operatorSaveService.GetCurrentNightIndex();
            bool hasMidNightSave = _operatorSaveService.HasMidNightSave;

            // 10夜分のチャプター情報を生成
            for (int i = 0; i < 10; i++)
            {
                string nightId = i.ToString();  // シーンのNightDataSetと同じ形式（0, 1, 2, ...）
                var chapter = new ChapterInfo
                {
                    chapterId = nightId,
                    title = GetNightTitle(i),
                    nightIndex = i
                };

                // 状態を判定
                // ロック解除条件：最初の夜 OR 前の夜がクリア済み
                bool previousNightCompleted = i == 0 || completedNights.Contains((i - 1).ToString());

                if (completedNights.Contains(nightId))
                {
                    chapter.state = ChapterState.Completed;
                    chapter.endState = _operatorSaveService.GetNightEndState(nightId);
                    chapter.endingTitle = GetEndingTitle(chapter.endState);
                    // クリア済みの夜のみ説明を表示
                    chapter.description = GetNightDescription(i);
                }
                else if (previousNightCompleted)
                {
                    // 前の夜がクリア済み → プレイ可能
                    if (i == currentNightIndex && hasMidNightSave)
                    {
                        chapter.state = ChapterState.InProgress;
                    }
                    else
                    {
                        chapter.state = ChapterState.Available;
                    }
                    // 未クリアの夜は説明を非表示
                    chapter.description = string.Empty;
                }
                else
                {
                    // 前の夜が未クリア → ロック
                    chapter.state = ChapterState.Locked;
                    chapter.description = string.Empty;
                }

                summary.chapters.Add(chapter);
            }

            summary.completedNights = completedNights.Count;
            summary.currentChapterId = currentNightIndex.ToString();

            // ルート情報を生成
            GenerateRoutes(summary);

            ProgressSummary = summary;

            // 現在のチャプターを自動選択
            if (currentNightIndex < 10)
            {
                SelectedChapter = summary.chapters[currentNightIndex];
            }

            Debug.Log($"[ChapterSelectViewModel] 進行状況を読み込み: {completedNights.Count}/10 完了");
        }

        /// <summary>
        /// ルート情報を生成
        /// </summary>
        private void GenerateRoutes(PlayerProgressSummary summary)
        {
            // 各夜間の接続を生成
            for (int i = 0; i < 9; i++)
            {
                var fromChapter = summary.chapters[i];
                var toChapter = summary.chapters[i + 1];

                var route = new RouteBranch
                {
                    fromChapterId = fromChapter.chapterId,
                    toChapterId = toChapter.chapterId,
                    routeId = $"route_{i + 1:D2}_to_{i + 2:D2}",
                    routeName = GetRouteName(fromChapter.endState),
                    isUnlocked = fromChapter.state == ChapterState.Completed,
                    requiredEndState = fromChapter.endState
                };

                summary.routes.Add(route);
            }
        }

        /// <summary>
        /// 夜のタイトルを取得（ローカライズ対応）
        /// </summary>
        private string GetNightTitle(int nightIndex)
        {
            // シナリオIDを使って翻訳データを読み込み
            string scenarioId = nightIndex.ToString();
            if (_dialogueLocalizationService.LoadNightTranslation(scenarioId))
            {
                string localizedTitle = _dialogueLocalizationService.GetScenarioTitle();
                if (!string.IsNullOrEmpty(localizedTitle))
                {
                    return localizedTitle;
                }
            }

            // フォールバック：日本語ハードコード
            return nightIndex switch
            {
                0 => "Night 01: 最初の夜",
                1 => "Night 02: 炎の中で",
                2 => "Night 03: 目撃者",
                3 => "Night 04: 繋がり",
                4 => "Night 05: 小さな声",
                5 => "Night 06: 真実の断片",
                6 => "Night 07: 組織の影",
                7 => "Night 08: 選択の時",
                8 => "Night 09: 暴露",
                9 => "Night 10: 最後の夜",
                _ => $"Night {nightIndex + 1:D2}"
            };
        }

        /// <summary>
        /// 夜の説明を取得（ローカライズ対応）
        /// </summary>
        private string GetNightDescription(int nightIndex)
        {
            // シナリオIDを使って翻訳データを読み込み
            string scenarioId = nightIndex.ToString();
            if (_dialogueLocalizationService.LoadNightTranslation(scenarioId))
            {
                string localizedDescription = _dialogueLocalizationService.GetScenarioDescription();
                if (!string.IsNullOrEmpty(localizedDescription))
                {
                    return localizedDescription;
                }
            }

            // フォールバック：日本語ハードコード
            return nightIndex switch
            {
                0 => "緊急通報センターでの最初の勤務。奇妙な通報が始まる。",
                1 => "火災通報。しかし、何かがおかしい。",
                2 => "目撃者からの通報。真実は一つではない。",
                3 => "点と点が繋がり始める。",
                4 => "聞こえてくる小さな声。誰かが助けを求めている。",
                5 => "真実の断片が集まり始める。",
                6 => "組織の存在が明らかになる。",
                7 => "重大な選択を迫られる。",
                8 => "全てが明らかになる夜。",
                9 => "最後の決断。全ての結末が決まる。",
                _ => ""
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

            // フォールバック: 日本語ハードコード
            return GetEndingTitleFallback(endState.Value);
        }

        /// <summary>
        /// エンドステートに対応するローカライズキーを取得
        /// </summary>
        private string? GetEndingTitleKey(EndStateType endState)
        {
            return endState switch
            {
                // Night01
                EndStateType.Contained => UILocalizationKeys.Result.Contained,
                EndStateType.Exposed => UILocalizationKeys.Result.Exposed,
                EndStateType.Complicit => UILocalizationKeys.Result.Complicit,
                EndStateType.Flagged => UILocalizationKeys.Result.Flagged,
                EndStateType.Absorbed => UILocalizationKeys.Result.Absorbed,
                // Night02
                EndStateType.Vigilant => UILocalizationKeys.Result.Vigilant,
                EndStateType.Compliant => UILocalizationKeys.Result.Compliant,
                EndStateType.Connected => UILocalizationKeys.Result.Connected,
                EndStateType.Isolated => UILocalizationKeys.Result.Isolated,
                EndStateType.Routine => UILocalizationKeys.Result.Routine,
                // Night03
                EndStateType.Crossroads => UILocalizationKeys.Result.Crossroads,
                EndStateType.Intervention => UILocalizationKeys.Result.Intervention,
                EndStateType.Disclosure => UILocalizationKeys.Result.Disclosure,
                EndStateType.Silence => UILocalizationKeys.Result.Silence,
                // Night04
                EndStateType.WitnessConnected => UILocalizationKeys.Result.WitnessConnected,
                EndStateType.WitnessOnly => UILocalizationKeys.Result.WitnessOnly,
                EndStateType.ConnectedOnly => UILocalizationKeys.Result.ConnectedOnly,
                EndStateType.Neither => UILocalizationKeys.Result.Neither,
                // Night05
                EndStateType.VoiceReached => UILocalizationKeys.Result.VoiceReached,
                EndStateType.VoiceDistant => UILocalizationKeys.Result.VoiceDistant,
                EndStateType.VoiceLost => UILocalizationKeys.Result.VoiceLost,
                // Night06
                EndStateType.StormPrepared => UILocalizationKeys.Result.StormPrepared,
                EndStateType.StormAware => UILocalizationKeys.Result.StormAware,
                EndStateType.StormDistant => UILocalizationKeys.Result.StormDistant,
                EndStateType.StormUnaware => UILocalizationKeys.Result.StormUnaware,
                // Night07
                EndStateType.MisakiProtected => UILocalizationKeys.Result.MisakiProtected,
                EndStateType.MisakiSafeUnaware => UILocalizationKeys.Result.MisakiSafeUnaware,
                EndStateType.MisakiTaken => UILocalizationKeys.Result.MisakiTaken,
                EndStateType.CollapseWitnessed => UILocalizationKeys.Result.CollapseWitnessed,
                // Night08
                EndStateType.TruthSeeker => UILocalizationKeys.Result.TruthSeeker,
                EndStateType.InformedCaution => UILocalizationKeys.Result.InformedCaution,
                EndStateType.SilentWitness => UILocalizationKeys.Result.SilentWitness,
                EndStateType.UnawareSurvivor => UILocalizationKeys.Result.UnawareSurvivor,
                // Night09
                EndStateType.FullAlliance => UILocalizationKeys.Result.FullAlliance,
                EndStateType.ActiveAlliance => UILocalizationKeys.Result.ActiveAlliance,
                EndStateType.PassiveTruth => UILocalizationKeys.Result.PassiveTruth,
                EndStateType.WhistleblowerSaved => UILocalizationKeys.Result.WhistleblowerSaved,
                EndStateType.WhistleblowerEndangered => UILocalizationKeys.Result.WhistleblowerEndangered,
                EndStateType.MisakiDiscovered => UILocalizationKeys.Result.MisakiDiscovered,
                EndStateType.TruthRevealed => UILocalizationKeys.Result.TruthRevealed,
                EndStateType.UncertainFuture => UILocalizationKeys.Result.UncertainFuture,
                // Night10
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
                // Night01
                EndStateType.Contained => "封じ込め",
                EndStateType.Exposed => "露出",
                EndStateType.Complicit => "共犯",
                EndStateType.Flagged => "要注意",
                EndStateType.Absorbed => "吸収",
                // Night02
                EndStateType.Vigilant => "警戒",
                EndStateType.Compliant => "従順",
                EndStateType.Connected => "接続",
                EndStateType.Isolated => "孤立",
                EndStateType.Routine => "日常",
                // Night03
                EndStateType.Crossroads => "分かれ道",
                EndStateType.Intervention => "介入",
                EndStateType.Disclosure => "開示",
                EndStateType.Silence => "沈黙",
                // Night04
                EndStateType.WitnessConnected => "証人と接続",
                EndStateType.WitnessOnly => "証人",
                EndStateType.ConnectedOnly => "接続のみ",
                EndStateType.Neither => "どちらもなし",
                // Night05
                EndStateType.VoiceReached => "届いた声",
                EndStateType.VoiceDistant => "遠い声",
                EndStateType.VoiceLost => "消えた声",
                // Night06
                EndStateType.StormPrepared => "嵐への備え",
                EndStateType.StormAware => "嵐の予感",
                EndStateType.StormDistant => "遠い雷鳴",
                EndStateType.StormUnaware => "静かな午後",
                // Night07
                EndStateType.MisakiProtected => "小さな光",
                EndStateType.MisakiSafeUnaware => "守られた秘密",
                EndStateType.MisakiTaken => "崩壊",
                EndStateType.CollapseWitnessed => "崩壊の夜",
                // Night08
                EndStateType.TruthSeeker => "真実を追う者",
                EndStateType.InformedCaution => "慎重な知識",
                EndStateType.SilentWitness => "沈黙の証人",
                EndStateType.UnawareSurvivor => "無知な生存者",
                // Night09
                EndStateType.FullAlliance => "完全な同盟",
                EndStateType.ActiveAlliance => "積極的な協力",
                EndStateType.PassiveTruth => "真実を知った沈黙",
                EndStateType.WhistleblowerSaved => "告発者を救った",
                EndStateType.WhistleblowerEndangered => "告発者の危機",
                EndStateType.MisakiDiscovered => "美咲の存在を知った",
                EndStateType.TruthRevealed => "真実が明らかに",
                EndStateType.UncertainFuture => "不確かな未来",
                // Night10
                EndStateType.TruthDawn => "真実の夜明け",
                EndStateType.InvestigationContinues => "調査は続く",
                EndStateType.IntoDarkness => "闇の中へ",
                EndStateType.UncertainDawn => "不確かな夜明け",
                _ => endState.ToString()
            };
        }

        /// <summary>
        /// ルート名を取得
        /// </summary>
        private string GetRouteName(EndStateType? endState)
        {
            if (endState == null) return "未確定";

            // エンドステートに基づいてルート名を返す（Good/Neutral/Bad categorization）
            return endState switch
            {
                // Good endings
                EndStateType.Exposed or EndStateType.TruthDawn or EndStateType.VoiceReached or
                EndStateType.WitnessConnected or EndStateType.Connected or EndStateType.Crossroads or
                EndStateType.Intervention or EndStateType.StormPrepared or EndStateType.MisakiProtected or
                EndStateType.TruthSeeker or EndStateType.FullAlliance or EndStateType.ActiveAlliance or
                EndStateType.WhistleblowerSaved or EndStateType.TruthRevealed => "最善ルート",

                // Bad endings
                EndStateType.Complicit or EndStateType.Absorbed or EndStateType.Isolated or
                EndStateType.Silence or EndStateType.Neither or EndStateType.VoiceLost or
                EndStateType.StormUnaware or EndStateType.MisakiTaken or EndStateType.CollapseWitnessed or
                EndStateType.UnawareSurvivor or EndStateType.WhistleblowerEndangered or
                EndStateType.IntoDarkness => "暗黒ルート",

                // Neutral/default
                _ => "中間ルート"
            };
        }

        /// <summary>
        /// チャプターを選択
        /// </summary>
        private void ExecuteSelectChapter(string? chapterId)
        {
            if (string.IsNullOrEmpty(chapterId)) return;

            var chapter = _progressSummary.chapters.Find(c => c.chapterId == chapterId);
            if (chapter != null)
            {
                SelectedChapter = chapter;
                Debug.Log($"[ChapterSelectViewModel] チャプター選択: {chapter.title}");
            }
        }

        /// <summary>
        /// チャプターを開始
        /// </summary>
        private void ExecuteStartChapter()
        {
            if (_selectedChapter == null || !CanStartChapter) return;

            Debug.Log($"[ChapterSelectViewModel] チャプター開始: {_selectedChapter.title}");
            OnChapterStartRequested?.Invoke(_selectedChapter.nightIndex);
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        private void ExecuteBack()
        {
            Debug.Log("[ChapterSelectViewModel] メインメニューに戻る");
            OnBackToMenuRequested?.Invoke();
        }

        /// <summary>
        /// 開始可能かを更新
        /// クリア済みの夜も再プレイ可能（フラグ収集のため）
        /// </summary>
        private void UpdateCanStartChapter()
        {
            CanStartChapter = _selectedChapter != null &&
                (_selectedChapter.state == ChapterState.Available ||
                 _selectedChapter.state == ChapterState.InProgress ||
                 _selectedChapter.state == ChapterState.Completed);
            StartChapterCommand.RaiseCanExecuteChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnChapterStartRequested = null;
                OnBackToMenuRequested = null;
            }
            base.Dispose(disposing);
        }
    }
}
