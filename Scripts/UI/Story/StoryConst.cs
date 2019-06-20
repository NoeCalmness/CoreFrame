public class StoryConst
{
    //战斗剧情摄像机停止锁定物体的标志
    public const int CAMERA_LOCK_ENDED_ID = -1;
    //剧情界面进入时候的渐隐时长
    public const float BASE_DIALOG_ALPHA_DURACTION = 0.3f;

    //剧情停止当前背景音乐的标志
    public const string STOP_CURRENT_MUSIC_FLAG = "0";
    //剧情恢复默认场景背景音乐的标志
    public const string RESET_CURRENT_MUSIC_FLAG = "1";
    //剧情恢复上次关闭的背景音乐的标志
    public const string RESET_LAST_MUSIC_FLAG = "2";

    /* 暂时保留
    //对话底板渐隐时间
    public const float DIALOG_TWEEN_ALPHA_DURACTION = 0.2f;
    //对话底板缩放动画
    public const float DIALOG_TWEEN_SCALE = 0.9f;
    //对话底板缩放动画时间
    public const float DIALOG_TWEEN_SCALE_DURACTION = 0.2f;
    */

    //剧场动画遮罩黑屏动画时间
    public const float MASK_PANEL_ALPHA_DURACTION = 1f;

    //剧场动画npc离开时候需要往左离场
    public const int THEATRE_NPC_LEAVE_TO_LEFT = -1;
    //剧场动画npc离开时候需要往右离场
    public const int THEATRE_NPC_LEAVE_TO_RIGHT = -2;
    //剧场动画npc暂时隐藏
    public const int THEATRE_NPC_HIDE = -3;

    //剧场动画npc离场动画的距离
    public const float THEATRE_NPC_LEAVE_DIS= 6f;

    //剧场动画坐标点的配置ID，对应show_creature_infos.xml 用于记录玩家不同的站立坐标，对应localPosition
    public const int THEATRE_PLAYER_POS_POINT_ID = 999;
    //剧场动画坐标点的配置ID，对应show_creature_infos.xml 用于记录特殊镜头不同的站立坐标，对应localPosition index = 0表示默认镜头位置
    public const int THEATRE_CAMERA_POS_POINT_ID = 1000;
    //剧场动画坐标点的配置ID，对应show_creature_infos.xml 用于记录场景中模型的坐标旋转，对应localPosition
    public const int THEATRE_MODEL_POS_POINT_ID = 1001;
    //剧场动画摄像机坐标的索引
    public const int THEATRE_CAMREA_POS_POINT_INDEX = 0;

    public const string THEATRE_STORY_ASSET_NAME = "theatre_dialog";
    public const string BATTLE_STORY_ASSET_NAME = "battle_dialog";
    //剧场对话遮罩资源
    public const string THEATRE_MASK_ASSET_NAME = "theatre_mask";
    //剧情GM工具资源
    public const string THEATRE_GM_ASSET_NAME = "theatre_gm";

    public const float BGM_CROSS_FADR_DURACTION = 0.1f;

    public const string EYE_CLIP_EFFECT = "eyeclip";
    public const string EYE_GAUSS_MATERIAL_ASSET = "ui_mat_gauss_blur";

    public const string DEFALUT_CONTENT_BG = "ui_public_talkingbg";

    #region 文本内容和替换昵称的常量定义
    /// <summary> 玩家名称 </summary>
    public const string PLAYER_NAME_PARAM = "PlayerName";
    /// <summary> 玩家性别 转义成小哥哥，小姐姐之类的</summary>
    public const string PLAYER_GENDER_PARAM = "PlayerGender";

    /// <summary> 选择的占卜类型 </summary>
    public const string DEVINE_TYPE_PARAM = "DevineType";
    /// <summary> 水晶球占卜结果 </summary>
    public const string CRYSTAL_DEVINE_RESULT_PARAM = "CrystalDevineResultName";
    /// <summary> 抽签占卜结果 </summary>
    public const string LOT_DEVINE_RESULT_PARAM = "LotDevineResultName";
    /// <summary> 占卜结果对心情的影响 - 整数 </summary>
    public const string DEVINE_INT_PARAM = "DevineResultValueInt";
    /// <summary> 占卜结果对心情的影响 - 百分数 </summary>
    public const string DEVINE_PERCENT_PARAM = "DevineResultValuePer";

    /// <summary> 道具名称，用于餐馆、商店之类的地方来显示商品名称 </summary>
    public const string SHOPITEM_NAME_PARAM = "ShopItemName";
    /// <summary> 道具价格，用于餐馆、商店之类的地方来显示商品价格 </summary>
    public const string SHOPITEM_PRICE_PARAM = "ShopItemPrice";

    /// <summary> 沙滩游玩类型的名称 </summary>
    public const string BEACH_PLAY_PARAM = "BeachPlayType";
    #endregion
}

public enum EnumStoryType
{
    None,

    TheatreStory,               //剧场对话,PVE模式下需要强制关闭玩家移动和AI

    FreeBattleStory,            //战斗剧情分支,PVE下玩家可以自由操作，而且AI不做特殊处理。不会产生蹦字效果

    PauseBattleStory,           //战斗剧情分支，PVE模式下需要强制关闭玩家移动和AI，文字可以点击使其迅速显示

    NpcTheatreStory,            //约会相关的NPC剧情,该模式下没有快进功能
}

public enum EnumStroyCheckFunc
{
    None,

    CameraShake,        //延迟检查摄像机震动

    SoundEffect,        //延迟播放的音效

    ContentDelay,       //延迟出现的文字内容

    ContentForce,       //文字出现的强制观看时间
}

public enum EnumContextStep
{
    //有延迟或者未开始
    Wait,

    //开始显示，但还未完成（也受到强制显示时间影响）
    Show,

    //仅仅是对话显示结束
    OnlyShowEnded,

    //全部等待都结束，可以点击跳转到下一步
    End
}

public enum EnumTheatreNpcLeaveType
{
    LeaveToRight = -2,

    LeaveToLeft = -1,

    DispearImme = 0,

}
