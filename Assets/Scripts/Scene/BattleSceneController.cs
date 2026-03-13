using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneController : MonoBehaviour
{
    [SerializeField] string editorSceneName = "EditorScene";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            BackToEditor();
        }
    }
    
    public void BackToEditor()
    {
        SceneManager.LoadScene(editorSceneName);
    }
}