using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text;

using Edge = MeshHelper.Edge;
public static class MeshHelper2
{
    static List<Vector3> vertices;
    static List<Vector3> normals;
    static List<Color> colors;
    static List<Vector2> uv;
    static List<Vector2> uv1;
    static List<Vector2> uv2;

    static List<int> indices;
    static Dictionary<uint, int> newVectices;


    /*
    public static void DrawAllEdges(Mesh m) {
        List<Edge> e = new List<Edge>();
        for (int i = 0; i < m.triangles.Length-1; i++) {
            e.Add(new Edge(m.triangles[i], m.triangles[i + 1]));
        }
        NormalsVisualizer.edges = e;
    }
    */

    public static void ReverseNormals(MeshFilter filter)
    {
        Mesh mesh = filter.sharedMesh;
        Vector3[] verticesOriginal = mesh.vertices;
        if (filter != null)
        {

            Vector3[] normals = mesh.normals;
            for (int i = verticesOriginal.Length; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;


            int[] triangles2 = mesh.triangles;
            for (int i = verticesOriginal.Length; i < triangles2.Length; i += 3)
            {
                    int temp = triangles2[i + 0];
                    triangles2[i + 0] = triangles2[i + 1];
                    triangles2[i + 1] = temp;
            }
            mesh.triangles = triangles2;
            
        }

        //mesh.RecalculateNormals();



    }

    //logique semble bonne mais marche pas
    public static List<Edge> getBoundaries(Vector3[] vertices,
    int[] triangles) {

        List<Edge> boundaries = new List<Edge>();
        for (int i = 0; i < triangles.Length - 1; i++)
        {
            boundaries.Add(new Edge(triangles[i], triangles[i + 1]));
        }

        //OPTIMISABLE (POTENTIELLEMENT AVEC JUSTE GET TAILLE A DESSUS DE N
        List<Edge> internals = new List<Edge>();
        foreach (Edge e in boundaries)
        {
            foreach (Edge e2 in boundaries)
            {
                if (e.IsOpposite(e2))
                {
                    internals.Add(e);
                    internals.Add(e2);
                }
            }
        }

        //Optimisable avec RemoveAll
        foreach (Edge e in internals)
        {
            boundaries.Remove(e);
        }

        return null;
    }

    public static List<Edge> getBoundariesByLimit(Vector3[] vertices,
    int[] triangles, float limite)
    {

        List<Edge> retour = new List<Edge>();

        for (int i = 0; i < triangles.Length-1; i++) {
            if (vertices[triangles[i]].y > limite && vertices[triangles[i + 1]].y > limite) {
                retour.Add(new Edge(triangles[i], triangles[i + 1]));
            }
        }
        return retour;
    }

    public static List<Edge> getBoundariesByLimit2(Vector3[] vertices,
int[] triangles, float limite)
    {

        List<Edge> retour = new List<Edge>();
        int j = 0;
        for (int i = 0; i < triangles.Length - 3; i+=3)
        {
            if (vertices[triangles[i]].y > limite && vertices[triangles[i + 1]].y > limite)
            {
                retour.Add(new Edge(triangles[i], triangles[i + 1]));
            }

            if (vertices[triangles[i+1]].y > limite && vertices[triangles[i + 2]].y > limite)
            {
                retour.Add(new Edge(triangles[i+1], triangles[i + 2]));
            }
            if (vertices[triangles[i]].y > limite && vertices[triangles[i + 2]].y > limite)
            {
                retour.Add(new Edge(triangles[i], triangles[i + 2]));
            }
        }


        return retour;
    }

    //https://stackoverflow.com/questions/3848923/how-to-extrude-a-flat-2d-mesh-giving-it-depth
    //Attention ordre important car indique normals
    public static void Extrude(MeshFilter filter, float txHauteur, float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve) {


        Mesh mesh = filter.sharedMesh;

        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] triangles = mesh.triangles;

        //-------------------1)
        List<Vector3> nVertices = new List<Vector3>(vertices);
        List<Vector2> nUVs = new List<Vector2>(uvs);

        List<Vector3> addedVertices = new List<Vector3>(vertices);

        List<int> newTriangles = new List<int>(triangles);

        //A changer pour arrondir au bord
        //float[,] heightMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);


        
        StringBuilder sb = new StringBuilder();

        StringBuilder sb2 = new StringBuilder();
        //probleme zone est censé etre moins dans le coin bas gauche
        //l'uv respecte la contrainte du lieu mais pas la heightmap


        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        //de -120 à 120

        for (int i = 0; i < addedVertices.Count; i++) {
            Vector3 v = addedVertices[i];

            int x = (int)(nUVs[i].x * width);
            int y =  (int)(nUVs[i].y * height);
            

            sb.Append(x + "," + y+"\n");
            sb2.Append(nUVs[i].x + "," + nUVs[i].y + "\n");

            v.y = 140 + heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;

            addedVertices[i] = v;
        }
        
        Debug.Log("sb1"+sb);
        Debug.Log("sb2"+sb2);
        Debug.Log("heightmap length :" + width);

        

        nVertices.AddRange(addedVertices);
        
        nUVs.AddRange(nUVs);

        //-----------------2)

        List<Edge> boundaries = getBoundariesByLimit2(vertices, triangles, 128 * txHauteur);


        //----------------3)

        int n = vertices.Length;


        for (int i = 0; i < triangles.Length; i++) {
            newTriangles.Add(triangles[i]+n);
        }


        //-------------4)

        //We display the triangle of the 2 sides because some triangles are inversed
        
        //Side 1
        foreach (Edge e in boundaries) {
            newTriangles.Add(e.a);
            newTriangles.Add(e.b);
            newTriangles.Add(e.b + n);

            newTriangles.Add(e.b + n);
            newTriangles.Add(e.a + n);
            newTriangles.Add(e.a);

        }
        

        //Side 2
        
        foreach (Edge e in boundaries)
        {
            newTriangles.Add(e.a);
            newTriangles.Add(e.a + n);
            newTriangles.Add(e.b + n);

            newTriangles.Add(e.b + n);
            newTriangles.Add(e.b);
            newTriangles.Add(e.a);

        }
        
        //5* normals
        List<Vector3> newNormals = new List<Vector3>(mesh.normals);

        for (int i = 0; i < n; i++)
        {
            newNormals[i] = -newNormals[i];
            newNormals.Add(Vector3.up);
        }
        for (int i = 0; i < triangles.Length; i += 3) {

            int temp = newTriangles[i + 0];
            newTriangles[i + 0] = newTriangles[i + 1];
            newTriangles[i + 1] = temp;
        }


        filter.sharedMesh.subMeshCount = 2;

        mesh.vertices = nVertices.ToArray();

        List<int> triangle1 = newTriangles.GetRange(0, triangles.Length);
        List<int> triangle2 = newTriangles.GetRange(triangles.Length, newTriangles.Count - triangles.Length);

        mesh.SetTriangles(triangle1, 0);

        mesh.SetTriangles(triangle2, 1);

        mesh.normals = newNormals.ToArray();
        mesh.uv = nUVs.ToArray();
       


    }

