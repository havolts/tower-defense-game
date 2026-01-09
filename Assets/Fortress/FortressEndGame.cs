using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortressEndGame : MonoBehaviour
{
    public GameObject panel;
    public GameObject endGame;
    public CameraController cam;

    void EndGame()
    {
        panel.SetActive(false);
        GameController.Instance.StopAllUnits();
        cam.frozen = true;
        endGame.SetActive(true);

    }
}
