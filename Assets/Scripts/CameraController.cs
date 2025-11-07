using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float speed = 20f;
    float edge = 10f;

    Camera cam;
    float scrollSpeed = -2f;



    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 mousePos = Input.mousePosition;
        float scroll = Input.mouseScrollDelta.y;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = 40.0f;
        }
        else
        {
            speed = 20.0f;
        }

        if (mousePos.x <= edge || Input.GetKey("a"))
            pos += Vector3.left * speed * Time.deltaTime;

        if (mousePos.x >= Screen.width - edge || Input.GetKey("d"))
            pos += Vector3.right * speed * Time.deltaTime;

        if (mousePos.y <= edge || Input.GetKey("s"))
            pos += Vector3.back * speed * Time.deltaTime;

        if (mousePos.y >= Screen.height - edge || Input.GetKey("w"))
            pos += Vector3.forward * speed * Time.deltaTime;

        float zoomDelta = scrollSpeed * 100.0f * scroll * Time.deltaTime;

        cam.fieldOfView -= zoomDelta;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 30, 70);


        transform.position = pos;
    }
}
