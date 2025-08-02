using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel; // 暂停面板
    public Button pauseButton;    // 暂停按钮
    public Button resumeButton;   // 继续按钮

    [Header("Button Images")]
    public Sprite pauseIcon;      // 暂停图标
    public Sprite playIcon;       // 播放图标

    private bool isPaused = false;

    void Start()
    {
        // 初始化按钮事件
        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(ResumeGame);

        // 初始状态
        pausePanel.SetActive(false);
        UpdatePauseButtonImage();
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    void PauseGame()
    {
        // 暂停游戏时间
        Time.timeScale = 0f;

        // 显示暂停面板
        pausePanel.SetActive(true);

        // 更新按钮图标
        UpdatePauseButtonImage();
    }

    void ResumeGame()
    {
        // 恢复游戏时间
        Time.timeScale = 1f;

        // 隐藏暂停面板
        pausePanel.SetActive(false);

        // 更新状态
        isPaused = false;

        // 更新按钮图标
        UpdatePauseButtonImage();
    }

    void UpdatePauseButtonImage()
    {
        // 根据暂停状态切换按钮图标
        pauseButton.image.sprite = isPaused ? playIcon : pauseIcon;
    }
}