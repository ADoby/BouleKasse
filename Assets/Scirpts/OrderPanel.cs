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
    }

    private void OnDestroy()
    {
        AppController.ResetOrderFinished -= Reset;
    }

    private void Reset()
    {
        ToggleButton.isOn = false;
    }

    public void Toggle(bool value)
    {
        Order.Finished = value;
    }
}