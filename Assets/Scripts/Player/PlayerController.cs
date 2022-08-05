using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{

    //references
    [SerializeField] private Controls controls;
    private CharacterController controller;
    [SerializeField] private Transform groundDirection;

    //movement inputs
    [SerializeField] Vector2 inputs, inputsNormalized;
    [SerializeField] float rotation;
    [SerializeField] bool walk;
    [SerializeField] bool jump;

    //velocity
    [SerializeField] Vector3 velocity;
    [SerializeField] float gravity = -9f;
    [SerializeField] float velocityY;
    [SerializeField] float terminalVelocity = -25f;

    //running
    [SerializeField] float runSpeed = 8f, walkSpeed = 4f;
    [SerializeField] float rotateSpeed = 0.75f;
    [SerializeField] float currentSpeed;

    //jumping
    [SerializeField] bool isJumping;
    [SerializeField] float jumpSpeed, jumpHeight = 4;
    [SerializeField] Vector3 jumpDirection;
    [SerializeField] Vector3 initialJumpInput;

    //ground
    Vector3 forwardDirection, collisionPoint;
    float slopeAngle, forwardAngle;
    float forwardMult;
    Ray groundRay;
    RaycastHit groundHit;

    //rotation with mouse
    public Vector2 turn;
    public float sensitivity = 30;

    //camera
    [SerializeField] private Camera cam;

    private void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        groundDirection = transform.Find("Ground Direction");
    }

    private void Update()
    {
        GetInputs();
        Locomation();
        GroundDirection();
        ChangeDirection();
    }

    //Gets movement inputs from the user defined from controls
    void GetInputs()
    {
        //FORWARDS BACKWARDS CONTROLS
        //forwards
        if (Input.GetKey(controls.forwards)) { inputs.y = 1; }

        //backswards
        if (Input.GetKey(controls.backwards))
        {
            //cancels movement if both forwards and backwards are pressed
            if (Input.GetKey(controls.forwards)) { inputs.y = 0; }
            else { inputs.y = -1; }
        }

        //if no forward or backward input, no movement
        if (!Input.GetKey(controls.forwards) && !Input.GetKey(controls.backwards)) { inputs.y = 0; }


        //STRAFE LEFT RIGHT
        //strafe right
        if (Input.GetKey(controls.turnRight) && Input.GetKey(controls.strafeCondition)) { inputs.x = 1; rotation = 0; }

        //strafe left
        if (Input.GetKey(controls.turnLeft) && Input.GetKey(controls.strafeCondition))
        {
            //cancels movement if both are pressed
            if (Input.GetKey(controls.turnRight) && Input.GetKey(controls.strafeCondition)) { inputs.x = 0; }
            else { inputs.x = -1; rotation = 0; }
        }

        //if no strafe direction input, no movement
        if (!(Input.GetKey(controls.turnLeft) && Input.GetKey(controls.strafeCondition)) && !(Input.GetKey(controls.turnRight) && Input.GetKey(controls.strafeCondition))) { inputs.x = 0; }


        //ROTATE LEFT RIGHT (only rotates if mouse1 not pressed)
        //rotate right
        if (Input.GetKey(controls.turnRight) && !Input.GetKey(controls.strafeCondition)) { rotation = 1; }

        //rotate left
        if (Input.GetKey(controls.turnLeft) && !Input.GetKey(controls.strafeCondition))
        {
            //cancels rotation if both left and right are pressed
            if (Input.GetKey(controls.turnRight)) { rotation = 0; }
            else { rotation = -1; }
        }

        //if rotation input, no rotation
        if (!Input.GetKey(controls.turnRight) && !Input.GetKey(controls.turnLeft)) { rotation = 0; }

        //ToggleWalk
        if (Input.GetKeyDown(controls.toggleWalk)) { walk = !walk; }

        //JUMPING
        jump = Input.GetKey(controls.jump);

    }

    //Converts inputs into movement
    void Locomation()
    {
        inputsNormalized = inputs;

        //running and walking
        currentSpeed = runSpeed;
        if (walk) { currentSpeed = walkSpeed; }

        //handles diagonal movement speed up
        if (inputsNormalized.y != 0 && inputsNormalized.x != 0)
        {
            currentSpeed *= 0.707f;
        }

        currentSpeed *= forwardMult;

        //Rotating
        Vector3 characterRotation = transform.eulerAngles + new Vector3(0, rotation * rotateSpeed, 0);
        transform.eulerAngles = characterRotation;

        //jumps the controller
        if (jump && controller.isGrounded) { Jump(); }
        //Applys gravity until terminal velocity is hit
        if (!controller.isGrounded && velocityY > terminalVelocity) { velocityY += gravity * Time.deltaTime; }

        //Applying inputs
        if (!isJumping) { velocity = (groundDirection.forward * inputsNormalized.magnitude) * currentSpeed + Vector3.up * velocityY; }
        else { velocity = jumpDirection * jumpSpeed + Vector3.up * velocityY; }


        //moving controller
        controller.Move(velocity * Time.deltaTime);

        //if controller is ground, set isJumping to false and its Y velocity to 0
        if (controller.isGrounded)
        {
            if (isJumping) { isJumping = false; }
            velocityY = 0;
        }
    }

    void Jump()
    {
        //set isJumping to true
        if (!isJumping) { isJumping = true; }

        //set jump direction, speed and initial input for jump direction
        initialJumpInput = transform.forward * inputs.y + transform.right * inputs.x;
        jumpDirection = (initialJumpInput).normalized;
        if (walk) { jumpSpeed = walkSpeed; }
        else { jumpSpeed = runSpeed; }

        //set Y velocity
        velocityY = Mathf.Sqrt(-gravity * jumpHeight);
    }

    void GroundDirection()
    {
        //SETTING FORWARDDIRECTION
        //Setting forward direction to controller position
        forwardDirection = transform.position;

        //Setting forward direction based on control input
        forwardDirection = inputsNormalized.magnitude > 0 ? forwardDirection += transform.forward * inputsNormalized.y + transform.right * inputsNormalized.x : forwardDirection += transform.forward;

        //Setting groundDirection to look in the same direction as above ^^
        groundDirection.LookAt(forwardDirection);

        //Setting ground ray
        groundRay.origin = collisionPoint + Vector3.up * 0.05f;
        groundRay.direction = Vector3.down;

        forwardMult = 1;

        if (Physics.Raycast(groundRay, out groundHit, 0.55f))
        {
            //grabbing the angle of the whole slope and the angle of the slope in the forward direction
            slopeAngle = Vector3.Angle(transform.up, groundHit.normal);
            forwardAngle = Vector3.Angle(groundDirection.forward, groundHit.normal) - 90;

            //Setting the groundDirection to include new forward angle
            if (forwardAngle < 0)
            {
                forwardMult = 1 / Mathf.Cos(forwardAngle * Mathf.Deg2Rad);
                groundDirection.eulerAngles -= new Vector3(forwardAngle, 0, 0);
            }
        }

    }

    void ChangeDirection()
    {
        //right-click character control
        if (Input.GetKey(KeyCode.Mouse1))
        {
            turn.x += Input.GetAxis("Mouse X") * sensitivity;

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                turn.x += transform.eulerAngles.y;
            }
            Cursor.lockState = CursorLockMode.Locked;
            transform.localRotation = Quaternion.Euler(0, turn.x, 0);
        }
        else
        {
            turn.x = 0;
            Cursor.lockState = CursorLockMode.None;
        }

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        collisionPoint = hit.point;
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId != OwnerClientId)
        {
            Destroy(cam.gameObject);
            Debug.Log("destorying cam");
            Destroy(this);
        }
    }
}
