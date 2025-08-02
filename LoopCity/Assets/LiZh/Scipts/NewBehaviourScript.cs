using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NewBehaviourScript : MonoBehaviour
{
        public TileBase boundaryTile; // �߽�ʹ�õ���Ƭ
        public bool generateOnStart = true;
        public bool showBoundaryTiles = true;
        public bool useCompositeCollider = true;

        private Tilemap sourceTilemap;
        public Tilemap boundaryTilemap;
        private Grid grid;
        private bool hasGenerated = false; // ȷ��ִֻ��һ��

        void Start()
        {
            sourceTilemap = GetComponent<Tilemap>();
            grid = GetComponentInParent<Grid>();

            if (generateOnStart)
            {
                CreateBoundaryTilemap();
            }
        }

        void Update()
        {
            // ȷ��ִֻ��һ��
            if (generateOnStart && !hasGenerated)
            {
                GenerateBoundary();
                hasGenerated = true;
            }
        }

        // ��Update��ִֻ��һ�εı߽����ɷ���
        public void GenerateBoundary()
        {
            if (boundaryTilemap == null)
            {
                CreateBoundaryTilemap();
            }

            // ����ɱ߽�
            boundaryTilemap.ClearAllTiles();

            // ��ȡԭʼTilemap��������Чλ��
            BoundsInt bounds = sourceTilemap.cellBounds;
            HashSet<Vector3Int> tilePositions = new HashSet<Vector3Int>();

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (sourceTilemap.HasTile(pos))
                    {
                        tilePositions.Add(pos);
                    }
                }
            }

            // �ҳ����б߽�λ��
            HashSet<Vector3Int> boundaryPositions = new HashSet<Vector3Int>();

            // ʹ�ð˷�����ȷ����ȫ����
            Vector3Int[] directions = {
            Vector3Int.up, Vector3Int.down,
            Vector3Int.left, Vector3Int.right,
            new Vector3Int(1, 1, 0), new Vector3Int(-1, 1, 0),
            new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0)
        };

            foreach (Vector3Int tilePos in tilePositions)
            {
                foreach (Vector3Int dir in directions)
                {
                    Vector3Int checkPos = tilePos + dir;

                    // ������λ��û��ԭʼ��Ƭ������Ϊ�߽��ѡ
                    if (!tilePositions.Contains(checkPos))
                    {
                        boundaryPositions.Add(checkPos);
                    }
                }
            }

            // ���˵�����Χ��λ�ã���ѡ�Ż���
            List<Vector3Int> positionsToRemove = new List<Vector3Int>();
            foreach (Vector3Int pos in boundaryPositions)
            {
                bool surrounded = true;
                foreach (Vector3Int dir in directions)
                {
                    if (!boundaryPositions.Contains(pos + dir))
                    {
                        surrounded = false;
                        break;
                    }
                }

                if (surrounded)
                {
                    positionsToRemove.Add(pos);
                }
            }

            foreach (Vector3Int pos in positionsToRemove)
            {
                boundaryPositions.Remove(pos);
            }

            // ���ñ߽���Ƭ
            foreach (Vector3Int pos in boundaryPositions)
            {
                boundaryTilemap.SetTile(pos, boundaryTile);
            }

            // �����ײ���
            AddCollisionComponents();

            // ��ѡ�����ر߽���Ƭ��ֻ������ײ��
            if (!showBoundaryTiles && boundaryTilemap.TryGetComponent<TilemapRenderer>(out var renderer))
            {
                renderer.enabled = false;
            }

            Debug.Log($"�߽�������ɣ������� {boundaryPositions.Count} ���߽���Ƭ");
        }

        void CreateBoundaryTilemap()
        {
            // ���һ򴴽��߽�Tilemap
            Transform boundaryTransform = grid.transform.Find("BoundaryTilemap");

            if (boundaryTransform != null)
            {
                boundaryTilemap = boundaryTransform.GetComponent<Tilemap>();
                return;
            }

            GameObject boundaryObj = new GameObject("BoundaryTilemap");
            boundaryObj.transform.SetParent(grid.transform);
            boundaryObj.transform.localPosition = Vector3.zero;
            boundaryTilemap = boundaryObj.AddComponent<Tilemap>();

            if (showBoundaryTiles)
            {
                TilemapRenderer renderer = boundaryObj.AddComponent<TilemapRenderer>();
                renderer.sortingOrder = -10; // ȷ����ԭʼ��ͼ�·�
            }

            //boundaryObj.layer = LayerMask.NameToLayer("Boundary");
        }

        void AddCollisionComponents()
        {
            // �Ƴ������
            RemoveCollisionComponents();

            // ���Tilemap��ײ��
            TilemapCollider2D tileCollider = boundaryTilemap.gameObject.AddComponent<TilemapCollider2D>();

            //if (useCompositeCollider)
            //{
            //    // ��Ӹ�����ײ��
            //    CompositeCollider2D compositeCollider = boundaryTilemap.gameObject.AddComponent<CompositeCollider2D>();
            //    //Rigidbody2D rb = boundaryTilemap.gameObject.AddComponent<Rigidbody2D>();

            //    //if (rb != null)
            //    //{
            //    //rb.bodyType = RigidbodyType2D.Static;
            //    //}
            //    //tileCollider.usedByComposite = true;

            //    compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            //    compositeCollider.GenerateGeometry();
            //}
        }

        void RemoveCollisionComponents()
        {
            TilemapCollider2D collider = boundaryTilemap.GetComponent<TilemapCollider2D>();
            if (collider != null) Destroy(collider);

            CompositeCollider2D composite = boundaryTilemap.GetComponent<CompositeCollider2D>();
            if (composite != null) Destroy(composite);

            //Rigidbody2D rb = boundaryTilemap.GetComponent<Rigidbody2D>();
            //if (rb != null) Destroy(rb);
        }

        // �༭������
        [ContextMenu("���ɱ߽�")]
        public void GenerateBoundaryEditor()
        {
            if (Application.isPlaying) return;

#if UNITY_EDITOR
            if (sourceTilemap == null) sourceTilemap = GetComponent<Tilemap>();
            if (grid == null) grid = GetComponentInParent<Grid>();

            CreateBoundaryTilemap();
            GenerateBoundary();
            UnityEditor.EditorUtility.SetDirty(boundaryTilemap);
#endif
        }
    
}
