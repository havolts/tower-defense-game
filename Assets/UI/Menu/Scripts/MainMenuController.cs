using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject instructionsPanel;

    [Header("Scene to load")]
    [SerializeField] private string gameSceneName = "Level0"; 

    private void Start()
    {
        ShowMain();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowMain()
    {
        if (mainPanel) mainPanel.SetActive(true);
        if (instructionsPanel) instructionsPanel.SetActive(false);
    }

    public void ShowInstructions()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (instructionsPanel) instructionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

