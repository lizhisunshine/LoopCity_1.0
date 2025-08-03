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
    private bool _isGameOver = false; // ��ֹ�ظ�����
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // ��ӳ��������¼�����
            InitializeCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCoins()
    {
        // ����Ƿ���Ҫ���ã��ϴ���Ϸ�ﵽ100��ң�
        bool needsReset = PlayerPrefs.GetInt(ResetFlagKey, 0) == 1;

        if (needsReset)
        {
            ResetCoinsImmediately(); // �������ý��
        }
        else
        {
            // ���ر���Ľ����
            _coinCount = PlayerPrefs.GetInt(CoinKey, 0);
        }

        UpdateCoinUI();
    }

    // �������ý�ң����ȴ��������أ�
    public void ResetCoinsImmediately()
    {
        _coinCount = 0;
        PlayerPrefs.SetInt(CoinKey, 0);
        PlayerPrefs.SetInt(ResetFlagKey, 0); // ������ñ�־
        UpdateCoinUI();
    }

    public void AddCoin()
    {
        _coinCount++;
        PlayerPrefs.SetInt(CoinKey, _coinCount);
        UpdateCoinUI();

        // ����Ƿ�ﵽ100���
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

        // �������ñ�־���´���Ϸ������
        PlayerPrefs.SetInt(ResetFlagKey, 1);

        ShowGameOverPanel();
    }


    // ԭ�е�GameOver������Ϊ˽�У����ڴﵽ100���ʱ����TriggerGameOver
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

        // ��ͣ��Ϸ
        Time.timeScale = 0;
    }

    // ���¿�ʼ��Ϸ
    public void RestartGame()
    {
        // �������ý��
        ResetCoinsImmediately();

        // ������Ϸ�������
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // �ָ���Ϸʱ��
        Time.timeScale = 1;

        // ���¼��س���
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        _isGameOver = false; // ���ñ�־
    }

    // ��������ʱ���°�UI
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���²���UIԪ��
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
        // �Ƴ��¼�����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}