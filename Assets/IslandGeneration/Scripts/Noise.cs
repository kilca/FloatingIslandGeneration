using UnityEngine;
using System.Collections;

public static class Noise {


    public static float[,] GenerateFallFromNoise(float[,] param, int size)
    {
        float[,] noiseMap = new float[size,size];

        for (int i = 0; i < size; i++) {

            for (int j = 0; j < size; j++) {
                if (param[i, j] > 0.9f)
                {
                    noiseMap[i, j] = 1.0f;
                }
                else {
                    noiseMap[i, j] = 0.0f;
                }
            }
        }

        return noiseMap;
    }


    //[!] Must give the heightmap & fall / Consider 0 as the peak and 1 as the rift
    public static float[,] GenerateRiverMap(float[,] noiseMap, int size)
    {

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        float[,] retour = new float[width, height];

        Vector2Int maxCoord = new Vector2Int(0, 0);
        float minHeight = 1;

        //we search the max height position
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (noiseMap[i, j] < minHeight)
                {
                    maxCoord = new Vector2Int(i, j);
                    minHeight = noiseMap[i, j];
                }
            }
        }


        //we don't consider going out of bound because of the fallmap
        Vector2Int moveDirection = new Vector2Int(0, 0);
        retour[maxCoord.x + moveDirection.x, maxCoord.y + moveDirection.y] = 0.9f;
        float currentMinHeight = minHeight;
        int count = 0;

        float lastPower = 0.9f;

        while (currentMinHeight < 1.0f && count < 150)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j == i)
                        continue;

                    int maxCoordx = maxCoord.x + i;
                    int maxCoordy = maxCoord.y + j;

                    if (noiseMap[maxCoordx, maxCoordy] > currentMinHeight)
                    {
                        currentMinHeight = noiseMap[maxCoordx, maxCoordy];
                        moveDirection = new Vector2Int(i, j);
                    }
                }
            }
            //Debug.Log(currentMinHeight);
            maxCoord = new Vector2Int(maxCoord.x + moveDirection.x, maxCoord.y + moveDirection.y);
            retour[maxCoord.x, maxCoord.y] = lastPower;
            count++;

        }
        float nextPower;
        //to optimize
        //Debug.Log("avant while");
        while (size > 0)
        {
            nextPower = lastPower / 1.5f;
            for (int x = 1; x < width-1; x++)
            {
                for (int y = 1; y < height-1; y++)
                {

                    if (retour[x, y] == lastPower)
                    {
                        for (int i = -1; i <= 1; i++) {
                            for(int j = -1; j <= 1; j++)
                            {
                                if (retour[x+i, y+j] == 0)
                                {
                                    retour[x + i, y + j] = nextPower;
                                }
                            }

                        }
                    }
                }
            }
            lastPower = nextPower;
            size--;
        }

        return retour;
    }


    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
		float[,] noiseMap = new float[mapWidth,mapHeight];

		System.Random prng = new System.Random (seed);
		Vector2[] octaveOffsets = new Vector2[octaves];
		for (int i = 0; i < octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + offset.x;
			float offsetY = prng.Next (-100000, 100000) + offset.y;
			octaveOffsets [i] = new Vector2 (offsetX, offsetY);
		}

		if (scale <= 0) {
			scale = 0.0001f;
		}

		float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;


		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
		
				float amplitude = 1;
				float frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++) {
					float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
					float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxNoiseHeight) {
					maxNoiseHeight = noiseHeight;
				} else if (noiseHeight < minNoiseHeight) {
					minNoiseHeight = noiseHeight;
				}
				noiseMap [x, y] = noiseHeight;
			}
		}

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
			}
		}

		return noiseMap;
	}

}
