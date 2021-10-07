using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaperToolController : MonoBehaviour {
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera shaperRoomCamera;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject shaperRoomUI;
    [SerializeField] private MeshModifier meshModifierScript;
    [SerializeField] private Transform shaperRoomPos;
    
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
        Cursor.lockState = CursorLockMode.None;

        Rigidbody targetRB = target.GetComponent<Rigidbody>();
        targetRB.constraints = RigidbodyConstraints.FreezeAll;
        target.position = shaperRoomPos.position;

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
        Cursor.lockState = CursorLockMode.Locked;

        Rigidbody targetRB = target.GetComponent<Rigidbody>();
        targetRB.constraints = RigidbodyConstraints.None;
        target.position = targetPrevPos;

        SwitchCameras();
        SwitchUI();

        meshModifierScript.targetObject = null;
        meshModifierScript.enabled = false;
        this.enabled = true;
    }
}
