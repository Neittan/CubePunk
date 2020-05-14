using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour {

    public CharacterMovement character;
    
    [Header("Character movement stats")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float crouchSpeedMult;
    [SerializeField] private float inJumpSpeedMult;
    [SerializeField] private float walkSpeedMult;
    [SerializeField] private float jumpForce;
    [SerializeField] private float extraJumpForceMult;
    [SerializeField] private float climbSpeed;
    
    [Header("Input Axes")]
    [SerializeField]private float xInput = 0f;
    [SerializeField]private float yInput = 0f;
    
    private void FixedUpdate() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        if (yInput > 0) character.MovementUp(climbSpeed * yInput * Time.fixedDeltaTime);
        if (yInput < 0) character.MovementDown(climbSpeed * yInput * Time.fixedDeltaTime, true);
        if (yInput == 0) {
            character.ClimbIdle();
            character.UnCrouch();
        }
        
        character.Move(xInput * moveSpeed * Time.fixedDeltaTime, crouchSpeedMult, inJumpSpeedMult, walkSpeedMult);
    }

    private void Update() {
        
        
        character.Walk(Input.GetKey(KeyCode.LeftShift));
        if (Input.GetKeyDown(KeyCode.Space)) {
            character.Jump(jumpForce, extraJumpForceMult);
        }

        
    }

}

