using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{

    [Header("Attack")]
    [SerializeField] private float attackColdDown;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius;
    [SerializeField] private LayerMask attackable;
    [SerializeField] private float timeSinceAttack;

    public bool onGround;
    private float horizontalDir;
    private float verticalDir;

    private void Awake()
    {
        onGround = this.GetComponent<PlayerController>().isOnGround;
    }

    // Start is called before the first frame update



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontalDir = getInput().x;
        verticalDir = getInput().y;
        
        if (Input.GetButton("Attack") && timeSinceAttack >= attackColdDown)
        {
            attack();
            Debug.Log("Attack");
        }
        else
        {
            timeSinceAttack += Time.deltaTime;
        }
    }

    void attack()
    {
        Collider2D[] objectsHit = null;
        timeSinceAttack = 0;
        if (verticalDir == 0)
        {
            objectsHit  = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, attackable);  
            
           
            //Do damage things here
            
        }
        else if(verticalDir > 0)
        {
            objectsHit = objectsHit = Physics2D.OverlapCircleAll(this.transform.position + (Vector3.up), attackRadius, attackable);
        }
        else if(verticalDir < 0)
        {
            objectsHit = Physics2D.OverlapCircleAll(this.transform.position + (Vector3.down), attackRadius, attackable);
        }
        if (objectsHit != null)
        {
            if(objectsHit.Length > 0)
            {
                //recoil
                Debug.Log("Apply recoil");
            }
        }
        for(int i = 0; i < objectsHit.Length; i++)
        {
            objectsHit[i].SendMessage("takeDamage", 2);
        }

    }

    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        if (Input.GetButton("Attack") && verticalDir == 0 )
        {
            Gizmos.DrawSphere(attackPoint.position , attackRadius);
        }else if (verticalDir > 0)
        {
            Gizmos.DrawSphere(this.transform.position + (Vector3.up), attackRadius);
        }
        else if (verticalDir < 0)
        {
            Gizmos.DrawSphere(this.transform.position + (Vector3.down), attackRadius);
        }

    }

    private Vector2 getInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
}
