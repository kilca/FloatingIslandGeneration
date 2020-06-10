using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshHelperNotUsed
{
    ///--------------------Unused---------------

    public static void ReverseNormals(MeshFilter filter)
    {
        Mesh mesh = filter.sharedMesh;
        Vector3[] verticesOriginal = mesh.vertices;
        if (filter != null)
        {

            Vector3[] normals = mesh.normals;
            for (int i = verticesOriginal.Length; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;


            int[] triangles2 = mesh.triangles;
            for (int i = verticesOriginal.Length; i < triangles2.Length; i += 3)
            {
                int temp = triangles2[i + 0];
                triangles2[i + 0] = triangles2[i + 1];
                triangles2[i + 1] = temp;
            }
            mesh.triangles = triangles2;

        }

    }

    //not used
    public static Mesh Smooth(MeshFilter meshfilter)
    {



        Mesh sourceMesh;
        Mesh workingMesh;

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


        return workingMesh;
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

    public static void RemoveVerticesSuperior(MeshFilter filter, float val)
    {

        Mesh mesh = filter.sharedMesh;

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

                if (vertices[triangles[i + j]].y < val)
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


    public static void RemoveVerticesUnder(MeshFilter filter, float val)
    {

        Mesh mesh = filter.sharedMesh;

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
                if (vertices[triangles[i + j]].y > val)
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



    }

    public static void ReverseNormals2(MeshFilter filter)
    {
        Mesh mesh = filter.sharedMesh;
        if (filter != null)
        {

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles2 = mesh.GetTriangles(m);
                for (int i = 0; i < triangles2.Length; i += 3)
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

}
