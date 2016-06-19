using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class PasswortPanel : MonoBehaviour
{
    public InputField PasswortBox;

    public static UnityAction<string> Finished;
    public static UnityAction Cancel;

    void OnEnable()
    {
        PasswortBox.text = "";
    }

    public void Click()
    {
        if (Finished != null)
            Finished.Invoke(PasswortBox.text);
    }

    public void CancelClick()
    {
        if (Cancel != null)
            Cancel.Invoke();
    }
}