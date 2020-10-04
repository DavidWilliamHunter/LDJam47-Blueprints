using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FinishLevel : MonoBehaviour
{
    public Transform flag;
    private ParticleSystem particles;

    private void Start()
    {
        particles = GetComponentInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterController2D controller = collision.gameObject.GetComponent<CharacterController2D>();
        if(controller)
        {
            GameObject go = GameObject.Find("LevelController");
            Assert.IsNotNull(go);

            LevelController levelController = go.GetComponent<LevelController>();

            levelController.Won();
            particles.Play();
            StartCoroutine("LowerFlag");
        }
    }

    private IEnumerator LowerFlag()
    {
        Debug.Log("You have won");
        float top = 3.84f; 
        float bottom = 0.27f;
        float loc = top;
        
        while(loc > bottom)
        {
            loc -= Time.deltaTime * 2.0f;
            flag.transform.localPosition = new Vector3(0.0f, loc, 1.0f);
            yield return null;
        }


    }
}
