using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToWorldScene : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string worldSceneName = "GameScene";

    public void LoadWorldScene()
    {
        SceneManager.LoadScene(worldSceneName);
    }
}