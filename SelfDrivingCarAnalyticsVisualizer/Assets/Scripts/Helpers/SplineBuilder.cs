using System.Collections.Generic;
using UnityEngine;

public class SplineBuilder
{
    private readonly Vector3[,] coefficients;

    private float[] distanceBetweenPoints;

    public float FlightTime { get; private set; }

    public SplineBuilder(Vector3[] points)
    {
        distanceBetweenPoints = new float[points.Length - 1];

        for (int i = 0; i < distanceBetweenPoints.Length; i++)
        {
            distanceBetweenPoints[i] = Vector3.Distance(points[i], points[i + 1]);
            FlightTime += distanceBetweenPoints[i];
        }

        coefficients = CalculateCoefficients(points);
    }

    public bool IsFinishedAtTime(float t)
    {
        int i = 0;
        while (i < distanceBetweenPoints.Length && distanceBetweenPoints[i] < t)
        {
            t -= distanceBetweenPoints[i];
            i++;
        }
        return i == distanceBetweenPoints.Length;
    }

    public Vector3 GetPositionAtTime(float t)
    {
        int splineIndex = 0;
        while (distanceBetweenPoints[splineIndex] < t)
        {
            t -= distanceBetweenPoints[splineIndex];
            splineIndex++;
        }

        t /= distanceBetweenPoints[splineIndex];

        return coefficients[splineIndex, 0] + coefficients[splineIndex, 1] * t + coefficients[splineIndex, 2] * t * t + coefficients[splineIndex, 3] * t * t * t;
    }

    public Vector3 GetSpeedAtTime(float t)
    {
        int splineIndex = 0;
        while (distanceBetweenPoints[splineIndex] < t)
        {
            t -= distanceBetweenPoints[splineIndex];
            splineIndex++;
        }

        t /= distanceBetweenPoints[splineIndex];

        return coefficients[splineIndex, 1] + 2 * coefficients[splineIndex, 2] * t + 3 * coefficients[splineIndex, 3] * t * t;
    }

    public Vector2[] GetSplinePoints(Vector2[] points, float step)
    {
        Vector3[] points3d = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            points3d[i] = points[i];
        }
        Vector3[] result3D = GetSplinePoints(points3d, step);
        Vector2[] result2D = new Vector2[result3D.Length];
        for (int i = 0; i < result3D.Length; i++)
        {
            result2D[i] = result3D[i];
        }
        return result2D;
    }

    public Vector3[] GetSplinePoints(Vector3[] points, float step)
    {
        Vector3[,] coefficients = CalculateCoefficients(points);

        List<Vector3> spline = new List<Vector3>();

        float t = 0;

        for (int i = 0; i < points.Length - 1; i++)
        {
            while (t < i + 1)
            {
                AddToSpline(spline, GetSplineAtTime(t));
                t += step;
            }
            if (spline[spline.Count - 1] != points[i + 1])
            {
                AddToSpline(spline, points[i + 1]);
            }
        }

        return spline.ToArray();
    }

    private static void AddToSpline(List<Vector3> spline, Vector3 point)
    {
        if (spline.Count == 0 || spline[spline.Count - 1] != point)
        {
            spline.Add(point);
        }
    }

    public Vector3 GetSplineAtTime(float t)
    {
        int splineIndex = Mathf.Min(Mathf.FloorToInt(t), coefficients.GetLength(0) - 1);
        return coefficients[splineIndex, 0] + coefficients[splineIndex, 1] * (t - splineIndex) + coefficients[splineIndex, 2] * (t - splineIndex) * (t - splineIndex) + coefficients[splineIndex, 3] * (t - splineIndex) * (t - splineIndex) * (t - splineIndex);
    }

    private static Vector3[,] CalculateCoefficients(Vector3[] points)
    {
        Vector3[,] coefficients = new Vector3[points.Length, 4];

        Vector3[] z = new Vector3[points.Length];
        Vector3[] a = new Vector3[points.Length];

        float[] l = new float[points.Length];
        float[] u = new float[points.Length];

        int n = points.Length - 1;

        for (int i = 1; i <= n - 1; i++)
        {
            a[i] = 3 * (points[i + 1] - 2 * points[i] + points[i - 1]);
        }

        coefficients[n, 2] = Vector3.zero;
        z[0] = Vector3.zero;
        z[n] = Vector3.zero;
        l[0] = l[n] = 1;
        u[0] = 0;

        for (int i = 1; i <= n - 1; i++)
        {
            l[i] = 4 - u[i - 1];
            u[i] = 1 / l[i];
            z[i] = (a[i] - z[i - 1]) / l[i];
        }

        for (int i = 0; i < points.Length; i++)
        {
            coefficients[i, 0] = points[i];
        }

        for (int j = n - 1; j >= 0; j--)
        {
            coefficients[j, 2] = z[j] - u[j] * coefficients[j + 1, 2];
            coefficients[j, 3] = (coefficients[j + 1, 2] - coefficients[j, 2]) / 3.0f;
            coefficients[j, 1] = points[j + 1] - points[j] - coefficients[j, 2] - coefficients[j, 3];
        }

        return coefficients;
    }
}