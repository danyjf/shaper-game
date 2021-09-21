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
    private List<int> closestVertIndices = new List<int>();
    private Vector3 osMousePos;
    private Plane interactionPlane;

    private void Start() {
        mesh = targetObject.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        //create array of vertexIndicator objects    
        CreateVertexIndicators();
    }
	
    private void Update() {
        EditMesh();
        UpdateVertexIndicators();
        UpdateObjectMesh();
    }

    private void CreateVertexIndicators() {
        vertexIndicators = new Transform[vertices.Length];

        for(int i = 0; i < vertices.Length; i++) {
            Vector3 worldPos = vertices[i] + targetObject.position;

            vertexIndicators[i] = Instantiate(vertexIndicator, worldPos, Quaternion.identity);
            vertexIndicators[i].SetParent(targetObject);
        }
    }

    private List<int> GetClosestVertex(Vector3 hitPosition) {
        List<int> closestCoincidentVertices = new List<int>();      //create list with the closest vertex and the coincident vertices
        closestCoincidentVertices.Add(0);                           //add first vertex as the closest vertex

        Vector3 wsVert = vertices[closestCoincidentVertices[0]] + targetObject.position;    //get the first vertex in world space
        float minDist = Vector3.Distance(hitPosition, wsVert);                              //get the distance from it to the mouse click
        
        for(int i = 1; i < vertices.Length; i++) {
            if(vertices[i] == vertices[closestCoincidentVertices[0]])   //check if the current vertex is coincident with the closest vertex
                closestCoincidentVertices.Add(i);                       //add list to the list if so

            wsVert = vertices[i] + targetObject.position;               //get the world position of the current vertex
            float dist = Vector3.Distance(hitPosition, wsVert);         //get the distance to the mouse click

            if(dist < minDist) {                        //check if the distance is less than the recorded min distance
                closestCoincidentVertices.Clear();      //clear the list of the closest vertex
                closestCoincidentVertices.Add(i);       //add new closest vertex

                minDist = dist;                         //redefine the min distance
            }
        }

        return closestCoincidentVertices;
    }

    private void UpdateObjectMesh() {
        //update the mesh itself
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    private void UpdateVertexIndicators() {
        //update the position of the vertexIndicators
        for(int i = 0; i < vertices.Length; i++) {
            vertexIndicators[i].localPosition = vertices[i];
        }
    }

    private void EditMesh() {
        //create ray from camera to mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if(Input.GetMouseButtonDown(0)) {
            //cast the ray intersecting with colliders
            if(Physics.Raycast(ray, out RaycastHit hit)) {
                //check if ray hit the targetObject
                if(hit.transform == targetObject) {
                    //find the closest vertex
                    closestVertIndices = GetClosestVertex(hit.point);
                    
                    //create plane parallel to the camera at the position of the closest vertex
                    interactionPlane = new Plane(mainCamera.transform.forward, vertices[closestVertIndices[0]] + targetObject.position);
                }
            }
        }

        //if the mouse button is still held down
        if(closestVertIndices.Count != 0) {
            //cast the ray intersecting with planes
            if(interactionPlane.Raycast(ray, out float enter)) {
                //find the mouse position in relation to the targetObject
                osMousePos = ray.GetPoint(enter) - targetObject.position;
                
                //update the position of all coincident closest vertices
                foreach(int vertIndex in closestVertIndices) {
                    vertices[vertIndex] = new Vector3(osMousePos.x, osMousePos.y, osMousePos.z);
                }
            }
        }

        if(Input.GetMouseButtonUp(0)) {
            closestVertIndices.Clear();
            targetObject.GetComponent<MeshCollider>().sharedMesh = mesh;    //update the mesh of the collider
        }
    }
}
