using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DeformableObject : MonoBehaviour
{
    public Sprite sprite;  // Drag sprite di Inspector
    public int subdivisionsX = 10;
    public int subdivisionsY = 10;
    public float vertexSelectRadius = 0.1f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private int selectedVertex = -1;
    private Camera mainCamera;
    private Plane interactionPlane;

    void Start()
    {
        mainCamera = Camera.main;
        interactionPlane = new Plane(Vector3.back, Vector3.zero);

        GenerateMesh();
        AssignMaterial();
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertexList = new List<Vector3>();
        List<int> triangleList = new List<int>();
        List<Vector2> uvList = new List<Vector2>();

        // ?? Perbaikan ukuran mesh pakai pixelsPerUnit
        Vector2 spriteSize = new Vector2(sprite.rect.width / sprite.pixelsPerUnit, sprite.rect.height / sprite.pixelsPerUnit);

        // UV mapping tetap sama
        Rect rect = sprite.textureRect;
        float texWidth = sprite.texture.width;
        float texHeight = sprite.texture.height;
        float uMin = rect.xMin / texWidth;
        float uMax = rect.xMax / texWidth;
        float vMin = rect.yMin / texHeight;
        float vMax = rect.yMax / texHeight;

        for (int y = 0; y <= subdivisionsY; y++)
        {
            for (int x = 0; x <= subdivisionsX; x++)
            {
                float xPos = (float)x / subdivisionsX * spriteSize.x - spriteSize.x / 2;
                float yPos = (float)y / subdivisionsY * spriteSize.y - spriteSize.y / 2;

                vertexList.Add(new Vector3(xPos, yPos, 0));

                float u = Mathf.Lerp(uMin, uMax, (float)x / subdivisionsX);
                float v = Mathf.Lerp(vMin, vMax, (float)y / subdivisionsY);
                uvList.Add(new Vector2(u, v));
            }
        }

        for (int y = 0; y < subdivisionsY; y++)
        {
            for (int x = 0; x < subdivisionsX; x++)
            {
                int index = y * (subdivisionsX + 1) + x;
                triangleList.Add(index);
                triangleList.Add(index + subdivisionsX + 1);
                triangleList.Add(index + 1);

                triangleList.Add(index + 1);
                triangleList.Add(index + subdivisionsX + 1);
                triangleList.Add(index + subdivisionsX + 2);
            }
        }

        vertices = vertexList.ToArray();
        triangles = triangleList.ToArray();
        uvs = uvList.ToArray();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }


    void AssignMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = sprite.texture;
        GetComponent<MeshRenderer>().material = mat;
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (interactionPlane.Raycast(ray, out distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);

            if (Input.GetMouseButtonDown(0))
            {
                float closestDistance = float.MaxValue;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 worldVertex = transform.TransformPoint(vertices[i]);
                    float dist = Vector3.Distance(worldPoint, worldVertex);
                    if (dist < vertexSelectRadius && dist < closestDistance)
                    {
                        closestDistance = dist;
                        selectedVertex = i;
                    }
                }
            }

            if (Input.GetMouseButton(0) && selectedVertex >= 0)
            {
                vertices[selectedVertex] = transform.InverseTransformPoint(worldPoint);
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
            }

            if (Input.GetMouseButtonUp(0))
            {
                selectedVertex = -1;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (vertices == null) return;

        Gizmos.color = Color.red;
        foreach (var v in vertices)
        {
            Gizmos.DrawSphere(transform.TransformPoint(v), 0.05f);
        }
    }
}




