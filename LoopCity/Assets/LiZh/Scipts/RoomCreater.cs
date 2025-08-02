using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomCreater : MonoBehaviour
{
    public Tilemap gameMap;
    public TileBase tile;
    public TileBase tile1;
    public List<Vector2> centerPoints;

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
            //����һ������ ���� �·������������
            Rooms[i + 1] = Rooms[i].CreatNewCenter(Rooms[i].CenterPoint);

            //�����ڵ����� ������������໥�ڵ����������ɵķ��䰴���������������߷����ƶ���
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

            //���������ɵķ���
            Rooms[i + 1].DrawFloor(gameMap, tile);
            Rooms[i + 1].DrawWall(gameMap, tile1);

        }
        //print(centerPoints.Count);
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
