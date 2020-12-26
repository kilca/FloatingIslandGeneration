using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using System.Linq;
using static MeshHelper;

/// <summary>
/// Here is where i've put all the function that could be useful but are doesn't used
/// due to high complexity or to some bugs
/// </summary>
public class MeshHelper2
{

    public struct TripleEdge
    {
        public int a;
        public int b;
        public int c;

        public TripleEdge(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        override
        public string ToString()
        {
            return "[" + a + "," + b + "]";
        }

    }

    //doesn't work (for the moment) but i kept it
    public static void CreateRiver(MeshFilter filter)
    {
        Mesh mesh = filter.sharedMesh;

        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] triangles = mesh.triangles;

        List<int> retour = new List<int>();

        int maxindex = -1;
        Vector3 maxVector = new Vector3(0, -10, 0);
        for(int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y > maxVector.y)
            {
                maxindex = i;
                maxVector = vertices[i];
            }

        }


        Dictionary<int,List<int>> indexClose = new Dictionary<int, List<int>>();
        List<int> nears;
        for (int i = 0; i < triangles.Length; i+=3) {

            for (int j = 0; j < 3; j++) {
                if (indexClose.ContainsKey(i+j))
                {
                    nears = indexClose[i+j];
                }
                else
                {
                    nears = new List<int>();
                }
                for (int k = 0; k < 3; k++) {
                    if ((i+k) != (i + j))
                    {
                        nears.Add(triangles[i + k]);
                    }
                }
                nears.Distinct().ToList();
                indexClose[i+j] = nears;
            }

        }

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject g = GameObject.Instantiate(cube, filter.transform);
        GameObject.Instantiate(cube, filter.transform.TransformPoint(vertices[maxindex]), Quaternion.identity, g.transform);


        float max = vertices[maxindex].y - 20;
        float nextHight = 2000;//just to be sure
        retour.Add(maxindex);

        //it seams that the close dictionaries doesn't work, but the algorithm seem good ...
        while (nextHight > max)
        {

            foreach (int i in indexClose[maxindex])
            {
                bool hasFound = false;
                if (vertices[i].y < nextHight) {
                    hasFound = true;
                    nextHight = vertices[i].y;
                    Debug.Log(nextHight);
                    retour.Add(i);
                    maxindex = i;
                    break;
                }
                if (!hasFound)
                {
                    Debug.Log("DO A LAKE");
                    nextHight = 0;
                }
            }
        }


        foreach (int i in retour)
        {
            GameObject.Instantiate(cube, filter.transform.TransformPoint(vertices[i]), Quaternion.identity,g.transform);

        }

    }

    //incorrect and not used, but the principle looks like it
    public static List<Edge> getBoundaries(Vector3[] vertices,
    int[] triangles)
    {

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


    //https://www.diva-portal.org/smash/get/diva2:830483/FULLTEXT01.pdf
    //----------------------Added (if it works) ---------------- 

    //would have been better to take those in a crux shape
    static List<int> neighbors(int i, List<int> triangles)
    {
        List<int> retour = new List<int>();
        for (int j = 0; j < triangles.Count; j += 3)
        {
            if (triangles[j] == i || triangles[j + 1] == i || triangles[j + 2] == i)
            {
                for (int k = 0; k < 3; k++)
                {
                    if ((j + k) != i)
                    {
                        retour.Add(j + k);
                    }
                }
            }
        }
        return retour;
    }


    public static List<Vector3> AVGShort2(List<Vector3> sharedVertex, List<Edge> edges, int X)
    {

        List<Vector3> retour = new List<Vector3>(sharedVertex);
        int div = 0;
        float posx;
        float posz;
        for (int i = 0; i < X; i++)
        {
            foreach (Edge e in edges)
            {
                Vector3 s = sharedVertex[e.a];
                div = 1;
                posx = s.x;
                posz = s.z;

                div++;
                posx += sharedVertex[e.b].x;
                posz += sharedVertex[e.b].z;

                retour[e.a] = new Vector3(posx / div, sharedVertex[e.a].y, posz / div);
            }

        }
        return retour;

    }

    //copy of algorithm in the article
    //works for certains edge but is buggy for some others (due to some incorrect Edge)
    public static void AVGSad(List<Edge> es, List<int> triangles, ref List<Vector3> sharedVertex, int X)
    {


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
            for (int j = 0; j < sharedVertex.Count; j++)
            {
                Vector3 s = sharedVertex[j];//ICI
                if (edgeDict.ContainsKey(j))
                {
                    div = 1;
                    posx = s.x;
                    posz = s.z;
                    foreach (int ind in neighbors(j, triangles))
                    {
                        if (edgeDict.ContainsKey(ind))
                        {
                            div++;
                            posx += sharedVertex[ind].x;
                            posz += sharedVertex[ind].z;
                        }
                    }
                    sharedVertex[j] = new Vector3(posx / div, sharedVertex[j].y, posz / div);
                }


            }
        }



    }

}
