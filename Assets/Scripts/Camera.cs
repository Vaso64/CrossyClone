using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] float speed = 0.5f;

    // Update is called once per frame
    void Update()
    {
        //transform.position += Vector3.forward * speed * Time.deltaTime; //Move camera
    }
}
