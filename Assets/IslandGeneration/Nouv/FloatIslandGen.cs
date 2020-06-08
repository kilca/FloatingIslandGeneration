using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CustomEditor(typeof(FloatIslandGen))]
public class FloatIslandGenEditor : Editor
{

    bool isFold1 = true;

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
    public int mapChunkSize = 241;

    [Range(0, 6)]
    public int levelOfDetail;

    public bool autoUpdate;

    public float a;
    public float b;

    [Header("Island Parts")]
    public IslandPart botIsland = new IslandPart();
    public IslandPart topIsland = new IslandPart();

    //------

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void Refresh() {
        if (meshFilter == null || meshRenderer == null) {
            Debug.LogError("Error, the meshs are null");
        }
        if (meshFilter.transform != meshRenderer.transform) {
            Debug.LogError("Error, the transform must be the same");
        }
        GenerateMap();
    }
    /*
    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
    */
    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    public void GenerateMap()
    {

        botIsland.fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b);
        botIsland.noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, botIsland.noiseScale, botIsland.octaves, botIsland.persistance, botIsland.lacunarity, botIsland.offset);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {


                //1.0f *  ou 0.5 *
                botIsland.noiseMap[x, y] = Mathf.Clamp01(botIsland.fallOffMap[x, y] + 1.0f * botIsland.noiseMap[x, y]);

                float currentHeight = botIsland.noiseMap[x, y];
                for (int i = 0; i < botIsland.regions.Length; i++)
                {
                    if (currentHeight <= botIsland.regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = botIsland.regions[i].colour;
                        break;
                    }
                }
            }
        }



        botIsland.noiseTexture = TextureGenerator.TextureFromHeightMap(botIsland.noiseMap);

        botIsland.colorTexture = TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize);


        botIsland.fallOffTexture = TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b));

        /*
        StringBuilder sb = new StringBuilder();
        foreach (Color c in botIsland.colorTexture.GetPixels32())
        {
            sb.Append(c);
        }
        Debug.Log(sb);
        */
        //We create from bottom
        DrawMesh(MeshGenerator.GenerateTerrainMesh(botIsland.noiseMap, botIsland.meshHeightMultiplier * 10, botIsland.meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));

        MeshHelper.RemovePlanePart(meshFilter, true);//remove aussi les bonnes parties
        MeshHelper.Extrude(meshFilter, 0.9f);//0.9 bon

    }
}
