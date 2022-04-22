using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAim : MonoBehaviour
{
    private Transform tf;
    private Vector2 mousePos;
    [SerializeField] private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        tf = gameObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    private void FixedUpdate()
    {
        faceMouse();
    }

    void faceMouse()
    {
        Vector2 lookDir = mousePos - (Vector2)tf.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        tf.right = lookDir;
    }
}
