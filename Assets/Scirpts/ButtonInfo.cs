using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ButtonInfo
{
    public static UnityAction<ButtonInfo> ButtonClicked;

    [Multiline(2)]
    public string Name = "";

    public float Price = 1.00f;
    public Sprite Sprite;

    public ProductButton Button;

    [ReadOnly]
    public int Count = 0;

    public void OnClicked()
    {
        if (ButtonClicked != null)
            ButtonClicked.Invoke(this);
    }

    public void Reset()
    {
        Count = 0;
        UpdateText();
    }

    public void UpdateText()
    {
        if (Button == null)
            return;
        if (Button.Label != null && string.IsNullOrEmpty(Button.Label.text))
            Button.Label.text = Name;
        if (Button.Price != null)
            Button.Price.text = AppController.GetPriceText(Price);
        if (Button.Count != null)
            Button.Count.text = Count.ToString();
        if (Button.Image != null)
            Button.Image.sprite = Sprite;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (Button.Label != null)
                UnityEditor.EditorUtility.SetDirty(Button.Label);
            if (Button.Price != null)
                UnityEditor.EditorUtility.SetDirty(Button.Price);
            if (Button.Count != null)
                UnityEditor.EditorUtility.SetDirty(Button.Count);

            if (Button.Image != null)
                UnityEditor.EditorUtility.SetDirty(Button.Image);
        }
#endif
    }
}