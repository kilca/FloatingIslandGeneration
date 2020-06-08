using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshEdit))]
public class MeshEditEditor : Editor
{


}
public class MeshEdit : MonoBehaviour
{


    private Mesh mesh;

    public bool invertFaces = false;
    private float hauteur = 0.13f;

    public float hauteurExtrusion = 1.0f;

    public int segments = 1;

    private Vector3[] verticesOriginal;
    private int[] trianglesOriginal;

    [Tooltip("taux de non applatissement du bas du terrain")]
    public float inverseNoise = 1.2f;


    [Tooltip("Divide meshes in submeshes to generate more triangles")]
    [Range(0, 10)]
    public int subdivisionLevel = 2;

    [Tooltip("Repeat the process this many times")]
    [Range(0, 10)]
    public int timesToSubdivide = 2;

    private List<int> triangleBas;

    public int coordX = 1;
    public int coordY = 4;

    private Material mat;

    // Start is called before the first frame update

    private SkyIslandGenerator skyGenerator;

    public void Init()
    {

        skyGenerator = transform.parent.GetComponent<SkyIslandGenerator>();

        this.hauteur = skyGenerator.hauteur;
        this.mat = skyGenerator.matBas;

        mesh = GetComponent<MeshFilter>().mesh;

        Debug.Log("hauteur : " + hauteur);

        RemoveVertices();
        Smooth();
        MyExtrusion();
        TestCouleur();
        ReverseNormals();
        
        
    }

    public void TestCouleur()
    {

        Vector3[] verts = mesh.vertices;

        Vector2[] uvs = mesh.uv;

        Material[] mats = new Material[2];
        mats[0] = GetComponent<MeshRenderer>().materials[0];
        mats[1] = mat;
        GetComponent<MeshRenderer>().materials = mats;

        mesh.subMeshCount = 2;
        List<Vector3> vertices2 = new List<Vector3>(verticesOriginal);
        List<int> triangle2 = new List<int>(trianglesOriginal);

        List<int> newVerts = new List<int>();

        mesh.SetTriangles(triangleBas.ToArray(), 1);
    }

    public void Smooth()
    {

        Mesh sourceMesh;
        Mesh workingMesh;
        MeshFilter meshfilter = gameObject.GetComponentInChildren<MeshFilter>();

        // Clone the cloth mesh to work on
        sourceMesh = new Mesh();
        // Get the sourceMesh from the originalSkinnedMesh
        sourceMesh = meshfilter.mesh;
        // Clone the sourceMesh 
        workingMesh = CloneMesh(sourceMesh);
        // Reference workingMesh to see deformations
        meshfilter.mesh = workingMesh;


        // Apply Laplacian Smoothing Filter to Mesh
        int iterations = 1;
        for (int i = 0; i < iterations; i++)
            //workingMesh.vertices = SmoothFilter.laplacianFilter(workingMesh.vertices, workingMesh.triangles);
            workingMesh.vertices = SmoothFilter.hcFilter(sourceMesh.vertices, workingMesh.vertices, workingMesh.triangles, 0.0f, 0.5f);


        mesh = workingMesh;



    }

    private static Mesh CloneMesh(Mesh mesh)
    {
        Mesh clone = new Mesh();
        clone.vertices = mesh.vertices;
        clone.normals = mesh.normals;
        clone.tangents = mesh.tangents;
        clone.triangles = mesh.triangles;
        clone.uv = mesh.uv;
        clone.uv2 = mesh.uv2;
        clone.bindposes = mesh.bindposes;
        clone.boneWeights = mesh.boneWeights;
        clone.bounds = mesh.bounds;
        clone.colors = mesh.colors;
        clone.name = mesh.name;
        //TODO : Are we missing anything?
        return clone;
    }



    public void ReverseNormals()
    {
        MeshFilter filter = GetComponent(typeof(MeshFilter)) as MeshFilter;
        if (filter != null)
        {
            Mesh mesh = filter.mesh;

            Vector3[] normals = mesh.normals;
            for (int i = verticesOriginal.Length; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles2 = mesh.GetTriangles(m);
                for (int i = verticesOriginal.Length; i < triangles2.Length; i += 3)
                {
                    int temp = triangles2[i + 0];
                    triangles2[i + 0] = triangles2[i + 1];
                    triangles2[i + 1] = temp;
                }
                mesh.SetTriangles(triangles2, m);
            }
        }

        mesh.RecalculateNormals();



    }

    public void MyExtrusion()
    {

        verticesOriginal = mesh.vertices;
        trianglesOriginal = mesh.triangles;
        int[] trianglesIteration = mesh.triangles;

        List<Vector3> boundary = new List<Vector3>();


        var boundaryPath = EdgeHelpers.GetEdges(mesh.triangles).FindBoundary().SortEdges();



        List<Vector3> newVertices = new List<Vector3>();
        foreach (Vector3 v in verticesOriginal)
        {
            newVertices.Add(v);
        }

        foreach (Vector3 v in verticesOriginal)
        {
            newVertices.Add(v + new Vector3(0, -hauteurExtrusion, 0) + new Vector3(0, -inverseNoise * v.y, 0));
        }

        List<int> newTriangles = new List<int>();
        foreach (int t in trianglesOriginal)
        {
            newTriangles.Add(t);
        }
        foreach (int t in trianglesOriginal)
        {
            newTriangles.Add(t + verticesOriginal.Length);
        }



        //step 4

        foreach (EdgeHelpers.Edge edge in boundaryPath)
        {
            newTriangles.Add(edge.v1);
            newTriangles.Add(edge.v2);
            newTriangles.Add(edge.v2 + verticesOriginal.Length);

            newTriangles.Add(edge.v2 + verticesOriginal.Length);
            newTriangles.Add(edge.v1 + verticesOriginal.Length);
            newTriangles.Add(edge.v1);


        }
        triangleBas = new List<int>();

        for (int i = trianglesOriginal.Length; i < newTriangles.Count; i++)
        {
            triangleBas.Add(newTriangles[i]);
        }


        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();

        mesh.RecalculateNormals();




    }




    public void RemoveVertices()
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshFilter>().mesh;
        }

        Debug.Log(" number of vertices " + mesh.vertices.Length);

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        List<Vector3> newVertices = new List<Vector3>();

        List<int> newTriangles = new List<int>();

        int[] bonEntier = new int[3];
        bool bonTriangle = true;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            bonTriangle = true;
            for (int j = 0; j < 3; j++)
            {
                if (vertices[triangles[i + j]].y > hauteur)
                {
                    newVertices.Add(vertices[triangles[i + j]]);
                    bonEntier[j] = triangles[i + j];
                }
                else
                {
                    bonTriangle = false;
                    break;
                }


            }
            if (bonTriangle)
                newTriangles.AddRange(bonEntier);


        }

        //mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();


    }


}
