# LifeLike Simulation - Interactive Drama System

## プロジェクト概要 (Project Overview)

実写動画を使用したインタラクティブドラマ/ビジュアルノベルシステム。
ジャンルに依存しない汎用フレームワーク（恋愛、ミステリー、ホラー等に対応可能）。

## 要件 (Requirements)

- **動画再生**: 実写動画のシームレスな再生
- **選択システム**: 通常選択、時限選択、ステータスベース選択
- **分岐構造**: リニア + 複数エンディング
- **関係性追跡**: 複数キャラクター × 複数軸（Love, Trust等）
- **セーブシステム**: オートセーブ
- **UIシステム**: uGUI (Canvas)
- **トランジション**: シーンごとに設定可能

---

## アーキテクチャ (Architecture)

### MVVM + ServiceLocator パターン

```
┌─────────────────────────────────────────────────────────┐
│                      Views (UI)                          │
│  MainMenuView, StorySceneView                            │
└─────────────────────────┬───────────────────────────────┘
                          │ データバインディング
┌─────────────────────────▼───────────────────────────────┐
│                    ViewModels                            │
│  MainMenuViewModel, StorySceneViewModel                  │
└─────────────────────────┬───────────────────────────────┘
                          │ サービス呼び出し
┌─────────────────────────▼───────────────────────────────┐
│                     Services                             │
│  IStoryService, IVideoService, IChoiceService,           │
│  IRelationshipService, ISaveService, ITransitionService  │
└─────────────────────────┬───────────────────────────────┘
                          │ データ読み込み
┌─────────────────────────▼───────────────────────────────┐
│                 Data (ScriptableObjects)                 │
│  StorySceneData, ChoiceData, CharacterData, GameStateData│
└─────────────────────────────────────────────────────────┘
```

---

## ディレクトリ構造 (Directory Structure)

```
Assets/
├── Scenes/
│   ├── Bootstrap.unity          # 初期化シーン（GameBootstrap配置）
│   ├── MainMenu.unity           # メインメニュー
│   └── StoryScene.unity         # ストーリー再生シーン
├── Scripts/
│   ├── Core/
│   │   ├── MVVM/
│   │   │   └── ViewModelBase.cs
│   │   ├── Commands/
│   │   │   └── RelayCommand.cs
│   │   ├── Services/
│   │   │   └── ServiceLocator.cs
│   │   └── GameBootstrap.cs
│   ├── Services/
│   │   ├── Story/
│   │   │   ├── IStoryService.cs
│   │   │   └── StoryService.cs
│   │   ├── Video/
│   │   │   ├── IVideoService.cs
│   │   │   └── VideoService.cs
│   │   ├── Choice/
│   │   │   ├── IChoiceService.cs
│   │   │   └── ChoiceService.cs
│   │   ├── Relationship/
│   │   │   ├── IRelationshipService.cs
│   │   │   └── RelationshipService.cs
│   │   ├── Save/
│   │   │   ├── ISaveService.cs
│   │   │   └── SaveService.cs
│   │   └── Transition/
│   │       ├── ITransitionService.cs
│   │       └── TransitionService.cs
│   ├── Data/
│   │   ├── StorySceneData.cs
│   │   ├── ChoiceData.cs
│   │   ├── CharacterData.cs
│   │   ├── GameStateData.cs
│   │   ├── TransitionSettings.cs
│   │   └── Conditions/
│   │       ├── StoryCondition.cs
│   │       └── StoryEffect.cs
│   ├── ViewModels/
│   │   ├── MainMenuViewModel.cs
│   │   └── StorySceneViewModel.cs
│   └── Views/
│       ├── MainMenuView.cs
│       └── StorySceneView.cs
├── UI/
│   └── Prefabs/
│       └── ChoiceButton.prefab
├── StoryData/
│   ├── Characters/
│   │   └── (CharacterData assets)
│   ├── Scenes/
│   │   └── (StorySceneData assets)
│   └── Variables/
│       └── InitialGameState.asset
└── Videos/
    └── (動画ファイル)
```

---

## コーディング規約 (Coding Conventions)

### 重要: コメントは日本語で記述

