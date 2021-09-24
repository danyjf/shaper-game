using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AreaDeformationStrategy {
    private Camera mainCamera;
    private Transform targetObject;
    private Mesh mesh;
    private Vector3[] vertices;
    private MeshCollider meshCollider;
    private float area = 0.1f;
    private float modificationSpeed = 0.1f;
    private List<int> vertInAreaIndices = new List<int>();

    public AreaDeformationStrategy(Camera mainCamera, Transform targetObject, Mesh mesh, Vector3[] vertices) {
        this.mainCamera = mainCamera;
        this.targetObject = targetObject;
        this.mesh = mesh;
        this.vertices = vertices;
        this.meshCollider = targetObject.GetComponent<MeshCollider>();
    }

    public void EditMesh() {
        if(Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit)) {
                if(hit.transform == targetObject) {
                    vertInAreaIndices = GetVerticesInArea(hit.point, area);

                    if(Input.GetMouseButton(0))
                        Intrude();
                    else
                        Extrude();

                    meshCollider.sharedMesh = mesh;
                }
            }
        }
    }

    private List<int> GetVerticesInArea(Vector3 hitPosition, float area) {
        List<int> verticesInArea = new List<int>();

        for(int i = 0; i < vertices.Length; i++) {
            Vector3 wsVert = targetObject.TransformPoint(vertices[i]);
            float dist = Vector3.Distance(hitPosition, wsVert);
            if(dist < area) {
                verticesInArea.Add(i);
            }
        }

        return verticesInArea;
    }

    private void Intrude() {
        foreach(int vertIndex in vertInAreaIndices) {
            vertices[vertIndex] += mainCamera.transform.forward * modificationSpeed * Time.deltaTime;
        }
    }

    private void Extrude() {
        foreach(int vertIndex in vertInAreaIndices) {
            vertices[vertIndex] -= mainCamera.transform.forward * modificationSpeed * Time.deltaTime;
        }
    }
}
