using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class RoomCreater : MonoBehaviour
{

    public Tilemap gameMap;
    public TileBase tile;
    public TileBase tile1;
    public TileBase tile2;
    public List<Vector2> centerPoints;

    public TileBase NextDoortile;
    public Vector2Int FurthestRoomPoint;

    public GameObject Player;
    // Start is called before the first frame update
    private void Awake()
    {
        Room startRoom = new Room(0, 0);
        startRoom.DrawFloor(gameMap, tile);
        startRoom.DrawWall(gameMap, tile1);
        centerPoints.Add(startRoom.CenterPoint);

        Room[] Rooms = new Room[101];
        Rooms[0] = startRoom;

        for (int i = 0; i < 12; i++)
        {
            //用上一个房间 创建 新房间的中心坐标
            Rooms[i + 1] = Rooms[i].CreatNewCenter(Rooms[i].CenterPoint);

            //设置遮挡保护 如果两个房间相互遮挡则让新生成的房间按照两房间中心连线方向移动。
            for(int k = 0;k<3;k++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (Vector2.Distance(Rooms[i + 1].CenterPoint, Rooms[j].CenterPoint) < Rooms[i + 1].xSize + Rooms[j].xSize ||
                        Vector2.Distance(Rooms[i + 1].CenterPoint, Rooms[j].CenterPoint) < Rooms[i + 1].ySize + Rooms[j].ySize)
                    {
                        Rooms[i + 1].CenterPoint.x += (Rooms[i + 1].CenterPoint.x - Rooms[i].CenterPoint.x) >= 0 ? 8 : -8;
                        Rooms[i + 1].CenterPoint.y += (Rooms[i + 1].CenterPoint.y - Rooms[i].CenterPoint.y) >= 0 ? 8 : -8;
                    }
                }

            }

            centerPoints.Add(Rooms[i + 1].CenterPoint);
            
            
            //绘制新生成的房间
            Rooms[i + 1].DrawFloor(gameMap, tile);
            Rooms[i + 1].DrawWall(gameMap, tile1);


        }

        CreatNextDoor(Rooms);
        //print(centerPoints.Count);
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Player.transform.position.x >= (FurthestRoomPoint.x) - 0.5 &&
            Player.transform.position.x <= (FurthestRoomPoint.x) + 0.5 &&
            Player.transform.position.y >= (FurthestRoomPoint.y) - 0.5 &&
            Player.transform.position.y <= (FurthestRoomPoint.y) + 0.5 )
        {
            SceneManager.LoadScene("MapTester");
        }
    }

    //下个关卡入口生成的方法 在离中心点最远的那个房间中心防止带有碰撞器的瓦片来监听。
    public void CreatNextDoor(Room[] Rooms)
    {
        Room FurthestRoom = Rooms[0];
        for (int i = 0; i < 12; i++)
        {
            if(Math.Abs( Rooms[i].CenterPoint.x)+ Math.Abs(Rooms[i].CenterPoint.y) <
                Math.Abs(Rooms[i+1].CenterPoint.x) + Math.Abs(Rooms[i+1].CenterPoint.y))
                FurthestRoom = Rooms[i+1];
        }
        FurthestRoomPoint =  new  Vector2Int((int) FurthestRoom.CenterPoint.x,(int)FurthestRoom.CenterPoint.y);
        //得到最远端房间

        gameMap.SetTile(new Vector3Int((int)FurthestRoom.CenterPoint.x, (int)FurthestRoom.CenterPoint.y,-1), NextDoortile);
    }
}
