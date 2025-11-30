### ステートの定義
各ステートは`State`クラスを継承します。
```csharp
using UnityEngine;
using FukaMiya.Utils;

public class IdleState : State
{
    public override void OnEnter()
    {
        Debug.Log("Idle開始");
    }

    public override void OnUpdate()
    {
        // 毎フレーム呼ばれます
    }

    public override void OnExit()
    {
        Debug.Log("Idle終了");
    }
}

public class WalkState : State { }
public class JumpState : State { }
```

### ステートマシンのセットアップ
エントリポイント内でステートマシンを初期化します。
```csharp
// ステートマシンを宣言
StateMachine stateMachine = new StateMachine();

// 各ステートを取得（自動生成されます）
var idle = stateMachine.At<IdleState>();
var walk = stateMachine.At<WalkState>();
var jump = stateMachine.At<JumpState>();

// シンプルな遷移: 入力があったら Idle -> Walk
idle.To<WalkState>()
    .When(() => Input.GetAxis("Horizontal") != 0)
    .Build();

// 入力がなくなったら Walk -> Idle
walk.To<IdleState>()
    .When(() => Input.GetAxis("Horizontal") == 0)
    .Build();

// どのステートからでもジャンプ（AnyState）
stateMachine.AnyState.To<JumpState>()
    .When(() => Input.GetKeyDown(KeyCode.Space))
    .Build();

// 初期ステートの設定
stateMachine.SetInitialState<IdleState>();

// ステートマシンを更新
stateMachine.Update();
```
ステートの遷移は`idle.To<WalkState>()`のように書くことができますが、同様に`idle.To(walk)`のように直接渡すこともできます。

### 遷移条件の設定
`And`, `Or` メソッドや、`Condition` ヘルパーを使って条件を組み合わせることができます。
```csharp
// 「地面にいて」かつ「(移動入力がある OR ブースト中)」なら RunState へ
idle.To<RunState>()
    .When(() => IsGrounded)
    .And(Condition.Any(
        () => Input.GetButton("Horizontal"),
        () => IsBoostActive
    ))
    .Build();
```

`AnyState`を用いて任意条件でどのステートからでも遷移することができます。同一ステートへの遷移はデフォルトでは許可されていません。
```csharp
// 既にダメージ状態であっても、さらにダメージを受けたら入り直す
stateMachine.AnyState.To<DamageState>()
    .When(() => IsDamageReceived)
    .SetAllowReentry(true) // 再入を許可
    .Build();
```

`Back()`を用いることで一つ前のステートに戻ることができます。`AnyState`での遷移から復帰する際に使えるでしょう。
```csharp
// Escキーが押されたら、ひとつ前の画面（ステート）に戻る
settingsState.Back()
    .When(() => Input.GetKeyDown(KeyCode.Escape))
    .Build();
```

### 遷移条件の優先順位
複数の条件が同じタイミングで満たされた時、
1. Weightがより高い
2. 先に登録されている

遷移条件が優先されます。
```csharp
// 優先度1: 攻撃ボタンが押されていたらその場で攻撃する
idle.To<AttackState>()
	.When(() => Input.GetButton("Attack"))
	.SetWeight(2.0f) // Weightはデフォルトで1.0f
	.Build();

// 優先度2: Runボタンが押されていたら走る
idle.To<RunState>().When(() => Input.GetButton("Run")).Build();

// 優先度3: そうでなくて、移動入力があれば歩く
idle.To<WalkState>().When(() => Input.GetButton("Horizontal")).Build();
```

### 状態遷移図の出力
ステートマシンに登録されている状態遷移一覧をマーメイド記法で出力することができます。
```csharp
Debug.Log(stateMachine.ToMermaidString());
```