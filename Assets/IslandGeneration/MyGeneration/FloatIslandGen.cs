using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CustomEditor(typeof(FloatIslandGen))]
public class FloatIslandGenEditor : Editor
{

    bool isFold1 = true;
    bool isFold2 = true;

    public override void OnInspectorGUI()
    {
        FloatIslandGen mapGen = (FloatIslandGen)target;

        bool defaultInspect = DrawDefaultInspector();

        if (defaultInspect)
        {
            if (mapGen.autoUpdate)
            {
                mapGen.Refresh();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.Refresh();
        }

        isFold1 = EditorGUILayout.Foldout(isFold1, "Show bot Textures");
        if (isFold1) {
            
            GUILayout.Label("noiseTexture");
            GUILayout.Label(mapGen.botIsland.noiseTexture);
            

            GUILayout.Label("colorTexture");
            GUILayout.Label(mapGen.botIsland.colorTexture);

            GUILayout.Label("falloffTexture");
            GUILayout.Label(mapGen.botIsland.fallOffTexture);
        }

        isFold2 = EditorGUILayout.Foldout(isFold2, "Show top Textures");
        if (isFold2)
        {

            GUILayout.Label("noiseTexture");
            GUILayout.Label(mapGen.topIsland.noiseTexture);


            GUILayout.Label("colorTexture");
            GUILayout.Label(mapGen.topIsland.colorTexture);

            GUILayout.Label("falloffTexture");
            GUILayout.Label(mapGen.topIsland.fallOffTexture);

        }

        if (mapGen.useRiver)
        {
            GUILayout.Label("River Map");
            GUILayout.Label(mapGen.riverTexture);
        }
           /*
        Texture2D myTexture = AssetPreview.GetAssetPreview(object);
        GUILayout.Label(myTexture);
        */

    }
}
public class FloatIslandGen : MonoBehaviour
{
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }

    [System.Serializable]
    public class IslandPart {

        public float noiseScale;

        public int octaves;
        [Range(0, 1)]
        public float persistance;
        public float lacunarity;

        public Vector2 offset;

        public float meshHeightMultiplier;
        public AnimationCurve meshHeightCurve;

        public TerrainType[] regions;

        [HideInInspector]
        public Texture2D noiseTexture;
        [HideInInspector]
        public Texture2D fallOffTexture;
        [HideInInspector]
        public Texture2D colorTexture;

        [HideInInspector]
        public float[,] fallOffMap;
        [HideInInspector]
        public float[,] noiseMap;


    }

    [Header("Global Values")]
    public int seed;

    //currently can't modify chunkSize
    public int mapChunkSize = 241;

    [Range(0, 6)]
    public int levelOfDetail;

    public bool autoUpdate;

    //0.8
    public float a;
    //0.5
    public float b;

    [Header("Island Parts")]
    public IslandPart botIsland = new IslandPart();
    public IslandPart topIsland = new IslandPart();

    //------

    public MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    //smooth coefficient
    [Range(0,12)]
    public int smoothStrength;

    public bool useRiver;

    [HideInInspector]
    public Texture2D riverTexture;

    private MeshHandler handler;

    public void Refresh() {
        if (meshFilter == null) {
            Debug.LogError("Error, the meshs are null");
            return;
        }

        GameObject river = GameObject.FindGameObjectWithTag("River");
        if (river != null)
            DestroyImmediate(river);

        meshRenderer = meshFilter.GetComponent<MeshRenderer>();

        handler = new MeshHandler(meshFilter, this);
        handler.GenerateMap();
        meshRenderer.sharedMaterials[1].mainTexture = topIsland.colorTexture;
    }
    
    public void DrawTexture(Texture2D texture)
    {
        //textureRender.sharedMaterial.mainTexture = texture;
        //textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
    
    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    public void PlaceRiver0(MeshFilter mf, float[,] riverNoise, List<Vector3> r)
    {
        Vector3 decalage = new Vector3(0, 320, 0);
        const int riverWidth = 80;


        GameObject g = (GameObject)Instantiate(Resources.Load("RiverLine"), decalage, Quaternion.identity,transform.parent);

        Vector3[] pos = new Vector3[r.Count];
        for (int i = 0; i < r.Count; i++)
        {
            pos[i] = mf.transform.TransformPoint(r[i]);
        }
        LineRenderer lr = g.GetComponent<LineRenderer>();

        lr.positionCount = pos.Length;
        lr.SetPositions(pos);

        lr.startWidth = riverWidth;
        lr.endWidth = riverWidth;

    }


    public void PlaceRiver(MeshFilter mf, float[,] riverNoise, List<Vector3> r)
    {

        GameObject g = (GameObject) Instantiate(Resources.Load("RiverLine"));

        Vector3 gPos = mf.transform.TransformPoint(r[0]);
        g.transform.position = gPos;

        Vector3[] pos = new Vector3[r.Count-1];
        for(int i = 1; i < r.Count; i++)
        {
            pos[i-1] = r[i]-r[0];
        }
        LineRenderer lr = g.GetComponent<LineRenderer>();

        Debug.Log(pos.Length);

        foreach (var t in pos)
        {
            Debug.Log(t);
        }

        lr.positionCount = pos.Length;
        lr.SetPositions(pos);
    }

}
