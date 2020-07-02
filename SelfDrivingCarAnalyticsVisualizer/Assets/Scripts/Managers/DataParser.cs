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
        //driveData.PointCloudFrames = new FrameCollection<PointCloudFrame>();

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
                            Point = new FloatPoint() { X = record.Y, Y = record.Z, Z = -record.X },
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
        List<CsvFrameEntity> records;

        using (StreamReader reader = new StreamReader(fileName))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = csv.GetRecords<CsvFrameEntity>().ToList();
            }
        }

        FloatPoint[] positionFrames = new FloatPoint[records.Count];

        for (int i = 0; i < positionFrames.Length; i++)
        {
            var record = records[i];
            positionFrames[i] = new FloatPoint() { X = record.X, Y = record.Z, Z = record.Y };
        }

        PointCloudFrame pointCloudFrame = new PointCloudFrame();
        pointCloudFrame.Points = positionFrames;
        pointCloudFrame.Timestamp = records.First().Timestamp;
        return pointCloudFrame;
    }
}
