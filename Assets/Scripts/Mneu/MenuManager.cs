using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Button startButton, resetButton;


    [SerializeField]
    private Transform propertyContainers;

    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(StartSimulation);
        resetButton.onClick.AddListener(ReloadScene);
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

        PlayerPrefs.Save();

        StartCoroutine(BufferTime());



        SceneManager.LoadScene("MainScene");
    }

    IEnumerator BufferTime()
    {
        yield return new WaitForSeconds(1f);
        //Debug.Log("Buffer Time");
    }

    public void ReloadScene()
    {
        // delete all playerprefs data
        PlayerPrefs.DeleteAll();

        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
