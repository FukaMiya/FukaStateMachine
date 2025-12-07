using UnityEngine;
using FukaMiya.Utils;

public class GameEntryPoint : MonoBehaviour
{
    private StateMachine stateMachine;
    public int Score = 0;

    void Start()
    {
        stateMachine = new StateMachine(
            new StateFactory((type) =>
            {
                return type switch
                {
                    _ when type == typeof(TitleState) => new TitleState(),
                    _ when type == typeof(InGameState) => new InGameState(),
                    _ when type == typeof(ResultState) => new ResultState(),
                    _ when type == typeof(SettingState) => new SettingState(),
                    _ when type == typeof(SecretState) => new SecretState(),
                    _ => throw new System.InvalidOperationException($"Unknown state type: {type}")
                };
            })
        );
        var titleState = stateMachine.At<TitleState>();
        var inGameState = stateMachine.At<InGameState>();
        var resultState = stateMachine.At<ResultState>();
        var settingState = stateMachine.At<SettingState>();

        // Whenに直接条件を渡す
        titleState.To<InGameState>()
            .When(Condition.Any(
                () => Input.GetKeyDown(KeyCode.Return),
                () => Input.GetMouseButtonDown(0)))
            .Build();

        // AnyStateからの遷移
        stateMachine.AnyState
            .To<SettingState>()
            .When(() => Input.GetKeyDown(KeyCode.Escape))
            .SetAllowReentry(false) //明示的に同じステートへの遷移を禁止
            .Build();

        // 元いた状態に戻る
        settingState.Back()
            .When(() => Input.GetKeyDown(KeyCode.Backspace))
            .Build();

        // 複雑な条件
        inGameState.To<SecretState>()
            .When(Condition.All(
                Condition.Any(
                    () => Input.GetKey(KeyCode.LeftShift),
                    () => Input.GetKey(KeyCode.RightShift)
                ),
                () => Input.GetKey(KeyCode.Alpha1),
                () => Input.GetKey(KeyCode.Alpha2),
                () => Input.GetKeyDown(KeyCode.Alpha3)))
            .Build();

        // コンテキスト付きの状態遷移
        inGameState.To<ResultState, int>(() => Score)
            .When(() => Input.GetKeyDown(KeyCode.Return))
            .Build();

        // コンテキスト無しの状態遷移も可能
        inGameState.To<ResultState>()
            .When(() => Input.GetKeyDown(KeyCode.Space))
            .Build();

        resultState.To<TitleState>()
            .When(() => Input.GetKeyDown(KeyCode.Return))
            .Build();

        settingState.To<TitleState>()
            .When(() => Input.GetKeyDown(KeyCode.Return))
            .Build();

        stateMachine.SetInitialState<TitleState>();

        // マーメイド記法で状態遷移図を出力
        Debug.Log(stateMachine.ToMermaidString());
    }

    void Update()
    {
        stateMachine.Update();
    }
}