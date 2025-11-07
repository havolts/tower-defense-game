using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Slider healthSlider3D;
    public bool isFriendly = false;
    public Canvas canvas;
    private Camera mainCamera;

    void Start() {
        mainCamera = Camera.main;
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>();

        if (canvas != null)
            canvas.enabled = false;
    }

    public void Start3DSlider(float maxValue) {
        healthSlider3D.maxValue = maxValue;
        healthSlider3D.value = maxValue;
    }

    public void Update3DSSlider(float value) {
        healthSlider3D.value = value;
        UpdateVisibility();
    }

    void Update() {
        if (canvas == null || mainCamera == null)
            return;

        canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.forward);

        if (isFriendly)
            UpdateVisibility();
    }

    void UpdateVisibility() {
        if (canvas == null)
            return;

        if (!isFriendly) {
            bool isHurt = healthSlider3D.value < healthSlider3D.maxValue;
            canvas.enabled = isHurt;
        }
        else {
            bool hovered = false;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                if (hit.transform.root == transform.root)
                    hovered = true;
            }
            canvas.enabled = hovered;
        }
    }
}

