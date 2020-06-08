using UnityEngine;
using System.Collections;
using Utils;
public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColourMap, Mesh,FallOffMap};
    public DrawMode drawMode;

    public enum MeshType { Terrain, Island, FlatFloatIsland, FloatIsland };

    [ConditionalField("drawMode", DrawMode.Mesh)]
    public MeshType meshType;

    const int mapChunkSize = 241;
	[Range(0,6)]
	public int levelOfDetail;
	public float noiseScale;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;

	public TerrainType[] regions;

    //----
    //FALLOFF
    private bool useFalloff = false;
    float[,] falloffMap;

    public float a;
    public float b;

    //---

    public MapDisplay display;

    void Start() {


    }

	public void GenerateMap() {
        useFalloff = false;
        display = GetComponent<MapDisplay>();
        if (drawMode != DrawMode.FallOffMap) {
            useFalloff = true;
            falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b);
        }
        else if (meshType != MeshType.Terrain)
        {
            useFalloff = true;
            if (meshType == MeshType.Island)
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, 3f, 2.2f);
            else if (meshType == MeshType.FlatFloatIsland)
            {
                //falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, 0.66f, 2.2f);
                //falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b);
                //0.17 0.43
                //0.8 0.5 //BON
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b);
            }
        }

        float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++) {
			for (int x = 0; x < mapChunkSize; x++) {

                if (meshType == MeshType.Island)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
                else if (meshType == MeshType.FlatFloatIsland) {

                   //1.0f *  ou 0.5 *
                    noiseMap[x, y] = Mathf.Clamp01(falloffMap[x, y] + 1.0f * noiseMap[x, y]);
                }

				float currentHeight = noiseMap [x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight <= regions [i].height) {
						colourMap [y * mapChunkSize + x] = regions [i].colour;
						break;
					}
				}
			}
		}
        
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            if (meshType == MeshType.Island || meshType == MeshType.Terrain)
            {
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
            }
            else if (meshType == MeshType.FlatFloatIsland) {
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier*10, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));

                //MeshHelper.DrawAllEdges(display.meshFilter.sharedMesh);
                MeshHelper.RemovePlanePart(display.meshFilter,true);//remove aussi les bonnes parties
                MeshHelper.Extrude(display.meshFilter, 0.9f);//0.9 bon

                //MeshHelper.ReverseNormals(display.meshFilter);
                //MeshHelper.FillTerrainTop(display.meshFilter, 0.95f);
                //MeshHelper.RemoveVerticesSuperior(display.meshFilter, 1.0f * 128);
                //MeshHelper.RemoveVerticesSuperior(display.meshFilter, 0.7f);
            }
        }
        else if (drawMode == DrawMode.FallOffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize,a,b)));
        }
	}

	void OnValidate() {
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
}