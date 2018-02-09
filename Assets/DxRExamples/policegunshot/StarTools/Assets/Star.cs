using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This is the main star class. Generally, you should be using the included prefabs instead of manually creating new stars. 
 * Please view the demo scene for more details.
 * Link to asset store page: https://www.assetstore.unity3d.com/en/#!/content/80595
 * Link to video demo: https://www.youtube.com/watch?v=fIn6SE-O1SM
 */
[ExecuteInEditMode]
public class Star : MonoBehaviour {

    //This array is 1-to-1 with the b-v and color lookups, so that we can interpolate to get values
    private static readonly float[] temperatureLookupDefault = {
        500000, //Made up entry for blending
        113017,
        56701,
        33605,
        22695,
        16954,
        13674,
        11677,
        10395,
        9531,
        8917,
        8455,
        8084,
        7767,
        7483,
        7218,
        6967,
        6728,
        6500,
        6285,
        6082,
        5895,
        5722,
        5563,
        5418,
        5286,
        5164,
        5052,
        4948,
        4849,
        4755,
        4664,
        4576,
        4489,
        4405,
        4322,
        4241,
        4159,
        4076,
        3989,
        3892,
        3779,
        3640,
        3463,
        3234,
        2942,
        2579,
        2150,
        1675,
        1195,
        0 //Made up entry for blending
    };

    //This array is 1-to-1 with the b-v and temperature lookups, so that we can interpolate to get values
    private static readonly string[] colorLookupDefault = {
        "0000ff",   //Made up color for blending
        "9bb2ff",
        "9eb5ff",
        "a3b9ff",
        "aabfff",
        "b2c5ff",
        "bbccff",
        "c4d2ff",
        "ccd8ff",
        "d3ddff",
        "dae2ff",
        "dfe5ff",
        "e4e9ff",
        "e9ecff",
        "eeefff",
        "f3f2ff",
        "f8f6ff",
        "fef9ff",
        "fff9fb",
        "fff7f5",
        "fff5ef",
        "fff3ea",
        "fff1e5",
        "ffefe0",
        "ffeddb",
        "ffebd6",
        "ffe9d2",
        "ffe8ce",
        "ffe6ca",
        "ffe5c6",
        "ffe3c3",
        "ffe2bf",
        "ffe0bb",
        "ffdfb8",
        "ffddb4",
        "ffdbb0",
        "ffdaad",
        "ffd8a9",
        "ffd6a5",
        "ffd5a1",
        "ffd29c",
        "ffd096",
        "ffcc8f",
        "ffc885",
        "ffc178",
        "ffb765",
        "ffa94b",
        "ff9523",
        "ff7b00",
        "ff5200",
        "000000" //Made up color for blending, black. In theory a star should never reach this.
    };

    //This array is 1-to-1 with the temperature and color lookups, so that we can interpolate to get values
    private static readonly float[] bMinusVLookupDefault = {
        -5.5f,  //Made up for blending
        -.40f,
        -.35f,
        -.30f,
        -.25f,
        -.20f,
        -.15f,
        -.10f,
        -.05f,
        0f,
        .05f,
        .10f,
        .15f,
        .20f,
        .25f,
        .30f,
        .35f,
        .40f,
        .45f,
        .50f,
        .55f,
        .60f,
        .65f,
        .70f,
        .75f,
        .80f,
        .85f,
        .90f,
        .95f,
        1.00f,
        1.05f,
        1.10f,
        1.15f,
        1.20f,
        1.25f,
        1.30f,
        1.35f,
        1.40f,
        1.45f,
        1.50f,
        1.55f,
        1.60f,
        1.65f,
        1.70f,
        1.75f,
        1.80f,
        1.85f,
        1.90f,
        1.95f,
        2.00f,
        8.5f   //Made up for blending
    };

    private float[] bMinusVLookup = bMinusVLookupDefault;
    private string[] colorLookup = colorLookupDefault;
    private float[] temperatureLookup = temperatureLookupDefault;

    public bool manualColors = false;   //If true, temperature/luminosity don't matter and color is set by user.

    public bool isPaused = false;

    public float timeScale = 1f;    //Speed at which shader changes
    private float localTime = 0;
    public float resolutionScale = 5f;  //Blurriness
    public float contrast = 1f; //How dark the sun spots are
    public Vector3 rotationRates = new Vector3(0, -1, 0);   //Since the texture for a star is based off of 3d noise, we can rotate the entire thing
    private Vector3 actualRotation = new Vector3(0, 0, 0);   //Since the texture for a star is based off of 3d noise, we can rotate the entire thing

    public Color baseStarColor = Color.white;

    public GameObject[] coronaStrips;   //A list of all corona objects

    MaterialPropertyBlock mpb;  //Used to mass apply settings to stars and coronas

    public float temperatureKelvin = 0;
    private float cachedB_v = 0;

