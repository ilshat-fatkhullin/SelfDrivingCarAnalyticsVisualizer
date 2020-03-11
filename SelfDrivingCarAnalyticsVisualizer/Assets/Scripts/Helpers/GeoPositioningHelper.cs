using System;
using UnityEngine;

public static class GeoPositioningHelper
{
    public static Vector2 GetMetersFromCoordinate(Coordinate coordinate)
    {
        float latitude = coordinate.Latitude * Mathf.Deg2Rad;
        float longitude = coordinate.Longitude * Mathf.Deg2Rad;
        float x = NumericConstants.EARTH_RADIUS * longitude;
        float y = NumericConstants.EARTH_RADIUS * Mathf.Log(Mathf.Tan(Mathf.PI / 4 + latitude / 2));
        return new Vector2(x, y);
    }

    public static Coordinate GetCoordinateFromMeters(Vector2 meters)
    {
        float longitude = meters.x / NumericConstants.EARTH_RADIUS;
        float latitude = (Mathf.Atan(Mathf.Exp(meters.y / NumericConstants.EARTH_RADIUS)) - Mathf.PI / 4) * 2;
        return new Coordinate(latitude * Mathf.Rad2Deg, longitude * Mathf.Rad2Deg);
    }
}

public class Coordinate
{
    public float Latitude { get; private set; }

    public float Longitude { get; private set; }

    public Coordinate(float latitude, float longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}