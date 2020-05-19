using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour {

    private CharacterMovement movement;
    
    [Header("Input")]
    [SerializeField] private float xInput;
    [SerializeField] private float yInput;
    
    [Header("Buttons")]
    [SerializeField] private bool crouchButton;
    [SerializeField] private bool walkButton;
    [SerializeField] private bool jumpButton;
    [SerializeField] private bool dodgeButton;

    private void Awake() {
        movement = GetComponent<CharacterMovement>();
    }

    private void FixedUpdate() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        
        movement.Move(xInput * Time.fixedDeltaTime, walkButton);
    }

    private void Update() {
        crouchButton = Input.GetKey(KeyCode.S);
        walkButton = Input.GetKey(KeyCode.LeftShift);
        jumpButton = Input.GetKeyDown(KeyCode.Space);
        dodgeButton = Input.GetKey(KeyCode.Q);
        
        if (jumpButton) movement.Jump();
        if (crouchButton) movement.Crouch(); else movement.UnCrouch();
        if (dodgeButton) movement.Dodge();
        
        
        
        

}

}