```csharp
/// <summary>
/// ストーリーシーン画面のViewModel
/// </summary>
/// <remarks>
/// 動画再生と選択肢表示を管理する
/// </remarks>
public class StorySceneViewModel : ViewModelBase
{
    /// <summary>現在のシーンデータ</summary>
    public StorySceneData? CurrentScene { get; set; }

    /// <summary>
    /// 選択肢を表示する
    /// </summary>
    private void ShowChoices()
    {
        // 選択肢がない場合はデフォルトの次シーンへ
        if (!CurrentScene.HasChoices)
        {
            _storyService.ProceedToNextScene();
        }
    }
}
```

### その他の規約
- `#nullable enable` を使用
- フィールドは `_camelCase`
- プロパティ・メソッドは `PascalCase`
- イベントハンドラは `On...` で始まる
- インターフェースは `I...` で始まる

---

## 主要コンポーネント

### 1. ServiceLocator

```csharp
// サービスの取得
var storyService = ServiceLocator.Instance.Get<IStoryService>();

// サービスの登録
ServiceLocator.Instance.Register<IStoryService>(new StoryService());
```

### 2. GameBootstrap

ゲーム起動時に実行され、すべてのサービスを初期化・登録する。
`DontDestroyOnLoad`でシーン間で維持される。

### 3. StorySceneData (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "LifeLike/Story Scene")]
public class StorySceneData : ScriptableObject
{
    public string sceneId;           // シーンの一意なID
    public VideoReference video;     // 動画参照（ローカル/AB対応）
    public List<ChoiceData> choices; // 選択肢リスト
    public string defaultNextSceneId;// デフォルトの次シーン
    public bool isEnding;            // エンディングかどうか
}
```

### 4. ChoiceData

```csharp
[Serializable]
public class ChoiceData
{
    public string choiceId;
    public string choiceText;
    public ChoiceType choiceType;    // Normal, Timed, StatBased
    public float timeLimit;          // 時限選択の制限時間
    public List<StoryCondition> requirements; // 表示条件
    public List<StoryEffect> effects;         // 選択時の効果
    public string nextSceneId;
}
```

### 5. CharacterData

```csharp
[CreateAssetMenu(menuName = "LifeLike/Character")]
public class CharacterData : ScriptableObject
{
    public string characterId;
    public string characterName;
    public Sprite portrait;
    public List<RelationshipAxis> relationshipAxes; // Love, Trust等
}
```

---

## サービス一覧

### IStoryService
ストーリー進行と変数管理

```csharp
public interface IStoryService
{
    StorySceneData? CurrentScene { get; }
    void StartNewGame(GameStateData data);
    void LoadScene(string sceneId);
    void SetVariable<T>(string name, T value);
    T? GetVariable<T>(string name);
    bool EvaluateCondition(StoryCondition condition);
    void ApplyEffect(StoryEffect effect);
}
```

### IVideoService
動画再生の制御（ローカル/AssetBundle対応）

```csharp
public interface IVideoService
{
    bool IsPlaying { get; }
    bool IsLoading { get; }
    double CurrentTime { get; }
    void Play(VideoClip clip);
    void PlayFromUrl(string url);
    Task<bool> PlayAsync(VideoReference videoReference);
    Task<bool> PreloadAsync(VideoReference videoReference);
    void Pause();
    void Resume();
    void Skip();
}
```

### IChoiceService
選択肢の表示と選択処理

```csharp
public interface IChoiceService
{
    void PresentChoices(IEnumerable<ChoiceData> choices);
    void SelectChoice(ChoiceData choice);
    bool IsChoiceAvailable(ChoiceData choice);
    void StartTimer(float duration);
}
```

### IRelationshipService
キャラクターとの関係性管理

```csharp
public interface IRelationshipService
{
    int GetRelationship(string characterId, string axisId);
    void ModifyRelationship(string characterId, string axisId, int delta);
}
```

### ISaveService
セーブデータ管理

```csharp
public interface ISaveService
{
    bool HasSaveData { get; }
    void AutoSave();
    bool Load();
    void DeleteSave();
}
```

### ITransitionService
画面遷移演出

```csharp
public interface ITransitionService
{
    Task ExecuteTransition(TransitionSettings settings, Action? onMidpoint);
    Task FadeOut(TransitionSettings settings);
    Task FadeIn(TransitionSettings settings);
}
```

### IAssetBundleService
AssetBundleのダウンロードと管理

```csharp
public interface IAssetBundleService
{
    string BaseUrl { get; set; }
    Task<bool> DownloadBundleAsync(string bundleName, uint version = 0);
    Task<VideoClip?> LoadVideoClipAsync(string bundleName, string assetName);
    Task<T?> LoadAssetAsync<T>(string bundleName, string assetName);
    bool IsBundleCached(string bundleName, uint version = 0);
    void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false);
}
```

---

## 動画システム（VideoReference）

動画はローカルファイルとAssetBundleの両方に対応。

### VideoReference

```csharp
[Serializable]
public class VideoReference
{
    public AssetSource source;        // Local, AssetBundle, StreamingAssets
    public VideoClip? localClip;      // ローカルの動画（Localの場合）
    public string streamingAssetsPath;// StreamingAssetsパス
    public string bundleName;         // AssetBundle名
    public string assetName;          // バンドル内のアセット名
    public uint bundleVersion;        // バージョン（キャッシュ用）
    public string directUrl;          // 直接URL
}
```

### AssetSource（ロード元）

```csharp
public enum AssetSource
{
    Local,          // ローカルファイル（直接参照）
    AssetBundle,    // AssetBundleからダウンロード
    StreamingAssets // StreamingAssetsフォルダ
}
```

### 使用例

```csharp
// ローカル動画を使用
var localRef = new VideoReference
{
    source = AssetSource.Local,
    localClip = myVideoClip
};

