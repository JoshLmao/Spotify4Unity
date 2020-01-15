using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CanvasController : MonoBehaviour
{
    public GameObject[] Screens;

    public Button[] Buttons;

    private void Start()
    {
        for(int i = 0; i < Buttons.Length; i++)
        {
            int index = i;
            Buttons[i].onClick.AddListener(() => OnChangeScreen(Buttons[index], index));
        }

        Screens[0].SetActive(true);
        Screens[1].SetActive(false);
    }

    private void OnChangeScreen(Button btn, int index)
    {
        for(int i = 0; i < Screens.Length; i++)
        {
            Screens[i].SetActive(i == index);
        }
    }
}
