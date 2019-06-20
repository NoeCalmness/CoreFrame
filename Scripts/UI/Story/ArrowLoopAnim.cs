using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArrowLoopAnim : MonoBehaviour
{
    private Vector3 m_originalPos;
    private Vector3 m_destinationPos;
    public float delta = 20;
    public float totalDuraction = 0.4f;

    private void Awake()
    {
        m_originalPos = transform.localPosition;
        m_destinationPos = new Vector3(m_originalPos.x, m_originalPos.y - delta, m_originalPos.z);
    }

    private void OnEnable()
    {
        transform.DOLocalMove(m_destinationPos,totalDuraction).SetLoops(-1,LoopType.Yoyo);
    }

    private void OnDisable()
    {
        transform.localPosition = m_originalPos;
        DOTween.Kill(transform);
    }

    // Use this for initialization
    void Start () { }
}
