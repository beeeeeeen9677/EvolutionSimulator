using UnityEngine;

public class TestRangeEffect : MonoBehaviour
{
    public float radius = 5f;
    public int segments = 50; //segments: The number of segments used to approximate the circle
    public Material boundaryMaterial;
    public Material fillMaterial;

    private void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 1];  // Stores the positions of the vertices
        int[] triangles = new int[segments * 3]; // Stores the indices that define the triangles of the mesh




        vertices[0] = Vector3.zero; // The center vertex is set to (0, 0, 0)
        for (int i = 0; i < segments; i++)
        {
            // A loop calculates the positions of the vertices around the circle
            // using trigonometric functions (Mathf.Cos and Mathf.Sin) to create a circular shape

            float angle = (float)i / segments * Mathf.PI * 2;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            // Another loop defines the triangles that make up the circle.
            // Each triangle is defined by three indices:

            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        // Assign materials
        meshRenderer.materials = new Material[] { fillMaterial, boundaryMaterial };
    }
}