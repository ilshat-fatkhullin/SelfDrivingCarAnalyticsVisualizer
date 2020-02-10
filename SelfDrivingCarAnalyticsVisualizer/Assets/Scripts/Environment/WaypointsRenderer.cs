using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WaypointsRenderer : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        EventBus.Instance.OnWaypointsUpdate.AddListener(OnWaypointsUpdate);
    }

    private void OnWaypointsUpdate(Vector3[] waypoints)
    {        
        _lineRenderer.positionCount = waypoints.Length;
        _lineRenderer.SetPositions(waypoints);
    }
}
