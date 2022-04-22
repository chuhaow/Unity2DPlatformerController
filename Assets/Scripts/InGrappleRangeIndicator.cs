using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGrappleRangeIndicator : MonoBehaviour
{
    [Header("Components")]
    private SpriteRenderer sr;
    // Start is called before the first frame update
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setIsInRange(bool isInRange)
    {
        if (isInRange)
        {
            sr.color = Color.green;
        }
        else
        {
            sr.color = Color.white;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        

    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }
}
