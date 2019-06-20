// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-09-26      11:12
//  * LastModify：2018-09-26      11:12
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class PetSummonSSS : MonoBehaviour, ISubRewardShow
{
    public Action OnBack { get; set; }
    private PItem2 itemCache;

    public Transform decomposePanel;
    public Transform originItem;
    public Transform item;
    public Button    comfirmButton;
    public Button    closeComfirmButton;
    public Button    CloseButton;
    public Text      NameText;

    private void Awake()
    {
        decomposePanel = transform.GetComponent<Transform>("tip_repeat");
        originItem     = transform.GetComponent<Transform>("tip_repeat/floor/Image/pet");
        item           = transform.GetComponent<Transform>("tip_repeat/floor/Image/item");
        comfirmButton  = transform.GetComponent<Button>   ("tip_repeat/floor/sure_btn");
        closeComfirmButton = transform.GetComponent<Button>("tip_repeat/floor/close");

        CloseButton = transform.GetComponent<Button>();
        NameText = transform.GetComponent<Text>("name_txt");
    }

    void Start()
    {
        CloseButton?.onClick.AddListener(() =>
        {
            if (itemCache.source == 0)
            {
                GoBack();
            }
            else
            {
                ShowDecomposeComfirm();
            }
        });
    }

    private void ShowDecomposeComfirm()
    {
        var petInfo = PetInfo.Create(ConfigManager.Get<PropItemInfo>(itemCache.source));
        Util.SetPetInfo(originItem, petInfo);
        Util.SetItemInfo(item, ConfigManager.Get<PropItemInfo>(itemCache.itemTypeId), 0, (int)itemCache.num);
        comfirmButton?.onClick.RemoveAllListeners();
        comfirmButton?.onClick.AddListener(GoBack);
        closeComfirmButton?.onClick.RemoveAllListeners();
        closeComfirmButton?.onClick.AddListener(GoBack);
        decomposePanel.SafeSetActive(true);
    }

    private void GoBack()
    {
        decomposePanel.SafeSetActive(false);
        gameObject.SetActive(false);
        var levelHome = Level.current as Level_Home;
        levelHome?.PetGameObject?.SetActive(false);
        OnBack?.Invoke();
    }

    public void Show(PItem2 rItem)
    {
        itemCache = rItem;
        gameObject.SetActive(true);
        var levelHome = Level.current as Level_Home;
        var pet = PetInfo.Create(ConfigManager.Get<PropItemInfo>(rItem.source != 0 ? rItem.source : rItem.itemTypeId));
        if (pet != null)
        {
            levelHome?.CreatePet(pet);
            Util.SetText(NameText, pet.CPetInfo.itemNameId);
        }
    }
}
