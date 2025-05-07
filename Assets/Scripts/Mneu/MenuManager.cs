using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Button startButton;


    [SerializeField]
    private Transform propertyContainers;

    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(StartSimulation);
    }

    public void StartSimulation() // on start button pressed
    {
        foreach(Transform child in propertyContainers)
        {
            MenuPropContainer container = child.GetComponent<MenuPropContainer>();
            if (container != null)
            {
                container.UpdateSettings();
            }
        }



        SceneManager.LoadScene("MainScene");
    }
}
