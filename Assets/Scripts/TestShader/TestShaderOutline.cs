using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestShaderOutline : MonoBehaviour
{
    private Material[] original;
    private Material[] modified;

    [SerializeField]
    private Material outline;

    private MeshRenderer meshRenderer;


    bool isOutlineApplied = false;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        original = meshRenderer.materials;
        List<Material> temp = original.ToList();
        temp.Add(outline);
        modified = temp.ToArray();

        Debug.Log(modified.Length);
    }



    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(!isOutlineApplied)
            {
                meshRenderer.materials = modified;
                Debug.Log("modified");

            }
            else
            {
                meshRenderer.materials = original;
                Debug.Log("original");
            }


            isOutlineApplied = !isOutlineApplied;

        }

    }
}
