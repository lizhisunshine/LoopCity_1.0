using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class EnemySpawner : MonoBehaviour
{
    [Header("��������")]
    public GameObject enemyPrefab1;
    public GameObject enemyPrefab2;  // ����Ԥ����
    public GameObject enemyPrefab3;  // ����Ԥ����
    // ����Ԥ����
    public int numberOfEnemies = 10; // ���ɵĵ�������

    [Header("���ɷ�Χ")]
    public bool useCustomBounds = false;
    public BoundsInt customBounds;  // �Զ������ɷ�Χ

    private Tilemap targetTilemap;
    private List<Vector3> validPositions = new List<Vector3>();

    void Start()
    {
        targetTilemap = GetComponent<Tilemap>();

        // ��ȡ��Чλ��
        CacheValidPositions();

        // ���ɵ���
        SpawnEnemies();
    }

    // ����������Чλ��
    private void CacheValidPositions()
    {
        validPositions.Clear();

        // ȷ�����ɷ�Χ
        BoundsInt bounds = useCustomBounds ? customBounds : targetTilemap.cellBounds;

        // ������Χ�ڵ����е�Ԫ��
        foreach (var position in bounds.allPositionsWithin)
        {
            if (targetTilemap.HasTile(position))
            {
                // ��ȡ��Ԫ������λ�õ���������
                Vector3 worldPosition = targetTilemap.GetCellCenterWorld(position);
                validPositions.Add(worldPosition+new Vector3(0,0,-2));
            }
        }

        //Debug.Log($"�ҵ� {validPositions.Count} ����Чλ��");
    }

    // ���ɵ���
    private void SpawnEnemies()
    {
        if (validPositions.Count == 0)
        {
            Debug.LogWarning("û���ҵ���Ч����λ�ã�");
            return;
        }

        // ȷ������������λ������
        int spawnCount = Mathf.Min(numberOfEnemies, validPositions.Count);
        List<Vector3> spawnPositions = new List<Vector3>(validPositions);

        for (int i = 0; i < spawnCount; i++)
        {
            // ���ѡ��һ��λ��
            int randomIndex = Random.Range(0, spawnPositions.Count);
            Vector3 spawnPos = spawnPositions[randomIndex];
            spawnPositions.RemoveAt(randomIndex);  // �����ظ�λ��

            // ʵ��������
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

        Debug.Log($"�ɹ����� {spawnCount} ������");
    }

    // ��Scene��ͼ�л������ɷ�Χ
    void OnDrawGizmosSelected()
    {
    }
}
