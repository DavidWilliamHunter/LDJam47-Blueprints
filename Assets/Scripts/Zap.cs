using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Zap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterController2D controller = collision.gameObject.GetComponent<CharacterController2D>();
        if (controller)
        {
            GameObject go = GameObject.Find("LevelController");
            Assert.IsNotNull(go);

            LevelController levelController = go.GetComponent<LevelController>();

            levelController.Lose();

            AudioController audio = collision.GetComponent<AudioController>();
            audio.PlayClip(audio.zap);
        }
    }
}
