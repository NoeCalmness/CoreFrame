// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-27      10:58
//  * LastModify：2018-07-27      13:34
//  ***************************************************************************************************/

#region

using UnityEngine;

#endregion

public class Board : MonoBehaviour
{
    public Camera target;
    
    // Update is called once per frame
    private void Update ()
    {
        if (!target) return;
        
        transform.LookAt(target.transform.position, Vector3.up);
        transform.Rotate(0, 180, 0);
    }
}
