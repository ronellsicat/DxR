using UnityEngine;
using System.Collections;

public class DataLoader : MonoBehaviour {
    public DataVisualizer Visualizer;
	// Use this for initialization
	void Start () {
        TextAsset jsonData = Resources.Load<TextAsset>("population");
        string json = jsonData.text;
        SeriesArray data = JsonUtility.FromJson<SeriesArray>(json);
        Visualizer.CreateMeshes(data.AllData);

    }
	
	void Update () {
	
	}
}
[System.Serializable]
public class SeriesArray
{
    public SeriesData[] AllData;
}