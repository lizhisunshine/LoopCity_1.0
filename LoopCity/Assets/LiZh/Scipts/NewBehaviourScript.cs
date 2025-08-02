using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NewBehaviourScript : MonoBehaviour
{
        public TileBase boundaryTile; // 边界使用的瓦片
        public bool generateOnStart = true;
        public bool showBoundaryTiles = true;
        public bool useCompositeCollider = true;

        private Tilemap sourceTilemap;
        public Tilemap boundaryTilemap;
        private Grid grid;
        private bool hasGenerated = false; // 确保只执行一次

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
            // 确保只执行一次
            if (generateOnStart && !hasGenerated)
            {
                GenerateBoundary();
                hasGenerated = true;
            }
        }

        // 在Update中只执行一次的边界生成方法
        public void GenerateBoundary()
        {
            if (boundaryTilemap == null)
            {
                CreateBoundaryTilemap();
            }

            // 清除旧边界
            boundaryTilemap.ClearAllTiles();

            // 获取原始Tilemap的所有有效位置
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

            // 找出所有边界位置
            HashSet<Vector3Int> boundaryPositions = new HashSet<Vector3Int>();

            // 使用八方向检测确保完全覆盖
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

                    // 如果这个位置没有原始瓦片，就作为边界候选
                    if (!tilePositions.Contains(checkPos))
                    {
                        boundaryPositions.Add(checkPos);
                    }
                }
            }

            // 过滤掉被包围的位置（可选优化）
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

            // 放置边界瓦片
            foreach (Vector3Int pos in boundaryPositions)
            {
                boundaryTilemap.SetTile(pos, boundaryTile);
            }

            // 添加碰撞组件
            AddCollisionComponents();

            // 可选：隐藏边界瓦片（只保留碰撞）
            if (!showBoundaryTiles && boundaryTilemap.TryGetComponent<TilemapRenderer>(out var renderer))
            {
                renderer.enabled = false;
            }

            Debug.Log($"边界生成完成，共放置 {boundaryPositions.Count} 个边界瓦片");
        }

        void CreateBoundaryTilemap()
        {
            // 查找或创建边界Tilemap
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
                renderer.sortingOrder = -10; // 确保在原始地图下方
            }

            //boundaryObj.layer = LayerMask.NameToLayer("Boundary");
        }

        void AddCollisionComponents()
        {
            // 移除旧组件
            RemoveCollisionComponents();

            // 添加Tilemap碰撞体
            TilemapCollider2D tileCollider = boundaryTilemap.gameObject.AddComponent<TilemapCollider2D>();

            //if (useCompositeCollider)
            //{
            //    // 添加复合碰撞体
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

        // 编辑器工具
        [ContextMenu("生成边界")]
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
