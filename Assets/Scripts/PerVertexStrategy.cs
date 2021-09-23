using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerVertexStrategy {
    private Camera mainCamera;
    private Transform targetObject;
    private Mesh mesh;
    private Vector3[] vertices;
    private List<int> closestVertIndices = new List<int>();
    private Plane interactionPlane;

    public PerVertexStrategy(Camera mainCamera, Transform targetObject, Mesh mesh, Vector3[] vertices) {
        this.mainCamera = mainCamera;
        this.targetObject = targetObject;
        this.mesh = mesh;
        this.vertices = vertices;
    }

    public void EditMesh() {
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
                    interactionPlane = new Plane(mainCamera.transform.forward, targetObject.TransformPoint(vertices[closestVertIndices[0]]));
                }
            }
        }

        //if the mouse button is still held down
        if(closestVertIndices.Count != 0) {
            //cast the ray intersecting with planes
            if(interactionPlane.Raycast(ray, out float enter)) {
                //find the mouse position in relation to the targetObject
                Vector3 osMousePos = targetObject.InverseTransformPoint(ray.GetPoint(enter));
                
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

    private List<int> GetClosestVertex(Vector3 hitPosition) {
        List<int> closestCoincidentVertices = new List<int>();      //create list with the closest vertex and the coincident vertices
        closestCoincidentVertices.Add(0);                           //add first vertex as the closest vertex

        Vector3 wsVert = targetObject.TransformPoint(vertices[closestCoincidentVertices[0]]);   //get the first vertex in world space
        float minDist = Vector3.Distance(hitPosition, wsVert);                                  //get the distance from it to the mouse click
        
        for(int i = 1; i < vertices.Length; i++) {
            if(vertices[i] == vertices[closestCoincidentVertices[0]])   //check if the current vertex is coincident with the closest vertex
                closestCoincidentVertices.Add(i);                       //add list to the list if so

            wsVert = targetObject.TransformPoint(vertices[i]);          //get the world position of the current vertex
            float dist = Vector3.Distance(hitPosition, wsVert);         //get the distance to the mouse click

            if(dist < minDist) {                        //check if the distance is less than the recorded min distance
                closestCoincidentVertices.Clear();      //clear the list of the closest vertex
                closestCoincidentVertices.Add(i);       //add new closest vertex

                minDist = dist;                         //redefine the min distance
            }
        }

        return closestCoincidentVertices;
    }
}