// AssetBundleから動画をロード
var abRef = new VideoReference
{
    source = AssetSource.AssetBundle,
    bundleName = "videos_chapter1",
    assetName = "scene_001",
    bundleVersion = 1
};

// VideoServiceで再生（非同期）
await _videoService.PlayAsync(videoReference);

// プリロード（事前ダウンロード）
await _videoService.PreloadAsync(videoReference);
```

---

## シーン構成

### Bootstrap シーン
- `GameBootstrap` を配置
- サービスの初期化
- MainMenuシーンへ自動遷移

### MainMenu シーン
- `MainMenuView` を配置
- 新規ゲーム/コンティニュー/設定/終了

### StoryScene シーン
- `StorySceneView` を配置
- VideoPlayerコンポーネント
- 選択肢UI（動的生成）
- タイマーUI

---

## ストーリーデータの作成

### 1. キャラクターを作成

```
Assets/StoryData/Characters/ で右クリック
→ Create → LifeLike → Character Data
```

設定項目:
- characterId: 一意なID
- characterName: 表示名
- relationshipAxes: 関係性軸（Love, Trust等）

### 2. ストーリーシーンを作成

```
Assets/StoryData/Scenes/ で右クリック
→ Create → LifeLike → Story Scene
```

設定項目:
- sceneId: 一意なID
- video: 動画参照（VideoReference）
- choices: 選択肢リスト
- defaultNextSceneId: デフォルト遷移先

### 3. ゲーム状態データを作成

```
Assets/StoryData/Variables/ で右クリック
→ Create → LifeLike → Game State Data
```

設定項目:
- startSceneId: 最初のシーン
- characters: 登場キャラクター
- allScenes: すべてのシーン
- initialVariables: 初期変数

---

## トランジション設定

```csharp
public enum TransitionType
{
    None,           // 即時切り替え
    FadeToBlack,    // 黒フェード
    FadeToWhite,    // 白フェード
    Crossfade,      // クロスフェード
    Custom          // カスタム
}

