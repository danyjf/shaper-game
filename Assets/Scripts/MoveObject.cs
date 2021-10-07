using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour {
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform objectHolder;
    [SerializeField] private Transform shapeableParent;
    [SerializeField] private float pickUpDistance = 3f;

    private Transform target;
    private Rigidbody targetRB;
    private bool holdingObject = false;

    private void Update() {
        if(Input.GetKeyDown(KeyCode.E)) {
            if(!holdingObject)
                PickUp();
            else
                Drop();
        }

        if(holdingObject) {
            target.position = objectHolder.position;
        }
    }

    private void PickUp() {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit)) {
            if(hit.transform.tag == "Shapeable" && Vector3.Distance(this.transform.position, hit.transform.position) < pickUpDistance) {
                target = hit.transform;
                targetRB = target.GetComponent<Rigidbody>();

                targetRB.useGravity = false;
                targetRB.freezeRotation = true;
                
                target.position = objectHolder.position;
                target.SetParent(objectHolder);

                holdingObject = true;
            }
        }
    }

    private void Drop() {
        target.SetParent(shapeableParent);
        
        targetRB.useGravity = true;
        targetRB.freezeRotation = false;

        holdingObject = false;
    }
}
