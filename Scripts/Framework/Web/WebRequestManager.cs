/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Web Request Manager
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.3
 * Created:  2019-03-13
 * 
 ***************************************************************************************************/

using LitJson;
using UnityEngine.Networking;
using System;
using System.Collections;

public class WebRequestManager : Singleton<WebRequestManager>
{
    public static WebRequest Request(string url, object postData, OnRequestComplete onComplete = null)
    {
        var request = new WebRequest() { onComplete = onComplete };

        Root.instance.StartCoroutine(_Request(url, postData, request));

        return request;
    }

    public static WebRequest<T> Request<T>(string url, object postData, OnRequestComplete<T> onComplete = null)
    {
        var request = new WebRequest<T>() { onComplete = onComplete };

        Root.instance.StartCoroutine(_Request(url, postData, request));

        return request;
    }

    public static WebRequest<T> Request<T>(string url,OnRequestComplete<T> onComplete = null)
    {
        var request = new WebRequest<T>() { onComplete = onComplete };

        Root.instance.StartCoroutine(_Request(url, request));

        return request;
    }

    private static IEnumerator _Request(string url, object postData, WebRequest request)
    {
        var post = JsonMapper.ToJson(postData);

        Logger.LogDetail($"WebRequestManager: Requesting <b><color=#45D9FF>{url}</color></b>, data [<b><color=#45D9FF>{post}</color></b>]");

        var _request = UnityWebRequest.Post(url, JsonMapper.ToJson(postData));
        _request.timeout = 5;
        yield return _request.SendWebRequest();

        string err = null;
        WebReply reply = null;

        var s = _request.downloadHandler.text;
        if (!_request.isNetworkError)
        {
            try
            {
                #if SHADOW_PACK
                if (!string.IsNullOrEmpty(s)) s = s.Replace(GeneralConfigInfo.sappName, Root.shadowAppName);
                #endif

                if (_request.responseCode == 200) // Currently we obly use response 200
                    reply = JsonMapper.ToObject<WebReply>(s);
                else err = $"Invalid response code: {_request.responseCode}\n{s}";
            }
            catch (Exception e)
            {
                err = e.Message;
                reply = null;
            }
        }

        if (reply == null)
        {
            reply = new WebReply() { code = -1 };

            Logger.LogError("WebRequestManager::_Request: URL = {0}, errMsg = {1}, data = {2}", url, err ?? (!_request.isNetworkError ? "Unknow Error!" : _request.error), s);
        }
        else Logger.LogDetail("WebRequestManager: Received from <b><color=#45D9FF>{0}</color></b>, data [<b><color=#45D9FF>{1}</color></b>]", url, s);

        _request.Dispose();

        request.reply = reply;
        request.isDone = true;
    }

    private static IEnumerator _Request<T>(string url, object postData, WebRequest<T> request)
    {
        var post = JsonMapper.ToJson(postData);

        Logger.LogDetail("WebRequestManager: Requesting <b><color=#45D9FF>{0}</color></b>, data [<b><color=#45D9FF>{1}</color></b>]", url, post);

        var _request = UnityWebRequest.Post(url, post);
        _request.timeout = 5;
        yield return _request.SendWebRequest();

        string err = null;
        WebReply<T> reply = null;

        var s = _request.downloadHandler.text;
        if (!_request.isNetworkError)
        {
            try
            {
                #if SHADOW_PACK
                if (!string.IsNullOrEmpty(s)) s = s.Replace(GeneralConfigInfo.sappName, Root.shadowAppName);
                #endif

                if (_request.responseCode == 200) // Currently we obly use response 200
                    reply = JsonMapper.ToObject<WebReply<T>>(s);
                else err = $"Invalid response code: {_request.responseCode}\n{s}";
            }
            catch (Exception e)
            {
                err = e.Message;
                reply = null;
            }
        }

        if (reply == null)
        {
            reply = new WebReply<T> { code = -1 };

            Logger.LogError("WebRequestManager: URL = {0}, errMsg = {1}, data = {2}", url, err ?? (!_request.isNetworkError ? "Unknow Error!" : _request.error), s);
        }
        else Logger.LogDetail("WebRequestManager: Received from <b><color=#45D9FF>{0}</color></b>, data [<b><color=#45D9FF>{1}</color></b>]", url, s);

        _request.Dispose();

        request.reply = reply;
        request.isDone = true;
    }

    private static IEnumerator _Request<T>(string url,WebRequest<T> request)
    {
        Logger.LogDetail("WebRequestManager: Requesting <b><color=#45D9FF>{0}</color></b>", url);

        var _request = UnityWebRequest.Get(url);
        _request.timeout = 5;
        yield return _request.SendWebRequest();

        string err = null;
        WebReply<T> reply = null;

        var s = _request.downloadHandler.text;
        if (!_request.isNetworkError)
        {
            try
            {
#if SHADOW_PACK
                if (!string.IsNullOrEmpty(s)) s = s.Replace(GeneralConfigInfo.sappName, Root.shadowAppName);
#endif

                if (_request.responseCode == 200) // Currently we obly use response 200
                    reply = JsonMapper.ToObject<WebReply<T>>(s);
                else err = $"Invalid response code: {_request.responseCode}\n{s}";
            }
            catch (Exception e)
            {
                err = e.Message;
                reply = null;
            }
        }

        if (reply == null)
        {
            reply = new WebReply<T> { code = -1 };

            Logger.LogError("WebRequestManager: URL = {0}, errMsg = {1}, data = {2}", url, err ?? (!_request.isNetworkError ? "Unknow Error!" : _request.error), s);
        }
        else Logger.LogDetail("WebRequestManager: Received from <b><color=#45D9FF>{0}</color></b>, data [<b><color=#45D9FF>{1}</color></b>]", url, s);

        _request.Dispose();

        request.reply = reply;
        request.isDone = true;
    }
}