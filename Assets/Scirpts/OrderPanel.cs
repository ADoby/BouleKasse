using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OrderPanel : MonoBehaviour
{
    public Text Label;
    public AppData.Order Order;
    public Transform parent;
    public Toggle ToggleButton;

    private void Awake()
    {
        AppController.ResetOrderFinished -= Reset;
        AppController.ResetOrderFinished += Reset;
        AppController.SelectAllOrdersCallback -= Select;
        AppController.SelectAllOrdersCallback += Select;
    }

    private void OnDestroy()
    {
        AppController.ResetOrderFinished -= Reset;
        AppController.SelectAllOrdersCallback -= Select;
    }

    private void Reset()
    {
        ToggleButton.isOn = false;
    }

    private void Select()
    {
        ToggleButton.isOn = true;
    }

    public void Toggle(bool value)
    {
        Order.Finished = value;
    }
}