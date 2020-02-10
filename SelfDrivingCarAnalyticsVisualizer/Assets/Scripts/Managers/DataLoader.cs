using Newtonsoft.Json;
using SFB;
using System.IO;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public void LoadData()
    {
        DriveData driveData = ReadFile();

        if (driveData == null)
            Debug.Log("Cannot read the file.");

        EventBus.Instance.OnDataLoad.Invoke(driveData);
    }

    private DriveData ReadFile()
    {
        var extensions = new[] {
            new ExtensionFilter("Data Files", "json" )
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open data file...", "", extensions, false);

        if (paths.Length == 0)
            return null;

        string dataString;

        using (StreamReader reader = new StreamReader(paths[0]))
        {
            dataString = reader.ReadToEnd();
        }

        return JsonConvert.DeserializeObject<DriveData>(dataString);
    }
}
