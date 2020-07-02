using geniikw.DataRenderer2D;
using System.Data.Odbc;
using UnityEngine;

public class PointCloudMeshBuilder : MonoBehaviour
{
    [SerializeField]
    private float particleSize;

    [SerializeField]
    private new ParticleSystem particleSystem;

    [SerializeField]
    private float gradientRange;

    private ParticleSystem.Particle[] cloud;

    private bool pointsUpdated;

    private DriveData driveData;

    private SplineBuilder splineBuilder;

    private void Start()
    {
        EventBus.Instance.OnDataLoad.AddListener(OnDataLoad);
        EventBus.Instance.OnTimelineValueChange.AddListener(OnTimelineValueChanged);
        EventBus.Instance.OnCurrentWaypointChange.AddListener(OnCurrentWaypointChanged);
        EventBus.Instance.OnSplineBuilderInitialized.AddListener(OnSplineBuilderInitialized);
    }

    private void Update()
    {
        if (pointsUpdated)
        {
            particleSystem.SetParticles(cloud, cloud.Length);
            pointsUpdated = false;
        }
    }

    private bool HasData()
    {
        return driveData != null && driveData.PointCloudFrames.Frames != null && driveData.PointCloudFrames.Frames.Length != 0;
    }

    private void OnDataLoad(DriveData driveData)
    {
        this.driveData = driveData;

        if (!HasData())
            return;

        LoadFrame(driveData.PointCloudFrames.Frames[0], 0);
    }

    private void OnTimelineValueChanged(float value)
    {
        if (!HasData())
            return;
        LoadFrame(driveData.PointCloudFrames.GetFrameAtNormalizedTime(value), value);
    }

    private void LoadFrame(PointCloudFrame frame, float time)
    {
        Vector3[] vertices = new Vector3[frame.Points.Length];
        Color[] colors = new Color[frame.Points.Length];
        for (int i = 0; i < frame.Points.Length; i++)
        {
            vertices[i] = frame.Points[i].ToVector();
            colors[i] = Color.Lerp(Color.green, Color.blue, vertices[i].y / gradientRange);
        }
        SetPoints(vertices, colors);

        float splineTime = driveData.PositionFrames.GetSplineTimeAtNormalizedTime(time);
        particleSystem.transform.localRotation = Quaternion.LookRotation(splineBuilder.GetVelocityAtTime(splineTime));
    }

    private void SetPoints(Vector3[] positions, Color[] colors)
    {
        cloud = new ParticleSystem.Particle[positions.Length];

        for (int ii = 0; ii < positions.Length; ++ii)
        {
            cloud[ii].position = positions[ii];
            cloud[ii].startColor = colors[ii];
            cloud[ii].startSize = particleSize;
        }

        pointsUpdated = true;
    }

    private void OnCurrentWaypointChanged(Vector3 current)
    {
        particleSystem.transform.position = current;
    }

    private void OnSplineBuilderInitialized(SplineBuilder splineBuilder)
    {
        this.splineBuilder = splineBuilder;
    }
}
