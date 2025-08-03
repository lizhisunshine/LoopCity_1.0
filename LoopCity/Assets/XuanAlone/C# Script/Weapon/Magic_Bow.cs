using UnityEngine;

public class Magic_Bow : MonoBehaviour
{
    [Header("��������")]
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

        // ȷ��firePoint������
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

        // ��ȡ���λ��
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // ���㷽���������ӹ���ָ����꣩
        Vector2 direction = (mousePos - transform.position).normalized;

        // ������ת�Ƕ�
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ӧ����ת
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // ������׼�������ڷ����ʸ��
        aimDirection = direction;
    }

    private void HandleAttackInput()
    {
        if (Input.GetMouseButton(0))
        {
            // ��鹥����ȴ
            if (Time.time - lastShotTime > attackCooldown)
            {
                FireArrow();
                lastShotTime = Time.time;

                // ������ҹ�������
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

        // ʵ������ʸ
        GameObject arrow = Instantiate(
            arrowPrefab,
            firePoint.position,
            Quaternion.identity
        );

        // ���ü�ʸ����
        ArrowController arrowController = arrow.GetComponent<ArrowController>();
        if (arrowController == null)
        {
            Debug.LogWarning("Arrow prefab missing ArrowController! Adding one.");
            arrowController = arrow.AddComponent<ArrowController>();
        }

        // ���ü�ʸ����
        arrowController.SetDirection(aimDirection, arrowSpeed, bowDamage);

        // ��ת��ʸʹ�䳯����ȷ����
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}

