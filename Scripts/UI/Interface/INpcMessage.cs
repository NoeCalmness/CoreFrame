public interface INpcMessage
{
    /// <summary>
    /// npc的动作信息
    /// </summary>
    NpcActionInfo actionInfo { get; }
    /// <summary>
    /// npc当前大阶段的各个小阶段的名字
    /// </summary>
    string[] belongStageName { get; }
    /// <summary>
    /// npc的体力
    /// </summary>
    int bodyPower { get; }
    /// <summary>
    /// npc当前等级名字
    /// </summary>
    string curLvName { get; }
    /// <summary>
    /// npc当前阶段的名字
    /// </summary>
    string curStageName { get; }
    /// <summary>
    /// npc当前的羁绊等级
    /// </summary>
    int fetterLv { get; }
    /// <summary>
    /// npc当前的羁绊阶段
    /// </summary>
    int fetterStage { get; }
    /// <summary>
    /// npc的头像
    /// </summary>
    string icon { get; }
    /// <summary>
    /// npc的信息是否为空
    /// </summary>
    bool isNull { get; }
    /// <summary>
    /// npc上次的心情值
    /// </summary>
    int lastMood { get; }
    /// <summary>
    /// npc最大的体力值
    /// </summary>
    int maxBodyPower { get; }
    /// <summary>
    /// npc最大的羁绊等级
    /// </summary>
    int maxFetterLv { get; }
    /// <summary>
    /// npc最大的羁绊阶段
    /// </summary>
    int maxFetterStage { get; }
    /// <summary>
    /// npc的模型
    /// </summary>
    string mode { get; }
    /// <summary>
    /// npc的心情
    /// </summary>
    int mood { get; }
    /// <summary>
    /// npc的名字
    /// </summary>
    string name { get; }
    /// <summary>
    /// npc的当前羁绊值
    /// </summary>
    int nowFetterValue { get; }
    /// <summary>
    /// npc的id
    /// </summary>
    ushort npcId { get; }
    /// <summary>
    /// npc的基本信息xml
    /// </summary>
    NpcInfo npcInfo { get; }
    /// <summary>
    /// npc的枚举类型
    /// </summary>
    NpcTypeID npcType { get; }
    /// <summary>
    /// npc的星力值
    /// </summary>
    uint starExp { get; set; }
    /// <summary>
    /// npc的星力等级
    /// </summary>
    int starLv { get; set; }
    /// <summary>
    /// 星级是否已经达到最大值
    /// </summary>
    bool starIsMax { get; }
    /// <summary>
    /// npc的星力进度
    /// </summary>
    float starProcess { get; }
    /// <summary>
    /// npc的动作阶段
    /// </summary>
    int stateStage { get; }
    /// <summary>
    /// npc升到下一级需要的羁绊值
    /// </summary>
    int toFetterValue { get; }
    /// <summary>
    /// npc在hierarchy下的节点名字
    /// </summary>
    string uiName { get; }

    /// <summary>
    /// 获得羁绊等级的名字
    /// </summary>
    /// <param name="stage">羁绊阶段</param>
    /// <param name="lv">羁绊等级</param>
    /// <returns></returns>
    string GetLvName(int stage, int lv);
    /// <summary>
    /// 获得羁绊阶段的名字
    /// </summary>
    /// <param name="stage">羁绊阶段</param>
    /// <returns></returns>
    string GetStageName(int stage);
    /// <summary>
    /// 获得当前羁绊阶段下的所有等级的名字
    /// </summary>
    /// <param name="stage">羁绊阶段</param>
    /// <returns></returns>
    string[] GetCurStageNames(int stage);
    /// <summary>
    /// 更新npc的体力值
    /// </summary>
    /// <param name="value"></param>
    void UpdateBobyPower(int value);
    /// <summary>
    /// 更新npc的羁绊等级
    /// </summary>
    /// <param name="lv"></param>
    void UpdateFetterLv(sbyte lv);
    /// <summary>
    /// 更新npc的羁绊值
    /// </summary>
    /// <param name="value"></param>
    void UpdateFetterValue(int value);
    /// <summary>
    /// 更新npc的模型(创建npc用)
    /// </summary>
    /// <param name="_itemTypeId"></param>
    void UpdateMode(ushort _itemTypeId);
    /// <summary>
    /// 更新npc的心情值
    /// </summary>
    /// <param name="value"></param>
    void UpdateMood(int value);

    bool isUnlockEngagement { get; }
}