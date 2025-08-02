using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // ���TextMeshPro�����ռ�

public enum WeaponType { MagicWand, EnergyBlaster, FrostScepter }

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon References")]
    public GameObject magicWand;
    public GameObject energyBlaster;
    public GameObject frostScepter;

    [Header("UI Elements")]
    public Image weaponIcon; // Image�������
    public TMP_Text weaponName; // ��ΪTMP_Text
    public TMP_Text keyHint; // ��ΪTMP_Text

    [Header("Weapon Sprites")]
    public Sprite wandSprite;
    public Sprite blasterSprite;
    public Sprite scepterSprite;

    private WeaponType currentWeapon = WeaponType.MagicWand;
    private Dictionary<WeaponType, GameObject> weaponObjects;

    void Start()
    {
        // ��ʼ�����������ֵ�
        weaponObjects = new Dictionary<WeaponType, GameObject>
        {
            { WeaponType.MagicWand, magicWand },
            { WeaponType.EnergyBlaster, energyBlaster },
            { WeaponType.FrostScepter, frostScepter }
        };

        // �����ʼ����
        SwitchWeapon(currentWeapon);
        UpdateUI();

        // ���ð�����ʾ
        keyHint.text = "E";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CycleWeapon();
        }
    }

    private void CycleWeapon()
    {
        // ��˳��ѭ���л�����
        switch (currentWeapon)
        {
            case WeaponType.MagicWand:
                SwitchWeapon(WeaponType.EnergyBlaster);
                break;
            case WeaponType.EnergyBlaster:
                SwitchWeapon(WeaponType.FrostScepter);
                break;
            case WeaponType.FrostScepter:
                SwitchWeapon(WeaponType.MagicWand);
                break;
        }
    }

    private void SwitchWeapon(WeaponType newWeapon)
    {
        // ���õ�ǰ����
        weaponObjects[currentWeapon].SetActive(false);

        // ����������
        currentWeapon = newWeapon;
        weaponObjects[currentWeapon].SetActive(true);

        // ����UI
        UpdateUI();

        // ����л�Ч������ѡ��
        StartCoroutine(WeaponSwitchEffect());
    }

    private void UpdateUI()
    {
        switch (currentWeapon)
        {
            case WeaponType.MagicWand:
                weaponIcon.sprite = wandSprite;
                weaponName.text = "Magic Wand";
                weaponName.color = new Color(1f, 0.95f, 0.4f); 
                break;
            case WeaponType.EnergyBlaster:
                weaponIcon.sprite = blasterSprite;
                weaponName.text = "Magic Book";
                weaponName.color = new Color(0.2f, 0.8f, 1f); 
                break;
            case WeaponType.FrostScepter:
                weaponIcon.sprite = scepterSprite;
                weaponName.text = "Magic Bow";
                weaponName.color = new Color(1f, 0.4f, 0.6f);
                break;
        }

        // ����ı�Ч������ѡ��
        weaponName.fontStyle = FontStyles.Bold;
        weaponName.alignment = TextAlignmentOptions.Center;
    }

    // �����л�ʱ���Ӿ�Ч��
    private IEnumerator WeaponSwitchEffect()
    {
        Vector3 originalScale = weaponIcon.transform.localScale;
        weaponIcon.transform.localScale = originalScale * 1.3f;

        yield return new WaitForSeconds(0.1f);

        weaponIcon.transform.localScale = originalScale;
    }
}