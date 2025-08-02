using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Restart : MonoBehaviour
{
    public Button button;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(RestartScene);

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("MapTester");
    }
}
