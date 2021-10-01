using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour {
    [SerializeField] private Transform groundChecker;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckerRadius = 0.4f;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float jumpHeight = 2f;
    
    private CharacterController controller;
    private bool isGrounded;
    private Vector3 velocity;
    private float verticalVelocity;
    
    private void Start() {
        controller = GetComponent<CharacterController>();
    }
	
    private void Update() {
        isGrounded = Physics.CheckSphere(groundChecker.position, groundCheckerRadius, groundLayer);

        if(isGrounded && verticalVelocity > 0)
            verticalVelocity = 2f;

        if(Input.GetButtonDown("Jump") && isGrounded) {
            float jumpForce = Mathf.Sqrt(jumpHeight * 2f * gravity);
            verticalVelocity = -jumpForce;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.forward * verticalInput + transform.right * horizontalInput;
        controller.Move(move * movementSpeed * Time.deltaTime);

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.down * verticalVelocity * Time.deltaTime);
    }
}
