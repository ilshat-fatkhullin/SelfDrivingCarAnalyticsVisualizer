using System.Collections.Generic;
using UnityEngine;

public class PointCloudMeshBuilder : MonoBehaviour
{
    [SerializeField]
    private float voxelSize;

    [SerializeField]
    private float worldVoxelSize;

    [SerializeField]
    private GameObject meshPrefab;

    private GameObject meshObject;

    private HashSet<IntPoint> filledPoints;

    private void Start()
    {
        EventBus.Instance.OnDataLoad.AddListener(OnDataLoad);

    }

    private void OnDataLoad(DriveData driveData)
    {
        if (driveData.PointCloudFrames == null)
            return;

        if (meshObject != null)
        {
            Destroy(meshObject);
        }

        filledPoints = new HashSet<IntPoint>(new IntPointEqualityComparer());

        foreach (var frame in driveData.PointCloudFrames.Frames)
        {
            var p = driveData.PositionFrames.GetFrameAtTime(frame.Timestamp).Point;

            foreach (var point in frame.Points)
            {
                int x = Mathf.RoundToInt((point.X + p.X) / voxelSize);
                int y = Mathf.RoundToInt((point.Y + p.Y) / voxelSize);
                int z = Mathf.RoundToInt((point.Z + p.Z) / voxelSize);

                var intPoint = new IntPoint() { X = x, Y = y, Z = z };

                if (!filledPoints.Contains(intPoint))
                {
                    filledPoints.Add(intPoint);
                }
            }
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3 right = Vector3.right * worldVoxelSize;
        Vector3 up = Vector3.up * worldVoxelSize;
        Vector3 forward = Vector3.forward * worldVoxelSize;

        meshObject = Instantiate(meshPrefab, Vector3.zero, Quaternion.identity);
        var meshFilter = meshObject.GetComponent<MeshFilter>();

        foreach (var point in filledPoints)
        {
            int x = point.X;
            int y = point.Y;
            int z = point.Z;

            int first = vertices.Count;

            Vector3 origin = x * right + y * up + z * forward;

            Vector3 a = origin;
            Vector3 b = origin + right;
            Vector3 c = origin + forward + right;
            Vector3 d = origin + forward;
            Vector3 e = a + up;
            Vector3 f = b + up;
            Vector3 g = c + up;
            Vector3 h = d + up;

            vertices.Add(a); // 0
            vertices.Add(b); // 1
            vertices.Add(c); // 2
            vertices.Add(d); // 3
            vertices.Add(e); // 4
            vertices.Add(f); // 5
            vertices.Add(g); // 6
            vertices.Add(h); // 7

            // AEF                  
            triangles.Add(first);
            triangles.Add(first + 4);
            triangles.Add(first + 5);

            // AFB                                        
            triangles.Add(first);
            triangles.Add(first + 5);
            triangles.Add(first + 1);

            // BFC
            triangles.Add(first + 1);
            triangles.Add(first + 5);
            triangles.Add(first + 2);

            // FGC
            triangles.Add(first + 5);
            triangles.Add(first + 6);
            triangles.Add(first + 2);

            // CGD
            triangles.Add(first + 2);
            triangles.Add(first + 6);
            triangles.Add(first + 3);

            // DGH
            triangles.Add(first + 3);
            triangles.Add(first + 6);
            triangles.Add(first + 7);

            // DHA

            triangles.Add(first + 3);
            triangles.Add(first + 7);
            triangles.Add(first);

            // AHE

            triangles.Add(first);
            triangles.Add(first + 7);
            triangles.Add(first + 4);

            // FEH

            triangles.Add(first + 5);
            triangles.Add(first + 4);
            triangles.Add(first + 7);

            // FHG

            triangles.Add(first + 5);
            triangles.Add(first + 7);
            triangles.Add(first + 6);

            // ABC

            triangles.Add(first);
            triangles.Add(first + 1);
            triangles.Add(first + 2);

            // ACD

            triangles.Add(first);
            triangles.Add(first + 2);
            triangles.Add(first + 3);
        }

        meshFilter.mesh.Clear();
        meshFilter.mesh.SetVertices(vertices);
        meshFilter.mesh.SetTriangles(triangles, 0);
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateTangents();
        meshFilter.mesh.Optimize();
    }
}
