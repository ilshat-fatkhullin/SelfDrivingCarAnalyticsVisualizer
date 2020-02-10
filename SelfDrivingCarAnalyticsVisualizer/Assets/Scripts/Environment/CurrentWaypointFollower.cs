using UnityEngine;

public class CurrentWaypointFollower : MonoBehaviour
{
    [SerializeField]
    private Vector3 _offset;

    private void Start()
    {
        EventBus.Instance.OnCurrentWaypointChange.AddListener(OnCurrentWaypointChanged);
    }

    private void OnCurrentWaypointChanged(Vector3 current)
    {
        transform.position = _offset + current;
        transform.LookAt(current);
    }
}
