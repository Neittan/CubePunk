using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour {

    public CharacterMovement character;
    
    [Header("Character movement stats")]
    
    
    [Header("Input")]
    [SerializeField] private float xInput;
    [SerializeField] private float yInput;
    private float inputDeadZone = 0.01f; 
    
    [SerializeField] private bool crouchButton;
    [SerializeField] private bool walkButton;
    [SerializeField] private bool jumpButton;
    [SerializeField] private bool dodgeButton;
    
    
    private void FixedUpdate() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        
        character.Move(xInput * Time.fixedDeltaTime, walkButton);
    }

    private void Update() {
        crouchButton = Input.GetKey(KeyCode.S);
        walkButton = Input.GetKey(KeyCode.LeftShift);
        jumpButton = Input.GetKeyDown(KeyCode.Space);
        dodgeButton = Input.GetKey(KeyCode.Q);
        
        if (jumpButton) character.Jump();
        if (crouchButton) character.Crouch(); else character.UnCrouch();
        if (dodgeButton) character.Dodge();
        
        
        
        

}

}

