using UnityEngine;
using System;

[ExecuteInEditMode]
public class Water : MonoBehaviour
{
    private readonly float pi = Mathf.PI;

    [Serializable]
    public struct Octave
    {
        public float waveAmplitude;
        [Range(0.01f, 10.0f)] public float waveLength;
        public float speed;
        public Vector2 direction;
        public float noiseAmount;
        [Range(0.1f, 10.0f)] public float noiseResolution;

    };

    [Range(4, 256)] public int meshResolution = 20;

    public Octave[] octaves;
    public float globalPhase;
    [Range(0.01f, 2.00f)]public float timeMultiplier;


    public Color myColor;
    public Texture myMainTex;
    public Texture myNormalTex;
    public Texture myMetallicMap;
    public Texture myOcclusionTex;
    public Texture myHeightMap;
    public Vector2 tile;
    [Range(0.0f, 1.0f)] public float metallic;
    [Range(0.0f, 1.0f)] public float smoothness;
    public Material myMaterial;
    public Shader myShader;

    private MeshFilter myMeshFilter;
    private MeshRenderer myMeshRenderer;
    private Mesh myMesh;

    private void Start() {

        if (!gameObject.GetComponent<MeshFilter>()) {
            myMeshFilter = gameObject.AddComponent<MeshFilter>();
        } else {
            myMeshFilter = GetComponent<MeshFilter>();
        }
        myMeshFilter.sharedMesh = new Mesh();
        myMesh = myMeshFilter.sharedMesh;

        if (!gameObject.GetComponent<MeshRenderer>()) {
            myMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        } else {
            myMeshRenderer = GetComponent<MeshRenderer>();
        }

        myMaterial = new Material(myShader);
    }

    private void Update() {
        UpdateMesh();
        UpdateMaterial();
    }

    private void UpdateMesh() {

        myMesh.Clear();

        Vector3[] newVertices = new Vector3[meshResolution * meshResolution];
        int[] newTriangles = new int[(meshResolution - 1) * (meshResolution - 1) * 6];
        Vector2[] newUVs = new Vector2[meshResolution * meshResolution];

        int triangleIdx = 0;

        for (int i = 0; i < meshResolution; i++)
        {
            for (int j = 0; j < meshResolution; j++)
            {

                int idx = i + j * meshResolution;
                float x = (float)i / (meshResolution - 1);
                float z = (float)j / (meshResolution - 1);
                float y = newVertices[idx].y;

                foreach (Octave oc in octaves)
                {
                    y += oc.waveAmplitude * oc.direction.x * Mathf.Cos(2 * pi * (globalPhase * oc.speed + x) / oc.waveLength);
                    y += oc.waveAmplitude * oc.direction.y * Mathf.Sin(2 * pi * (globalPhase * oc.speed + z) / oc.waveLength);

                    y += oc.noiseAmount * Mathf.PerlinNoise(transform.localScale.x * oc.noiseResolution * x + globalPhase * oc.direction.x, transform.localScale.z * oc.noiseResolution * z + globalPhase * oc.direction.y);
                }
                
                newVertices[idx] = new Vector3(x, y, z);

                if (i != meshResolution-1 && j != meshResolution-1) {
                    newTriangles[triangleIdx] = idx;
                    newTriangles[triangleIdx + 1] = idx + meshResolution;
                    newTriangles[triangleIdx + 2] = idx + meshResolution + 1;
                    newTriangles[triangleIdx + 3] = idx;
                    newTriangles[triangleIdx + 4] = idx + meshResolution + 1;
                    newTriangles[triangleIdx + 5] = idx + 1;
                    triangleIdx += 6;
                }

                newUVs[idx] = new Vector2(i, j);
            }
        }

        if (Application.isPlaying) {
            globalPhase += Time.deltaTime * timeMultiplier;;
        }

        myMesh.vertices = newVertices;
        myMesh.triangles = newTriangles;
        myMesh.uv = newUVs;

        myMesh.RecalculateNormals();
        myMesh.RecalculateTangents();
        myMesh.OptimizeReorderVertexBuffer();

    }

    private void UpdateMaterial() {

        myMaterial.SetColor("_Color", myColor);

        myMaterial.SetTexture("_MainTex", myMainTex);
        myMaterial.SetTexture("_BumpMap", myNormalTex);
        myMaterial.SetTexture("_OcclusionMap", myOcclusionTex);
        myMaterial.SetTexture("_MetallicGlossMap", myMetallicMap);
        myMaterial.SetTexture("_ParallaxMap", myHeightMap);

        myMaterial.SetFloat("_Glossiness", smoothness);
        myMaterial.SetFloat("_Metallic", metallic);

        myMaterial.SetTextureScale("_MainTex", new Vector2(tile.x, tile.y));

        myMeshRenderer.sharedMaterial = myMaterial;
    }


}