[Serializable]
public class TransitionSettings
{
    public TransitionType type;
    public float duration;
    public AnimationCurve curve;
}
```

---

## 条件と効果

### StoryCondition（条件）

選択肢の表示条件やシーン分岐に使用

```csharp
// 例: Love >= 50 の場合に表示
condition.variableName = "char_sakura_love";
condition.variableType = VariableType.Integer;
condition.comparisonOperator = ComparisonOperator.GreaterThanOrEqual;
condition.intValue = 50;
```

### StoryEffect（効果）

選択時に変数を変更

```csharp
// 例: Love +10
effect.variableName = "char_sakura_love";
effect.variableType = VariableType.Integer;
effect.operation = EffectOperation.Add;
effect.intValue = 10;
```

---

## よくあるパターン

### ViewからViewModelへのバインディング

```csharp
private void Start()
{
    _viewModel.PropertyChanged += OnViewModelPropertyChanged;
}

private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    switch (e.PropertyName)
    {
        case nameof(StorySceneViewModel.IsShowingChoices):
            UpdateChoicesUI();
            break;
    }
}
```

### シーン遷移

```csharp
// ViewModelから
OnGameStartRequested?.Invoke();

// Viewで
private void OnGameStartRequested()
{
    SceneManager.LoadScene("StoryScene");
}
```

---

## セットアップ手順

1. **Bootstrapシーンを作成**
   - 空のGameObjectに`GameBootstrap`をアタッチ
   - `GameStateData`を設定
   - Build Settingsで最初のシーンに設定

2. **MainMenuシーンを作成**
   - Canvas配下にボタンを配置
   - `MainMenuView`をアタッチ
   - ボタンを参照設定

3. **StorySceneシーンを作成**
   - VideoPlayerを配置
   - `StorySceneView`をアタッチ
   - 選択肢ボタンのプレハブを作成

4. **ストーリーデータを作成**
   - キャラクター、シーン、ゲーム状態を作成
   - 動画ファイルを配置

---

---

## Operator Mode（オペレーターモード）

### コンセプト: "Operator: Night Signal"

夜間緊急オペレーターとして電話を受ける。
通話は断片的で、偏りがあり、時には虚偽。
真実は語られるのではなく、**浮かび上がる**。

### Core Pillars

1. **One Operator, Many Calls, One Truth**
   - プレイヤーはデスクから離れない
   - 世界は声だけで構築される
   - 通話は不完全、偏見あり、時に虚偽

2. **"Ridiculous → Serious" Narrative Curve**
   - 最初は馬鹿げた通話（騒音苦情、いたずら）
   - 徐々に繋がりが見えてくる
   - 「これらはランダムじゃなかった」という気づき

3. **Lying Is a Feature**
   - オペレーターは嘘をつける
   - 良い/悪いのラベルなし、結果のみ

### Operator Mode Data Structures

#### CallerData（発信者）

```csharp
[CreateAssetMenu(menuName = "LifeLike/Operator/Caller Data")]
public class CallerData : ScriptableObject
{
    public string callerId;
    public string displayName;       // 最初は「不明」の場合も
    public string realName;          // 判明後の本名
    public Sprite silhouetteImage;   // フェーズ1用
    public Sprite revealedImage;     // 判明後
    public CallerPersonality personality; // 正直さ、安定性、協力性、攻撃性
    public List<CallerRelation> relations; // 他の発信者との関係
    public string hiddenInfo;        // 隠している情報
    public string trueMotivation;    // 本当の目的
}
```

#### CallData（通話）

```csharp
[CreateAssetMenu(menuName = "LifeLike/Operator/Call Data")]
public class CallData : ScriptableObject
{
    public string callId;
    public CallerData caller;
    public int incomingTimeMinutes;  // 着信時刻
    public float ringDuration;       // 着信持続時間
    public int priority;             // 優先度
    public string startSegmentId;
    public List<CallSegment> segments;
    public List<StoryEffect> onEndEffects;   // 終了時の効果
    public List<StoryEffect> onMissedEffects; // 不在着信時の効果
    public bool isCritical;          // スキップ不可
}
```

#### CallSegment（通話セグメント）

```csharp
[Serializable]
public class CallSegment
{
    public string segmentId;
    public CallMediaReference media; // シルエット/音声/動画
    public List<ResponseData> responses; // プレイヤーの応答選択肢
    public float responseTimeLimit;  // 応答制限時間
    public string timeoutResponseId; // タイムアウト時のデフォルト
    public List<string> autoDiscoveredEvidenceIds; // 自動発見証拠
}
```

#### ResponseData（応答）

```csharp
[Serializable]
public class ResponseData
{
    public string responseId;
    public string displayText;       // 表示テキスト
    public string actualText;        // 実際の発言
    public bool isSilence;           // 沈黙か
    public bool isLie;               // 嘘か
    public bool presentsEvidence;    // 証拠を提示するか
    public string evidenceIdToPresent;
    public List<StoryCondition> conditions;
    public List<string> requiredEvidenceIds;
    public List<StoryEffect> effects;
    public int trustImpact;          // 信頼度への影響
    public string nextSegmentId;
    public bool endsCall;
    public bool discoversEvidence;
    public string discoveredEvidenceId;
}
```

#### EvidenceData（証拠）

```csharp
[Serializable]
public class EvidenceData
{
    public string evidenceId;
    public EvidenceType evidenceType; // Statement, Timestamp, Location, Contradiction, Silence等
    public string content;
    public string sourceCallerId;
    public string sourceCallId;
    public EvidenceReliability reliability; // Unverified, Verified, Disproven等
    public bool isActuallyTrue;      // 真実かどうか（内部フラグ）
    public List<string> relatedCallerIds;
    public List<string> contradictingEvidenceIds;
    public bool isDiscovered;
    public bool isUsable;
}
```

**重要**: 証拠は「真実」ではなく「使用可能」。それが深みを生む。

#### TrustEdge（信頼関係）

```csharp
[Serializable]
public class TrustEdge
{
    public string fromId;
    public string toId;
    public TrustTargetType targetType; // Operator, OtherCaller, Assumption
    public int trustValue;           // -100 ~ 100
    public TrustLevel trustLevel;    // Hostile ~ Devoted
    public List<string> trustHistory;
}
```

#### NightScenarioData（一夜のシナリオ）

```csharp
[CreateAssetMenu(menuName = "LifeLike/Operator/Night Scenario")]
public class NightScenarioData : ScriptableObject
{
    public string scenarioId;
    public string title;
    public int startTimeMinutes;     // 例: 22:00 = 1320
    public int endTimeMinutes;       // 例: 06:00 = 360（翌日）
    public float realSecondsPerGameMinute;
    public List<CallerData> callers;
    public List<CallData> calls;
    public List<EvidenceTemplate> evidenceTemplates;
    public List<WorldStateSnapshot> initialWorldStates;
    public string theTruth;          // シナリオの真実
    public List<ScenarioEnding> endings;
}
```

### Operator Mode Services

#### IEvidenceService
証拠の発見・使用・管理

```csharp
public interface IEvidenceService
{
    IReadOnlyList<EvidenceData> DiscoveredEvidence { get; }
    event Action<EvidenceData> OnEvidenceDiscovered;
    event Action<EvidenceData, EvidenceData> OnContradictionFound;

