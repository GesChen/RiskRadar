using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class WeatherEvent
{
	public string Type { get; set; }
	public int TotalCount { get; set; }
	public List<string> TimesOfyear { get; set; }
	public override string ToString()
	{
		return $"WeatherEvent of type {Type}, happened {TotalCount} times";
	}
}
public class CityData
{
	// events and bounds
	public List<WeatherEvent> Events { get; set; }
	public List<List<List<List<float>>>> Geometry { get; set; }
	public override string ToString()
	{
		return $"CityData with {Events.Count} events";
	}
}
public class State
{
	// cityname, cityData
	public Dictionary<string, CityData> Cities { get; set; }
	public override string ToString()
	{
		return $"City with {Cities.Count} cities";
	}
}

public class LoadData : MonoBehaviour
{
	public static Dictionary<string, State> Data;
	// Start is called before the first frame update
	void Start()
	{
		Load();
	}

	void Load()
	{
		string pathToData = "D:\\Projects\\Programming\\Python Scripts\\.temps\\processstormdata\\get storm data\\jsonout.json";

		string jsonContent = File.ReadAllText(pathToData);

		Data = JsonConvert.DeserializeObject<Dictionary<string, State>>(jsonContent);
	}
}
