using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform transform;
    [SerializeField]
    private float speed;



    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Vertical") != 0)
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                transform.position += Vector3.left * Time.deltaTime * speed;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                transform.position += Vector3.right * Time.deltaTime * speed;
            }
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            if (Input.GetAxis("Horizontal") > 0)
            {
                transform.position += Vector3.forward * Time.deltaTime * speed;
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                transform.position += Vector3.back * Time.deltaTime * speed;
            }
        }
    }
}
