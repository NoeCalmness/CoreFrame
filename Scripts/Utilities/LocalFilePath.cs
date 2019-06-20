/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Local file w/r path helper.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-19
 * 
 ***************************************************************************************************/

/// <summary>
/// Manage all local file path
/// </summary>
public static class LocalFilePath
{
    /// <summary>
    /// Save all screenshot image
    /// </summary>
    public const string SCREENSHOT = "ScreenShots";
    /// <summary>
    /// Save all cached avatars
    /// </summary>
    public const string AVATAR     = "Avatars";
    /// <summary>
    /// All hash list files
    /// </summary>
    #if UNITY_EDITOR
    public const string HASHLIST   = "HashList";
    #else
    public const string HASHLIST   = "";
    #endif
}
