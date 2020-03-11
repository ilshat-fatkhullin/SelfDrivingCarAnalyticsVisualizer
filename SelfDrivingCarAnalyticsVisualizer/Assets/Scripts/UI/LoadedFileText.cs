using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LoadedFileText : MonoBehaviour
{
    private Text _text;

    private void Start()
    {
        EventBus.Instance.OnFileLoad.AddListener(OnFileLoaded);

        _text = GetComponent<Text>();
        _text.text = "";
    }

    private void OnFileLoaded(string fileName)
    {
        _text.text = System.IO.Path.GetFileName(fileName);
    }
}
