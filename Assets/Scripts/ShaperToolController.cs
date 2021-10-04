using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaperToolController : MonoBehaviour {
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera shaperRoomCamera;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject shaperRoomUI;
    [SerializeField] private MeshModifier meshModifierScript;
    [SerializeField] private Transform shapeableRoomPos;
    
    private Transform target;
	private Vector3 targetPrevPos;

    private void Update() {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit)) {
                if(hit.transform.tag == "Shapeable") {
                    target = hit.transform;
                    targetPrevPos = hit.transform.position;
                    ShaperRoomSetup();
                }
            }
        }
    }

    private void ShaperRoomSetup() {
        target.position = shapeableRoomPos.position;
        Cursor.lockState = CursorLockMode.None;

        SwitchCameras();
        SwitchUI();

        meshModifierScript.targetObject = target;
        meshModifierScript.enabled = true;
        this.enabled = false;
    }

    private void SwitchCameras() {
        playerCamera.enabled = !playerCamera.enabled;
        shaperRoomCamera.enabled = !shaperRoomCamera.enabled;
    }

    private void SwitchUI() {
        gameUI.SetActive(!gameUI.activeSelf);
        shaperRoomUI.SetActive(!shaperRoomUI.activeSelf);
    }

    public void ApplyShape() {
        target.position = targetPrevPos;
        Cursor.lockState = CursorLockMode.Locked;

        SwitchCameras();
        SwitchUI();

        meshModifierScript.targetObject = null;
        meshModifierScript.enabled = false;
        this.enabled = true;
    }
}
