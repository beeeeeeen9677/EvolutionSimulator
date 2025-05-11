using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSettingInit : MonoBehaviour
{
    [SerializeField]
    private Transform menuSettingContent;

    [SerializeField]
    private GameObject menuSettingContainerPrefab;


    [SerializeField]
    private List<SettingInfo> settingInfoList;

    // Start is called before the first frame update
    void Start()
    {
        foreach(SettingInfo valueInfo in settingInfoList)
        {
            AddSettingContainers(valueInfo);
        }   
    }

    private void AddSettingContainers(SettingInfo valueInfo)
    {
        MenuPropContainer container = Instantiate(menuSettingContainerPrefab, menuSettingContent).GetComponent<MenuPropContainer>();

        container.InitSettingField(valueInfo);
    }



}

[System.Serializable]
public class SettingInfo
{
    public string propertyName;
    public SettingValueType inputType;
    public string playerPrefName;
    public float minValue;
    public float maxValue;
    public bool isEnv;
}

public enum SettingValueType
{
    Int,
    Float,
}