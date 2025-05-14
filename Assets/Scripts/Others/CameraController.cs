using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform transform;
    [SerializeField]
    private float speed;

    private Camera cam;

    private float zoomSpeed = 10f;

    private float minZoom = 30f;

    private float maxZoom = 60f;




    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();

        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        if (Input.GetAxis("Vertical") != 0)
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                transform.position += new Vector3(transform.forward.x, 0, transform.forward.z) * Time.deltaTime * speed;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                transform.position += new Vector3(transform.forward.x, 0, transform.forward.z) * -1 * Time.deltaTime * speed;
            }
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            Vector3 direction = new Vector3(transform.forward.x, 0, transform.forward.z);
            Quaternion rotation = Quaternion.identity;

            if (Input.GetAxis("Horizontal") > 0)
            {
                rotation = Quaternion.AngleAxis(90, Vector3.up);
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                rotation = Quaternion.AngleAxis(-90, Vector3.up);
            }

            transform.position += rotation * direction * Time.deltaTime * speed;
        }

        // Rotation 
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -0.2f, 0, Space.World);
        }
        else if(Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, 0.2f, 0, Space.World);
        }



        // Zoom with mouse scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            }
            else
            {
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * zoomSpeed, minZoom, maxZoom);
            }
        }

    }
}
