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
        }

        isFold2 = EditorGUILayout.Foldout(isFold2, "Show top Textures");
        if (isFold2)
        {

            GUILayout.Label("noiseTexture");
            GUILayout.Label(mapGen.topIsland.noiseTexture);


            GUILayout.Label("colorTexture");
            GUILayout.Label(mapGen.topIsland.colorTexture);
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

    public void GenerateMap()
    {

        botIsland.fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b);
        botIsland.noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, botIsland.noiseScale, botIsland.octaves, botIsland.persistance, botIsland.lacunarity, botIsland.offset);

        topIsland.fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b);
        topIsland.noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, topIsland.noiseScale, topIsland.octaves, topIsland.persistance, topIsland.lacunarity, topIsland.offset);

        Color[] colourMapBot = new Color[mapChunkSize * mapChunkSize];
        Color[] colourMapTop = new Color[mapChunkSize * mapChunkSize];


        
        //for falloffmap of top Island (same of botIsland
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {

                topIsland.noiseMap[x, y] = Mathf.Clamp01(botIsland.fallOffMap[x, y] + 1.0f * topIsland.noiseMap[x, y]);

            }
        }
        

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {


                //1.0f * or 0.5 *
                botIsland.noiseMap[x, y] = Mathf.Clamp01(botIsland.fallOffMap[x, y] + 1.0f * botIsland.noiseMap[x, y]);


                float currentHeightBot = botIsland.noiseMap[x, y];//bot
                float currentHeightTop = topIsland.noiseMap[x, y];

                for (int i = 0; i < botIsland.regions.Length; i++)
                {
                    if (currentHeightBot <= botIsland.regions[i].height)
                    {
                        colourMapBot[y * mapChunkSize + x] = botIsland.regions[i].colour;
                        break;
                    }
                }

                for (int i = 0; i < topIsland.regions.Length; i++)
                {
                    if (currentHeightTop <= topIsland.regions[i].height)
                    {
                        colourMapTop[y * mapChunkSize + x] = topIsland.regions[i].colour;
                        break;
                    }
                }

            }
        }


        botIsland.noiseTexture = TextureGenerator.TextureFromHeightMap(botIsland.noiseMap);
        botIsland.colorTexture = TextureGenerator.TextureFromColourMap(colourMapBot, mapChunkSize, mapChunkSize);
        botIsland.fallOffTexture = TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b));

        topIsland.noiseTexture = TextureGenerator.TextureFromHeightMap(topIsland.noiseMap);
        topIsland.colorTexture = TextureGenerator.TextureFromColourMap(colourMapTop, mapChunkSize, mapChunkSize);
        topIsland.fallOffTexture = TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b));

        //We create from bottom
        DrawMesh(MeshGenerator.GenerateTerrainMesh(botIsland.noiseMap, botIsland.meshHeightMultiplier * 10, botIsland.meshHeightCurve, levelOfDetail), botIsland.colorTexture);

        MeshHelper.RemovePlanePart(meshFilter, true);//remove aussi les bonnes parties
        MeshHelper.Extrude(meshFilter, 0.9f,topIsland.noiseMap,topIsland.meshHeightMultiplier, topIsland.meshHeightCurve);//0.9 bon

        meshRenderer.sharedMaterials[1].mainTexture = topIsland.colorTexture;

    }
}
