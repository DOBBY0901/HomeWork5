using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MiniGameSceneLoader : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string battleshipSceneName = "BattleshipScene";

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TMP_Text interactionText;

    [Header("Message")]
    [SerializeField] private string interactMessage = "배틀쉽 시작 [E]";

    private bool playerInRange;

    private void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.eKey.wasPressedThisFrame)
        {
            LoadBattleshipScene();
        }
    }

    private void LoadBattleshipScene()
    {
        SceneManager.LoadScene(battleshipSceneName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (interactionUI != null)
            interactionUI.SetActive(true);

        if (interactionText != null)
            interactionText.text = interactMessage;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
}