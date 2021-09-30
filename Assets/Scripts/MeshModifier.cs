using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshModifier : MonoBehaviour {
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform targetObject;
    [SerializeField] private Strategy strategy;
    [SerializeField] private float rotationSpeed = 5000f;
    private enum Strategy {PerVertex, AreaDeformation};
    private PerVertexStrategy perVertexStrategy;
    private AreaDeformationStrategy areaDeformationStrategy;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Transform[] vertexIndicators;
    private float originalVolume;
    private float previousVolume;
    private float newVolume;

    private void Start() {
        mesh = targetObject.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        originalVolume = VolumeOfMesh(mesh);

        perVertexStrategy = new PerVertexStrategy(mainCamera, targetObject, mesh, vertices);
        areaDeformationStrategy = new AreaDeformationStrategy(mainCamera, targetObject, mesh, vertices, originalVolume);
    }
	
    private void Update() {
        previousVolume = VolumeOfMesh(mesh);

        switch(strategy) {
            case Strategy.PerVertex:
                perVertexStrategy.EditMesh();
                break;
            case Strategy.AreaDeformation:
                areaDeformationStrategy.EditMesh(previousVolume, vertices);
                break;
        }

        newVolume = VolumeOfMesh(mesh);

        // UpdateVertexIndicators();
        bool modifyMesh = ValidateModification();
        UpdateObjectMesh(modifyMesh);
        RotateMesh();
    }

    private List<int> GetClosestVertex(Vector3 hitPosition) {
        List<int> closestCoincidentVertices = new List<int>();      // create list with the closest vertex and the coincident vertices
        closestCoincidentVertices.Add(0);                           // add first vertex as the closest vertex

        Vector3 wsVert = targetObject.TransformPoint(vertices[closestCoincidentVertices[0]]);   // get the first vertex in world space
        float minDist = Vector3.Distance(hitPosition, wsVert);                                  // get the distance from it to the mouse click
        
        for(int i = 1; i < vertices.Length; i++) {
            if(vertices[i] == vertices[closestCoincidentVertices[0]])   // check if the current vertex is coincident with the closest vertex
                closestCoincidentVertices.Add(i);                       // add list to the list if so

            wsVert = targetObject.TransformPoint(vertices[i]);          // get the world position of the current vertex
            float dist = Vector3.Distance(hitPosition, wsVert);         // get the distance to the mouse click

            if(dist < minDist) {                        // check if the distance is less than the recorded min distance
                closestCoincidentVertices.Clear();      // clear the list of the closest vertex
                closestCoincidentVertices.Add(i);       // add new closest vertex

                minDist = dist;                         // redefine the min distance
            }
        }

        return closestCoincidentVertices;
    }

    private bool ValidateModification() {
        bool isValidated = true;

        if(areaDeformationStrategy.isIntruding && newVolume > previousVolume) {
            isValidated = false;
        }

        return isValidated;
    }

    private void UpdateObjectMesh(bool modifyMesh) {
        // update the mesh itself
        if(modifyMesh) {
            mesh.vertices = vertices;
        }else {
            vertices = mesh.vertices;
        }
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    private void RotateMesh() {
        if(Input.GetMouseButton(2)) {
            Vector3 yInputRotationAxis = mainCamera.transform.right;

            targetObject.RotateAround(targetObject.position, yInputRotationAxis, Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime);
            targetObject.RotateAround(targetObject.position, Vector3.up, Input.GetAxis("Mouse X") * -rotationSpeed * Time.deltaTime);
        }
    }

    private float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;

        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    private float VolumeOfMesh(Mesh mesh) {
        float volume = 0;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }

        return Mathf.Abs(volume);
    }
}
