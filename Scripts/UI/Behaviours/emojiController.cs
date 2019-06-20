using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class emojiController : MonoBehaviour
{

    private Image img;
    public List<Sprite> ImageSprite = new List<Sprite>();
    private int imageFrame = 0;
    private float m_Delta = 0;
    private bool IsPlaying = false;
    private bool AutoPlay = true;
    private float FPS = 6;

    void Start()
    {
        ImageSprite.Reverse();
        RectTransform rect = transform.GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(200, 200);
        rect.anchorMax = new Vector2(1, 1);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchoredPosition = new Vector3(0, 0, 0);
        rect.sizeDelta = new Vector2(0, 0);
        img = transform.GetComponent<Image>();
        if (AutoPlay)
        {
            IS_Play();
        }
        else
        {
            NO_Play();
        }
    }

    public void IS_Play()
    {
        IsPlaying = true;
        imageFrame = 0;
        SetPrite(imageFrame);
    }

    public void NO_Play()
    {
        IsPlaying = false;
    }

    private void SetPrite(int index)
    {
        img.sprite = ImageSprite[index];
    }

    void Update()
    {
        if (!IsPlaying || ImageSprite.Count <= 1)
        {
            return;
        }
        m_Delta += Time.deltaTime;
        if (m_Delta > 1 / FPS)
        {
            m_Delta = 0;
            imageFrame++;
            if (imageFrame >= ImageSprite.Count)
            {
                imageFrame = 0;
            }
            SetPrite(imageFrame);
        }
    }

    public void Stop()
    {
        imageFrame = 0;
        SetPrite(imageFrame);
        IsPlaying = false;
    }
}
