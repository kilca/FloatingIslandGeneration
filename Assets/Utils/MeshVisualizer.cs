using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(MeshFilter))]
public class MeshVisualizer : Editor
{

    private Mesh mesh;

    private bool isOn = true;


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
        
        if (mesh.vertices.Length > 1000){
            return;
        }
        

        if (mesh == null)
        {
            return;
        }


            Vector3[] vertices = mesh.vertices;
            for(int i = 0; i < vertices.Length; i++)
            {
                Handles.Label(vertices[i],""+i);
            }
        
        /*
        foreach (Vector3 v in vertices)
            {
                Handles.matrix = (target as MeshFilter).transform.localToWorldMatrix;
                Handles.color = Color.yellow;
                
                Handles.DrawLine(
                    mesh.vertices[e.a],
                    mesh.vertices[e.b]);
                
                Handles.Label(v,""+v.x+","+v.z);

            }
            */
        

        
    }
}