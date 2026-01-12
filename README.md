# LifeLike Simulation

実写動画を使用したインタラクティブドラマゲームを作成するためのUnityフレームワーク。分岐するストーリー、複数のエンディング、様々なジャンル（恋愛、ミステリー、ホラーなど）に対応。

<img src="/Result.PNG" width="600">

## 機能

- **動画再生**: 実写動画クリップのシームレスな再生
- **選択システム**: 通常選択、時限選択、ステータスベース選択
- **分岐ストーリー**: リニアなストーリーと複数エンディング
- **キャラクター関係性**: 複数軸での関係性追跡（Love, Trust, Friendshipなど）
- **AssetBundle対応**: サーバーからのダウンロードまたはローカル読み込み
- **オートセーブ**: 自動セーブシステム
- **トランジション設定**: 黒フェード、白フェード、クロスフェードなど

## 動作環境

- Unity 6000.x (Unity 6)
- Videoモジュール有効

## プロジェクト構成

```
Assets/
├── Scripts/
│   ├── Core/           # MVVM基底クラス、ServiceLocator、Bootstrap
│   ├── Services/       # Story, Video, Choice, Relationship, Save, Transition, AssetBundle
│   ├── Data/           # ScriptableObject（StoryScene, Character, Choiceなど）
│   ├── ViewModels/     # MainMenu, StoryScene ViewModel
│   └── Views/          # uGUI View
├── Scenes/             # Bootstrap, MainMenu, StoryScene
├── StoryData/          # ストーリーコンテンツ（ScriptableObject）
├── UI/                 # プレハブ
└── Videos/             # 動画ファイル（またはAssetBundle使用）
```

## クイックスタート

### 1. Bootstrapシーンのセットアップ

1. `Bootstrap`という名前の新しいシーンを作成
2. 空のGameObjectに`GameBootstrap`コンポーネントを追加
3. `GameStateData`アセットを作成して割り当て
4. Build Settingsで`Bootstrap`を最初のシーンに設定

### 2. ストーリーコンテンツの作成

**キャラクターを作成:**
```
Projectで右クリック → Create → LifeLike → Character Data
```

**ストーリーシーンを作成:**
```
Projectで右クリック → Create → LifeLike → Story Scene
```

**ゲーム状態を作成:**
```
Projectで右クリック → Create → LifeLike → Game State Data
```

### 3. 動画ソースの設定

動画は3つのソースから読み込み可能:

| ソース | 用途 |
|--------|------|
| `Local` | VideoClipを直接参照（開発時） |
| `AssetBundle` | サーバーからダウンロード（本番環境） |
| `StreamingAssets` | ビルド済みローカルファイル |

### 4. ゲームの実行

1. Bootstrapシーンを開く
2. Playボタンを押す
3. 自動的にMainMenuが読み込まれる

## アーキテクチャ

**MVVM + ServiceLocator**パターンを使用:

```
Views (UI) → ViewModels → Services → Data (ScriptableObjects)
```

### サービス一覧

| サービス | 役割 |
|----------|------|
| `IStoryService` | ストーリー進行と変数管理 |
| `IVideoService` | 動画再生（ローカル + AssetBundle） |
| `IChoiceService` | 選択肢の表示と選択処理 |
| `IRelationshipService` | キャラクター関係性の追跡 |
| `ISaveService` | オートセーブ管理 |
| `ITransitionService` | 画面遷移演出 |
| `IAssetBundleService` | AssetBundleのダウンロードとキャッシュ |

## 動画参照（VideoReference）

```csharp
// ローカル動画
video.source = AssetSource.Local;
video.localClip = myClip;

// AssetBundle動画
video.source = AssetSource.AssetBundle;
video.bundleName = "chapter1_videos";
video.assetName = "scene_001";
video.bundleVersion = 1;

// StreamingAssets動画
video.source = AssetSource.StreamingAssets;
video.streamingAssetsPath = "Videos/intro.mp4";
```

## 条件と効果

**条件の例**（Love >= 50 の場合に選択肢を表示）:
```csharp
condition.variableName = "char_sakura_love";
condition.comparisonOperator = ComparisonOperator.GreaterThanOrEqual;
condition.intValue = 50;
```

**効果の例**（Loveに10を加算）:
```csharp
effect.variableName = "char_sakura_love";
effect.operation = EffectOperation.Add;
effect.intValue = 10;
```

## ドキュメント

詳細な技術ドキュメントは [CLAUDE.md](CLAUDE.md) を参照してください。

## ライセンス

[ライセンスをここに記載]
