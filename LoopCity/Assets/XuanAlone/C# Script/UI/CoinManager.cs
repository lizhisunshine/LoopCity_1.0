using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("UI Settings")]
    public float Coin = 100;
    public TMP_Text coinText;
    public GameObject gameOverPanel;

    private const string CoinKey = "PlayerCoins";
    private const string ResetFlagKey = "CoinResetFlag";
    private int _coinCount;
    private bool _isGameOver = false; // 防止重复触发
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // 添加场景加载事件监听
            InitializeCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCoins()
    {
        // 检查是否需要重置（上次游戏达到100金币）
        bool needsReset = PlayerPrefs.GetInt(ResetFlagKey, 0) == 1;

        if (needsReset)
        {
            ResetCoinsImmediately(); // 立即重置金币
        }
        else
        {
            // 加载保存的金币数
            _coinCount = PlayerPrefs.GetInt(CoinKey, 0);
        }

        UpdateCoinUI();
    }

    // 立即重置金币（不等待场景加载）
    public void ResetCoinsImmediately()
    {
        _coinCount = 0;
        PlayerPrefs.SetInt(CoinKey, 0);
        PlayerPrefs.SetInt(ResetFlagKey, 0); // 清除重置标志
        UpdateCoinUI();
    }

    public void AddCoin()
    {
        _coinCount++;
        PlayerPrefs.SetInt(CoinKey, _coinCount);
        UpdateCoinUI();

        // 检查是否达到100金币
        if (_coinCount >= Coin )
        {
            GameOver();
        }
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = _coinCount.ToString();
        }
    }

    public void TriggerGameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        // 设置重置标志，下次游戏会清零
        PlayerPrefs.SetInt(ResetFlagKey, 1);

        ShowGameOverPanel();
    }


    // 原有的GameOver方法改为私有，并在达到100金币时调用TriggerGameOver
    private void GameOver()
    {
        TriggerGameOver();
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 暂停游戏
        Time.timeScale = 0;
    }

    // 重新开始游戏
    public void RestartGame()
    {
        // 立即重置金币
        ResetCoinsImmediately();

        // 隐藏游戏结束面板
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 恢复游戏时间
        Time.timeScale = 1;

        // 重新加载场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        _isGameOver = false; // 重置标志
    }

    // 场景加载时重新绑定UI
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重新查找UI元素
        if (coinText == null)
        {
            coinText = GameObject.FindGameObjectWithTag("CoinText")?.GetComponent<TMP_Text>();
        }

        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.FindGameObjectWithTag("GameOverPanel");
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        UpdateCoinUI();
    }

    void OnDestroy()
    {
        // 移除事件监听
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}