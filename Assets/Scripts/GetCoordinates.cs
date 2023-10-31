public IEnumerator TryLoad()
	{
		string query = LocationInputField.text;

		float longitude = 0;
		float latitude = 0;

		// checks if is coordinate format (regex magic)
		string pattern = @"^\s*(-?\d+(\.\d+)?)\s*,\s*(-?\d+(\.\d+)?)\s*$";
		Match match = Regex.Match(query, pattern);
		if (match.Success)
		{
			longitude = float.Parse(match.Groups[3].Value);
			latitude = float.Parse(match.Groups[1].Value);
		}
		else // try to find address
		{
			string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={UnityWebRequest.EscapeURL(query)}&key=AIzaSyC8eYyUYKeBR_hLPzCfnGDJqHZw62y_zdo";

			using (UnityWebRequest www = UnityWebRequest.Get(url))
			{
				yield return www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.Success)
				{
					string jsonResult = www.downloadHandler.text;
					try
					{
						// Parse the JSON response
						GeocodingResponse data = JsonConvert.DeserializeObject<GeocodingResponse>(jsonResult);

						
						// Check if the response contains results
						if (data.results.Count > 0)
						{
							// Extract latitude and longitude from the first result
							latitude = data.results[0].geometry.location.lat;
							longitude = data.results[0].geometry.location.lng;
						}
						else
						{
							Debug.LogWarning("No results found in the geocoding response.");
						}
					}
					catch (Exception e)
					{
						Debug.LogError($"Error parsing geocoding response: {e.Message}");
					}
				}
				else
				{
					Debug.LogError($"Error: {www.error}");
				}
			}

		}

		// successfully got coords
		if (longitude != 0 || latitude != 0)
		{
			StartCoroutine(ShowLocation(longitude, latitude));
		}
		else // error
		{
			errorText.gameObject.SetActive(true);
			errorText.text = "Could not find address";
			yield return new WaitForSeconds(2);
			errorText.gameObject.SetActive(false);
		}
	}