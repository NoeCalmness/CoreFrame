/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Creature const values
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-26
 * 
 ***************************************************************************************************/

public partial class Creature
{
    public const int COLLIDER_LAYER_ATTACK    = 1;
    public const int COLLIDER_LAYER_EFFECT    = 2;
    public const int COLLIDER_LAYER_HIT       = 3;
    public const int COLLIDER_LAYER_COLLISION = 0;

    public const int MAX_WEAPON_ID            = 100;

    //the defalut id of robot's attributes in config_monster_attribute
    public const int ROBOT_ATTRIBUTE_ID       = 100;

    /// <summary>
    /// Default avatar box id
    /// </summary>
    public const int DEFAULT_AVATAR_BOX       = 901;

    /// <summary>
    /// Element type shortcut
    /// </summary>
    public const int None = 0, Wind = 1, Fire = 2, Water = 3, Thunder = 4, Ice = 5;
    public const int ElementStartIndex = (int)CreatureFields.ElementDefenseWind - 1;

    /// <summary>
    /// Default weapon animator name. args: main weaponID, gender
    /// </summary>
    public const string ANIMATOR_NAME = "animator_weapon_{0}_{1}";

    /// <summary>
    /// Default UI animator name. args: main weaponID, gender
    /// </summary>
    public const string ANIMATOR_NAME_SIMPLE = "animator_simple_{0}_{1}";

    /// <summary>
    /// Male hair bone node name
    /// </summary>
    public const string HAIR_NODE_MALE   = "hair_m_point";
    /// <summary>
    /// Female hair bone node name
    /// </summary>
    public const string HAIR_NODE_FEMALE = "hair_f_point";

    /// <summary>
    /// Avatar atlas prefix
    /// </summary>
    public const string AVATAR_NAME = "avatar_class_{0}";

    /// <summary>
    /// Static avatar box (used for monsters)
    /// </summary>
    public const string STATIC_AVATA_BOX = "avatar_box_{0}";

    /// <summary>
    /// damage point root point name
    /// </summary>
    public const string DAMAGE_POINT_ROOT_NAME = "damage_position";
    /// <summary>
    /// the child damage point parent
    /// </summary>
    public static readonly string[] DAMAGE_CHILD_POINT_NAMES = new string[] { "damage_point", "buff_point" };

    /// <summary>
    /// Morph node names, see CreatureMorph
    /// </summary>
    public static readonly string[] MORPH_NODE_NAMES = new string[] { "normal", "awake" };

    /// <summary>
    /// Get the weapon animator name of target creature
    /// </summary>
    /// <param name="weaponID">creature's weapon ID</param>
    /// <param name="gender">creature's gender 0 = female 1 = male</param>
    /// <param name="simple">use simple animator ?</param>
    public static string GetAnimatorName(int weaponID, int gender, bool simple)
    {
        if (simple) return GetAnimatorNameSimple(weaponID, gender);
        return GetAnimatorName(weaponID, gender);
    }

    /// <summary>
    /// Get the weapon animator name of target creature
    /// </summary>
    /// <param name="weaponID">creature's weapon ID</param>
    /// <param name="gender">creature's gender 0 = female 1 = male</param>
    /// <returns></returns>
    public static string GetAnimatorName(int weaponID, int gender)
    {
        return Util.Format(ANIMATOR_NAME, weaponID, weaponID < MAX_WEAPON_ID ? gender : 1);
    }

    /// <summary>
    /// Get the UI animator name of target creature
    /// </summary>
    /// <param name="weaponID">creature's weapon ID</param>
    /// <param name="gender">creature's gender 0 = female 1 = male</param>
    /// <returns></returns>
    public static string GetAnimatorNameSimple(int weaponID, int gender)
    {
        return Util.Format(ANIMATOR_NAME_SIMPLE, weaponID, gender);
    }

    /// <summary>
    /// Get hair node name
    /// </summary>
    /// <param name="gender"></param>
    /// <returns></returns>
    public static string GetHairNodeName(int gender)
    {
        return gender == 0 ? HAIR_NODE_FEMALE : gender == 1 ? HAIR_NODE_MALE : "";
    }

    /// <summary>
    /// Get morph node name
    /// </summary>
    /// <param name="morph"></param>
    /// <returns></returns>
    public static string GetMorphNodeName(CreatureMorph morph)
    {
        var idx = (int)morph;
        return idx < 0 || idx >= MORPH_NODE_NAMES.Length ? string.Empty : MORPH_NODE_NAMES[idx];
    }

    /// <summary>
    /// Get class avatar name
    /// </summary>
    /// <param name="_class"></param>
    /// <returns></returns>
    public static string GetClassAvatarName(CreatureVocationType _class)
    {
        return Util.Format(AVATAR_NAME, (int)_class);
    }

    /// <summary>
    /// Get class avatar name
    /// </summary>
    /// <param name="_class"></param>
    /// <returns></returns>
    public static string GetClassAvatarName(byte _class)
    {
        return Util.Format(AVATAR_NAME, _class);
    }

    /// <summary>
    /// Get class avatar name
    /// </summary>
    /// <param name="_class"></param>
    /// <returns></returns>
    public static string GetClassAvatarName(int _class)
    {
        return Util.Format(AVATAR_NAME, _class);
    }

    /// <summary>
    /// Get static avatar box (used for monsters)
    /// </summary>
    /// <returns></returns>
    public static string GetStaticAvatarBox(int id)
    {
        return Util.Format(STATIC_AVATA_BOX, id);
    }
}
