using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BindWidgetBehavior : MonoBehaviour
{
    private bool isBind = false;
    protected virtual void Awake()
    {
        BindWidget();
    }

    protected void BindWidget()
    {
        if (isBind) return;
        isBind = true;

        Window_BindWidget.BindWidget(this, transform);
    }
}
