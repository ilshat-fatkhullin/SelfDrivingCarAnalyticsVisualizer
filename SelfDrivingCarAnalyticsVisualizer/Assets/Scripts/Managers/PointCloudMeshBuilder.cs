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

    private void Start()
    {
        EventBus.Instance.OnDataLoad.AddListener(OnDataLoad);
        EventBus.Instance.OnTimelineValueChange.AddListener(OnTimelineValueChanged);
        EventBus.Instance.OnCurrentWaypointChange.AddListener(OnCurrentWaypointChanged);
    }

    private void Update()
    {
        if (pointsUpdated)
        {
            particleSystem.SetParticles(cloud, cloud.Length);
            pointsUpdated = false;
        }
    }

    private void OnDataLoad(DriveData driveData)
    {
        this.driveData = driveData;
        LoadFrame(driveData.PointCloudFrames.Frames[0]);
    }

    private void OnTimelineValueChanged(float value)
    {
        if (driveData == null)
            return;
        LoadFrame(driveData.PointCloudFrames.GetFrameAtNormalizedTime(value));
    }

    private void LoadFrame(PointCloudFrame frame)
    {
        Vector3[] vertices = new Vector3[frame.Points.Length];
        Color[] colors = new Color[frame.Points.Length];
        for (int i = 0; i < frame.Points.Length; i++)
        {
            vertices[i] = frame.Points[i].ToVector();
            colors[i] = Color.Lerp(Color.green, Color.blue, vertices[i].y / gradientRange);
        }
        SetPoints(vertices, colors);
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
}
