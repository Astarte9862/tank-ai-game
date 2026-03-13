using UnityEngine;
using TMPro;

public class BattleJudge : MonoBehaviour
{
    [SerializeField] int minimumAgentsToStart = 2;
    [SerializeField] TMP_Text resultText;

    bool battleStarted;
    bool battleEnded;

    void Update()
    {
        if (battleEnded)
            return;

        if (TankRegistry.Instance == null)
            return;

        int registeredCount = 0;
        foreach (var agent in TankRegistry.Instance.Agents)
        {
            if (agent != null)
                registeredCount++;
        }

        if (!battleStarted)
        {
            if (registeredCount < minimumAgentsToStart)
                return;

            battleStarted = true;
        }

        bool teamAAlive = false;
        bool teamBAlive = false;

        foreach (var agent in TankRegistry.Instance.Agents)
        {
            if (agent == null)
                continue;

            TankHealth health = agent.GetComponent<TankHealth>();
            if (health == null || health.IsDead)
                continue;

            TankTeam team = agent.GetComponent<TankTeam>();
            if (team == null)
                continue;

            if (team.Team == TeamType.A)
                teamAAlive = true;
            else if (team.Team == TeamType.B)
                teamBAlive = true;
        }

        if (!teamAAlive && !teamBAlive)
        {
            EndBattle("DRAW");
            return;
        }

        if (!teamAAlive)
        {
            EndBattle("TEAM B WINS");
            return;
        }

        if (!teamBAlive)
        {
            EndBattle("TEAM A WINS");
            return;
        }
    }

    void EndBattle(string result)
    {
        battleEnded = true;

        Debug.Log(result);

        if (resultText != null)
            resultText.text = result;
    }
}