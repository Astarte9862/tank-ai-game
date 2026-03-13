using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorSceneController : MonoBehaviour
{
    [SerializeField] NodeEditorManager editorManager;
    [SerializeField] string battleSceneName = "BattleScene";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            StartBattle();
        }
    }

    public void StartBattle()
    {
        if (editorManager == null)
        {
            Debug.LogWarning("EditorSceneController: editorManager is null.");
            return;
        }

        editorManager.SaveGraph("PlayerAI.json");

        Debug.Log("Loading battle scene...");
        SceneManager.LoadScene(battleSceneName);
    }
}