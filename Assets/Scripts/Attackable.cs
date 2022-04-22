using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attackable : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float health;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void takeDamage(float dmg)
    {
        health -= dmg;
        Debug.Log("Ouch");
    }
}
