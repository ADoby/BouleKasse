using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour
{
    public UnityEvent DoAction;

    public void OnPointerDown()
    {
        Down = true;
    }

    public Toggle toggle;

    public void OnPointerUp()
    {
        if (Down == false)
        {
            toggle = GetComponent<Toggle>();
        }
        Down = false;
        Timer = 0;
    }

    public bool Down = false;
    public float Timer = 0;
    public float Times = 5f;

    private void Update()
    {
        if (toggle != null)
        {
            toggle.isOn = false;
            toggle = null;
        }
        if (Down)
        {
            Timer += Time.deltaTime;
            if (Timer >= Times)
            {
                Down = false;
                if (DoAction != null)
                    DoAction.Invoke();
            }
        }
        else
        {
            Timer = 0;
        }
    }
}