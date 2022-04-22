using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Player;
    public Vector3 location;
  
    
    // Update is called once per frame
    void Update()
    {
        location = Player.transform.position;
        location.z = -10;
        transform.position = location;
    }
}
