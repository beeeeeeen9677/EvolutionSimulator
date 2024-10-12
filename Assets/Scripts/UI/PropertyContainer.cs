using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropertyContainer : MonoBehaviour
{
    [SerializeField]
    private Text propertyName;
    [SerializeField]
    private Text propertyValue;

    public delegate string UpadateContainerValue();
    public UpadateContainerValue valueUpdateFunc;

    private bool stopUpdate;


    private void Awake()
    {
        stopUpdate = false;
    }

    public void SetPropertyName(string nameStr)
    {
        propertyName.text = nameStr;
    }


    public void SetValueText(string valueStr)
    {
        propertyValue.text = valueStr;
    }


    /*
    public void SetValueUpdateDelegate(PropertyInspectorUIManager.UpadateContainerValue func)
    {
        valueUpdateFunc = func;
    }
    */


    private void Update()
    {
        if(stopUpdate)
            return; 
        try
        {
            propertyValue.text = valueUpdateFunc?.Invoke();

        }
        catch(ArgumentException ex)
        {
            StopUpdating();
            Debug.Log("PropertyContainer: " + ex.Message);
        }
    }

    public void StopUpdating()
    {
        stopUpdate = true;
    }

}
