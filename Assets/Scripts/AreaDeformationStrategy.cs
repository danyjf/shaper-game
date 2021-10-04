using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AreaDeformationStrategy {
    private Camera mainCamera;
    private Transform targetObject;
    private Material targetObjectMat;
    private Mesh mesh;
    private Vector3[] vertices;
    private float originalVolume;
    private MeshCollider meshCollider;
    private float area = 0.2f;
    private float modificationSpeed = 0.1f;
    private List<int> vertInAreaIndices = new List<int>();
    private List<float> distances = new List<float>();

    public bool isIntruding;
    public bool isExtruding;

    public AreaDeformationStrategy(Camera mainCamera, Transform targetObject, Mesh mesh, Vector3[] vertices, float originalVolume) {
        this.mainCamera = mainCamera;
        this.targetObject = targetObject;
        this.targetObjectMat = targetObject.GetComponent<Renderer>().material;
        this.mesh = mesh;
        this.vertices = vertices;
        this.originalVolume = originalVolume;
        this.isIntruding = false;
        this.isExtruding = false;
        this.meshCollider = targetObject.GetComponent<MeshCollider>();
    }

    public void EditMesh(float currentVolume, Vector3[] vertices) {
        this.vertices = vertices;

        if(Input.GetMouseButtonDown(0))
            isIntruding = true;
        if(Input.GetMouseButtonUp(0))
            isIntruding = false;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit)) {
            if(hit.transform == targetObject) {
                Vector3 obMousePos = targetObject.InverseTransformPoint(hit.point);
                targetObjectMat.SetVector("_MousePosition", new Vector4(obMousePos.x, obMousePos.y, obMousePos.z, 0));
                targetObjectMat.SetFloat("_DeformationEffectArea", area);

                vertInAreaIndices = GetVerticesInArea(hit.point, area);
                
                if(isIntruding)
                    Intrude();
                else if(Input.GetMouseButton(1) && currentVolume < originalVolume)
                    Extrude();

                meshCollider.sharedMesh = mesh;
            }else {
                targetObjectMat.SetVector("_MousePosition", new Vector4(2, 2, 2, 0));
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
