using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeFont : MonoBehaviour
{
    private Font font;

    [ContextMenu("Change Font")]
    public void Change()
    {
        font = Resources.Load<Font>("font_yahei");
        Text[] allText = transform.GetComponentsInChildren<Text>(true);
        foreach (var item in allText)
        {
            if (item.font == null) item.font = font;
        }

        GameObject.DestroyImmediate(this);
    }

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
