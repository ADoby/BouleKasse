using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadioButton : Button
{
    public bool IsChecked = false;

    public void Check()
    {
        IsChecked = true;
        DoStateTransition(SelectionState.Highlighted, false);
    }

    public void UnCheck()
    {
        IsChecked = false;
        DoStateTransition(SelectionState.Normal, false);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (IsChecked && state == SelectionState.Normal)
            state = SelectionState.Highlighted;
        if (!IsChecked && state == SelectionState.Highlighted)
            state = SelectionState.Normal;

        base.DoStateTransition(state, instant);
    }

    protected override void Awake()
    {
        base.Awake();
        onClick.AddListener(OnClick);
    }

    public UnityAction<RadioButton> OnRadioButtonClicked;

    private void OnClick()
    {
        if (OnRadioButtonClicked != null)
            OnRadioButtonClicked.Invoke(this);
    }
}