    public void SetLookupTables(float[] temperature, float[] bMinusV, string[] colors) {
        this.temperatureLookup = temperature;
        this.colorLookup = colors;
        this.bMinusVLookup = bMinusV;
        if (!StarLookupTablesTest()) {
            Debug.LogError("Invalid lookup tables set. Please ensure that all tables are the same length, and are in order.");
        }
    }

    //Set materials
    public void OnRenderObject() {
        if (mpb == null) {
            mpb = new MaterialPropertyBlock();
        }

        //Set light properties
        if (GetComponentInChildren<Light>() != null)
            GetComponentInChildren<Light>().color = GetColor();

        //Handle pauses
        if (!GetIsPaused()) {
            localTime += Time.deltaTime * timeScale * 0.05f;    //Use 0.05 to set '1' timescale to a reasonable default
            actualRotation += Time.deltaTime * rotationRates * timeScale * 0.05f;   //
        }

        //Set star properties
        mpb.SetColor("_StarColor", GetColor());
        mpb.SetVector("_StarCenter", new Vector4(transform.position.x, transform.position.y, transform.position.z, transform.lossyScale.x / 2f));
        mpb.SetVector("_RotRate", new Vector4(actualRotation.x, actualRotation.y, actualRotation.z, 0));
        mpb.SetFloat("_LocalTime", localTime);
        mpb.SetFloat("_Resolution", resolutionScale);
        mpb.SetFloat("_Contrast", contrast);

        //Apply properties
        GetComponent<Renderer>().SetPropertyBlock(mpb);
        for (int i = 0; i < coronaStrips.Length; i++) {
            coronaStrips[i].GetComponent<Renderer>().SetPropertyBlock(mpb);
        }
        if (GetComponent<ParticleSystemRenderer>() != null) {
            GetComponent<ParticleSystemRenderer>().SetPropertyBlock(mpb);
            if (GetIsPaused()) {
                GetComponent<ParticleSystem>().Pause();
            }
            else {
                GetComponent<ParticleSystem>().Play();
            }
        }
    }

    //Returns the color in Kelvin
    public float GetTemperature() {
        return temperatureKelvin;
    }

    //Sets the temperature in Kelvin, and recalculates the B-V and colors
    public void SetTemperature(float temp_kelvin) {
        temperatureKelvin = temp_kelvin;
        RecalculateB_V();
        RecalculateScienceColor();
    }

    //Returns the Blue-Violet value
    public float GetB_V() {
        return cachedB_v;
    }

    //Sets the Blue-Violet value, and recalculates the temperature and color off of it.
    public void SetB_V(float bmv) {
        cachedB_v = bmv;
        RecalculateTemperature();
        RecalculateScienceColor();
    }

    //Returns a star's classification based off of the current temperature
    public string GetStarClass() {
        if (GetTemperature() < 3700)
            return "M";
        if (GetTemperature() < 5200)
            return "K";
        if (GetTemperature() < 6000)
            return "G";
        if (GetTemperature() < 7500)
            return "F";
        if (GetTemperature() < 10000)
            return "A";
        if (GetTemperature() < 30000)
            return "B";
        //Temperature > 30000
        return "O";
    }

    //Only use this function if you've changed the lookup tables and need to test that they are still valid
    public bool StarLookupTablesTest() {
        //Test lengths
        if (bMinusVLookup.Length != temperatureLookup.Length || temperatureLookup.Length != colorLookup.Length)
            return false;
        //Test that temperature and b-v are linear
        for (int i = 0; i < bMinusVLookup.Length - 1; i++) {
            if (bMinusVLookup[i] >= bMinusVLookup[i + 1])
                return false;
            if (temperatureLookup[i] <= temperatureLookup[i + 1])
                return false;
        }
        return true;
    }

    //On startup, make sure that B-V, color, and temperature data are accurate. Temperature takes precedence by default.
    public void Start() {
        if (!manualColors) {
            SetTemperature(GetTemperature());
        }
    }

    //Get temp from B-v
    private void RecalculateTemperature() {
        /*
http://www.vendian.org/mncharity/dir3/starcolor/details.html
 */
        //First find if it's out of bounds, and if so set the appropriate color
        if (GetB_V() <= bMinusVLookup[0]) {
            temperatureKelvin = temperatureLookup[0];
        } else if (GetB_V() >= bMinusVLookup[bMinusVLookup.Length - 1]) {
            temperatureKelvin = temperatureLookup[temperatureLookup.Length - 1];
        } else {
            //It's in bounds, so find the closest two color/temperature pairs and do a linear interpolation
            //Or the exact color if it matches perfectly
            //This can be sped up later on TODO
            int max = 0;
            int min = bMinusVLookup.Length - 1;
            for (int i = 0; i < bMinusVLookup.Length; i++) {
                //Handle exact match
                if (bMinusVLookup[i] == GetB_V()) {
                    temperatureKelvin = temperatureLookup[i];
                    return;
                }
                if (bMinusVLookup[i] < GetB_V() && bMinusVLookup[i] > bMinusVLookup[max]) {
                    max = i;
                } else if (bMinusVLookup[i] > GetB_V() && bMinusVLookup[i] < bMinusVLookup[min]) {
                    min = i;
                }
            }

            float interpolatedTemp = Mathf.Lerp(
                temperatureLookup[min],
                temperatureLookup[max],
                (float)(GetB_V() - bMinusVLookup[min]) / (float)(bMinusVLookup[max] - bMinusVLookup[min]));

            //Interpolate
            temperatureKelvin = interpolatedTemp;
        }
    }

