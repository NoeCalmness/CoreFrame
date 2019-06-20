using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewInputFile : InputField
{

    public Chathyperlink sendtxt;
    private string link;

    protected override void Start()
    {
        sendtxt = transform.Find("clone").GetComponent<Chathyperlink>();
    }

    public void set(string mes)
    {
        sendtxt.text = mes;

        text += sendtxt.m_OutputText;//这里可能会带有color
        link = sendtxt.m_OutputText;

    }
    public void Send()
    {
        string nowtxt = text;
        if (link != null)
        {
            nowtxt = nowtxt.Replace(link, sendtxt.text);
        }
        sendtxt.text = nowtxt;
    }
}
