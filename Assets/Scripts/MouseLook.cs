using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour {
    [SerializeField] private float sensitivity;

    private Transform player;
    private float verticalRotation = 0;
    private float horizontalRotation = 0;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;

        player = GameObject.FindWithTag("Player").transform;
    }
	
    private void Update() {
        verticalRotation += -Input.GetAxis("Mouse Y") * sensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);
        horizontalRotation += Input.GetAxis("Mouse X") * sensitivity;

        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        player.localRotation = Quaternion.Euler(0, horizontalRotation, 0);
    }
}
