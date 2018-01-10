using UnityEngine;
using System.Collections;

public class CameraObrbit : MonoBehaviour {
    public float MinDistance = 1.0f;
    public float MaxDistance = 1.3f;
    float distance= 1000;
    float distanceTarget;
    Vector2 mouse ;
    Vector2 mouseOnDown ;
    Vector2 rotation;
    Vector2 target =new Vector2(Mathf.PI* 3 / 2, Mathf.PI / 6 );
    Vector2 targetOnDown ;
    // Use this for initialization
    void Start () {
        distanceTarget = transform.position.magnitude;

	}
    bool down = false;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            down = true;
            mouseOnDown.x = Input.mousePosition.x;
            mouseOnDown.y = -Input.mousePosition.y;

            targetOnDown.x = target.x;
            targetOnDown.y = target.y;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            down = false;
        }
        if(down)
        {
            mouse.x = Input.mousePosition.x;
            mouse.y = -Input.mousePosition.y;

            float zoomDamp = distance / 1;

            target.x = targetOnDown.x + (mouse.x - mouseOnDown.x) * 0.005f* zoomDamp;
            target.y = targetOnDown.y + (mouse.y - mouseOnDown.y) * 0.005f* zoomDamp;
            
            target.y = Mathf.Clamp(target.y, -Mathf.PI / 2 + 0.01f, Mathf.PI / 2 - 0.01f);
        }

        distanceTarget -= Input.GetAxis("Mouse ScrollWheel");
        distanceTarget = Mathf.Clamp(distanceTarget, MinDistance, MaxDistance);

        rotation.x += (target.x - rotation.x) * 0.1f;
        rotation.y += (target.y - rotation.y) * 0.1f;
        distance += (distanceTarget - distance) * 0.3f;
        Vector3 position;
        position.x = distance * Mathf.Sin(rotation.x) * Mathf.Cos(rotation.y);
        position.y = distance * Mathf.Sin(rotation.y);
        position.z = distance * Mathf.Cos(rotation.x) * Mathf.Cos(rotation.y);
        transform.position = position;
        transform.LookAt(Vector3.zero);
    }
}
