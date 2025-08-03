using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class EnemySpawner : MonoBehaviour
{
    [Header("敌人设置")]
    public GameObject enemyPrefab1;
    public GameObject enemyPrefab2;  // 敌人预制体
    public GameObject enemyPrefab3;  // 敌人预制体
    // 敌人预制体
    public int numberOfEnemies = 10; // 生成的敌人数量

    [Header("生成范围")]
    public bool useCustomBounds = false;
    public BoundsInt customBounds;  // 自定义生成范围

    private Tilemap targetTilemap;
    private List<Vector3> validPositions = new List<Vector3>();

    void Start()
    {
        targetTilemap = GetComponent<Tilemap>();

        // 获取有效位置
        CacheValidPositions();

        // 生成敌人
        SpawnEnemies();
    }

    // 缓存所有有效位置
    private void CacheValidPositions()
    {
        validPositions.Clear();

        // 确定生成范围
        BoundsInt bounds = useCustomBounds ? customBounds : targetTilemap.cellBounds;

        // 遍历范围内的所有单元格
        foreach (var position in bounds.allPositionsWithin)
        {
            if (targetTilemap.HasTile(position))
            {
                // 获取单元格中心位置的世界坐标
                Vector3 worldPosition = targetTilemap.GetCellCenterWorld(position);
                validPositions.Add(worldPosition+new Vector3(0,0,-2));
            }
        }

        //Debug.Log($"找到 {validPositions.Count} 个有效位置");
    }

    // 生成敌人
    private void SpawnEnemies()
    {
        if (validPositions.Count == 0)
        {
            Debug.LogWarning("没有找到有效生成位置！");
            return;
        }

        // 确保不超过可用位置数量
        int spawnCount = Mathf.Min(numberOfEnemies, validPositions.Count);
        List<Vector3> spawnPositions = new List<Vector3>(validPositions);

        for (int i = 0; i < spawnCount; i++)
        {
            // 随机选择一个位置
            int randomIndex = Random.Range(0, spawnPositions.Count);
            Vector3 spawnPos = spawnPositions[randomIndex];
            spawnPositions.RemoveAt(randomIndex);  // 避免重复位置

            // 实例化敌人
            int a = Random.Range(0, 3);
            switch(a)
            { 
                case 0:
                    Instantiate(enemyPrefab1, spawnPos, Quaternion.identity);

                    break;
                case 1:
                    Instantiate(enemyPrefab2, spawnPos, Quaternion.identity);

                    break;
                case 2:
                    Instantiate(enemyPrefab3, spawnPos, Quaternion.identity);

                    break;

            }
        }

        Debug.Log($"成功生成 {spawnCount} 个敌人");
    }

    // 在Scene视图中绘制生成范围
    void OnDrawGizmosSelected()
    {
    }
}
