using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using System.Linq;
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


    ///--------------------Used---------------

    public static List<Edge> getBoundariesByLimit(Vector3[] vertices,
int[] triangles, float limite)
    {

        List<Edge> retour = new List<Edge>();
        for (int i = 0; i < triangles.Length; i+=3)
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
    

    //from : https://stackoverflow.com/questions/3848923/how-to-extrude-a-flat-2d-mesh-giving-it-depth
    //Be careful, the order is important because of the normals
    public static List<Vector3> Extrude(MeshFilter filter, float txHauteur, float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int R, float[,] riverHeight) {

        List<Vector3> retour = new List<Vector3>();

        Mesh mesh = filter.sharedMesh;

        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] triangles = mesh.triangles;

        //-------------------1)
        List<Vector3> nVertices = new List<Vector3>(vertices);
        List<Vector2> nUVs = new List<Vector2>(uvs);

        List<int> newTriangles = new List<int>(triangles);

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        //x and z vertices from -120 to 120
        List<Edge> boundaries = getBoundariesByLimit(vertices, triangles, 143.0f * txHauteur);

        nVertices = SmoothBorderByAverage(boundaries, newTriangles,ref nVertices, R);

        List<Vector3> addedVertices = new List<Vector3>(nVertices);

        for (int i = 0; i < addedVertices.Count; i++) {
            Vector3 v = addedVertices[i];

            int x = (int)(nUVs[i].x * width);
            int y =  (int)(nUVs[i].y * height);

            //v.y = 140 + heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;

            float f = Mathf.Clamp01(-1.0f *(heightMap[x, y]-1.0f));
            v.y = 140 + heightMultiplier * heightCurve.Evaluate(f) - riverHeight[x,y] * 40;

            if (riverHeight[x, y] == 0.9f)
            {
                retour.Add(v);
            }

            addedVertices[i] = v;
        }

        nVertices.AddRange(addedVertices);
        
        nUVs.AddRange(nUVs);

        //-----------------2)
        
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


        filter.sharedMesh.subMeshCount = 9;

        mesh.vertices = nVertices.ToArray();

        List<int> triangle1 = newTriangles.GetRange(0, triangles.Length);
        List<int> triangle2 = newTriangles.GetRange(triangles.Length, newTriangles.Count - triangles.Length);

        mesh.SetTriangles(triangle1, 0);

        mesh.SetTriangles(triangle2, 1);

        mesh.normals = newNormals.ToArray();
        mesh.uv = nUVs.ToArray();

        return retour;


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

    //would have been better to take those in a crux shape
    static List<int> EdgeNeighbors(int i, List<Edge> es)
    {
        List<int> retour = new List<int>();

        foreach (Edge e in es) {
            if (e.a == i) {
                retour.Add(e.b);
            }else if (e.b == i)
            {
                retour.Add(e.a);
            }
        }
        return retour.Distinct().ToList();
    }

    /// <summary>
    /// Smooth the border of the Island by calculating the avg of every node.
    /// It permit to remove some triangle aspect
    /// come from : https://www.diva-portal.org/smash/get/diva2:830483/FULLTEXT01.pdf
    /// </summary>
    public static List<Vector3> SmoothBorderByAverage(List<Edge> es, List<int> triangles, ref List<Vector3> sharedVertex, int X)
    {

        List<Vector3> retour = new List<Vector3>(sharedVertex);

        int div = 0;
        float posx;
        float posz;

        Dictionary<int, Edge> edgeDict = new Dictionary<int, Edge>();
        foreach (Edge e in es)
        {
            if (!edgeDict.ContainsKey(e.a))
                edgeDict.Add(e.a, e);
            if (!edgeDict.ContainsKey(e.b))
                edgeDict.Add(e.b, e);
        }

        for (int i = 0; i < X; i++)
        {
            for (int j = 0; j < retour.Count; j++)
            {
                Vector3 s = retour[j];//ICI
                if (edgeDict.ContainsKey(j))
                {
                    div = 1;
                    posx = s.x;
                    posz = s.z;
                    foreach (int ind in EdgeNeighbors(j, es))
                    {
                        if (edgeDict.ContainsKey(ind))
                        {
                            div++;
                            posx += retour[ind].x;
                            posz += retour[ind].z;
                        }
                    }
                    if (div != 1)
                        retour[j] = new Vector3(posx / div, sharedVertex[j].y, posz / div);
                }


            }
        }

        return retour;

    }


}