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
    //�������ÿ������Ļ���
    //������¼ÿ�����䶼�е� ���Ƶ����Ժ���Ϊ
    public Vector2 CenterPoint;
    public int xSize;
    public int ySize;

    E_RoomState roomState;
}
