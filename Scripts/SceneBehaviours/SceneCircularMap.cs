using UnityEngine;

public class SceneCircularMap : MonoBehaviour
{
    public float speed = 5;
    public Transform[] maplist;
    Vector3 front = Vector3.zero;
    Vector3 last = Vector3.zero;
    Vector3 targetPos = Vector3.zero;
    private void Start()
    {
        front = maplist[0].localPosition + new Vector3(0,0,-40);
        targetPos = new Vector3(front.x, front.y, 0);
        last = maplist[maplist.Length - 1].localPosition;
    }

    void Update()
    {
        MapMove();
    }
    void MapMove()
    {
        for(int i = 0;i< maplist.Length;i++)
        {
            if(maplist[i].localPosition.z >= last.z)
            {
                targetPos.z = front.z + (maplist[i].localPosition.z - last.z);
                maplist[i].localPosition = targetPos;
            }
             maplist[i].Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
        }
    }
}