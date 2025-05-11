using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPropContainer : MonoBehaviour
{
    [SerializeField]
    private Text propertyName;

    [SerializeField]
    private InputField inputField;

    [SerializeField]
    private GameObject typeTag;

    private string playerPrefName; // PlayerPrefs data will be saved in MenuManager


    private float minValue;
    private float maxValue;

    private bool isEnv;

    public void InitSettingField(SettingInfo valueInfo)
    {
        // change input field content type
        Text placeholder = inputField.transform.Find("Placeholder").GetComponent<Text>();
        switch (valueInfo.inputType)
        {
            case SettingValueType.Int:
                inputField.contentType = InputField.ContentType.IntegerNumber;
                placeholder.text = "Integer";
                break;

            case SettingValueType.Float:
                inputField.contentType = InputField.ContentType.DecimalNumber;
                placeholder.text = "Decimal";
                break;

            default:
                return;
        }


        minValue = valueInfo.minValue;
        maxValue = valueInfo.maxValue;
        isEnv = valueInfo.isEnv;


        if (maxValue != 0)
        {
            if (minValue >= maxValue)
            {
                Debug.LogError("Min value must be less than max value");
                return;
            }
            else
            {
                placeholder.text += $"   ({minValue} - {maxValue})";
            }
        }
        else
        {
            placeholder.text += $"   (min: {minValue})";
        }



        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);


        // Set lable
        propertyName.text = valueInfo.propertyName;
        playerPrefName = valueInfo.playerPrefName;


        typeTag.SetActive(isEnv);



        // Init value by PlayerPrefs
        if (PlayerPrefs.HasKey(playerPrefName))
        {
            string value = "";

            switch (valueInfo.inputType)
            {
                case SettingValueType.Int:
                    value = PlayerPrefs.GetInt(playerPrefName).ToString();
                    break;
                case SettingValueType.Float:
                    value = PlayerPrefs.GetFloat(playerPrefName).ToString();
                    break;
            }

            PlayerPrefs.Save(); // Critical for builds!

            inputField.text = value;
        }
    }


    public void UpdateSettings()
    {
        // save to PlayerPrefs
        if (float.TryParse(inputField.text, out float result))
        {
            switch (inputField.contentType)
            {
                case InputField.ContentType.IntegerNumber:
                    PlayerPrefs.SetInt(playerPrefName, Mathf.RoundToInt(result));
                    break;
                case InputField.ContentType.DecimalNumber:
                    PlayerPrefs.SetFloat(playerPrefName, result);
                    break;
            }
        }
    }


    private void OnInputFieldValueChanged(string value)
    {
        // var value will be default 0 if not init in inspector

        if (float.TryParse(value, out float result))
        {
            // Debug.Log(result);
            if (result < minValue)
            {
                result = minValue;
            }

            if (maxValue != 0 && result > maxValue) // no need to check when maxValue is 0
            {
                result = maxValue;
            }

            inputField.text = result.ToString();
        }
    }
}

