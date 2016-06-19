using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class PreferredSize : MonoBehaviour
{
    public LayoutElement Layout;
    public bool WidthFromCanvas = false;
    public Canvas Canvas;

    private void Start()
    {
        Do();
    }

    private void Update()
    {
        Do();
    }

    private void Do()
    {
        if (Layout == null)
            return;
        if (Canvas != null)
        {
            if (WidthFromCanvas)
                Layout.preferredWidth = Canvas.pixelRect.width / Canvas.scaleFactor;
        }
    }
}