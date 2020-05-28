using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterimage : MonoBehaviour {

    [SerializeField]
    private float activeTime = 0.1f;
    private float timeActivated;
    private float alpha;
    private float alphaSet = 0.8f;
    [SerializeField]
    private float alphaMult = 0.9f;
    
    private Transform player;
    private SpriteRenderer SR;
    private SpriteRenderer playerSR;

    private Color color;

    private void OnEnable() {
        SR = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player").transform;
        playerSR = player.GetComponent<SpriteRenderer>();

        alpha = alphaSet;
        SR.sprite = playerSR.sprite;
        transform.position = player.position;
        transform.rotation = player.rotation;
        transform.localScale = player.localScale;
        timeActivated = Time.time;
    }

    private void FixedUpdate() {
        alpha *= alphaMult;
        color = new Color(1,1,1,alpha);
        SR.color = color;

        if (Time.time >= (timeActivated + activeTime)) {
            PlayerAfterimagePool.Instance.AddToPool(gameObject);
        }
    }

}
