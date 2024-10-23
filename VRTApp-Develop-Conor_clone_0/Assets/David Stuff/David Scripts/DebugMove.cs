using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMove : MonoBehaviour
{
    [SerializeField] Transform cameraPos;

    void Start()
    {
        Camera.main.transform.parent = transform;
        Camera.main.transform.position = cameraPos.position;
    }
}