    //Get b-v from temp
    private void RecalculateB_V() {
        /*
http://www.vendian.org/mncharity/dir3/starcolor/details.html
 */
        //First find if it's out of bounds, and if so set the appropriate color
        if (GetTemperature() >= temperatureLookup[0]) {
            cachedB_v = bMinusVLookup[0];
        } else if (GetTemperature() <= temperatureLookup[temperatureLookup.Length - 1]) {
            cachedB_v = bMinusVLookup[bMinusVLookup.Length - 1];
        } else {
            //It's in bounds, so find the closest two color/temperature pairs and do a linear interpolation
            //Or the exact color if it matches perfectly
            //This can be sped up later on TODO
            int max = 0;
            int min = temperatureLookup.Length - 1;
            for (int i = 0; i < temperatureLookup.Length; i++) {
                //Handle exact match
                if (temperatureLookup[i] == GetTemperature()) {
                    cachedB_v = bMinusVLookup[i];
                    return;
                }
                if (temperatureLookup[i] > GetTemperature() && temperatureLookup[i] < temperatureLookup[max]) {
                    max = i;
                } else if (temperatureLookup[i] < GetTemperature() && temperatureLookup[i] > temperatureLookup[min]) {
                    min = i;
                }
            }

            float interpolatedB_V = Mathf.Lerp(
                bMinusVLookup[min],
                bMinusVLookup[max],
                (float)(GetTemperature() - temperatureLookup[min]) / (float)(temperatureLookup[max] - temperatureLookup[min]));

            //Interpolate
            cachedB_v = interpolatedB_V;
        }
    }

    //Approximates a color based off of temperature data
    //These are just approximations from a lookup table, 
    // there is no fast and accurate equation as far as I'm aware
    // besides doing the actual physics equations
    //Must be called AFTER the temperature reset
    private void RecalculateScienceColor() {
        /*
        http://www.vendian.org/mncharity/dir3/starcolor/details.html
         */
        //First find if it's out of bounds, and if so set the appropriate color
        if (GetTemperature() >= temperatureLookup[0]) {
            baseStarColor = HexCodeToColor(colorLookup[0]);
            return;
        } else if (GetTemperature() <= temperatureLookup[temperatureLookup.Length - 1]) {
            baseStarColor = HexCodeToColor(colorLookup[colorLookup.Length - 1]);
            return;
        } else {
            //It's in bounds, so find the closest two color/temperature pairs and do a linear interpolation
            //Or the exact color if it matches perfectly
            //This can be sped up later on TODO
            int max = 0;
            int min = temperatureLookup.Length - 1;
            for (int i = 0; i < temperatureLookup.Length; i++) {
                //Handle exact match
                if (temperatureLookup[i] == GetTemperature()) {
                    baseStarColor = HexCodeToColor(colorLookup[i]);
                    return;
                }
                if (temperatureLookup[i] > GetTemperature() && temperatureLookup[i] < temperatureLookup[max]) {
                    max = i;
                } else if (temperatureLookup[i] < GetTemperature() && temperatureLookup[i] > temperatureLookup[min]) {
                    min = i;
                }
            }

            Color interpolatedColor = Color.Lerp(
                HexCodeToColor(colorLookup[min]),
                HexCodeToColor(colorLookup[max]),
                (float)(GetTemperature() - temperatureLookup[min]) / (float)(temperatureLookup[max] - temperatureLookup[min]));

            //Interpolate
            baseStarColor = interpolatedColor;
        }
    }

    //Returns the current base color of the star
    public Color GetColor() {
        return baseStarColor;
    }

    //Sets the color of the star- keep in mind that this will also enable manual color usage, so changing the temperature/b-v after will not directly affect colors.
    public void SetColor(Color newColor) {
        manualColors = true;
        baseStarColor = newColor;
    }

    //Helper function to convert a hex color code to a Unity color object
    private Color HexCodeToColor(string hex) {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public void Pause() {
        isPaused = true;
    }

    public void TogglePause() {
        isPaused = !isPaused;
    }

    public void UnPause() {
        isPaused = false;
    }

    public bool GetIsPaused() {
        return isPaused;
    }
}
