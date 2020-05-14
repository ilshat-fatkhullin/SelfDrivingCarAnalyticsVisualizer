using Newtonsoft.Json;
using SFB;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class DataParser : MonoBehaviour
{
    public void LoadData()
    {
        string fileName = FileSelectionHelper.SelectFile(new ExtensionFilter("Data Files", "csv"));
        if (fileName == null)
        {
            return;
        }

        DriveData driveData = ParseFileToJson(fileName);

        using (StreamWriter writer = new StreamWriter("output.json"))
        {
            writer.Write(JsonConvert.SerializeObject(driveData));
        }

        if (driveData == null)
        {
            Debug.Log("Cannot read the file.");
            return;
        }

        EventBus.Instance.OnFileLoad.Invoke(fileName);
        EventBus.Instance.OnDataLoad.Invoke(driveData);
    }

    private DriveData ParseFileToJson(string fileName)
    {
        DriveData driveData = new DriveData();

        driveData.PositionFrames = ParsePositionFrames(fileName);
        driveData.PointCloudFrames = ParsePointCloudFrames(fileName);

        driveData.SpeedFrames = new FrameCollection<SingleValueFrame>()
        {
            Frames = new SingleValueFrame[]
            {
                new SingleValueFrame() { Value = 0},
                new SingleValueFrame() { Value = 1},
            },
        };
        driveData.AccelerationFrames = new FrameCollection<SingleValueFrame>()
        {
            Frames = new SingleValueFrame[]
            {
                new SingleValueFrame() { Value = 0},
                new SingleValueFrame() { Value = 1},
            },
        };
        driveData.PerceptionFrames = new FrameCollection<SingleValueFrame>()
        {
            Frames = new SingleValueFrame[]
            {
                new SingleValueFrame() { Value = 0},
                new SingleValueFrame() { Value = 1},
            },
        };

        return driveData;
    }

    private FrameCollection<PositionFrame> ParsePositionFrames(string fileName)
    {
        List<PositionFrame> positionFrames = new List<PositionFrame>();

        using (StreamReader reader = new StreamReader(fileName))
        {
            for (string l = reader.ReadLine(); l != null; l = reader.ReadLine())
            {
                string[] parts = l.Split(';', ',');
                float[] xyz = new float[3];
                float timestamp;
                bool isValid = true;

                for (int i = 17; i < 20; i++)
                {
                    if (!float.TryParse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture, out xyz[i - 17]))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (!float.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out timestamp))
                {
                    isValid = false;
                }

                if (isValid)
                {
                    float x = xyz[0], y = xyz[2], z = xyz[1];
                    if (positionFrames.Count > 0)
                    {
                        var last = positionFrames[positionFrames.Count - 1].Point;
                        if (last.X == x && last.Y == y && last.Z == z)
                        {
                            continue;
                        }
                    }

                    positionFrames.Add(
                        new PositionFrame() 
                        { 
                            Point = new FloatPoint() { X = x, Y = y, Z = z },
                            Timestamp = timestamp
                        });
                }
            }
        }

        return new FrameCollection<PositionFrame>() { Frames = positionFrames.ToArray() };
    }

    private FrameCollection<PointCloudFrame> ParsePointCloudFrames(string fileName)
    {
        List<PointCloudFrame> frames = new List<PointCloudFrame>();

        string directory = Path.GetDirectoryName(fileName);
        
        foreach (string path in Directory.GetFiles(directory))
        {
            if (Path.GetFileName(path) == Path.GetFileName(fileName))
                continue;

            frames.Add(ParsePointCloudFrame(path));
        }

        return new FrameCollection<PointCloudFrame>() { Frames = frames.ToArray() };
    }

    private PointCloudFrame ParsePointCloudFrame(string fileName)
    {
        PointCloudFrame pointCloudFrame = new PointCloudFrame();
        List<FloatPoint> positionFrames = new List<FloatPoint>();

        float averageTimestamp = 0;
        int timestampRecords = 0;

        using (StreamReader reader = new StreamReader(fileName))
        {
            for (string l = reader.ReadLine(); l != null; l = reader.ReadLine())
            {
                string[] parts = l.Split(';', ',');
                float[] xyz = new float[3];
                float timestamp;
                bool isValid = true;

                for (int i = 3; i < 6; i++)
                {
                    if (!float.TryParse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture, out xyz[i - 3]))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (!float.TryParse(parts[11], NumberStyles.Any, CultureInfo.InvariantCulture, out timestamp))
                {
                    continue;
                }
                else
                {
                    averageTimestamp += timestamp;
                    timestampRecords++;
                }

                if (isValid)
                {
                    positionFrames.Add(new FloatPoint() { X = xyz[1], Y = xyz[2], Z = xyz[0] });
                }
            }
        }

        pointCloudFrame.Points = positionFrames.ToArray();
        pointCloudFrame.Timestamp = averageTimestamp / timestampRecords;
        return pointCloudFrame;
    }
}
