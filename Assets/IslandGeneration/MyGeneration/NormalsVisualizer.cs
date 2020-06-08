using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(MeshFilter))]
public class NormalsVisualizer : Editor
{

    private Mesh mesh;

    private bool isOn = true;

    public bool showNormals = true;

    public static List<MeshHelper.Edge> edges;

    void OnEnable()
    {
        MeshFilter mf = target as MeshFilter;
        if (mf != null)
        {
            mesh = mf.sharedMesh;
        }
    }

    void OnSceneGUI()
    {

        if (!isOn)
            return;

        if (mesh == null)
        {
            return;
        }
        if (!showNormals && edges == null)
            return;
        if (!showNormals)
        {
            foreach (MeshHelper.Edge e in edges)
            {
                Handles.matrix = (target as MeshFilter).transform.localToWorldMatrix;
                Handles.color = Color.yellow;
                /*
                Handles.DrawLine(
                    mesh.vertices[e.a],
                    mesh.vertices[e.b]);
                */
                Handles.Label(mesh.vertices[e.a], "" +e.a);
                Handles.Label(mesh.vertices[e.b], "" + e.b);

            }
        }
        else
        {


            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Handles.matrix = (target as MeshFilter).transform.localToWorldMatrix;
                Handles.color = Color.yellow;
                /*
                Handles.DrawLine(
                    mesh.vertices[i],
                    mesh.vertices[i] + mesh.normals[i] * 10);
                */
                Handles.Label(mesh.vertices[i], "" + i);
            }
        }
        
    }
}