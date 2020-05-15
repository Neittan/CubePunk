using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour {

    public CharacterMovement character;
    
    [Header("Character movement stats")]
    
    
    [Header("Input")]
    [SerializeField]private float xInput;
    [SerializeField]private float yInput;
    [SerializeField]private bool jumpDownButton;
    [SerializeField]private bool walkButton;
    
    
    private void FixedUpdate() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        character.MoveLeftRight(xInput * Time.fixedDeltaTime, walkButton);
        character.MoveUpDown(yInput * Time.fixedDeltaTime, jumpDownButton);
        
        
    }

    private void Update() {
        jumpDownButton = Input.GetKey(KeyCode.LeftShift);
        walkButton = Input.GetKey(KeyCode.LeftShift);
        if (Input.GetKeyDown(KeyCode.Space)) character.Jump();
        
        

}

}

