using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Camera followCamera;

    public Transform Parallax;
    public Vector3 parallaxStartLocation;

    float vertExtent;
    float horzExtent;

    float wanderExt = 60.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (followCamera == null)
            followCamera = Camera.main;

        vertExtent = followCamera.orthographicSize;
        horzExtent = vertExtent * Screen.width / Screen.height;

        parallaxStartLocation = Parallax.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPos = followCamera.transform.position;
        Vector3 thisPos = transform.position;
        Vector3 newPos = new Vector3(thisPos.x, thisPos.y, cameraPos.z);
        followCamera.transform.position = newPos;

        Vector3 parallaxShift = newPos / 10.0f;
        parallaxShift.z = 0.0f;

        Parallax.transform.position = parallaxStartLocation + parallaxShift;

    }
}
