using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using System.Linq;

public class MeshHelper2
{
    //doesn't work (for the moment)
    public static void CreateRiver(MeshFilter filter)
    {
        //attention faut recup que ceux de haut
        //edit pas besoin vu que debut ceux de haut
        //pareil pour fin tant que ca vas pas trop bas
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
                    Debug.Log("DO A LAC");
                    nextHight = 0;
                }
            }
        }


        foreach (int i in retour)
        {
            GameObject.Instantiate(cube, filter.transform.TransformPoint(vertices[i]), Quaternion.identity,g.transform);

        }

    }

}
