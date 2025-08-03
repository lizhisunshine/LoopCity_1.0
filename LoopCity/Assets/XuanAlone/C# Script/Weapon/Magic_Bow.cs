using UnityEngine;

public class Magic_Bow : MonoBehaviour
{
    [Header("弓箭设置")]
    public float bowDamage = 15f;
    public float attackCooldown = 0.3f;
    public float arrowSpeed = 15f;
    public GameObject arrowPrefab;
    public Transform firePoint;

    private Camera mainCamera;
    private PlayerBlackboard playerBlackboard;
    private float lastShotTime = 0f;
    private Vector2 aimDirection;

    void Start()
    {
        mainCamera = Camera.main;

        Player_FSM playerFSM = GetComponentInParent<Player_FSM>();
        if (playerFSM != null)
        {
            playerBlackboard = playerFSM.blackboard;

            if (playerBlackboard != null)
            {
                playerBlackboard.attackDamage = bowDamage;
                playerBlackboard.attackCooldown = attackCooldown;
                playerBlackboard.bulletPrefab = arrowPrefab;
            }
        }

        // 确保firePoint已设置
        if (firePoint == null)
        {
            Debug.LogError("FirePoint not assigned! Creating default.");
            firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = Vector3.right * 0.5f;
        }
    }

    void Update()
    {
        RotateTowardsMouse();
        HandleAttackInput();
    }

    private void RotateTowardsMouse()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // 获取鼠标位置
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // 计算方向向量（从弓箭指向鼠标）
        Vector2 direction = (mousePos - transform.position).normalized;

        // 计算旋转角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 应用旋转
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 保存瞄准方向（用于发射箭矢）
        aimDirection = direction;
    }

    private void HandleAttackInput()
    {
        if (Input.GetMouseButton(0))
        {
            // 检查攻击冷却
            if (Time.time - lastShotTime > attackCooldown)
            {
                FireArrow();
                lastShotTime = Time.time;

                // 触发玩家攻击动画
                if (playerBlackboard != null && playerBlackboard.animator != null)
                {
                    playerBlackboard.animator.SetTrigger("Attack");
                }
            }
        }
    }

    private void FireArrow()
    {
        if (arrowPrefab == null || firePoint == null)
        {
            Debug.LogError("Arrow prefab or fire point not assigned!");
            return;
        }

        // 实例化箭矢
        GameObject arrow = Instantiate(
            arrowPrefab,
            firePoint.position,
            Quaternion.identity
        );

        // 设置箭矢属性
        ArrowController arrowController = arrow.GetComponent<ArrowController>();
        if (arrowController == null)
        {
            Debug.LogWarning("Arrow prefab missing ArrowController! Adding one.");
            arrowController = arrow.AddComponent<ArrowController>();
        }

        // 设置箭矢方向
        arrowController.SetDirection(aimDirection, arrowSpeed, bowDamage);

        // 旋转箭矢使其朝向正确方向
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}

