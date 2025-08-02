using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 添加TextMeshPro命名空间

public enum WeaponType { MagicWand, EnergyBlaster, FrostScepter }

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon References")]
    public GameObject magicWand;
    public GameObject energyBlaster;
    public GameObject frostScepter;

    [Header("UI Elements")]
    public Image weaponIcon; // Image组件不变
    public TMP_Text weaponName; // 改为TMP_Text
    public TMP_Text keyHint; // 改为TMP_Text

    [Header("Weapon Sprites")]
    public Sprite wandSprite;
    public Sprite blasterSprite;
    public Sprite scepterSprite;

    private WeaponType currentWeapon = WeaponType.MagicWand;
    private Dictionary<WeaponType, GameObject> weaponObjects;

    void Start()
    {
        // 初始化武器对象字典
        weaponObjects = new Dictionary<WeaponType, GameObject>
        {
            { WeaponType.MagicWand, magicWand },
            { WeaponType.EnergyBlaster, energyBlaster },
            { WeaponType.FrostScepter, frostScepter }
        };

        // 激活初始武器
        SwitchWeapon(currentWeapon);
        UpdateUI();

        // 设置按键提示
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
        // 按顺序循环切换武器
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
        // 禁用当前武器
        weaponObjects[currentWeapon].SetActive(false);

        // 启用新武器
        currentWeapon = newWeapon;
        weaponObjects[currentWeapon].SetActive(true);

        // 更新UI
        UpdateUI();

        // 添加切换效果（可选）
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

        // 添加文本效果（可选）
        weaponName.fontStyle = FontStyles.Bold;
        weaponName.alignment = TextAlignmentOptions.Center;
    }

    // 武器切换时的视觉效果
    private IEnumerator WeaponSwitchEffect()
    {
        Vector3 originalScale = weaponIcon.transform.localScale;
        weaponIcon.transform.localScale = originalScale * 1.3f;

        yield return new WaitForSeconds(0.1f);

        weaponIcon.transform.localScale = originalScale;
    }
}