using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour {

    public CharacterController2D character;
    
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float climbSpeed;
    
    [Header("Input Axes")]
    [SerializeField]private float xInput = 0f;
    [SerializeField]private float yInput = 0f;
    
    private void Awake() {
        moveSpeed = 240f;
        climbSpeed = 120f;
        jumpHeight = 8f;
    }
    
    private void FixedUpdate() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        

        if (yInput > 0) character.MovementUp(climbSpeed * yInput * Time.fixedDeltaTime);
        if (yInput < 0) character.MovementDown(climbSpeed * yInput * Time.fixedDeltaTime);
        if (yInput == 0f && character.isCrouching) character.UnCrouch();
        if (yInput == 0 && character.isClimbing) character.ClimbIdle();

        character.Move(xInput * moveSpeed * Time.fixedDeltaTime);

    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            character.Jump(jumpHeight);
        }

        character.isWalking = Input.GetKey(KeyCode.LeftShift);

    }

}

