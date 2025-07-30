using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer)), ExecuteInEditMode]
public class PlatformGenerator : MonoBehaviour
{
    private SplineContainer splineContainer;
    private float distance;
    private List<Vector3> points;
    private List<Vector3> tans;
    private GameObject childMesh;

    [SerializeField] Mesh wallMesh;
    [SerializeField] Material wallMaterial;
    [SerializeField] Material floorMaterial;
    [SerializeField] float distanceSubtractor = 0.1f;
    [SerializeField] float floorHeight = 0;
    [SerializeField] private float rotationOffsetDegrees = 0f;


    public struct VertexInfo
    {
        public int index;
        public Vector3 position;
        public Vector2 uv;

        public VertexInfo(int index, Vector3 position, Vector2 uv)
        {
            this.index = index;
            this.position = position;
            this.uv = uv;
        }

        public VertexInfo(int index, Vector3 position)
        {
            this.index = index;
            this.position = position;
            this.uv = position;
        }
    }

    public struct Quad
    {
        public List<VertexInfo> verticies;
        public List<int> triangles;

        public Quad(int offset, params Vector3[] vertexPositions)
        {
            verticies = new List<VertexInfo>();
            triangles = new List<int>();

            for (int i = 0; i < vertexPositions.Length; i++)
            {
                verticies.Add(new VertexInfo(i + offset, vertexPositions[i]));
            }

            CalculateUVs();

            SetTriangles();
        }

        private void SetTriangles()
        {
            int a = verticies[0].index;
            int b = verticies[1].index;
            int c = verticies[2].index;
            int d = verticies[3].index;

            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);

            triangles.Add(c);
            triangles.Add(d);
            triangles.Add(a);
        }

        public List<Vector3> GetVerticies()
        {
            return new List<Vector3> {
                verticies[0].position,
                verticies[1].position,
                verticies[2].position,
                verticies[3].position,
            };
        }

        public List<Vector2> GetUVs()
        {
            return new List<Vector2> {
                verticies[0].uv,
                verticies[1].uv,
                verticies[2].uv,
                verticies[3].uv,
            };
        }

        public List<int> GetTriangles()
        {
            return triangles;
        }

