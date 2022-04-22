using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Varible values 
/* moveAcceleration - 75
 * maxSpeed - 12
 * groundLinDrag  - 4
 * jumpForce - 10
 * fallMultiplier - 8
 * lowJumpFallMultiplier - 5
 * airLinDrag - 2.5
 *
 */

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask cornerCorrectionLayer;
    [SerializeField] private LayerMask wallMask;
    private LayerMask grappleMask = 9;

    [Header("Movement Variables")]
    [SerializeField] private float moveAcceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float groundLinDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float fallMultiplier = 8f;
    [SerializeField] private float lowJumpFallMultiplier = 5f;
    [SerializeField] private float airLinDrag = 2.5f;
    [SerializeField] private float coyoteTime= 0.1f;
    [SerializeField] private float coyoteTimeCounter = 0;
    private bool canMove;
    private float horizontalDir;
    private float verticalDir;
    
    private bool dirChange => (rb.velocity.x > 0f && horizontalDir < 0f || rb.velocity.x < 0f && horizontalDir > 0f); // True when moving right and press left or vice versa
    [Header("Ground Collision")]
    [SerializeField] private float groundRaycastLen;
    [SerializeField] private Vector3 groundRaycastOffset; // Helps with edge detection
    public bool isOnGround;

    [Header("Jump Correction")]
    [SerializeField] private float jumpCorrectionRaycastLen;
    [SerializeField] private Vector3 edgeRaycastOffset;
    [SerializeField] private Vector3 innerRaycastOffset;
    private bool canJumpCorrect;

    [Header("Dash")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashLength;
    [SerializeField] private float dashBufferLength;
    [SerializeField] private float dashBufferCounter;
    private bool isDashing;
    private bool hasDashed;
    private bool canDash => dashBufferCounter > 0f && !hasDashed;

    [Header("Wall Collision")]
    [SerializeField] private float wallRaycastLength;
    [SerializeField] private float wallGrabBufferLength;
    [SerializeField] private float wallGrabBufferCounter;
    [SerializeField] private float offWallBufferLength;
    [SerializeField] private float offWallBufferCounter;
    [SerializeField] private float xWallForce;
    [SerializeField] private float yWallForce;
    private bool isWallJump => isGrabWall && Input.GetButton("Jump");
    [SerializeField] private float wallJumpTime;
    private bool onWall;
    private bool isOnRightWall;
    [SerializeField] private float wallSlideSpeed;
    [SerializeField] private bool wantOffWall;
    private bool wantToGrabWall => onWall && !isOnGround;
    private bool isGrabWall => wallGrabBufferCounter <= 0f && !wantOffWall;
    //private bool isWallSlide => onWall && !isOnGround && Input.GetButton("WallGrab") && rb.velocity.y < 0f;

    [Header("Grapple")]
    [SerializeField] private float grappleSpeed;
    [SerializeField] private float grappleLength;
    [SerializeField] private float grappleBufferLength;
    [SerializeField] private float grappleDeceleration;
    private GameObject grapplePoint = null; 
    private float grappleBufferCounter;
    private bool isGrappling;
    private bool hasGrappled;
    private bool isInRange;
    private bool isIntersecting;
    private bool canGrapple => grappleBufferCounter > 0f && !hasGrappled && isInRange && grapplePoint;

    [Header("Animation")]
    private bool isFacingRight = true;

    [Header("Knockback")]
    [SerializeField] private float knockbackSpeedX;
    [SerializeField] private float knockbackSpeedY;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalDir = getInput().x;
        verticalDir = getInput().y;

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            jump();
        }
        if (Input.GetButtonDown("Dash"))
        {
            dashBufferCounter = dashBufferLength;
            Debug.Log("Dashing");
        }
        else
        {
            if(dashBufferCounter > 0f)
            {
                dashBufferCounter -= Time.deltaTime;
            }
            
        }
        if (wantToGrabWall)
        {
            wallGrabBufferCounter -= Time.deltaTime;
        }
        else
        {
            wallGrabBufferCounter = wallGrabBufferLength;
        }

        if (wantOffWall)
        {
            offWallBufferCounter -= Time.deltaTime;
        }
        else
        {
            offWallBufferCounter = offWallBufferLength;
        }
        if(offWallBufferCounter <= 0)
        {
            wantOffWall = false;
        }

        if (Input.GetButtonDown("Grapple"))
        {
            grappleBufferCounter = grappleBufferLength;
            Debug.Log("Grapple time: "+ canGrapple);
        }
        else
        {
            grappleBufferCounter -= Time.deltaTime;
        }
        if(horizontalDir < 0f && isFacingRight)
        {
            flip();
        }else if(horizontalDir > 0f && !isFacingRight)
        {
            flip();
        }

    }

    void FixedUpdate()
    {
        checkCollision();
        if (canDash)
        {
            StartCoroutine(dash());
        }
        if(!isDashing)
        {
            if (canGrapple)
            {
                StartCoroutine(grapple());
            }

            move();

            
            
            if (isOnGround)
            {
                applyGroundDrag();
                coyoteTimeCounter = coyoteTime;
                hasDashed = false;
            }
            else
            {
                applyAirDrag();
                Falling();
                coyoteTimeCounter -= Time.fixedDeltaTime;
            }
            
        }
        

        if (canJumpCorrect)
        {
            jumpCorrect();
        }
        if (isGrabWall)
        {
            
            grabWall();
            
        }
        else
        {
            rb.gravityScale = 1f;
            canMove = true;
            
        }

        if (isWallJump)
        {

            StartCoroutine("wallJump");
            Debug.Log("Wall Jump");
        }
       
        
    }

    void flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

   
    IEnumerator wallJump()
    {
        
        float wallJumpStartTime = Time.time;
        float jumpDir;
        onWall = false;
        

        rb.gravityScale = 0f;
        rb.drag = 0f;
        
        if (isOnRightWall)
        {
            jumpDir = -1;
        }
        else
        {
            jumpDir = 1;
        }

        while (Time.time < wallJumpStartTime + wallJumpTime)
        {
            rb.velocity = new Vector2(xWallForce * jumpDir, yWallForce);
            yield return null;
        }
        rb.gravityScale = 1f;

    }

    void attachToWall()
    {
        if (!wantOffWall)
        {
            if (isOnRightWall && horizontalDir >= 0f)
            {
                rb.velocity = new Vector2(3f, rb.velocity.y);
            }
            else if (!isOnRightWall && horizontalDir <= 0f)
            {
                rb.velocity = new Vector2(-3f, rb.velocity.y);
            }
        }
        

        //Add flip
    }

    

    void grabWall()
    {
        canMove = false;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(rb.velocity.x, -maxSpeed * wallSlideSpeed);
        if(isOnRightWall && horizontalDir < 0)
        {
            wantOffWall = true;
        }
        else if(!isOnRightWall && horizontalDir > 0)
        {
            wantOffWall = true;
        }
        else
        {
            wantOffWall = false;
        }
        attachToWall();
    }

    private void applyAirDrag()
    {
        rb.drag = airLinDrag;
    }

    IEnumerator grapple()
    {
        float grappleStart = Time.time;
        float defaultMaxMoveSpeed = maxSpeed;
        hasGrappled = true;
        
        Vector2 grappleDir = grapplePoint.transform.position - transform.position;
        Debug.Log("Target Location:" + grapplePoint.transform.position); 
        Debug.Log("Grapple Dir: " + grappleDir);
        
        Debug.DrawLine(transform.position, grappleDir,Color.black,10000);
        Debug.DrawLine(transform.position, grapplePoint.transform.position, Color.black, 10000);
        grappleDir = grappleDir.normalized;
        Debug.Log("Grapple Dir Norm: " + grappleDir);
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        maxSpeed = grappleSpeed;
        
        Debug.Log("Velocity" + grappleDir * (grappleSpeed));
        Debug.Log("Real Velocity" + new Vector2(1f,0) * (grappleSpeed));
        if (grappleDir.x != 0 || grappleDir.y != 0)
        {
            while (grappleStart + grappleLength > Time.time)
            {
                
                rb.velocity = grappleDir * (grappleSpeed);


                yield return null;
            }
        }
        while(maxSpeed > defaultMaxMoveSpeed )
        {
            maxSpeed *= grappleDeceleration;
            yield return null;
        }
        maxSpeed = defaultMaxMoveSpeed;
        hasGrappled = false;
        rb.gravityScale = 1f;
        //Debug.DrawLine(transform.position, grapplePoint.transform.position, Color.blue, 99999);


    }

    IEnumerator dash()
    {
        float currHori = horizontalDir;
        float currVerti = verticalDir;
        float dashStartTime = Time.time;

        hasDashed = true;
        isDashing = true;

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.drag = 0f;

        Vector2 dashDir;
        if(currHori != 0f || currVerti != 0f)
        {
            dashDir = new Vector2(currHori, 0f);
        }
        else
        {
            // Implement code for facing direction
            // For temp use it will default to dash right
            dashDir = Vector2.right;
        }

        while(Time.time < dashStartTime + dashLength)
        {
            rb.velocity = dashDir.normalized * dashSpeed;
            yield return null;
        }
        rb.gravityScale = 1f;
        isDashing = false;
        
    }

    private void jump()
    {
        applyAirDrag();
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteTimeCounter = 0;
    }

    private void move()
    {
        rb.AddForce(new Vector2(horizontalDir, 0f) * moveAcceleration);
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
           rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }
        
    }

    private void applyGroundDrag()
    {
        if(Mathf.Abs(horizontalDir) < 0.4f || dirChange)
        {
            rb.drag = groundLinDrag;
        }
        else
        {
            rb.drag = 0f;
        }
    }

    private Vector2 getInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void checkCollision()
    {
        isOnGround = Physics2D.Raycast(transform.position + groundRaycastOffset, Vector2.down,groundRaycastLen, groundMask)||
                     Physics2D.Raycast(transform.position - groundRaycastOffset, Vector2.down, groundRaycastLen, groundMask) && !isGrappling;

        // If only the edge rays hit something
        canJumpCorrect = Physics2D.Raycast(transform.position + edgeRaycastOffset, Vector2.up, jumpCorrectionRaycastLen, cornerCorrectionLayer) &&
                         !Physics2D.Raycast(transform.position + innerRaycastOffset, Vector2.up, jumpCorrectionRaycastLen, cornerCorrectionLayer) ||
                         Physics2D.Raycast(transform.position - edgeRaycastOffset, Vector2.up, jumpCorrectionRaycastLen, cornerCorrectionLayer) &&
                         !Physics2D.Raycast(transform.position - innerRaycastOffset, Vector2.up, jumpCorrectionRaycastLen, cornerCorrectionLayer);

        onWall = Physics2D.Raycast(transform.position, Vector2.right, wallRaycastLength, wallMask) ||
                 Physics2D.Raycast(transform.position, Vector2.left, wallRaycastLength, wallMask);
        isOnRightWall = Physics2D.Raycast(transform.position, Vector2.right, wallRaycastLength, wallMask);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == grappleMask)
        {
            if (grapplePoint)
            {
                grapplePoint.SendMessage("setIsInRange", false);
                isIntersecting = true;
                Debug.Log("Exit old");
            }
            isInRange = true;
            grapplePoint = collision.gameObject;
            grapplePoint.SendMessage("setIsInRange", true);
            
            Debug.Log("In Range");
        }
        Debug.Log(collision.gameObject.layer + " = " + grappleMask.value);
        Debug.Log(groundMask.value);


    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == grappleMask)
        {
            isInRange = true;
            grapplePoint = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == grappleMask )
        {
            if (!isIntersecting)
            {
                isInRange = false;
                grapplePoint.SendMessage("setIsInRange", false);
                grapplePoint = null;
                //Debug.DrawLine(transform.position, collision.transform.position, Color.blue, 99999);
                Debug.Log("Out of Range");
            }
            isIntersecting = false;
        }

    }

    private void knockback(bool isKnockbackX, bool isKnockbackY)    //Do this in a enum
    {
        if (isKnockbackX)
        {
            if (isFacingRight)
            {
                rb.velocity = new Vector2(-knockbackSpeedX, 0f);
            }
            else
            {
                rb.velocity = new Vector2(knockbackSpeedX, 0f);
            }
        }
        if (isKnockbackY)
        {
            if(verticalDir < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, knockbackSpeedY);
                rb.gravityScale = 0;
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -knockbackSpeedY);
                rb.gravityScale = 0;
            }
        }
        rb.gravityScale = 1;
    }


    private void jumpCorrect()
    {
        float jumpVelocity = rb.velocity.y;
        float newPosition;  // Distance to push until not hitting edge
        
        //Push Right
        RaycastHit2D hit = Physics2D.Raycast(transform.position - innerRaycastOffset + Vector3.up * jumpCorrectionRaycastLen, Vector3.left, jumpCorrectionRaycastLen, cornerCorrectionLayer);
        if(hit.collider != null)
        {
            newPosition = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * jumpCorrectionRaycastLen, transform.position
                - edgeRaycastOffset + Vector3.up * jumpCorrectionRaycastLen);
            transform.position = new Vector3(transform.position.x + newPosition, transform.position.y, 0f);
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
            Debug.Log("Pushing Right ("+hit.point.x +"," + transform.position.y + "," + "0) + (" +Vector3.up * jumpCorrectionRaycastLen + ")"  );
            Debug.Log("To" + (transform.position - edgeRaycastOffset + Vector3.up * jumpCorrectionRaycastLen));
            Debug.Log("New Pos" + newPosition);
            return;
        }

        hit = Physics2D.Raycast(transform.position + innerRaycastOffset + Vector3.up * jumpCorrectionRaycastLen, Vector3.right, jumpCorrectionRaycastLen, cornerCorrectionLayer);
        if (hit.collider != null)
        {
            newPosition = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * jumpCorrectionRaycastLen, transform.position
                + edgeRaycastOffset + Vector3.up * jumpCorrectionRaycastLen);
            transform.position = new Vector3(transform.position.x - newPosition, transform.position.y, 0f);
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
            Debug.Log("Pushing Left");
            return;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + groundRaycastOffset, transform.position + groundRaycastOffset + Vector3.down * groundRaycastLen);
        Gizmos.DrawLine(transform.position - groundRaycastOffset, transform.position - groundRaycastOffset + Vector3.down * groundRaycastLen);

        //Corner Check
        Gizmos.DrawLine(transform.position + edgeRaycastOffset, transform.position + edgeRaycastOffset + Vector3.up * jumpCorrectionRaycastLen);
        Gizmos.DrawLine(transform.position - edgeRaycastOffset, transform.position - edgeRaycastOffset + Vector3.up * jumpCorrectionRaycastLen);
        Gizmos.DrawLine(transform.position + innerRaycastOffset, transform.position + innerRaycastOffset + Vector3.up * jumpCorrectionRaycastLen);
        Gizmos.DrawLine(transform.position - innerRaycastOffset, transform.position - innerRaycastOffset + Vector3.up * jumpCorrectionRaycastLen);

        //Correction Distiance Check
        Gizmos.DrawLine(transform.position - innerRaycastOffset + Vector3.up * jumpCorrectionRaycastLen,
            transform.position - innerRaycastOffset + Vector3.up * jumpCorrectionRaycastLen + Vector3.left * jumpCorrectionRaycastLen);
        Gizmos.DrawLine(transform.position + innerRaycastOffset + Vector3.up * jumpCorrectionRaycastLen,
            transform.position + innerRaycastOffset + Vector3.up * jumpCorrectionRaycastLen + Vector3.right * jumpCorrectionRaycastLen);

        //Wall Check
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallRaycastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallRaycastLength);
    }

    private void Falling()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpFallMultiplier - 1) * Time.deltaTime;
        }
        
    }
}

