using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkyboxSwitcher : MonoBehaviour {

	Material[] mats;
	public int current = 0;
	string message = 
@"Press '[' and ']' to change the skybox.
Right-Click and drag to look around (You will need 'Mouse X/Mouse Y/Mouse ScrollWheel' axis in Input Manager)
WASDQE to move the camera. Press b to stop, and n to slow down.";
	
	

	void Start() {
		mats = Resources.LoadAll<Material>("");

		Load(0);
	}
	
	void Update() {
		if (Input.GetKeyDown("[")) { Load(--current); }
		if (Input.GetKeyDown("]")) { Load(++current); }

	}

	void OnGUI() {
		string readout = "Speed (mouse wheel): " + CameraScroll.main.speed;
		readout += "\nAcceleration (r/t): " + CameraScroll.main.acceleration;
		readout += "\nVelocity: " + CameraScroll.main.velocity;
		readout += "\n" + mats[current].name;

		GUI.Box(new Rect(0, 0, Screen.width, Screen.height * .1f), readout);
		GUI.Box(new Rect(0, Screen.height * .9f, Screen.width, Screen.height * .1f), message);


	}
	
	void Load(int i) {
		if (i < 0) { i = mats.Length + i; }
		i = i % mats.Length;
		current = i;
		Material mat = mats[i];
		RenderSettings.skybox = mat;



		float step = mat.GetFloat("_StepSize");
		
		mat.SetFloat("_CamScroll", 55 * Mathf.Sign(step));

	}

}
