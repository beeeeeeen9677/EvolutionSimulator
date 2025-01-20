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


        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -0.2f, 0, Space.World);
        }
        else if(Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, 0.2f, 0, Space.World);
        }
    }
}
