using System;
using System.Runtime.CompilerServices;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

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
        ApplyMovement();
    }

    private void Update() {
        CheckInput();
        ApplyActions();
    }

    private void CheckInput() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        crouchButton = Input.GetKey(KeyCode.S);
        walkButton = Input.GetKey(KeyCode.LeftShift);
        jumpButton = Input.GetKeyDown(KeyCode.Space);
        dodgeButton = Input.GetKey(KeyCode.LeftControl);
    }

    private void ApplyMovement() {
        movement.Move(xInput * Time.fixedDeltaTime, walkButton);
    }
    private void ApplyActions(){
    if (jumpButton) movement.Jump();
    movement.Crouch(crouchButton);
    if (dodgeButton) movement.Dodge();
    }

}

