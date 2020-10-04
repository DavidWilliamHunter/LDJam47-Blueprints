using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Sprite Controlled;
    public Sprite Uncontrolled;
    public Sprite TryingToHold;
    public Sprite Holding;

    protected CharacterController2D controller;
    protected SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Sprite currentSprite = Uncontrolled;
        if (controller.IsHolding)
            currentSprite = Holding;
        else if (controller.WillHold)
            currentSprite = TryingToHold;
        else if (controller.PlayerControlled)
            currentSprite = Controlled;

        spriteRenderer.sprite = currentSprite;
    }
}
