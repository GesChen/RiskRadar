using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataViz : MonoBehaviour
{
    public Material baseMat;
	public float alpha;
	public Color visualizationColor;
    public void Visualize(string type)
    {
        // find min and max values of totalcounts
        int minCount = 0;
        int maxCount = 0;
        foreach (var state in LoadData.Data)
        {
            foreach (var city in state.Value.Cities)
            {
                WeatherEvent weatherEvent = city.Value.Events.FirstOrDefault(e => e.Type == type);
                if (weatherEvent == null) 
                    continue;

                if (weatherEvent.TotalCount > maxCount)
                    maxCount = weatherEvent.TotalCount;
            }
        }

		float maxSat = RGBtoHSV(visualizationColor).S;
		HSVColor hsvCol = RGBtoHSV(visualizationColor);
		hsvCol.A = alpha;

		foreach (Transform state in GetFirstLevelChildTransforms(GameObject.Find("Visuals").transform))
        {
            foreach(Transform city in GetFirstLevelChildTransforms(state))
            {
                WeatherEvent weatherEvent = LoadData.Data[state.name].Cities[city.name].Events.FirstOrDefault(e => e.Type == type);
				if (weatherEvent == null)
				{
					weatherEvent = new WeatherEvent();
					weatherEvent.TotalCount = minCount;
				}
				// found
				float scale = Mathf.InverseLerp(minCount, maxCount, weatherEvent.TotalCount);

				Material mat = Instantiate(baseMat);
				hsvCol.S = maxSat * scale;
				mat.color = ConvertHSVtoRGB(hsvCol);
				foreach (MeshRenderer renderer in city.GetComponentsInChildren<MeshRenderer>())
				{
					renderer.material = mat;
				} //set all children parts
            }
        }
    }
	Transform[] GetFirstLevelChildTransforms(Transform parent)
	{
		int childCount = parent.childCount;
		Transform[] childTransforms = new Transform[childCount];

		for (int i = 0; i < childCount; i++)
		{
			childTransforms[i] = parent.GetChild(i);
		}

		return childTransforms;
	}
	struct HSVColor
	{
		public float H; // Hue
		public float S; // Saturation
		public float V; // Value
		public float A;

		public HSVColor(float h, float s, float v, float a)
		{
			H = h;
			S = s;
			V = v;
			A = a;
		}

		public override string ToString()
		{
			return $"H={H}, S={S}, V={V}, A={A}";
		}
	}
	static HSVColor RGBtoHSV(Color color)
	{
		float red = color.r;
		float green = color.g;
		float blue = color.b;

		float cmax = Math.Max(red, Math.Max(green, blue));
		float cmin = Math.Min(red, Math.Min(green, blue));
		float delta = cmax - cmin;

		// Calculate Hue
		float hue;
		if (delta == 0)
		{
			hue = 0;
		}
		else if (cmax == red)
		{
			hue = ((green - blue) / delta) % 6;
		}
		else if (cmax == green)
		{
			hue = (blue - red) / delta + 2;
		}
		else
		{
			hue = (red - green) / delta + 4;
		}

		hue *= 60;
		if (hue < 0)
		{
			hue += 360;
		}

		// Calculate Saturation
		float saturation = (cmax == 0) ? 0 : delta / cmax;

		// Calculate Value
		float value = cmax;

		return new HSVColor(hue, saturation, value, color.a);
	}
	static Color ConvertHSVtoRGB(HSVColor hsvColor)
	{
		float c = hsvColor.V * hsvColor.S;
		float x = c * (1 - Mathf.Abs((hsvColor.H / 60) % 2 - 1));
		float m = hsvColor.V - c;

		float r, g, b;

		switch ((int)(hsvColor.H / 60))
		{
			case 0:
				r = c;
				g = x;
				b = 0;
				break;
			case 1:
				r = x;
				g = c;
				b = 0;
				break;
			case 2:
				r = 0;
				g = c;
				b = x;
				break;
			case 3:
				r = 0;
				g = x;
				b = c;
				break;
			case 4:
				r = x;
				g = 0;
				b = c;
				break;
			default:
				r = c;
				g = 0;
				b = x;
				break;
		}

		return new Color(r + m, g + m, b + m, hsvColor.A);
	}
}
