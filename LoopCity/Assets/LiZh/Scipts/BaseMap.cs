using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum E_RoomState
{ 
    normal,
    boss,
    gift,
}

public class BaseMap
{
    //这个类是每个房间的基类
    //用来记录每个房间都有的 相似的属性和行为
    public Vector2 CenterPoint;
    public int xSize;
    public int ySize;

    E_RoomState roomState;
}
