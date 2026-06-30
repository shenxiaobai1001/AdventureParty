using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>配置</summary>
public class Config
{
    public static int ClearType=1;

    /// <summary>横向移动右边界（固定）</summary>
    public static float MoveRightBound = 0f;
    public static float PlayerMoveRightBound = 6;
    public static float PlayerMoveLeftBound = -6;
    public static float CameraMoveLeftBound = 0f;


    public static Color32 skinColor1 = new Color32(206,170,156,255);

    public static Color32 skinColor2 = new Color32(186, 126, 103, 255);

    public static Color32 skinColor3 = new Color32(85, 50, 28, 255);

    public static Color32 OnGetSkinKid()
    {
        int index =UnityEngine.Random.Range(0, 4);
        switch (index)
        {
            case 0:
                return  skinColor1;
            case 1:
                return skinColor2;
            case 2:
                return skinColor3;
        }
        return Color.white;
    }
}
public enum MoveType
{
    Normal,
    HighLeft,
    HighRight,
    MaxLeft,
    MaxRight
}

public enum CarType
{
    Motorcycle,
    Sedan,
    Truck,
    SpecialVehicle,
}

public enum KidMoveType
{
    None = 0,
    MoveTarget,
    MoveTargetPause,
    Move,
    Pause,
    Hit,
    Pushed,
    Slip,
}

/// <summary>事件合集 </summary>
public enum Events
{
    None,
    KidPass,
    KidPause,
    BigenMoveBlock,
    OnModVideoPlayEnd,
    OnModVideoPlayStart,
    RoadCreateFinished,
    RoadDestroyFinished, 
    OnBarryExecutEnd,
    OnShowSystem,
    OnKidToRoad,
    OnKidEnd,
    OnSystemSettingsChanged,
    OnBreakBlock,
    OnChildDance,
}
public enum BarrageState//功能状态
{
    Tigger,
    Ready,
    Underway,
    Pause,
    Finsh,
    Close,
}

public enum BarraegExecutType//功能处理状态
{
    ReadyExecut,
    Executing,
    ExecutEnd,
}