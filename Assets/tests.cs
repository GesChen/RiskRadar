using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event
{
	public string Name { get; set; }
	public DateTime Date { get; set; }
}

public class EventContainer
{
	public List<Event> Events { get; set; }
}

public class tests : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		try
		{
			// Sample JSON data
			string jsonData = @"
				{
					""events"": [
						{ ""name"": ""Event1"", ""date"": ""2023-01-01"" },
						{ ""name"": ""Event2"", ""date"": ""2023-02-15"" },
						{ ""name"": ""Event3"", ""date"": ""2023-03-30"" }
					]
				}";
				

			// Deserialize the JSON string into an object of type EventContainer
			EventContainer eventContainer = Newtonsoft.Json.JsonConvert.DeserializeObject<EventContainer>(jsonData);

			// Now, you can work with the deserialized object
			Debug.Log("Parsed JSON:");

			foreach (var eventData in eventContainer.Events)
			{
				Debug.Log($"Event: {eventData.Name}, Date: {eventData.Date.ToShortDateString()}");
			}

			// Perform further processing as needed
		}
		catch (Exception ex)
		{
			Debug.Log($"Error: {ex.Message}");
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
