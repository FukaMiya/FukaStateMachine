using UnityEngine;
using FukaMiya.Utils;

public class GameEntryPoint : MonoBehaviour
{
    private StateMachine stateMachine;
    public int InitialScore = 100;

    void Start()
    {
        var factory = new StateFactory();
        factory.Register<TitleState>(() => new TitleState());
        factory.Register<InGameState>(() => new InGameState(InitialScore));
        factory.Register<ResultState>(() => new ResultState());
        factory.Register<SettingState>(() => new SettingState());
        factory.Register<SecretState>(() => new SecretState());
        stateMachine = new StateMachine(factory);

        var titleState = stateMachine.At<TitleState>();
        var inGameState = stateMachine.At<InGameState>() as InGameState;
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

        // 動的コンテキスト付きの状態遷移
        inGameState.To<ResultState, int>(() => inGameState.Score)
            .When(() => Input.GetKeyDown(KeyCode.Return))
            .Build();

        // 動的コンテキスト無しの状態遷移も可能
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