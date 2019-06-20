/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * IDestroyable interface.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-02
 * 
 ***************************************************************************************************/

public interface IDestroyable
{
    bool destroyed { get; }
    bool pendingDestroy { get; }

    void Destroy();
}
