using System;
using System.Runtime.CompilerServices;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

public class PlayerInput : MonoBehaviour {

    private CharacterController2D character;
    
    [Header("Input")]
    [SerializeField] private float xInput;
    [SerializeField] private float yInput;
    
    [Header("Buttons")]
    [SerializeField] private bool crouchButton;
    [SerializeField] private bool walkButton;
    [SerializeField] private bool jumpButton;
    [SerializeField] private bool dodgeButton;

    private void Awake() {
        character = GetComponent<CharacterController2D>();
    }

    private void Update() {
        CheckInput();
        ApplyMovement();
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
        character.Move(xInput * Time.fixedDeltaTime, walkButton);
    }

    private void ApplyActions() {
        if (jumpButton) character.Jump();
        if (crouchButton) character.Crouch(); else character.UnCrouch();
        if (dodgeButton) character.Dodge();
    }

}

