using UnityEngine;
using FukaMiya.Utils;

public class GameEntryPoint : MonoBehaviour
{
    private StateMachine stateMachine;

    void Start()
    {
        stateMachine = new StateMachine();
        var titleState = stateMachine.At<TitleState>();
        var inGameState = stateMachine.At<InGameState>();
        var mainMenuState = stateMachine.At<MainMenuState>();

        titleState.To<InGameState>()
            .When(Condition.Any(
                () => Input.GetKeyDown(KeyCode.Return),
                () => Input.GetKeyDown(KeyCode.Space)))
            .Build();

        inGameState.To<MainMenuState>()
            .When(() => Input.GetKeyDown(KeyCode.Escape))
            .Build();

        mainMenuState.To<InGameState>()
            .When(() => Input.GetKeyDown(KeyCode.Escape))
            .Build();

        mainMenuState.To<TitleState>()
            .When(() => Input.GetKeyDown(KeyCode.Return))
            .Build();

        stateMachine.SetInitialState<TitleState>();
    }

    void Update()
    {
        stateMachine.Update();
    }
}
