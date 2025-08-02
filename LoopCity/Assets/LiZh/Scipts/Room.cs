using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Room :BaseMap
{
    //���캯���Զ����ɷ���ߴ�
    public Room()
    {
        xSize = Random.Range(2, 7);
        ySize = Random.Range(2, 7);
    }

    public Room(int a,int b)
    {
        CenterPoint.x = a;
        CenterPoint.y = b;
        xSize = Random.Range(2, 7);
        ySize = Random.Range(2, 7);
    }

    //���ݵ�ǰ����λ�������·���λ��
    public Room CreatNewCenter(Vector2 center1)
    {
        Room newRoom = new Room();
        //���ô��뷿����������� ���ƫ�Ƶõ��·�������ĵ�����
        newRoom.CenterPoint.x += center1.x+((Random.Range(0,2) == 1)? + Random.Range(10, 20) : -Random.Range(10,20));
        newRoom.CenterPoint.y += center1.y+((Random.Range(0,2) == 1 )? +Random.Range(10, 20) : -Random.Range(10, 20));
       
        return newRoom;
    }

    //�������ɵķ���ذ�
    public void DrawFloor(Tilemap tileMap, TileBase tile)
    {
        for (int i = (int)CenterPoint.y - ySize; i < CenterPoint.y + ySize; i++)
        {
            for (int j = (int)CenterPoint.x - xSize; j < CenterPoint.x + xSize; j++)
            {
                tileMap.SetTile(new Vector3Int(j,i,0), tile);
            }
        }
    }

    public void DrawWall(Tilemap tileMap, TileBase tile)
    {

        for (int j = (int)CenterPoint.x - xSize-1; j < CenterPoint.x + xSize+1; j++)
        {
            tileMap.SetTile(new Vector3Int(j, (int)CenterPoint.y - ySize - 1, 0), tile);
            tileMap.SetTile(new Vector3Int(j, (int)CenterPoint.y + ySize , 0), tile);

        }

        for (int i = (int)CenterPoint.y - ySize; i < CenterPoint.y + ySize; i++)
        {
            tileMap.SetTile(new Vector3Int((int)CenterPoint.x - xSize - 1 ,i, 0), tile);
            tileMap.SetTile(new Vector3Int((int)CenterPoint.x + xSize  ,i, 0), tile);
        }
    }
}
