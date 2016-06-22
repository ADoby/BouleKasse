using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProductButton : MonoBehaviour
{
    public string Name = "";
    public Text Label;
    public Text Price;
    public Text Count;

    public Image Image;

    public ButtonInfo info;

    public UnityAction<ProductButton> Clicked;

    public GameObject Overlay;

    public void SetSelected(bool value)
    {
        Overlay.SetActive(value);
    }

    private void Awake()
    {
        if (info == null)
            return;
        AppController.OnFinished -= info.Reset;
        AppController.OnFinished += info.Reset;
    }

    public void OnClick()
    {
        if (Clicked != null)
            Clicked.Invoke(this);
        info.OnClicked();
    }
}