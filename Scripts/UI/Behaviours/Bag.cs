using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Bag : MonoBehaviour
{
    private List<Transform> gridList = new List<Transform>();
    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            gridList.Add(transform.GetChild(i));
        }
    }

    //添加物品  
    public void AddItem(string itemName, int count = 1)
    {
        //如果有相同的物品，则只是更改包里该物品的数量；否则实例化该物品，改数量  
        bool hasSameItem = false;

        for (int i = 0; i < gridList.Count; i++)
        {
            //如果有物品  
            if (gridList[i].childCount > 0)
            {
                Transform tra = gridList[i].GetChild(0);
                string name = tra.gameObject.name;
                string name1 = itemName + "(Clone)";
                //如果有该物品  
                //string name = itemName.Substring(itemName.LastIndexOf('/') + 1);
                if (name.Equals(name1))
                {
                    hasSameItem = true;
                    ModifyCount(tra.GetChild(0).GetComponent<Text>(), count);
                    break;
                }
            }
        }

        if (!hasSameItem)
        {
            for (int i = 0; i < gridList.Count; i++)
            {
                if (gridList[i].childCount == 0)
                {
                    GameObject go = Instantiate(Resources.Load(itemName)) as GameObject;

                    go.transform.SetParent(gridList[i], false);

                    ModifyCount(go.transform.GetChild(0).GetComponent<Text>(), count);
                    break;
                }
            }
        }
    }

    private void ModifyCount(Text text, int count)
    {
        int temp = int.Parse(text.text);
        temp += count;
        text.text = temp.ToString();
    }

    //整理  
    public void Tidy()
    {
        List<Transform> tempList = new List<Transform>();
        for (int i = 0; i < gridList.Count; i++)
        {
            if (gridList[i].childCount > 0) tempList.Add(gridList[i].GetChild(0));
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            tempList[i].SetParent(gridList[i]);
            tempList[i].position = gridList[i].position;
        }
    }
    //private void Update()
    //{
    //    Tidy();
    //}
}