using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Room :BaseMap
{
    //构造函数自动生成房间尺寸
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

    //根据当前房间位置生成新房间位置
    public Room CreatNewCenter(Vector2 center1)
    {
        Room newRoom = new Room();
        //利用传入房间的中心坐标 随机偏移得到新房间的中心点坐标
        newRoom.CenterPoint.x += center1.x+((Random.Range(0,2) == 1)? + Random.Range(10, 20) : -Random.Range(10,20));
        newRoom.CenterPoint.y += center1.y+((Random.Range(0,2) == 1 )? +Random.Range(10, 20) : -Random.Range(10, 20));
       
        return newRoom;
    }

    //绘制生成的房间地板
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