    bool DiscoverEvidence(string evidenceId);
    EvidenceData CreateStatementEvidence(string sourceCallerId, string content, bool isTrue);
    bool UseEvidence(string evidenceId);
    void UpdateReliability(string evidenceId, EvidenceReliability reliability);
    bool CheckContradiction(string evidenceId1, string evidenceId2);
}
```

#### ITrustGraphService
信頼関係の管理

```csharp
public interface ITrustGraphService
{
    event Action<string, string, int, TrustLevel> OnTrustChanged;
    event Action<CallerAssumption> OnAssumptionDisproven;

    int GetOperatorTrust(string callerId);
    TrustLevel GetOperatorTrustLevel(string callerId);
    void ModifyOperatorTrust(string callerId, int delta, string reason);
    void ModifyCallerTrust(string fromId, string toId, int delta, string reason);
    IReadOnlyList<CallerAssumption> GetAssumptions(string callerId);
    void DisproveAssumption(string assumptionId);
}
```

#### ICallFlowService
通話フローの管理

```csharp
public interface ICallFlowService
{
    CallData? CurrentCall { get; }
    CallSegment? CurrentSegment { get; }
    IReadOnlyList<CallData> IncomingCalls { get; }

    event Action<CallData> OnIncomingCall;
    event Action<CallData> OnCallStarted;
    event Action<CallSegment> OnSegmentChanged;
    event Action<IReadOnlyList<ResponseData>> OnResponsesPresented;
    event Action<CallData, CallState> OnCallEnded;
    event Action<CallData> OnCallMissed;

