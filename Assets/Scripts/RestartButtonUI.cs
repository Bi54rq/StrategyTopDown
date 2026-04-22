using UnityEngine;

public class RestartButtonUI : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject targetButton;

    private void Update()
    {
        if (gameManager == null || targetButton == null) return;

        targetButton.SetActive(gameManager.Phase == GamePhase.Combat);
    }
}