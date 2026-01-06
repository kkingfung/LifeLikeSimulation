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

## バージョン情報

- Unity: 6000.x
- 作成日: 2026-01-06