    void LoadScenario(NightScenarioData scenario);
    bool AnswerCall(string callId);
    bool HoldCall();
    void EndCall();
    void SelectResponse(string responseId);
    void SelectSilence();
}
```

#### IWorldStateService
世界状態と時間の管理

```csharp
public interface IWorldStateService
{
    int CurrentTimeMinutes { get; }
    string FormattedTime { get; }
    bool IsScenarioEnded { get; }

    event Action<int, string> OnTimeChanged;
    event Action<WorldStateSnapshot> OnWorldStateChanged;
    event Action<ScenarioEnding> OnScenarioEnded;
    event Action<CallData> OnCallTriggered;

    void LoadScenario(NightScenarioData scenario);
    void UpdateTime(float deltaTime);
    void AddWorldState(WorldStateSnapshot state);
    void RevealStateToPlayer(string stateId);
    ScenarioEnding? CheckEndingConditions();
}
```

### 段階的開発

**フェーズ1**: シルエット画像 + テキスト
**フェーズ2**: 音声追加
**フェーズ3**: 動画追加

```csharp
public enum CallMediaType
{
    SilhouetteText,  // フェーズ1
    SilhouetteVoice, // フェーズ2
    Video            // フェーズ3
}
```

### Ending Types

```csharp
public enum EndingType
{
    TruthRevealed,      // 真相解明
    DamageMinimized,    // 被害最小化
    SomeoneSaved,       // 誰かを救った
    SomeoneAbandoned,   // 誰かを見捨てた
    BecameAccomplice,   // 共犯者になった
    EveryoneSaved,      // 全員を救った
    NooneSaved,         // 誰も救えなかった
    Neutral,            // 中立/曖昧
    CoverUpSucceeded,   // 隠蔽成功
    JusticeServed       // 正義執行
}
```

「成功」エンディングは悲劇的かもしれない。
「失敗」は道徳的に正しいかもしれない。
その曖昧さがブランド。

---

## ディレクトリ構造（更新版）

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── MVVM/
│   │   ├── Commands/
│   │   ├── Services/
│   │   └── GameBootstrap.cs
│   ├── Services/
│   │   ├── Story/          # 既存
│   │   ├── Video/          # 既存
│   │   ├── Choice/         # 既存
│   │   ├── Relationship/   # 既存
│   │   ├── Save/           # 既存
│   │   ├── Transition/     # 既存
│   │   ├── AssetBundle/    # 既存
│   │   ├── Evidence/       # NEW: 証拠システム
│   │   ├── TrustGraph/     # NEW: 信頼グラフ
│   │   ├── CallFlow/       # NEW: 通話フロー
│   │   └── WorldState/     # NEW: 世界状態
│   ├── Data/
│   │   ├── StorySceneData.cs
│   │   ├── ChoiceData.cs
│   │   ├── CharacterData.cs
│   │   ├── CallerData.cs       # NEW
│   │   ├── CallData.cs         # NEW
│   │   ├── CallMediaReference.cs # NEW
│   │   ├── EvidenceData.cs     # NEW
│   │   ├── TrustData.cs        # NEW
│   │   └── NightScenarioData.cs # NEW
│   ├── ViewModels/
│   │   ├── MainMenuViewModel.cs
│   │   ├── StorySceneViewModel.cs
│   │   └── OperatorViewModel.cs # NEW
│   └── Views/
│       ├── MainMenuView.cs
│       ├── StorySceneView.cs
│       └── OperatorView.cs     # TODO
├── StoryData/
│   └── Operator/               # NEW: オペレーターモード用
│       ├── Scenarios/
│       ├── Callers/
│       ├── Calls/
│       └── Evidence/
└── ...
```

---

## バージョン情報

- Unity: 6000.x
- 作成日: 2026-01-06
- 更新日: 2026-01-07 (Operator Mode追加)
