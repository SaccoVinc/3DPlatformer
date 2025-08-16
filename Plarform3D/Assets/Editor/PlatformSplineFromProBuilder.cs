// Assets/Editor/PlatformSplineWindow.cs
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Splines;

public class PlatformSplineWindow : EditorWindow
{
    private string splineName = "PlatformSpline";
    private bool closedSpline = true;

    [MenuItem("Tools/Platform/Platform Spline Generator")]
    public static void ShowWindow()
    {
        GetWindow<PlatformSplineWindow>("Platform Spline");
    }

    private void OnGUI()
    {
        GUILayout.Label("Spline Settings", EditorStyles.boldLabel);
        splineName = EditorGUILayout.TextField("Spline Name", splineName);
        closedSpline = EditorGUILayout.Toggle("Closed Spline", closedSpline);

        if (GUILayout.Button("Generate from Selected Cubes"))
        {
            GenerateSpline();
        }
    }

    private void GenerateSpline()
    {
        var selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Select one or more ProBuilder cubes.", "OK");
            return;
        }

        var topPoints = new List<Vector3>();

        foreach (var go in selected)
        {
            var pb = go.GetComponent<ProBuilderMesh>();
            if (pb == null) continue;

            var positions = pb.positions;
            if (positions.Count == 0) continue;

            float maxY = float.MinValue;
            foreach (var pos in positions)
            {
                float wy = go.transform.TransformPoint(pos).y;
                if (wy > maxY) maxY = wy;
            }

            foreach (var pos in positions)
            {
                var wp = go.transform.TransformPoint(pos);
                if (Mathf.Abs(wp.y - maxY) < 0.0005f)
                    topPoints.Add(wp);
            }
        }

        if (topPoints.Count < 3)
        {
            EditorUtility.DisplayDialog("Not enough points", "Need at least 3 top vertices.", "OK");
            return;
        }

        var hull = ComputeConvexHullXZ(topPoints);

        // Create spline object
        var splineGO = new GameObject(splineName);
        var container = splineGO.AddComponent<SplineContainer>();
        var spline = new Spline { Closed = closedSpline };

        foreach (var p in hull)
        {
            var knot = new BezierKnot(p);
            spline.Add(knot);
        }

        // Auto tangents
        foreach (var p in hull)
        {
            var knot = new BezierKnot(p, Vector3.forward, Vector3.back);
            spline.Add(knot);
        }

        for (int i = 0; i < spline.Count; i++)
        {
            spline.SetTangentMode(i, TangentMode.AutoSmooth);
        }

        container.Spline = spline;

        Selection.activeObject = splineGO;
        Undo.RegisterCreatedObjectUndo(splineGO, "Create Platform Spline");
    }

    private List<Vector3> ComputeConvexHullXZ(List<Vector3> points)
    {
        var pts = points
            .Select(p => new Vector3(p.x, p.y, p.z))
            .Distinct()
            .OrderBy(p => p.x)
            .ThenBy(p => p.z)
            .ToList();

        var lower = new List<Vector3>();
        foreach (var p in pts)
        {
            while (lower.Count >= 2 && CrossXZ(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }

        var upper = new List<Vector3>();
        for (int i = pts.Count - 1; i >= 0; i--)
        {
            var p = pts[i];
            while (upper.Count >= 2 && CrossXZ(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(p);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);
        lower.AddRange(upper);

        return lower;
    }

    private float CrossXZ(Vector3 a, Vector3 b, Vector3 c)
    {
        var ab = new Vector2(b.x - a.x, b.z - a.z);
        var ac = new Vector2(c.x - a.x, c.z - a.z);
        return ab.x * ac.y - ab.y * ac.x;
    }
}
