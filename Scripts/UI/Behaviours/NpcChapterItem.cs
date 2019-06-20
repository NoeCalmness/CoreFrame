// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-02-26      10:15
//  *LastModify：2019-02-26      10:15
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class NpcChapterItem : AssertOnceBehaviour
{
    private Transform generalNode;
    private Transform okNode;
    private Transform challengeNode;
    private Text      generalText;
    private Text      challengeText;
    private Button    generalButton;
    private Button    challengeButton;
    private Transform[] _stars;
    private Transform locker;
    private Transform challengeLocker;
    private Transform notPassChallenge;
    private Transform passChallenge;

    private ChaseTask dataCache;
    private Action<ChaseTask> _onClick;

    protected override void Init()
    {
        generalNode   = transform.Find("general");
        okNode        = transform.Find("ok");
        challengeNode = transform.Find("challenge");
        generalText   = transform.GetComponent<Text>("general/Text");
        challengeText = transform.GetComponent<Text>("challenge/Text");
        generalButton = transform.GetComponent<Button>("general");
        challengeButton = transform.GetComponent<Button>("challenge/general");
        locker          = transform.GetComponent<Transform>("general/clock");
        challengeLocker = transform.GetComponent<Transform>("challenge/off_icon/clock");
        notPassChallenge = transform.GetComponent<Transform>("challenge/off_icon");
        passChallenge    = transform.GetComponent<Transform>("challenge/general");

        var stars = transform?.Find("challenge/general/star");
        _stars = new Transform[stars.childCount];
        for (var i = 0; i < stars?.childCount; i++)
        {
            var star = stars.GetChild(i);
            _stars[i] = star?.GetChild(0);
        }

        generalButton  .onClick.AddListener(() =>
        {
            if (dataCache.taskData != null)
                _onClick.Invoke(dataCache);
        });
        challengeButton.onClick.AddListener(() =>
        {
            if (dataCache.taskData != null)
                _onClick.Invoke(dataCache);
        });
    }

    public void BindData(ChaseTask rTask, Action<ChaseTask> onClick)
    {
        AssertInit();
        dataCache = rTask;
        _onClick = onClick;

        
        Util.SetText(generalText  , rTask.taskConfigInfo.name);
        Util.SetText(challengeText, rTask.taskConfigInfo.name);

        locker          .SafeSetActive(rTask.taskData == null);
        challengeLocker .SafeSetActive(rTask.taskData == null);

        if (rTask.taskData == null)
        {
            generalNode     .SafeSetActive(rTask.taskConfigInfo.maxChanllengeTimes == 1);
            okNode          .SafeSetActive(false);
            challengeNode   .SafeSetActive(rTask.taskConfigInfo.maxChanllengeTimes != 1);
            notPassChallenge.SafeSetActive(true);
            passChallenge   .SafeSetActive(false);
            generalText     .SafeSetActive(false);
        }
        else
        {
            generalNode     .SafeSetActive(rTask.taskConfigInfo.maxChanllengeTimes == 1 && rTask.taskData.state == 1);
            okNode          .SafeSetActive(rTask.taskData.state == 3);
            challengeNode   .SafeSetActive(rTask.taskConfigInfo.maxChanllengeTimes != 1);
            notPassChallenge.SafeSetActive(false);
            passChallenge   .SafeSetActive(true);
            generalText     .SafeSetActive(true);

            if (passChallenge?.gameObject.activeSelf ?? false)
            {
                for (var i = 0; i < _stars?.Length; i++)
                    _stars[i].SafeSetActive(rTask.star > i);
            }
        }
    }
}
