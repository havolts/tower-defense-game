using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image healthFill;
    public bool friendly = false;

    private Health healthComponent;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        healthComponent = GetComponent<Health>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (healthComponent != null)
        {
            healthComponent.Died += () => Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        if (healthComponent == null) return;

        healthFill.fillAmount = healthComponent.CurrentHealthFraction();
        healthFill.color = new Color(
            friendly ? 0f : 1f,
            friendly ? 1f : 0f,
            0f,
            1f
        );

        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);


        if (friendly)
        {
            canvasGroup.alpha = IsMouseOver() ? 1f : 0f;
        }
        else
        {
            canvasGroup.alpha = healthComponent.CurrentHealthFraction() < 1f ? 1f : 0f;
        }
    }

    bool IsMouseOver()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider == GetComponent<Collider>();
        }
        return false;
    }
}
