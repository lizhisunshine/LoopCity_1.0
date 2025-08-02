using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class IrregularBoundaryGenerator : MonoBehaviour
{
    public TileBase boundaryTile; // �߽���Ƭ������ʹ�ô�ɫ��
    public float boundaryOffset = 1f; // �߽�����ƫ�ƾ���

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
        // ����ר�ñ߽�Tilemap
        GameObject boundaryObj = new GameObject("BoundaryTilemap");
        boundaryObj.transform.SetParent(grid.transform);
        boundaryObj.transform.localPosition = Vector3.zero;

        boundaryTilemap = boundaryObj.AddComponent<Tilemap>();
        boundaryTilemap.tileAnchor = sourceTilemap.tileAnchor;

        TilemapRenderer renderer = boundaryObj.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = -10; // ȷ����ԭʼ��ͼ�·�
    }

    void GenerateBoundaryCollider()
    {
        // ����1: ������б�Ե��Ƭ
        HashSet<Vector3Int> edgePositions = new HashSet<Vector3Int>();
        BoundsInt bounds = sourceTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (sourceTilemap.HasTile(pos))
                {
                    // ������������ĸ������Ƿ��п�λ
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

        // ����2: �ڱ�Ե��Ƭ��Χ���ɱ߽�
        foreach (Vector3Int edgePos in edgePositions)
        {
            // ������չ�ĸ�����
            PlaceBoundaryTile(edgePos + Vector3Int.up);
            PlaceBoundaryTile(edgePos + Vector3Int.down);
            PlaceBoundaryTile(edgePos + Vector3Int.left);
            PlaceBoundaryTile(edgePos + Vector3Int.right);
        }

        // ����3: �����ײ���
        AddCollisionComponents();
    }

    void PlaceBoundaryTile(Vector3Int position)
    {
        // ֻ�ڿ�λ�÷��ñ߽���Ƭ
        if (!sourceTilemap.HasTile(position)
            && !boundaryTilemap.HasTile(position))
        {
            boundaryTilemap.SetTile(position, boundaryTile);
        }
    }

    void AddCollisionComponents()
    {
        // �����ײ�����
        TilemapCollider2D tileCollider = boundaryTilemap.gameObject.AddComponent<TilemapCollider2D>();
        CompositeCollider2D compositeCollider = boundaryTilemap.gameObject.AddComponent<CompositeCollider2D>();

        // ���ø���
        Rigidbody2D rb = boundaryTilemap.gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        //rb.constraints = RigidbodyConstraints.FreezePositionY;
        //rb.bodyType = RigidbodyType2D.Static;
        //rb.simulated = true;

        // ������ײ��
        tileCollider.usedByComposite = true;
        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        compositeCollider.GenerateGeometry(); // �����Ż������ײ��
    }
}
