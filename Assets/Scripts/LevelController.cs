using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public string menuSceneName = "MainMenu";
    public string nextLevelName = "Level1";
    public GameObject Winner;
    public GameObject Looser;

    public bool IsOver = false;

    private void OnEnable()
    {
        IsOver = false;
    }

    public void Won()
    {
        if (!IsOver)
        {
            Winner.SetActive(true);
            StartCoroutine("WinCooldown");
            IsOver = true;
        }
    }

    private IEnumerator WinCooldown()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(nextLevelName);
    }

    internal void Lose()
    {
        if (!IsOver)
        {
            Looser.SetActive(true);
            StartCoroutine("LoseCooldown");
            IsOver = true;
        }
    }

    private IEnumerator LoseCooldown()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(menuSceneName);
    }
}
