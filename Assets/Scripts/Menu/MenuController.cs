using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public Transform InstructionPanel;

    public string nextLevel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKeyDown && InstructionPanel.gameObject.activeSelf)
        {
            InstructionPanel.gameObject.SetActive(false);
        }
    }

    public void OnNewGame()
    {
        SceneManager.LoadScene(nextLevel);
    }

    public void DisplayInstructions()
    {
        //InstructionPanel.gameObject.SetActive(true);
        StartCoroutine("DoDisplayInstructions");
    }

    public void Quit()
    {
        StartCoroutine("DoQuit");
    }

    public IEnumerator DoDisplayInstructions()
    {
        yield return null;
        InstructionPanel.gameObject.SetActive(true);
    }

    public IEnumerator DoQuit()
    {
        yield return null;
        Application.Quit();
    }

}
