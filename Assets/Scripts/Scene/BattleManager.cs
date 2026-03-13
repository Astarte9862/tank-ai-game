using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] string editorSceneName = "EditorScene";
    [SerializeField] bool returnToEditorOnFinish = false;
    [SerializeField] float returnDelay = 2f;

    [Header("Debug")]
    [SerializeField] bool logAliveCounts = false;

    bool battleFinished;

    void Update()
    {
        if (battleFinished)
            return;

        CheckBattleState();
    }

    void CheckBattleState()
    {
        if (TankRegistry.Instance == null)
            return;

        int aliveA = 0;
        int aliveB = 0;

        foreach (var agent in TankRegistry.Instance.Agents)
        {
            if (agent == null)
                continue;

            if (!agent.gameObject.activeInHierarchy)
                continue;

            TankTeam team = agent.GetComponent<TankTeam>();
            TankHealth health = agent.GetComponent<TankHealth>();

            if (team == null || health == null)
                continue;

            if (health.IsDead)
                continue;

            if (team.Team == TeamType.A)
                aliveA++;
            else if (team.Team == TeamType.B)
                aliveB++;
        }

        if (logAliveCounts)
        {
            Debug.Log($"Alive  A:{aliveA}  B:{aliveB}");
        }

        if (aliveA <= 0 && aliveB <= 0)
        {
            FinishBattle("Draw");
            return;
        }

        if (aliveA <= 0)
        {
            FinishBattle("Team B Win");
            return;
        }

        if (aliveB <= 0)
        {
            FinishBattle("Team A Win");
            return;
        }
    }

    void FinishBattle(string result)
    {
        battleFinished = true;
        Debug.Log($"Battle Finished: {result}");

        if (returnToEditorOnFinish)
        {
            StartCoroutine(ReturnToEditorCoroutine());
        }
    }

    IEnumerator ReturnToEditorCoroutine()
    {
        yield return new WaitForSeconds(returnDelay);
        SceneManager.LoadScene(editorSceneName);
    }
}