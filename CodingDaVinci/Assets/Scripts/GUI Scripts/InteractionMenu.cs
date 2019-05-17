using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionMenu : MonoBehaviour
{
    public static InteractionMenu Instance { get; private set; }
    
    //public CanvasGroup CompleteBuildMenu;
    //public CanvasGroup MainMenu;

    public GameObject CompleteBuildMenu;
    public GameObject MainMenu;

    private void Awake()
    {
        Instance = this;
        Instance.GetComponent<GameObject>().SetActive(false);
        Instance.GetComponent<CanvasGroup>().alpha = 0;

    }

    public void ToggleCompleteMenu(bool visible)
    {
        if (visible)
        {
            Instance.GetComponent<GameObject>().SetActive(true);
            Instance.GetComponent<CanvasGroup>().alpha = 1;
        }
        else
        {
            Instance.GetComponent<GameObject>().SetActive(false);
            Instance.GetComponent<CanvasGroup>().alpha = 0;
        }
    }

    public void ToggleVisibility(bool visible)
    {
        if(visible)
        {
            CompleteBuildMenu.GetComponent<CanvasGroup>().alpha = 1;
            CompleteBuildMenu.SetActive(true);

            MainMenu.GetComponent<CanvasGroup>().alpha = 0;
            MainMenu.SetActive(false);
        }
        else
        {
            CompleteBuildMenu.GetComponent<CanvasGroup>().alpha = 0;
            CompleteBuildMenu.SetActive(false);

            MainMenu.GetComponent<CanvasGroup>().alpha = 1;
            MainMenu.SetActive(true);
        }
    }
}
