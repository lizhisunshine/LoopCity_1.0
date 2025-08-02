using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public PlayerHealth playerHealth;

    void Start()
    {
        // ȷ���ҵ���ҽ������
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (playerHealth != null && healthSlider != null)
        {
            // ����Ѫ�����ֵ
            healthSlider.maxValue = playerHealth.maxHealth;
            
            // �ؼ��޸����������õ�ǰѪ��ֵ
            healthSlider.value = playerHealth.maxHealth;
            
            // ע���¼�
            playerHealth.onTakeDamage.AddListener(UpdateHealthBar);
            playerHealth.onDeath.AddListener(HandleDeath);
        }
        else
        {
            Debug.LogError("HealthBar: Missing playerHealth or healthSlider reference");
        }
    }

    void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            // ƽ������Ч��
            healthSlider.value = playerHealth.currentHealth;
        }
    }

    void HandleDeath()
    {
        healthSlider.value = 0;
    }

    void OnDestroy()
    {
        // ��ȫȡ���¼�����
        if (playerHealth != null)
        {
            playerHealth.onTakeDamage.RemoveListener(UpdateHealthBar);
            playerHealth.onDeath.RemoveListener(HandleDeath);
        }
    }
}