//
// Source : https://wiki.unity3d.com/index.php/SmoothMouseLook
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

[AddComponentMenu("Camera-Control/Camera Controller")]
public class CameraController : MonoBehaviour {

	/*
	Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
	Converted to C# 27-02-13 - no credit wanted.
	Simple flycam I made, since I couldn't find any others made public.  
	Made simple to use (drag and drop, done) for regular keyboard layout  
	wasd : basic movement
	shift : Makes camera accelerate
	space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/

	[Header("Keyboard")]
	public float mainSpeed = 1.0f; //regular speed
	public float shiftAdd = 2.0f; //multiplied by how long shift is held.  Basically running
	public float maxShift = 10.0f; //Maximum speed when holdin gshift
	private float totalRun = 1.0f;

	public InputActionReference forward, backward, left, right, run, vertical;

	[Header("Mouse")]
	public InputActionReference X_axis;
	public InputActionReference Y_axis;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationX = 0F;
	float rotationY = 0F;

	private List<float> rotArrayX = new List<float>();
	float rotAverageX = 0F;

	private List<float> rotArrayY = new List<float>();
	float rotAverageY = 0F;

	public float frameCounter = 20;

	Quaternion originalRotation;

	void Update() {
		rotAverageY = 0f;
		rotAverageX = 0f;
		
		rotationY += Y_axis?.action.ReadValue<float>() ?? 0 * sensitivityY;
		rotationX += X_axis?.action.ReadValue<float>() ?? 0 * sensitivityX;

		rotArrayY.Add(rotationY);
		rotArrayX.Add(rotationX);

		if (rotArrayY.Count >= frameCounter) {
			rotArrayY.RemoveAt(0);
		}
		if (rotArrayX.Count >= frameCounter) {
			rotArrayX.RemoveAt(0);
		}

		for (int j = 0; j < rotArrayY.Count; j++) {
			rotAverageY += rotArrayY[j];
		}
		for (int i = 0; i < rotArrayX.Count; i++) {
			rotAverageX += rotArrayX[i];
		}

		rotAverageY /= rotArrayY.Count;
		rotAverageX /= rotArrayX.Count;

		rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
		rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

		Quaternion yQuaternion = Quaternion.AngleAxis (rotAverageY, Vector3.left);
		Quaternion xQuaternion = Quaternion.AngleAxis (rotAverageX, Vector3.up);

		transform.localRotation = originalRotation * xQuaternion * yQuaternion;


		//Keyboard commands
		float f = 0.0f;
		Vector3 p = GetBaseInput();
		if (run.action.IsPressed()) {
			totalRun += Time.deltaTime;
			p = p * totalRun * shiftAdd;
			p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
			p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
			p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
		} else {
			totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
			p = p * mainSpeed;
		}

		p = p * Time.deltaTime;
		Vector3 newPosition = transform.position;
		if (vertical.action.IsPressed()) { //If player wants to move on X and Z axis only
			transform.Translate(p);
			newPosition.x = transform.position.x;
			newPosition.z = transform.position.z;
			transform.position = newPosition;
		} else {
			transform.Translate(p);
		}
	}

	void Start() {
		Rigidbody rb = GetComponent<Rigidbody>();

		if (rb)
			rb.freezeRotation = true;
		originalRotation = transform.localRotation;
	}

	public static float ClampAngle(float angle, float min, float max) {
		angle = angle % 360;
		if ((angle >= -360F) && (angle <= 360F)) {
			if (angle < -360F) {
				angle += 360F;
			}
			if (angle > 360F) {
				angle -= 360F;
			}
		}
		return Mathf.Clamp(angle, min, max);
	}

	private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
		Vector3 p_Velocity = new Vector3();
		if (forward.action.IsPressed()) {
			p_Velocity += new Vector3(0, 0, 1);
		}
		if (backward.action.IsPressed()) {
			p_Velocity += new Vector3(0, 0, -1);
		}
		if (left.action.IsPressed()) {
			p_Velocity += new Vector3(-1, 0, 0);
		}
		if (right.action.IsPressed()) {
			p_Velocity += new Vector3(1, 0, 0);
		}
		return p_Velocity;
	}
}