    //txHauteur = taux d'erreur a laquel on definit la hauteur de l'ile (ex : 0.9 -> 90%);
    //faire attention et revoir pour recup l'edge
    //techniquement extrude
    public static void FillTerrainTop(MeshFilter filter, float txHauteur) {

        Dictionary<int, int> mapMiror = new Dictionary<int, int>();
        

        Mesh mesh = filter.sharedMesh;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;


        List<Vector3> nVertices = new List<Vector3>(mesh.vertices);
        List<int> newTriangles = new List<int>(mesh.triangles);

        List<int> edges = new List<int>();
        for (int i = 0; i < vertices.Length; i++) {

            //si pas EdgeTop
            if (vertices[i].y < txHauteur * 128)
            {
                nVertices.Add(new Vector3(vertices[i].x, 128, vertices[i].z));
                mapMiror.Add(i, nVertices.Count - 1);
            }
            else {
                edges.Add(i);
            }
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (edges.Contains(i)) {
                newTriangles.Add(triangles[i]);
            } else if (mapMiror.ContainsKey(triangles[i])) {//normalement le if n'est pas censé etre un cas (wtf)
                newTriangles.Add(mapMiror[triangles[i]]);
            }
        }
        mesh.vertices = nVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();

    }

    public static void RemovePlanePart(MeshFilter filter, bool removeVertices)
    {
        //old position of array to newPosition of array
        Dictionary<int, int> mapOldToNew = new Dictionary<int, int>();

        Mesh mesh = filter.sharedMesh;

        Vector2[] uvs = mesh.uv;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        List<Vector3> nVertices = new List<Vector3>();

        List<Vector2> nvUVS = new List<Vector2>();

        List<int> newTriangles = new List<int>();

        //edit remplacé i+j par count

        for (int i = 0; i < triangles.Length; i += 3)
        {

            float a = vertices[triangles[i]].y;
            float b = vertices[triangles[i + 1]].y;
            float c = vertices[triangles[i + 2]].y;

            if (a != b || a != c || b != c)
            {
                for (int j = 0; j < 3; j++) {
                 
                    if (!mapOldToNew.ContainsKey(triangles[i+j])) {
                        nvUVS.Add(uvs[triangles[i+j]]);
                        nVertices.Add((vertices[triangles[i+j]]));
                        mapOldToNew.Add(triangles[i + j], nVertices.Count-1);
                    }
                    newTriangles.Add(triangles[i+j]);
                }

            }

        }

        //Debug.Log("avant vertices count :" + vertices.Length);
        //Debug.Log("apres vertices count :" + nVertices.Count);
        if (removeVertices)
        {
            //on reassigne les triangle aux nouveaux vertices
            for (int i = 0; i < newTriangles.Count; i++)
            {
                if (mapOldToNew.ContainsKey(newTriangles[i]))
                {
                    newTriangles[i] = mapOldToNew[newTriangles[i]];
                }

            }
            mesh.triangles = newTriangles.ToArray();
            mesh.SetVertices(nVertices);
            mesh.uv = nvUVS.ToArray();
        }
        else {
            mesh.triangles = newTriangles.ToArray();
        }


        mesh.RecalculateBounds();
        mesh.RecalculateNormals();


    }


