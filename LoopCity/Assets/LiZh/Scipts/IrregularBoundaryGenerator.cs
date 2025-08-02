using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class IrregularBoundaryGenerator : MonoBehaviour
{
    public TileBase boundaryTile; // 边界瓦片（建议使用纯色）
    public float boundaryOffset = 1f; // 边界向外偏移距离

    private Tilemap sourceTilemap;
    private Tilemap boundaryTilemap;
    private Grid grid;

    void Start()
    {
        sourceTilemap = GetComponent<Tilemap>();
        grid = GetComponentInParent<Grid>();

        CreateBoundaryTilemap();
        GenerateBoundaryCollider();
    }

    void CreateBoundaryTilemap()
    {
        // 创建专用边界Tilemap
        GameObject boundaryObj = new GameObject("BoundaryTilemap");
        boundaryObj.transform.SetParent(grid.transform);
        boundaryObj.transform.localPosition = Vector3.zero;

        boundaryTilemap = boundaryObj.AddComponent<Tilemap>();
        boundaryTilemap.tileAnchor = sourceTilemap.tileAnchor;

        TilemapRenderer renderer = boundaryObj.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = -10; // 确保在原始地图下方
    }

    void GenerateBoundaryCollider()
    {
        // 步骤1: 检测所有边缘瓦片
        HashSet<Vector3Int> edgePositions = new HashSet<Vector3Int>();
        BoundsInt bounds = sourceTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (sourceTilemap.HasTile(pos))
                {
                    // 检查上下左右四个方向是否有空位
                    if (!sourceTilemap.HasTile(pos + Vector3Int.up) ||
                        !sourceTilemap.HasTile(pos + Vector3Int.down) ||
                        !sourceTilemap.HasTile(pos + Vector3Int.left) ||
                        !sourceTilemap.HasTile(pos + Vector3Int.right))
                    {
                        edgePositions.Add(pos);
                    }
                }
            }
        }

        // 步骤2: 在边缘瓦片外围生成边界
        foreach (Vector3Int edgePos in edgePositions)
        {
            // 向外扩展四个方向
            PlaceBoundaryTile(edgePos + Vector3Int.up);
            PlaceBoundaryTile(edgePos + Vector3Int.down);
            PlaceBoundaryTile(edgePos + Vector3Int.left);
            PlaceBoundaryTile(edgePos + Vector3Int.right);
        }

        // 步骤3: 添加碰撞组件
        AddCollisionComponents();
    }

    void PlaceBoundaryTile(Vector3Int position)
    {
        // 只在空位置放置边界瓦片
        if (!sourceTilemap.HasTile(position)
            && !boundaryTilemap.HasTile(position))
        {
            boundaryTilemap.SetTile(position, boundaryTile);
        }
    }

    void AddCollisionComponents()
    {
        // 添加碰撞体组件
        TilemapCollider2D tileCollider = boundaryTilemap.gameObject.AddComponent<TilemapCollider2D>();
        CompositeCollider2D compositeCollider = boundaryTilemap.gameObject.AddComponent<CompositeCollider2D>();

        // 配置刚体
        Rigidbody2D rb = boundaryTilemap.gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        //rb.constraints = RigidbodyConstraints.FreezePositionY;
        //rb.bodyType = RigidbodyType2D.Static;
        //rb.simulated = true;

        // 连接碰撞体
        tileCollider.usedByComposite = true;
        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        compositeCollider.GenerateGeometry(); // 生成优化后的碰撞体
    }
}
