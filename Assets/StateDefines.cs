using UnityEngine;
using FukaMiya.Utils;

public class TitleState : State
{
    public override void OnEnter()
    {
        Debug.Log("Entered Title State");
    }
}

public class InGameState : State
{
    public override void OnEnter()
    {
        Debug.Log("Entered InGame State");
    }
}

public class MainMenuState : State
{
    public override void OnEnter()
    {
        Debug.Log("Entered MainMenu State");
    }
}