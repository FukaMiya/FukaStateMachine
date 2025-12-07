using UnityEngine;
using FukaMiya.Utils;

public sealed class TitleState : State
{
    protected override void OnEnter()
    {
        Debug.Log("Entered Title State");
    }
}

public sealed class InGameState : State
{
    private readonly int initialScore;
    public int Score { get; private set; }

    public InGameState(int initialScore)
    {
        this.initialScore = initialScore;
    }

    protected override void OnEnter()
    {
        Score = initialScore;
        Debug.Log("Entered InGame State");
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Score += 10;
            Debug.Log($"Score increased to {Score}");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Score -= 10;
            Debug.Log($"Score decreased to {Score}");
        }
    }
}

public sealed class ResultState : State<int>
{
    protected override void OnEnter()
    {
        Debug.Log($"Entered Result State with Score: {Context}");
    }
}

public sealed class SettingState : State
{
    protected override void OnEnter()
    {
        Debug.Log("Entered Setting State");
    }
}

public sealed class SecretState : State
{
    protected override void OnEnter()
    {
        Debug.Log($"Entered Secret State");
    }
}