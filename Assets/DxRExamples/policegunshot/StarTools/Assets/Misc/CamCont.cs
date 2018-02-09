using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CamCont : MonoBehaviour
{

    public Camera[] cams;

    public float rotationX = 0.0f;
    public float rotationY = 0.0f;

    private float mouseSensitivity = 140;

	public UnityEngine.UI.Slider slider;
	public UnityEngine.UI.Text fovtext;

    private float baseSpeed = 100;

    public bool showSkybox;
	public Text playPauseText;

    public KeyCode forward = KeyCode.W;
    public KeyCode backward = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode up = KeyCode.Q;
    public KeyCode down = KeyCode.E;
    public KeyCode[] zoomIn = { KeyCode.Equals, KeyCode.KeypadPlus };
    public KeyCode[] zoomOut = { KeyCode.Minus, KeyCode.KeypadMinus };
    public KeyCode toggleLock = KeyCode.Space;
    public int grabButton = 1;

    // Use this for initialization
    void Start()
    {
        cams = gameObject.GetComponentsInChildren<Camera>();
    }

    public void toggleSkybox()
    {
        showSkybox = !showSkybox;
    }

    public void OffsettedMovement()
    {
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            Cursor.lockState = CursorLockMode.Locked;

        //rotationY = Mathf.Clamp(rotationY, -90, 90);
        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

        float finalMovementSpeed = baseSpeed * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? 4 : 1);

        Vector3 tempPos = this.transform.position;
        //Handle up/down movement
        if (Input.GetKey(up))
            tempPos += transform.up * finalMovementSpeed;
        if (Input.GetKey(down))
            tempPos -= transform.up * finalMovementSpeed;
        //Regular movement
        if (Input.GetKey(forward))
            tempPos += transform.forward * finalMovementSpeed;
        if (Input.GetKey(backward))
            tempPos -= transform.forward * finalMovementSpeed;
        if (Input.GetKey(right))
            tempPos += transform.right * finalMovementSpeed;
        if (Input.GetKey(left))
            tempPos -= transform.right * finalMovementSpeed;

        this.transform.position = tempPos;
    }

    public void OrbitMovement()
    {

    }

    // Update is called once per frame
    void Update()
    {

        OffsettedMovement();
    }
}