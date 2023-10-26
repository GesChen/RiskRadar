using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadMap : MonoBehaviour
{
	public Material[] materials;

	public void Load(int month)
	{
		StartCoroutine(StartLoading(month));
	}
	IEnumerator StartLoading(int month)
	{
		if (Directory.Exists("Assets/Images"))
		{
			Dictionary<int, string> months = new()
			{
				{ 1, "jan" },
				{ 2, "feb" },
				{ 3, "mar" },
				{ 4, "apr" },
				{ 5, "may" },
				{ 6, "jun" },
				{ 7, "jul" },
				{ 8, "aug" },
				{ 9, "sep" },
				{ 10, "oct" },
				{ 11, "nov" },
				{ 12, "dec" }
			};

			Dictionary<string, string> nameMappings = new()
			{
				{ "llu" , "_1_upper_left"  },
				{ "lru" , "_1_upper_right" },
				{ "rlu" , "_2_upper_left"  },
				{ "rru" , "_2_upper_right" },
				{ "lld" , "_1_lower_left"  },
				{ "lrd" , "_1_lower_right" },
				{ "rld" , "_2_lower_left"  },
				{ "rrd" , "_2_lower_right" }
			};

			foreach (Material m in materials)
			{
				string path = months[month] + nameMappings[m.name] + ".png";

				byte[] fileData = File.ReadAllBytes("Assets/Images/" + path);

				Texture2D image = new Texture2D(2, 2);

				bool success = image.LoadImage(fileData);

				if (success)
				{
					m.SetTexture("_MainTex", image);
				}
				else
				{
					Debug.LogError($"Could not find image {path}");
				}
				yield return new WaitForSeconds(.1f); // split workload over multiple frames
			}
			Debug.Log($"Loaded month {months[month]}");
			Main.totalProgress++;
		}
	}
}
