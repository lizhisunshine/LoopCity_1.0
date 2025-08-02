using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class Magic_wand : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // 获取主摄像机
    }

    void Update()
    {
        RotateTowardsMouse();
    }

    private void RotateTowardsMouse()
    {
        // 获取鼠标的世界坐标（Z=0 用于2D）
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // 计算方向向量（从箭头尾部指向鼠标）
        Vector2 direction = (mousePos - transform.position).normalized;

        // 计算旋转角度（使用Atan2获取角度，转换为度数）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 应用旋转（箭头初始指向右方，需额外偏移）
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
