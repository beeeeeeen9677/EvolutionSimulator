using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    private float destroyTimer = 1.5f;

    private void Update()
    {
        transform.position += Vector3.up * 2 * Time.deltaTime;

        destroyTimer -= Time.deltaTime;
        if ( destroyTimer < 0 )
            Destroy(gameObject);
    }
}
