using UnityEngine;

public class CameraController : MonoBehaviour
{
    float speed = 20f;
    float edge = -10f;
    Camera cam;
    float scrollSpeed = -2f;

    [HideInInspector] public bool frozen = false;
    [HideInInspector] public Transform followTarget;
    public Vector3 offset = new Vector3(0, 10, -10);

    Vector3 velocity = Vector3.zero; // for SmoothDamp

    public GameObject endGame;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (frozen)
        {
            if(followTarget != null)
            {
                // Smooth position
                Vector3 targetPos = followTarget.position + offset;
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 0.15f);

                // Smooth rotation
                Vector3 direction = (followTarget.position - transform.position).normalized;
                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
                }
            }
            else
            {
                endGame.SetActive(true);
                return;
            }
        }

        // Normal free camera movement
        Vector3 pos = transform.position;
        Vector3 mousePos = Input.mousePosition;
        float scroll = Input.mouseScrollDelta.y;

        speed = Input.GetKey(KeyCode.LeftShift) ? 40f : 20f;

        if (mousePos.x <= edge || Input.GetKey("a")) pos += Vector3.left * speed * Time.deltaTime;
        if (mousePos.x >= Screen.width - edge || Input.GetKey("d")) pos += Vector3.right * speed * Time.deltaTime;
        if (mousePos.y <= edge || Input.GetKey("s")) pos += Vector3.back * speed * Time.deltaTime;
        if (mousePos.y >= Screen.height - edge || Input.GetKey("w")) pos += Vector3.forward * speed * Time.deltaTime;

        float zoomDelta = scrollSpeed * 100.0f * scroll * Time.deltaTime;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - zoomDelta, 10, 70);

        transform.position = pos;
    }
}