        private void CalculateUVs()
        {
            VertexInfo a = verticies[0];
            VertexInfo b = verticies[1];
            VertexInfo c = verticies[2];
            VertexInfo d = verticies[3];

            float bottomDistanceX = Vector3.Distance(a.position, d.position);
            float topDistanceX = Vector3.Distance(b.position, c.position);
            float distanceXDifference = topDistanceX - bottomDistanceX;
            float distanceYLeft = Vector3.Distance(a.position, b.position);
            float distanceYRight = Vector3.Distance(d.position, c.position);

            Vector2 v0 = new Vector2(0f, 0f);
            Vector2 v1 = new Vector2(0f, distanceYLeft);
            Vector2 v2 = new Vector2(topDistanceX, distanceYRight);
            Vector2 v3 = new Vector2(bottomDistanceX, 0f);

            a.uv = v0;
            b.uv = v1;
            c.uv = v2;
            d.uv = v3;

            verticies[0] = a;
            verticies[1] = b;
            verticies[2] = c;
            verticies[3] = d;

        }
    }

    void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
    }

    private void OnEnable()
    {
        Spline.Changed += OnSplineChanged;
    }
    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }

    private void OnSplineChanged(Spline spline, int value, SplineModification modification)
    {
        if (spline == splineContainer.Spline && modification == SplineModification.KnotModified)
        {
            CalculatePoints();
            GenerateMesh();
        }
    }

    private void OnValidate()
    {
        if (wallMesh == null) return;

        // Inizializza splineContainer se non è stato fatto
        if (splineContainer == null)
            splineContainer = GetComponent<SplineContainer>();

        if (splineContainer == null || splineContainer.Spline == null) return;

        distance = wallMesh.bounds.size.x - distanceSubtractor;

        CalculatePoints();
        GenerateMesh();
    }

    private void CalculatePoints()
    {
        points = new List<Vector3>();
        tans = new List<Vector3>();

        Spline spline = splineContainer.Spline;

        points.Add(spline.EvaluatePosition(0f));
        tans.Add(spline.EvaluateTangent(0f));

        if (distance <= 0f) return;

        spline.GetPointAtLinearDistance(0f, distance, out float t);

        while (t < 1f)
        {
            points.Add(spline.EvaluatePosition(t));
            tans.Add(spline.EvaluateTangent(t));

            spline.GetPointAtLinearDistance(t, distance, out t);
        }

    }

    private void OnDrawGizmosSelected()
    {
        if (points == null) return;

        foreach (Vector3 point in points)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(point, 0.25f);
        }

    }

    private void GenerateMesh()
    {
        // Controllo di sicurezza per points
        if (points == null || points.Count == 0) return;

        if (childMesh == null)
        {
            childMesh = new GameObject("Mesh", typeof(MeshRenderer), typeof(MeshFilter));
            childMesh.transform.SetParent(transform, false);
        }

        List<CombineInstance> instances = new();

        Vector3 position;
        Matrix4x4 offsetMatrix;
        CombineInstance combineInstance;
        Mesh meshInstance = Instantiate(wallMesh);

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 dir = points[i] - points[i + 1];
            position = points[i];

            Vector3 lookDir = new Vector3(-dir.z, dir.y, dir.x);
            Quaternion baseRotation = Quaternion.LookRotation(lookDir);
            Quaternion offsetRotation = Quaternion.Euler(0, rotationOffsetDegrees, 0);
            Quaternion finalRotation = baseRotation * offsetRotation;

            offsetMatrix = Matrix4x4.TRS(position, finalRotation, Vector3.one);


            for (int s = 0; s < meshInstance.subMeshCount; s++)
            {
                combineInstance = new CombineInstance();
                combineInstance.mesh = meshInstance;
                combineInstance.transform = offsetMatrix;
                combineInstance.subMeshIndex = s;
                instances.Add(combineInstance);
            }
        }

        instances = CombineBySubmeshIndex(instances);

        BuildSimpleFloorMesh(instances);

        List<SubMeshDescriptor> subMeshes = InstancesToSubMeshData(instances);

        Mesh finalMesh = new Mesh();
        finalMesh.name = "Wall mesh";
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        finalMesh.CombineMeshes(instances.ToArray(), false);
        finalMesh.SetSubMeshes(subMeshes.ToArray());

        MeshFilter filter = childMesh.GetComponent<MeshFilter>();
        filter.sharedMesh = finalMesh;

        // Applica i materiali
        ApplyMaterials();
    }

    private void BuildSimpleFloorMesh(List<CombineInstance> instances)
    {
        // Controlli di sicurezza
        if (points == null || points.Count < 3) return; // Serve almeno 3 punti per creare un piano

        List<Vector3> floorVertices = new List<Vector3>();
        List<Vector2> floorUVs = new List<Vector2>();
        List<int> floorTriangles = new List<int>();

        // Aggiungi tutti i punti della spline all'altezza del pavimento
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 floorPoint = points[i];
            floorPoint.y = floorHeight;
            floorVertices.Add(floorPoint);

            // UV semplici basate sulla posizione XZ normalizzate
            floorUVs.Add(new Vector2(floorPoint.x * 0.1f, floorPoint.z * 0.1f));
        }

        // Triangolazione a ventaglio dal primo vertice con ordine invertito per normali corrette
        for (int i = 1; i < points.Count - 1; i++)
        {
            // Triangolo: 0, i+1, i (ordine invertito per normale verso l'alto)
            floorTriangles.Add(0);
            floorTriangles.Add(i + 1);
            floorTriangles.Add(i);
        }

        // Crea la mesh del pavimento
        Mesh floorMesh = new Mesh();
        floorMesh.name = "Floor Mesh";
        floorMesh.vertices = floorVertices.ToArray();
        floorMesh.triangles = floorTriangles.ToArray();
        floorMesh.uv = floorUVs.ToArray();
        floorMesh.RecalculateNormals();

        // Aggiungi alla lista delle istanze
        CombineInstance floorInstance = new CombineInstance();
        floorInstance.transform = Matrix4x4.identity;
        floorInstance.mesh = floorMesh;
        floorInstance.subMeshIndex = 0;

        instances.Add(floorInstance);
    }

    private List<SubMeshDescriptor> InstancesToSubMeshData(List<CombineInstance> instances)
    {
        List<SubMeshDescriptor> descriptors = new List<SubMeshDescriptor>();

        int triangleOffset = 0;

        foreach (var ci in instances)
        {
            Mesh mesh = ci.mesh;
            Matrix4x4 transform = ci.transform;

            int[] meshTris = mesh.GetTriangles(0);
            descriptors.Add(new SubMeshDescriptor(triangleOffset, meshTris.Length));
            triangleOffset += meshTris.Length;
        }

        return descriptors;
    }

    private List<CombineInstance> CombineBySubmeshIndex(List<CombineInstance> meshes)
    {
        Dictionary<int, List<CombineInstance>> instanceDictionary = new Dictionary<int, List<CombineInstance>>();
        foreach (CombineInstance item in meshes)
        {
            if (instanceDictionary.TryGetValue(item.subMeshIndex, out List<CombineInstance> values))
            {
                values.Add(item);
            }
            else
            {
                instanceDictionary.Add(item.subMeshIndex, new List<CombineInstance>() { item });
            }
        }

        List<CombineInstance> inst = new List<CombineInstance>();
        foreach (var valueSet in instanceDictionary.Values)
        {
            List<CombineInstance> combineInstances = valueSet;
            Mesh m = new Mesh();
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.CombineMeshes(combineInstances.ToArray(), true);

            CombineInstance c = new CombineInstance();
            c.transform = Matrix4x4.identity;
            c.mesh = m;
            c.subMeshIndex = combineInstances[0].subMeshIndex;
            inst.Add(c);
        }

        return inst;
    }

    private void ApplyMaterials()
    {
        if (childMesh == null) return;

        MeshRenderer renderer = childMesh.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        // Crea un array di materiali per i submesh
        List<Material> materials = new List<Material>();

        // Aggiunge i materiali del muro per tutti i submesh tranne l'ultimo (che è il pavimento)
        Mesh mesh = childMesh.GetComponent<MeshFilter>().sharedMesh;
        if (mesh != null)
        {
            for (int i = 0; i < mesh.subMeshCount - 1; i++)
            {
                materials.Add(wallMaterial);
            }

            // Aggiunge il materiale del pavimento per l'ultimo submesh
            materials.Add(floorMaterial);
        }

        renderer.materials = materials.ToArray();
    }
}