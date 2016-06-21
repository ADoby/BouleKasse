using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProductButton : MonoBehaviour
{
    public Text Label;
    public Text Price;
    public Text Count;

    public Image Image;

    public ButtonInfo info;

    private void Awake()
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