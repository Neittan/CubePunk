using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{

    private CharacterStatus status;

    private float inputHor;
    private float inputVert;

    private void Awake()
    {
         
    }

    
    void Update()
    {
        inputHor = Input.GetAxis("Horizontal");
        inputVert = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.A)) {
            
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            
        }
    }
}
