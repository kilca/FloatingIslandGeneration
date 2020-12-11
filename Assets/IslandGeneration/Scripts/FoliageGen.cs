using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CustomEditor(typeof(FoliageGen))]
public class FoliageGenEditor : Editor
{


    public override void OnInspectorGUI()
    {
        FoliageGen mapGen = (FoliageGen)target;

        bool defaultInspect = DrawDefaultInspector();

        GUILayout.Label("noiseTexture");
        if (mapGen.res != null && mapGen.res.Length != 0)
            GUILayout.Label(TextureGenerator.TextureFromHeightMap(mapGen.res));

        if (GUILayout.Button("Generate"))
        {
            mapGen.Refresh();
        }
    }
}

public class FoliageGen : MonoBehaviour
{

    public FloatIslandGen gen;

    public float heightAdd = 10.0f;

    [Range(1, 8)]
    public int R;

    public Transform genTransform;
    public GameObject tree;

    public float[,] res;

    public void Refresh() {

        if (gen.topIsland.fallOffMap == null) {
            Debug.LogWarning("Island not generated before, did it for you");
            gen.GenerateMap();
        }

        DestroyChilds(genTransform);
        
        List<Vector3> vs = PlaceTrees2(gen.mapChunkSize, gen.mapChunkSize, gen.seed, R, gen.topIsland.fallOffMap);

        DestroyChilds(genTransform);
        InstantiateTrees(vs);

    }

    public void InstantiateTrees(List<Vector3> vs) {
        foreach (Vector3 v in vs) {
            Vector3 pos = gen.meshFilter.transform.TransformPoint(v) + new Vector3(0,heightAdd,0);
            Instantiate(tree, pos, Quaternion.identity, genTransform);
        }
    }

    public void DestroyChilds(Transform t)
    {
        int childs = t.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(t.GetChild(i).gameObject);
        }

    }

    public List<Vector3> PlaceTrees(int height, int width, int seed, int R, float[,] fallOffMap)
    {

        List<Vector3> retour = new List<Vector3>();

        float[,] bluenoise = Noise.GenerateNoiseMap(width, height, seed, 40, 5, 0.5f, 4, new Vector2(0, 0));
        res = new float[width, height];

        for (int yc = 0; yc < height; yc++)
        {
            for (int xc = 0; xc < width; xc++)
            {
                double max = 0;
                // there are more efficient algorithms than this
                for (int yn = yc - R; yn <= yc + R; yn++)
                {
                    for (int xn = xc - R; xn <= xc + R; xn++)
                    {
                        if (0 <= yn && yn < height && 0 <= xn && xn < width)
                        {
                            double e = bluenoise[yn, xn];
                            if (e > max) { max = e; }
                        }
                    }
                }
                if (bluenoise[yc, xc] == max)
                {
                    res[yc, xc] = 1;
                    // place tree at xc,yc
                }
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (fallOffMap[i, j] == 1.0f) {
                    res[i, j] = 0;
                }
                else if(res[i,j] == 1.0f){

                    float f = Mathf.Clamp01(-1.0f * (gen.topIsland.noiseMap[i, j] - 1.0f));
                    float y = 140 + gen.topIsland.meshHeightMultiplier * gen.topIsland.meshHeightCurve.Evaluate(f);

                    retour.Add(new Vector3(i, y, j));

                }
            }
        }

        //blanc endroit ou truc

        return retour;


    }

    public List<Vector3> PlaceTrees2(int height, int width, int seed, int R, float[,] fallOffMap)
    {

        List<Vector3> retour = new List<Vector3>();

        float[,] bluenoise = Noise.GenerateNoiseMap(width, height, seed, 40, 5, 0.5f, 4, new Vector2(0, 0));
        res = new float[width, height];

        for (int yc = 0; yc < height; yc++)
        {
            for (int xc = 0; xc < width; xc++)
            {
                double max = 0;
                // there are more efficient algorithms than this
                for (int yn = yc - R; yn <= yc + R; yn++)
                {
                    for (int xn = xc - R; xn <= xc + R; xn++)
                    {
                        if (0 <= yn && yn < height && 0 <= xn && xn < width)
                        {
                            double e = bluenoise[yn, xn];
                            if (e > max) { max = e; }
                        }
                    }
                }
                if (bluenoise[yc, xc] == max)
                {
                    res[yc, xc] = 1;
                    // place tree at xc,yc
                }
            }
        }


        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        for (int y = 0; y < height; y += 1)
        {
            for (int x = 0; x < width; x += 1)
            {
                if (fallOffMap[x, y] == 1.0f)
                {
                    res[x, y] = 0;
                }
                else if (res[x, y] == 1.0f)
                {

                    float f = Mathf.Clamp01(-1.0f * (gen.topIsland.noiseMap[x, y] - 1.0f));
                    float yy = 140 + 10 * gen.topIsland.meshHeightCurve.Evaluate(f);

                    retour.Add(new Vector3(topLeftX+x, yy, topLeftZ-y));

                }
            }
        }
        return retour;


    }



}
