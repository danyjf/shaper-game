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
    private float area = 0.2f;
    private float modificationSpeed = 0.1f;
    private List<int> vertInAreaIndices = new List<int>();
    private List<float> distances = new List<float>();

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
        distances.Clear();

        for(int i = 0; i < vertices.Length; i++) {
            Vector3 wsVert = targetObject.TransformPoint(vertices[i]);
            float dist = Vector3.Distance(hitPosition, wsVert);
            if(dist < area) {
                verticesInArea.Add(i);
                distances.Add(dist);
            }
        }

        return verticesInArea;
    }

    private void Intrude() {
        for(int i = 0; i < vertInAreaIndices.Count; i++) {
            float dist = 1 - (distances[i] / area);
            Vector3 wsVert = targetObject.TransformPoint(vertices[vertInAreaIndices[i]]);
            wsVert += mainCamera.transform.forward * modificationSpeed * dist * Time.deltaTime;
            vertices[vertInAreaIndices[i]] = targetObject.InverseTransformPoint(wsVert);
        }
    }

    private void Extrude() {
        for(int i = 0; i < vertInAreaIndices.Count; i++) {
            float dist = 1 - (distances[i] / area);
            Vector3 wsVert = targetObject.TransformPoint(vertices[vertInAreaIndices[i]]);
            wsVert -= mainCamera.transform.forward * modificationSpeed * dist * Time.deltaTime;
            vertices[vertInAreaIndices[i]] = targetObject.InverseTransformPoint(wsVert);
        }
    }
}
