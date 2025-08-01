using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class Magic_wand : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // ��ȡ�������
    }

    void Update()
    {
        RotateTowardsMouse();
    }

    private void RotateTowardsMouse()
    {
        // ��ȡ�����������꣨Z=0 ����2D��
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // ���㷽���������Ӽ�ͷβ��ָ����꣩
        Vector2 direction = (mousePos - transform.position).normalized;

        // ������ת�Ƕȣ�ʹ��Atan2��ȡ�Ƕȣ�ת��Ϊ������
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ӧ����ת����ͷ��ʼָ���ҷ��������ƫ�ƣ�
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
