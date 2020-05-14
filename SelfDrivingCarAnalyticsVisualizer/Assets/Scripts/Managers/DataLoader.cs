using Newtonsoft.Json;
using SFB;
using System.IO;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public void LoadData()
    {
        string fileName = FileSelectionHelper.SelectFile(new ExtensionFilter("Data Files", "json"));
        if (fileName == null)
        {
            return;
        }

        DriveData driveData = ReadJsonFile(fileName);

        if (driveData == null)
        {
            Debug.Log("Cannot read the file.");
            return;
        }

        EventBus.Instance.OnFileLoad.Invoke(fileName);
        EventBus.Instance.OnDataLoad.Invoke(driveData);
    }    

    private DriveData ReadJsonFile(string fileName)
    {
        string dataString;

        using (StreamReader reader = new StreamReader(fileName))
        {
            dataString = reader.ReadToEnd();
        }

        return JsonConvert.DeserializeObject<DriveData>(dataString);
    }
}
