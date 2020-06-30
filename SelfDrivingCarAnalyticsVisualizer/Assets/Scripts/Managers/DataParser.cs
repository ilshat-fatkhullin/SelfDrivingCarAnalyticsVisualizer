using CsvHelper;
using Newtonsoft.Json;
using SFB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        IEnumerable<CsvPositionEntity> records;

        using (StreamReader reader = new StreamReader(fileName))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = csv.GetRecords<CsvPositionEntity>().ToList();
            }
        }

        List<PositionFrame> positionFrames = new List<PositionFrame>();
        CsvPositionEntity lastRecord = null;

        foreach (var record in records)
        {
            if (lastRecord != null && 
                    (Mathf.Approximately(record.Timestamp, lastRecord.Timestamp) ||
                    (Mathf.Approximately(record.X, lastRecord.X) &&
                     Mathf.Approximately(record.Y, lastRecord.Y) &&
                     Mathf.Approximately(record.Z, lastRecord.Z))
                ))
                continue;

            lastRecord = record;

            positionFrames.Add(
                        new PositionFrame()
                        {
                            Point = new FloatPoint() { X = record.X, Y = 0, Z = record.Y },
                            Timestamp = record.Timestamp
                        });            
        }

        return new FrameCollection<PositionFrame>() { Frames = positionFrames.ToArray() };
    }

    private FrameCollection<PointCloudFrame> ParsePointCloudFrames(string fileName)
    {
        List<PointCloudFrame> frames = new List<PointCloudFrame>();

        string directory = Path.GetDirectoryName(fileName);

        foreach (string path in Directory.GetFiles(directory).OrderBy(f => f))
        {
            if (Path.GetFileName(path) == Path.GetFileName(fileName))
                continue;

            frames.Add(ParsePointCloudFrame(path));
        }

        return new FrameCollection<PointCloudFrame>() { Frames = frames.ToArray() };
    }

    private PointCloudFrame ParsePointCloudFrame(string fileName)
    {
        IEnumerable<CsvFrameEntity> records;

        using (StreamReader reader = new StreamReader(fileName))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = csv.GetRecords<CsvFrameEntity>().ToList();
            }
        }
        
        List<FloatPoint> positionFrames = new List<FloatPoint>();

        foreach (var record in records)
        {
            positionFrames.Add(new FloatPoint() { X = record.X, Y = record.Z, Z = record.Y });
        }

        PointCloudFrame pointCloudFrame = new PointCloudFrame();
        pointCloudFrame.Points = positionFrames.ToArray();
        pointCloudFrame.Timestamp = records.First().Timestamp;
        return pointCloudFrame;
    }

    //private PointCloudFrame ParsePointCloudFrame(string fileName)
    //{
    //    PointCloudFrame pointCloudFrame = new PointCloudFrame();
    //    List<FloatPoint> positionFrames = new List<FloatPoint>();

    //    float averageTimestamp = 0;
    //    int timestampRecords = 0;

    //    using (StreamReader reader = new StreamReader(fileName))
    //    {
    //        for (string l = reader.ReadLine(); l != null; l = reader.ReadLine())
    //        {
    //            string[] parts = l.Split(';', ',');
    //            float[] xyz = new float[3];
    //            float timestamp;
    //            bool isValid = true;

    //            for (int i = 3; i < 6; i++)
    //            {
    //                if (!float.TryParse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture, out xyz[i - 3]))
    //                {
    //                    isValid = false;
    //                    break;
    //                }
    //            }

    //            if (!float.TryParse(parts[11], NumberStyles.Any, CultureInfo.InvariantCulture, out timestamp))
    //            {
    //                continue;
    //            }
    //            else
    //            {
    //                averageTimestamp += timestamp;
    //                timestampRecords++;
    //            }

    //            if (isValid)
    //            {
    //                positionFrames.Add(new FloatPoint() { X = xyz[0], Y = xyz[2], Z = xyz[1] });
    //            }
    //        }
    //    }

    //    pointCloudFrame.Points = positionFrames.ToArray();
    //    pointCloudFrame.Timestamp = averageTimestamp / timestampRecords;
    //    return pointCloudFrame;
    //}
}
