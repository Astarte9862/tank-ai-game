using UnityEngine;

public class RuntimeAIContext
{
    public float moveValue;
    public float turn;
    public float turretTurn;
    public bool fire;

    public bool enemyAhead;
    public bool wallAhead;
    public bool turretAimed;

    public void ClearFrameCommands()
    {
        moveValue = 0f;
        turn = 0f;
        turretTurn = 0f;
        fire = false;
    }

    public void StopAllMotion()
    {
        moveValue = 0f;
        turn = 0f;
        turretTurn = 0f;
    }
}