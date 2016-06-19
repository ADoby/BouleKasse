using UnityEngine;
using System.Collections.Generic;

public class RadioButtonGroup : MonoBehaviour
{
    public List<RadioButton> RadioButtons;

    void Awake()
    {
        RadioButtons.Clear();
        RadioButton[] buttons = GetComponentsInChildren<RadioButton>();
        RadioButtons.AddRange(buttons);
        for (int i = 0; i < RadioButtons.Count; i++)
        {
            if (RadioButtons[i] != null)
            {
                RadioButtons[i].OnRadioButtonClicked -= ButtonClicked;
                RadioButtons[i].OnRadioButtonClicked += ButtonClicked;
            }
        }
    }

    public RadioButton CurrentChecked = null;

    public void ButtonClicked(RadioButton button)
    {
        CurrentChecked = button;
        for (int i = 0; i < RadioButtons.Count; i++)
        {
            if (RadioButtons[i] == null)
            {
                RadioButtons.RemoveAt(i);
                i--;
                continue;
            }
            if (RadioButtons[i] != button)
                RadioButtons[i].UnCheck();
        }
        button.Check();
    }
}