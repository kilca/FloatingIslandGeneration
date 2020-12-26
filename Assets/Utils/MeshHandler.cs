using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FloatIslandGen;

public class MeshHandler
{

    private FloatIslandGen gen;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private IslandPart botIsland;
    private IslandPart topIsland;

    //-------------------

    private int mapChunkSize;
    private float a;
    private float b;
    private int seed;

    public MeshHandler(MeshFilter mf, FloatIslandGen gen)
    {
        this.meshFilter = mf;
        this.meshRenderer = mf.GetComponent<MeshRenderer>();

        this.gen = gen;

        this.botIsland = gen.botIsland;
        this.topIsland = gen.topIsland;

        this.mapChunkSize = gen.mapChunkSize;
        this.a = gen.a;
        this.b = gen.b;
        this.seed = gen.seed;
    }

    public void GenerateMap()
    {

        botIsland.fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b);
        botIsland.noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, botIsland.noiseScale, botIsland.octaves, botIsland.persistance, botIsland.lacunarity, botIsland.offset);

        //topIsland.fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, a, b);
        topIsland.fallOffMap = botIsland.noiseMap;
        topIsland.noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, topIsland.noiseScale, topIsland.octaves, topIsland.persistance, topIsland.lacunarity, topIsland.offset);

        Color[] colourMapBot = new Color[mapChunkSize * mapChunkSize];
        Color[] colourMapTop = new Color[mapChunkSize * mapChunkSize];

        //Generate for bot side

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


            }
        }

        //generate for top side

        //for falloffmap of top Island (same of botIsland

        topIsland.fallOffMap = Noise.GenerateFallFromNoise(topIsland.fallOffMap, mapChunkSize);

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {

                topIsland.noiseMap[x, y] = Mathf.Clamp01(botIsland.noiseMap[x, y] * topIsland.noiseMap[x, y] + topIsland.fallOffMap[x, y]);

            }
        }


        //generate color for top side

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {

                float currentHeightTop = topIsland.noiseMap[x, y];

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
        botIsland.fallOffTexture = TextureGenerator.TextureFromHeightMap(botIsland.fallOffMap);


        topIsland.noiseTexture = TextureGenerator.TextureFromHeightMap(topIsland.noiseMap);
        topIsland.colorTexture = TextureGenerator.TextureFromColourMap(colourMapTop, mapChunkSize, mapChunkSize);
        topIsland.fallOffTexture = TextureGenerator.TextureFromHeightMap(topIsland.fallOffMap);

        //We create from bottom
        gen.DrawMesh(MeshGenerator.GenerateTerrainMesh(botIsland.noiseMap, botIsland.meshHeightMultiplier * 10, botIsland.meshHeightCurve, gen.levelOfDetail), botIsland.colorTexture);

        MeshHelper.RemovePlanePart(meshFilter, true);//remove aussi les bonnes parties

        if (gen.useRiver)
        {
            var riverMap = Noise.GenerateRiverMap(topIsland.noiseMap, 3);
            gen.riverTexture = TextureGenerator.TextureFromHeightMap(riverMap);
            List<Vector3> r = MeshHelper.Extrude(meshFilter, 0.9f, topIsland.noiseMap, topIsland.meshHeightMultiplier, topIsland.meshHeightCurve, gen.smoothStrength, riverMap);//0.9 bon
            gen.PlaceRiver0(meshFilter, riverMap, r);
        }
        else
        {
            MeshHelper.Extrude(meshFilter, 0.9f, topIsland.noiseMap, topIsland.meshHeightMultiplier, topIsland.meshHeightCurve, gen.smoothStrength, new float[mapChunkSize, mapChunkSize]);//0.9 bon
        }

        //MeshHelper2.CreateRiver(meshFilter);
    }



}
