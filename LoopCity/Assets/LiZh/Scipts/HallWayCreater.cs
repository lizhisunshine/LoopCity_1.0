using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HallWayCreater : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase hallwaytile;
    public RoomCreater roomCreater;
    //得到当前所有房间中心点 放进一个列表里面
    List<Vector2> pointList;
    List<Vector2> pointList2;

    //int maxX =0;
    //int minX=0;
    //int maxY=0;
    //int minY=0;
    public void Start()
    {
        //获取房间中心点列表
        pointList = GetComponent<RoomCreater>().centerPoints;

        if (pointList != null)
        {
            print(pointList.Count);
        }

        //通过遍历得到当前房间中心点集合的x和y的极值
        //for (int i = 0; i < pointList.Count; i++)
        //{
        //    if (pointList[i].x > 0 && pointList[i].x > maxX)
        //    {
        //        maxX = (int)pointList[i].x;
        //    }
        //    if (pointList[i].x < 0 && pointList[i].x < minY)
        //    {
        //        maxX = (int)pointList[i].x;
        //    }
        //    if (pointList[i].y > 0 && pointList[i].x > maxY)
        //    {
        //        maxX = (int)pointList[i].x;
        //    }
        //    if (pointList[i].y < 0 && pointList[i].x < minY)
        //    {
        //        maxX = (int)pointList[i].x;
        //    }
        //}

        //while( pointList.Count!=0 )
        //{ 
        //    Vector2 NowHighestPoint = new Vector2(0,-10000);
        //    for (int j = 0; j < pointList.Count; j++)
        //    {
        //        if (pointList[j].y > NowHighestPoint.y)
        //        { 
        //            NowHighestPoint=pointList[j];
        //        }
        //        //pointList.Remove(pointList[j]);
        //    }
        //    pointList2.Add(NowHighestPoint);

        //    pointList.Remove(NowHighestPoint);
        //}

        pointList.Sort((a, b) => b.y.CompareTo(a.y));
        if (pointList == null)
        {
            print("111");
        }

        for (int i = 0; i < pointList.Count-1; i++)
        {
            //if (pointList[i].y != pointList[i+1].y)

            for (int j = (int)pointList[i].y; j > (int)pointList[i + 1].y-2; j--)
            {
                tilemap.SetTile(new Vector3Int((int)pointList[i].x, j, 0), hallwaytile);

                tilemap.SetTile(new Vector3Int((int)pointList[i].x+1, j, 0), hallwaytile);
                tilemap.SetTile(new Vector3Int((int)pointList[i].x-1, j, 0), hallwaytile);

            }

            if ((int)pointList[i].x> (int)pointList[i + 1].x)
            {
                for (int j = (int)pointList[i].x; j > (int)pointList[i + 1].x; j--)
                { 
                    tilemap.SetTile(new Vector3Int(j,(int)pointList[i+1].y, 0), hallwaytile);

                    tilemap.SetTile(new Vector3Int(j, (int)pointList[i + 1].y+1, 0), hallwaytile);
                    tilemap.SetTile(new Vector3Int(j, (int)pointList[i + 1].y-1, 0), hallwaytile);

                }
            }

            if ((int)pointList[i].x < (int)pointList[i + 1].x)
            {
                for (int j = (int)pointList[i].x; j < (int)pointList[i + 1].x; j++)
                {
                    tilemap.SetTile(new Vector3Int(j, (int)pointList[i + 1].y, 0), hallwaytile);

                    tilemap.SetTile(new Vector3Int(j, (int)pointList[i + 1].y - 1, 0), hallwaytile);
                    tilemap.SetTile(new Vector3Int(j, (int)pointList[i + 1].y + 1, 0), hallwaytile);

                }
            }

        }
    }
}
