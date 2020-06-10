using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text;

//other meshHelper : https://wiki.unity3d.com/index.php/MeshHelper

/*
 *MeshHelper made for floating island generation 
 */

public static class MeshHelper
{

    public struct Edge
    {

        public int a;
        public int b;

        public Edge(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        public bool IsOpposite(Edge e)
        {
            return (a == e.b && b == e.a);
        }

        override
        public string ToString()
        {
            return "[" + a + "," + b + "]";
        }

    }


    //incorrect and not used, but the principle looks like it
    public static List<Edge> getBoundaries(Vector3[] vertices,
    int[] triangles) {

        List<Edge> boundaries = new List<Edge>();
        for (int i = 0; i < triangles.Length - 1; i++)
        {
            boundaries.Add(new Edge(triangles[i], triangles[i + 1]));
        }

        //OPTIMISABLE (POTENTIELLEMENT AVEC JUSTE GET TAILLE A DESSUS DE N
        List<Edge> internals = new List<Edge>();
        foreach (Edge e in boundaries)
        {
            foreach (Edge e2 in boundaries)
            {
                if (e.IsOpposite(e2))
                {
                    internals.Add(e);
                    internals.Add(e2);
                }
            }
        }

        //Optimisable avec RemoveAll
        foreach (Edge e in internals)
        {
            boundaries.Remove(e);
        }

        return null;
    }


    ///--------------------Used---------------

    public static List<Edge> getBoundariesByLimit(Vector3[] vertices,
int[] triangles, float limite)
    {

        List<Edge> retour = new List<Edge>();
        for (int i = 0; i < triangles.Length - 3; i+=3)
        {
            if (vertices[triangles[i]].y > limite && vertices[triangles[i + 1]].y > limite)
            {
                retour.Add(new Edge(triangles[i], triangles[i + 1]));
            }

            if (vertices[triangles[i+1]].y > limite && vertices[triangles[i + 2]].y > limite)
            {
                retour.Add(new Edge(triangles[i+1], triangles[i + 2]));
            }
            if (vertices[triangles[i]].y > limite && vertices[triangles[i + 2]].y > limite)
            {
                retour.Add(new Edge(triangles[i], triangles[i + 2]));
            }
        }


        return retour;
    }

    //https://stackoverflow.com/questions/3848923/how-to-extrude-a-flat-2d-mesh-giving-it-depth
    //Attention ordre important car indique normals
    public static void Extrude(MeshFilter filter, float txHauteur, float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve) {


        Mesh mesh = filter.sharedMesh;

        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] triangles = mesh.triangles;

        //-------------------1)
        List<Vector3> nVertices = new List<Vector3>(vertices);
        List<Vector2> nUVs = new List<Vector2>(uvs);

        List<Vector3> addedVertices = new List<Vector3>(vertices);

        List<int> newTriangles = new List<int>(triangles);

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        //x and z vertices from -120 to 120

        for (int i = 0; i < addedVertices.Count; i++) {
            Vector3 v = addedVertices[i];

            int x = (int)(nUVs[i].x * width);
            int y =  (int)(nUVs[i].y * height);

            v.y = 140 + heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;

            addedVertices[i] = v;
        }
        

        

        nVertices.AddRange(addedVertices);
        
        nUVs.AddRange(nUVs);

        //-----------------2)

        List<Edge> boundaries = getBoundariesByLimit(vertices, triangles, 128 * txHauteur);


        //----------------3)

        int n = vertices.Length;


        for (int i = 0; i < triangles.Length; i++) {
            newTriangles.Add(triangles[i]+n);
        }


        //-------------4)

        //We display the triangle of the 2 sides because some triangles are inversed
        
        //Side 1
        foreach (Edge e in boundaries) {
            newTriangles.Add(e.a);
            newTriangles.Add(e.b);
            newTriangles.Add(e.b + n);

            newTriangles.Add(e.b + n);
            newTriangles.Add(e.a + n);
            newTriangles.Add(e.a);

        }
        

        //Side 2
        
        foreach (Edge e in boundaries)
        {
            newTriangles.Add(e.a);
            newTriangles.Add(e.a + n);
            newTriangles.Add(e.b + n);

            newTriangles.Add(e.b + n);
            newTriangles.Add(e.b);
            newTriangles.Add(e.a);

        }
        
        //5* normals
        List<Vector3> newNormals = new List<Vector3>(mesh.normals);

        for (int i = 0; i < n; i++)
        {
            newNormals[i] = -newNormals[i];
            newNormals.Add(Vector3.up);
        }
        for (int i = 0; i < triangles.Length; i += 3) {

            int temp = newTriangles[i + 0];
            newTriangles[i + 0] = newTriangles[i + 1];
            newTriangles[i + 1] = temp;
        }


        filter.sharedMesh.subMeshCount = 2;

        mesh.vertices = nVertices.ToArray();

        List<int> triangle1 = newTriangles.GetRange(0, triangles.Length);
        List<int> triangle2 = newTriangles.GetRange(triangles.Length, newTriangles.Count - triangles.Length);

        mesh.SetTriangles(triangle1, 0);

        mesh.SetTriangles(triangle2, 1);

        mesh.normals = newNormals.ToArray();
        mesh.uv = nUVs.ToArray();
       


    }
    
    public static void RemovePlanePart(MeshFilter filter, bool removeVertices)
    {
        //old position of array to newPosition of array
        Dictionary<int, int> mapOldToNew = new Dictionary<int, int>();

        Mesh mesh = filter.sharedMesh;

        Vector2[] uvs = mesh.uv;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        List<Vector3> nVertices = new List<Vector3>();

        List<Vector2> nvUVS = new List<Vector2>();

        List<int> newTriangles = new List<int>();

        //edit remplacé i+j par count

        for (int i = 0; i < triangles.Length; i += 3)
        {

            float a = vertices[triangles[i]].y;
            float b = vertices[triangles[i + 1]].y;
            float c = vertices[triangles[i + 2]].y;

            if (a != b || a != c || b != c)
            {
                for (int j = 0; j < 3; j++) {
                 
                    if (!mapOldToNew.ContainsKey(triangles[i+j])) {
                        nvUVS.Add(uvs[triangles[i+j]]);
                        nVertices.Add((vertices[triangles[i+j]]));
                        mapOldToNew.Add(triangles[i + j], nVertices.Count-1);
                    }
                    newTriangles.Add(triangles[i+j]);
                }

            }

        }


        if (removeVertices)
        {
            //we reassign triangles to new vertices
            for (int i = 0; i < newTriangles.Count; i++)
            {
                if (mapOldToNew.ContainsKey(newTriangles[i]))
                {
                    newTriangles[i] = mapOldToNew[newTriangles[i]];
                }

            }
            mesh.triangles = newTriangles.ToArray();
            mesh.SetVertices(nVertices);
            mesh.uv = nvUVS.ToArray();
        }
        else {
            mesh.triangles = newTriangles.ToArray();
        }


        mesh.RecalculateBounds();
        mesh.RecalculateNormals();


    }

}