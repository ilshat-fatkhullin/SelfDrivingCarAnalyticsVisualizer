using UnityEngine;
using UnityEngine.UI;

public class Switchers : MonoBehaviour
{
    [SerializeField] private SwitchingPair[] Pairs;

    private void Start()
    {
        for (int i = 0; i < Pairs.Length; i++)
        {
            int j = i;
            Pairs[i].Button.onClick.AddListener(delegate { OnButtonClicked(j); });
        }

        OnButtonClicked(0);
    }

    private void OnButtonClicked(int index)
    {
        foreach (var pair in Pairs)
        {
            pair.Object.SetActive(false);
            pair.Button.interactable = true;
        }

        Pairs[index].Object.SetActive(true);
        Pairs[index].Button.interactable = false;
    }
}

[System.Serializable]
public class SwitchingPair
{
    public Button Button;
    public GameObject Object;
}