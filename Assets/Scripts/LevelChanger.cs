using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelChanger : MonoBehaviour
{

    public String sceneName;
    public GameObject txtLoading;
    public GameObject btnPlay;

    public void OnClick()
    {
        btnPlay.SetActive(false);
        txtLoading.SetActive(true);
        new WaitForSeconds(0.4f);
        SceneManager.LoadScene(sceneName);
    }
}
