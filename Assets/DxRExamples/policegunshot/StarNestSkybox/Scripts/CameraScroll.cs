using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraScroll : MonoBehaviour {

	public float speed = 55;
	public float acceleration = 2f;
	public float lookSensitivity = 5;

	public Vector3 velocity { get; private set; }
	float yaw = 0;
	float pitch = 0;
	public static CameraScroll main;

	void Awake() { main = this; }
	
	void Update() {
		speed *= 1 + Input.GetAxis("Mouse ScrollWheel");
		speed = Mathf.Clamp(speed, 1f, 1000f);

		if (Input.GetKeyDown("r")) { acceleration /= 2f; }
		if (Input.GetKeyDown("t")) { acceleration *= 2f; }
		acceleration = Mathf.Clamp(acceleration, 1f/8f, 8f);

		Vector3 input = new Vector3();

		if (Input.GetKey("a")) { input.x = -1; }
		if (Input.GetKey("d")) { input.x =  1; }

		if (Input.GetKey("q")) { input.y = -1; }
		if (Input.GetKey("e")) { input.y =  1; }

		if (Input.GetKey("s")) { input.z = -1; }
		if (Input.GetKey("w")) { input.z =  1; }

		transform.rotation = Quaternion.Euler(pitch, yaw, 0);

		if (input.magnitude > 0) {
			velocity = Vector3.Lerp(velocity, transform.rotation * input, Time.deltaTime * acceleration);
		}
		if (Input.GetKey("b")) { velocity = Vector3.zero; }
		if (Input.GetKey("n")) { velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * acceleration); }

		transform.position += velocity * Time.deltaTime * speed;

		if (Input.GetMouseButton(1)) {
			yaw += Input.GetAxis("Mouse X") * lookSensitivity;
			pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
			if (yaw > 360) { yaw -= 360; }
			if (yaw < 0) { yaw += 360; }
			pitch = Mathf.Clamp(pitch, -89, 89);

		}

	}
	
}
