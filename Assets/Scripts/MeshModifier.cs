using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshModifier : MonoBehaviour {
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Transform vertexIndicator;

    [SerializeField]
    private Transform targetObject;

    private Mesh mesh;
    private Vector3[] vertices;
    private Transform[] vertexIndicators;
    private Transform selectedVertex;

    private void Start() {
        mesh = targetObject.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        //create array of vertexIndicator objects    
        CreateVertexIndicators();
    }
	
    private void Update() {
        if(Input.GetMouseButton(0)) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit)) {
                // check if ray hit the targetObject
                if(hit.transform == targetObject) {
                    // find the mouse position in relation to the targetObject
                    Vector3 osMousePos = hit.point - targetObject.position;
                    
                    // find the closest vertex
                    int closestVertIndex = GetClosestVertex(hit.point);

                    vertices[closestVertIndex] = new Vector3(osMousePos.x, osMousePos.y, vertices[closestVertIndex].z);
                }
            }
        }

        //update the position of the vertexIndicators
        for(int i = 0; i < vertices.Length; i++) {
            vertexIndicators[i].localPosition = vertices[i];
        }

        //update the mesh itself
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    private void CreateVertexIndicators() {
        vertexIndicators = new Transform[vertices.Length];

        for(int i = 0; i < vertices.Length; i++) {
            Vector3 worldPos = vertices[i] + targetObject.position;

            vertexIndicators[i] = Instantiate(vertexIndicator, worldPos, Quaternion.identity);
            vertexIndicators[i].SetParent(targetObject);
        }
    }

    private int GetClosestVertex(Vector3 hitPosition) {
        int closestVertIndex = 0;
        Vector3 wsVert = vertices[closestVertIndex] + targetObject.position;
        float minDist = Vector3.Distance(hitPosition, wsVert);
        
        for(int i = 1; i < vertices.Length; i++) {
            wsVert = vertices[i] + targetObject.position;
            float dist = Vector3.Distance(hitPosition, wsVert);

            if(dist < minDist) {
                minDist = dist;
                closestVertIndex = i;
            }
        }

        return closestVertIndex;
    }
}
