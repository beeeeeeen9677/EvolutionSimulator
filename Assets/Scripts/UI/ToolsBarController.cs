using UnityEngine;
using UnityEngine.UI;

public class ToolsBarController : MonoBehaviour
{
    public static ToolsBarController instance;


    [SerializeField] private GameObject toolsbar;
    [SerializeField] private Button inspectButton;
    [SerializeField] private Button lakeButotn;
    [SerializeField] private Button treeButton;
    private Button[] buttons;


    private PropertyInspectorUIManager inspecterManager;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        buttons = new Button[] { inspectButton, lakeButotn, treeButton };
    }


    // Start is called before the first frame update
    void Start()
    {
        inspecterManager = PropertyInspectorUIManager.instance;

        inspectButton.onClick.AddListener(OnInspectButtonClick);
        lakeButotn.onClick.AddListener(OnLakeButtonClick);
        treeButton.onClick.AddListener(OnTreeButtonClick);
    }

    private void OnInspectButtonClick()
    {
        SetButtonState(inspectButton);

        PropertyInspectorUIManager.instance.ToggleInspectorSystem();
    }

    private void OnLakeButtonClick()
    {
        SetButtonState(lakeButotn);

        PlaceObjectManager.instance.ToggleInspectorSystem(PlaceObjectType.Lake);
    }

    private void OnTreeButtonClick()
    {
        SetButtonState(treeButton);

        PlaceObjectManager.instance.ToggleInspectorSystem(PlaceObjectType.Tree);
    }

    private void SetButtonState(Button button) // hide all buttons except the one clicked
    {
        foreach (Button b in buttons)
        {
            if (b == button)
            {
                b.gameObject.SetActive(true);
            }
            else
            {
                b.gameObject.SetActive(false);
            }
        }
    }


    public void ResetButtonState() // show all buttons
    {
        foreach (Button b in buttons)
        {
            b.gameObject.SetActive(true);
        }
    }
}
