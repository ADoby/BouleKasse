using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class ProductButton : MonoBehaviour
{
    public Text Label;
    public Text Price;
    public Text Count;

    public ButtonInfo info;

    void Awake()
    {
        if (info == null)
            return;
        AppController.OnFinished -= info.Reset;
        AppController.OnFinished += info.Reset;
    }

    public void OnClick()
    {
        info.OnClicked();
    }
}