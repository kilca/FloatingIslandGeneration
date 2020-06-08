using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Utils;


[CustomEditor(typeof(SkyIslandGenerator))]
public class SkyIslandGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SkyIslandGenerator me = (SkyIslandGenerator)target;
        

    }


}
public class SkyIslandGenerator : MonoBehaviour
{

    public float distanceIles = 2.0f;


    public bool changerTailleIles = false;

    [ConditionalField("changerTailleIles")]
    public float tailleIles = 3.5f;

    public bool supprimerIleCentre;

    private int seed;

    public float minY = -500;
    public float maxY = 500;

    public Material matBas;

    public float hauteur = 0.13f;


    const float scale = 5f;

    //static MapGenerator mapGenerator;


    public int nbIleX;
    public int nbIleY;

    public int taille = 120;
    public Material mapMaterial;

    /*
    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        seed = mapGenerator.seed;

    }
    */
    public void DebuterFlot()
    {
        foreach (Transform t in transform)
        {
            Debug.Log("nb vertices : " + t.GetComponent<MeshFilter>().mesh.vertices.Length);
            t.GetComponent<MeshEdit>().Init();
        }
        //transform.GetChild(0).GetComponent<MeshEdit>().Init();


    }





}
