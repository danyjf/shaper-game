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
        CreateVertexIndicators();
    }
	
    private void Update() {
        if(Input.GetMouseButton(0)) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit)) {
                if(hit.transform.tag == "VertexIndicator") {
                    Transform objectHit = hit.transform;

                    objectHit.position = new Vector3(hit.point.x, hit.point.y, objectHit.position.z);
                }
            }
        }

        for(int i = 0; i < vertexIndicators.Length; i++) {
            vertices[i] = vertexIndicators[i].localPosition;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    private void CreateVertexIndicators() {
        vertexIndicators = new Transform[vertices.Length];

        for(int i = 0; i < vertices.Length; i++) {
            Vector3 worldPt = vertices[i] + targetObject.position;
            vertexIndicators[i] = Instantiate(vertexIndicator, worldPt, Quaternion.identity);
            vertexIndicators[i].SetParent(targetObject);
        }
    }
}
