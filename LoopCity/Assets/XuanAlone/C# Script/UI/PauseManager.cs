using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel; // ��ͣ���
    public Button pauseButton;    // ��ͣ��ť
    public Button resumeButton;   // ������ť

    [Header("Button Images")]
    public Sprite pauseIcon;      // ��ͣͼ��
    public Sprite playIcon;       // ����ͼ��

    private bool isPaused = false;

    void Start()
    {
        // ��ʼ����ť�¼�
        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(ResumeGame);

        // ��ʼ״̬
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
        // ��ͣ��Ϸʱ��
        Time.timeScale = 0f;

        // ��ʾ��ͣ���
        pausePanel.SetActive(true);

        // ���°�ťͼ��
        UpdatePauseButtonImage();
    }

    void ResumeGame()
    {
        // �ָ���Ϸʱ��
        Time.timeScale = 1f;

        // ������ͣ���
        pausePanel.SetActive(false);

        // ����״̬
        isPaused = false;

        // ���°�ťͼ��
        UpdatePauseButtonImage();
    }

    void UpdatePauseButtonImage()
    {
        // ������ͣ״̬�л���ťͼ��
        pauseButton.image.sprite = isPaused ? playIcon : pauseIcon;
    }
}