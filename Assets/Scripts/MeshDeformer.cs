using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    Vector3 point = hit.point;
                    int nearestIndex = -1;
                    float nearestDist = 0.2f; // threshold jarak ke vertex
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 worldPos = transform.TransformPoint(vertices[i]);
                        float dist = Vector3.Distance(worldPos, point);
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            nearestIndex = i;
                        }
                    }

                    if (nearestIndex != -1)
                    {
                        Vector3 localPoint = transform.InverseTransformPoint(hit.point);
                        vertices[nearestIndex] = new Vector3(localPoint.x, localPoint.y, 0);
                        mesh.vertices = vertices;
                        mesh.RecalculateBounds();

                        // Update collider
                        GetComponent<MeshCollider>().sharedMesh = null;
                        GetComponent<MeshCollider>().sharedMesh = mesh;
                    }
                }
            }
        }
    }
}