    public static void RemoveVerticesSuperior(MeshFilter filter, float val)
    {

        Mesh mesh = filter.sharedMesh;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        List<Vector3> newVertices = new List<Vector3>();

        List<int> newTriangles = new List<int>();

        int[] bonEntier = new int[3];
        bool bonTriangle = true;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            bonTriangle = true;
            for (int j = 0; j < 3; j++)
            {

                if (vertices[triangles[i + j]].y < val)
                {
                    newVertices.Add(vertices[triangles[i + j]]);
                    bonEntier[j] = triangles[i + j];
                }
                else
                {
                    bonTriangle = false;
                    break;
                }


            }
            if (bonTriangle)
                newTriangles.AddRange(bonEntier);


        }

        //mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();


    }


    public static void RemoveVerticesUnder(MeshFilter filter, float val)
    {

        Mesh mesh = filter.sharedMesh;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        List<Vector3> newVertices = new List<Vector3>();

        List<int> newTriangles = new List<int>();

        int[] bonEntier = new int[3];
        bool bonTriangle = true;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            bonTriangle = true;
            for (int j = 0; j < 3; j++)
            {
                if (vertices[triangles[i + j]].y > val)
                {
                    newVertices.Add(vertices[triangles[i + j]]);
                    bonEntier[j] = triangles[i + j];
                }
                else
                {
                    bonTriangle = false;
                    break;
                }


            }
            if (bonTriangle)
                newTriangles.AddRange(bonEntier);


        }

        //mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();



    }

    public static void ReverseNormals2(MeshFilter filter)
    {
        Mesh mesh = filter.sharedMesh;
        if (filter != null)
        {

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles2 = mesh.GetTriangles(m);
                for (int i = 0; i < triangles2.Length; i += 3)
                {
                    int temp = triangles2[i + 0];
                    triangles2[i + 0] = triangles2[i + 1];
                    triangles2[i + 1] = temp;
                }
                mesh.SetTriangles(triangles2, m);
            }
        }

        mesh.RecalculateNormals();



    }

    static void InitArrays(Mesh mesh)
    {
        vertices = new List<Vector3>(mesh.vertices);
        normals = new List<Vector3>(mesh.normals);
        colors = new List<Color>(mesh.colors);
        uv = new List<Vector2>(mesh.uv);
        uv2 = new List<Vector2>(mesh.uv2);
        indices = new List<int>();
    }
    static void CleanUp()
    {
        vertices = null;
        normals = null;
        colors = null;
        uv = null;
        uv1 = null;
        uv2 = null;
        indices = null;
    }

    #region Subdivide4 (2x2)
    static int GetNewVertex4(int i1, int i2)
    {
        int newIndex = vertices.Count;
        uint t1 = ((uint)i1 << 16) | (uint)i2;
        uint t2 = ((uint)i2 << 16) | (uint)i1;
        if (newVectices.ContainsKey(t2))
            return newVectices[t2];
        if (newVectices.ContainsKey(t1))
            return newVectices[t1];

        newVectices.Add(t1, newIndex);

        vertices.Add((vertices[i1] + vertices[i2]) * 0.5f);
        if (normals.Count > 0)
            normals.Add((normals[i1] + normals[i2]).normalized);
        if (colors.Count > 0)
            colors.Add((colors[i1] + colors[i2]) * 0.5f);

        return newIndex;
    }


    /// <summary>
    /// Devides each triangles into 4. A quad(2 tris) will be splitted into 2x2 quads( 8 tris )
    /// </summary>
    /// <param name="mesh"></param>
    public static void Subdivide4(Mesh mesh)
    {
        newVectices = new Dictionary<uint, int>();

        InitArrays(mesh);

        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            int a = GetNewVertex4(i1, i2);
            int b = GetNewVertex4(i2, i3);
            int c = GetNewVertex4(i3, i1);
            indices.Add(i1); indices.Add(a); indices.Add(c);
            indices.Add(i2); indices.Add(b); indices.Add(a);
            indices.Add(i3); indices.Add(c); indices.Add(b);
            indices.Add(a); indices.Add(b); indices.Add(c); // center triangle
        }
        mesh.vertices = vertices.ToArray();
        if (normals.Count > 0)
            mesh.normals = normals.ToArray();
        if (colors.Count > 0)
            mesh.colors = colors.ToArray();
        if (uv.Count > 0)
            mesh.uv = uv.ToArray();
        if (uv2.Count > 0)
            mesh.uv2 = uv2.ToArray();

        mesh.triangles = indices.ToArray();

        CleanUp();
    }
    #endregion Subdivide4 (2x2)

    #region Subdivide9 (3x3)
    static int GetNewVertex9(int i1, int i2, int i3)
    {
        int newIndex = vertices.Count;

        // center points don't go into the edge list
        if (i3 == i1 || i3 == i2)
        {
            uint t1 = ((uint)i1 << 16) | (uint)i2;
            if (newVectices.ContainsKey(t1))
                return newVectices[t1];
            newVectices.Add(t1, newIndex);
        }

        // calculate new vertex
        vertices.Add((vertices[i1] + vertices[i2] + vertices[i3]) / 3.0f);
        if (normals.Count > 0)
            normals.Add((normals[i1] + normals[i2] + normals[i3]).normalized);
        return newIndex;
    }


    /// <summary>
    /// Devides each triangles into 9. A quad(2 tris) will be splitted into 3x3 quads( 18 tris )
    /// </summary>
    /// <param name="mesh"></param>
    public static void Subdivide9(Mesh mesh)
    {
        newVectices = new Dictionary<uint, int>();

        InitArrays(mesh);

        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            int a1 = GetNewVertex9(i1, i2, i1);
            int a2 = GetNewVertex9(i2, i1, i2);
            int b1 = GetNewVertex9(i2, i3, i2);
            int b2 = GetNewVertex9(i3, i2, i3);
            int c1 = GetNewVertex9(i3, i1, i3);
            int c2 = GetNewVertex9(i1, i3, i1);

            int d = GetNewVertex9(i1, i2, i3);

            indices.Add(i1); indices.Add(a1); indices.Add(c2);
            indices.Add(i2); indices.Add(b1); indices.Add(a2);
            indices.Add(i3); indices.Add(c1); indices.Add(b2);
            indices.Add(d); indices.Add(a1); indices.Add(a2);
            indices.Add(d); indices.Add(b1); indices.Add(b2);
            indices.Add(d); indices.Add(c1); indices.Add(c2);
            indices.Add(d); indices.Add(c2); indices.Add(a1);
            indices.Add(d); indices.Add(a2); indices.Add(b1);
            indices.Add(d); indices.Add(b2); indices.Add(c1);
        }

        mesh.vertices = vertices.ToArray();
        if (normals.Count > 0)
            mesh.normals = normals.ToArray();

        mesh.triangles = indices.ToArray();

        CleanUp();
    }
    #endregion Subdivide9 (3x3)


    #region Subdivide
    /// <summary>
    /// This functions subdivides the mesh based on the level parameter
    /// Note that only the 4 and 9 subdivides are supported so only those divides
    /// are possible. [2,3,4,6,8,9,12,16,18,24,27,32,36,48,64, ...]
    /// The function tried to approximate the desired level 
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="level">Should be a number made up of (2^x * 3^y)
    /// [2,3,4,6,8,9,12,16,18,24,27,32,36,48,64, ...]
    /// </param>
    public static void Subdivide(Mesh mesh, int level)
    {
        if (level < 2)
            return;
        while (level > 1)
        {
            // remove prime factor 3
            while (level % 3 == 0)
            {
                Subdivide9(mesh);
                level /= 3;
            }
            // remove prime factor 2
            while (level % 2 == 0)
            {
                Subdivide4(mesh);
                level /= 2;
            }
            // try to approximate. All other primes are increased by one
            // so they can be processed
            if (level > 3)
                level++;
        }
    }
    #endregion Subdivide

    public static Mesh DuplicateMesh(Mesh mesh)
    {
        return (Mesh)UnityEngine.Object.Instantiate(mesh);
    }
}