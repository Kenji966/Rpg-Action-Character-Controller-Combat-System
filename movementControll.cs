using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public class movementControll : MonoBehaviour
{
    #region Other Class
        AttackSystem _attackSystem;
        CursorLock _CursorLock;
        camControll _camControll;
        
    #endregion

    #region Inspector Variables


    #region Input
    [Foldout("- Input Variables")]
    public int inputType = 0;
        string horizontalInput = "Horizontal";
        string verticallInput = "Vertical";

     KeyCode jumpInput = KeyCode.Space;
     KeyCode strafeInput = KeyCode.Tab;
     KeyCode sprintInput = KeyCode.LeftShift;

    #endregion


    #region Movemont Variables
    [Foldout("- Movement Variables")]
    [Header("- Movement Action")]
        [Range(1f, 20f)]
        public float movementSmooth;
    [Foldout("- Movement Variables")]
        public float walkSpeed, runningSpeed, sprintSpeed, moveSpeed, Magnitude;

    [Header("- Action Check")]
    [Foldout("- Movement Variables")]
        public bool stopMove = false;


    Vector3 input, moveDirection, inputSmooth;
    Rigidbody _rigidbody;
    Transform cam;
    bool walkByDefault = false, isSprinting = false, useContinuousSprint = true;

    #endregion


    #region Jump Variables
    [Foldout("- Jump Variables")]
        public float jumpHeight, jumpCounter, jumpTimer, distanceToGround;
        bool IsGrounded, isJumping;

    #endregion


    #region Anim
    [Foldout("- Animator Variables")]
    [Range(0f, 1f)]
        public float animationSmooth = 0.2f;
        float verticalSpeed, horizontalSpeed;
        Animator anim;

    #endregion


    #region PlayerAction Manager
    [Foldout("- PlayerAction Manager")]
        public int ActionIndex;
    [Foldout("- PlayerAction Manager")]
    [SerializeField]
        public PlayerAction[] ActionList;

    PlayerAction SaveAction;

    #endregion


    #endregion

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _attackSystem = GetComponent<AttackSystem>();
        anim = GetComponent<Animator>();
        cam = GameObject.Find("3rdCamera").transform;

        _CursorLock = GameObject.Find("EventSystem").GetComponent<CursorLock>();
        _camControll = GameObject.Find("CamHolder").GetComponent<camControll>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_CursorLock.isLocked)
            return;

        InputCheck();
        checkGround();
        ActionCheck();
        AttackCheck();

        JumpInput();
        ControlJumpBehaviour();
        SetControllerMoveSpeed();
        MovemontInput();
        SprintInput();
    }


    #region Check Variables
    public void InputCheck() 
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            inputType = 0;
            horizontalInput = "Horizontal";
            verticallInput = "Vertical";

            jumpInput = KeyCode.Space;
            strafeInput = KeyCode.Tab;
            sprintInput = KeyCode.LeftShift;
        }
        if (Input.GetAxis("Axis 1") != 0 || Input.GetAxis("Axis 2") != 0)
        {
            inputType = 1;
            horizontalInput = "Axis 1";
            verticallInput = "Axis 2";

            jumpInput = KeyCode.JoystickButton0;
            strafeInput = KeyCode.JoystickButton9;
            sprintInput = KeyCode.JoystickButton5;
        }
    }
    
    void AttackCheck() 
    {
        if (!anim.GetCurrentAnimatorStateInfo(1).IsName("New State") || anim.GetCurrentAnimatorStateInfo(0).IsName("Evade") || !checkGround())
        {
            stopMove = true;
            isSprinting = false;
            anim.SetBool("IsSprinting", isSprinting);

            if (checkGround())
            {
                Magnitude = 0;
                anim.SetFloat("InputMagnitude", 0);
            }
            return;
        }
        else if (anim.GetCurrentAnimatorStateInfo(1).IsName("New State") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Evade") && checkGround())
        {
            stopMove = false;
        }
    }

    #endregion


    #region Movement & Animation Function

    void MovemontInput()
    {   
        anim.SetFloat("InputMagnitude", stopMove ? 0f : Magnitude, IsGrounded ? animationSmooth : animationSmooth, Time.deltaTime*10);
        

        //Movemont
        input.x = Input.GetAxis(horizontalInput);
        switch (inputType) 
        {
            case 0:
                input.z = Input.GetAxis(verticallInput);
                break;
            case 1:
                input.z = -Input.GetAxis(verticallInput);
                break;
        }

        //Vector3 targetPosition = (useRootMotion ? animator.rootPosition : _rigidbody.position) + _direction * (stopMove ? 0 : moveSpeed) * Time.deltaTime;

        if (input.x == 0 && input.z == 0 && Magnitude == 0.5f)
        {
            Magnitude = 0;
            anim.SetFloat("InputMagnitude", 0);
        }
        // ActionIndex
        if(input.x != 0 && input.z < 0.2f && Magnitude > 0.5f)
        {
            if(!_camControll.lockOn)
               ActionIndex = 1;
        }


        var right = cam.right;
        right.y = 0;
        var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
        inputSmooth = Vector3.Lerp(inputSmooth, input, movementSmooth * Time.deltaTime);
        moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
        // moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
        if (!anim.GetCurrentAnimatorStateInfo(1).IsName("New State") || anim.GetCurrentAnimatorStateInfo(0).IsName("Evade"))
            moveDirection = new Vector3(0, moveDirection.y, 0);
        MoveCharacter(moveDirection, 0);



        //Animation
        //  animator.SetFloat(vAnimatorParameters.InputVertical, stopMove ? 0 : verticalSpeed, freeSpeed.animationSmooth, Time.deltaTime);
        anim.SetFloat("InputHorizontal", Input.GetAxis(horizontalInput));
        anim.SetFloat("InputVertical", Input.GetAxis(verticallInput));

        Vector3 relativeInput = transform.InverseTransformDirection(moveDirection);
        verticalSpeed = relativeInput.z;
        horizontalSpeed = relativeInput.x;

        var newInput = new Vector2(verticalSpeed, horizontalSpeed);
        
        if(walkByDefault)
            Magnitude = Mathf.Clamp(newInput.magnitude, 0,  walkSpeed);
        else
            Magnitude = Mathf.Clamp(isSprinting ? newInput.magnitude + 0.5f : newInput.magnitude, 0, isSprinting ? sprintSpeed : runningSpeed);


    }

    void SprintInput()
    {
        if (input.x == 0 && input.z == 0)
        {
            isSprinting = false;
        }
        anim.SetBool("IsSprinting", isSprinting);
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton5)) // Input.GetAxis("Axis 10") > 0.0000001f　 Sprint(true);
        {
            Sprint(true);
        }
        else if (Input.GetKeyUp(KeyCode.JoystickButton5) || Input.GetKeyUp(KeyCode.LeftShift)) //Input.GetAxis("Axis 10") <= 0.0000001f
        {
            Sprint(false);
        }
    }

    void Sprint(bool value)
    {
        var sprintConditions = (input.sqrMagnitude > 0.1f && IsGrounded);
          //  && !( (horizontalSpeed <= -0.1f || verticalSpeed <= 0.1f)));

        if (value && sprintConditions)
        {
            if (input.sqrMagnitude > 0.1f)
            {
                if (IsGrounded && useContinuousSprint)
                {
                    isSprinting = !isSprinting;
                }
                else if (!isSprinting)
                {
                    if (!_camControll.lockOn)
                        ActionIndex = 2;
                    isSprinting = true;
                }
            }
            else if (!useContinuousSprint && isSprinting)
            {
                isSprinting = false;
            }
        }
        else if (isSprinting && !value)
        {
            isSprinting = false;
        }
        if (isSprinting && value)
        {
            if(!_camControll.lockOn)
                ActionIndex = 2;
            isSprinting = true;
        }
    }

    void MoveCharacter(Vector3 _direction, int Status)
    {     
        //if(_direction!=Vector3.zero)
        //    print(_direction);

        inputSmooth = Vector3.Lerp(inputSmooth, input, movementSmooth * Time.deltaTime);
        if (!IsGrounded || isJumping) return;

        _direction.y = 0;
        _direction.x = Mathf.Clamp(_direction.x, -1f, 1f);
        _direction.z = Mathf.Clamp(_direction.z, -1f, 1f);
        // limit the input
        if (_direction.magnitude > 1f)
            _direction.Normalize();


        switch (Status) 
        {
            case 0: //MovementStatus

                Vector3 targetPosition = (_rigidbody.position) + _direction * moveSpeed * Time.deltaTime;
                Vector3 targetVelocity = (targetPosition - transform.position) / Time.deltaTime;
                bool useVerticalVelocity = true;
                if (useVerticalVelocity) targetVelocity.y = _rigidbody.velocity.y;

                Vector3 movementDirection = input;
                movementDirection.Normalize();

                //print(_direction);
                if (moveDirection != Vector3.zero && anim.GetCurrentAnimatorStateInfo(1).IsName("New State") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Evade"))
                {
                    transform.forward = _direction;
                   if (_camControll.zoom < 30)
                        _camControll.zoom = 40;
                }
              //  print(targetVelocity);
                     _rigidbody.velocity = targetVelocity;

                break;

            case 1: //switchCam

               // Vector3 switchPosition = (_rigidbody.position) + _direction * moveSpeed * Time.deltaTime;
               // Vector3 switcVelocity = (switchPosition - transform.position) / Time.deltaTime;
                transform.forward = new Vector3(0, 0, 0.01f) ;

             //   _rigidbody.velocity = switcVelocity;
                break;
        }
    }

    void SetControllerMoveSpeed()
    {
        if (walkByDefault)
            moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? runningSpeed : walkSpeed, movementSmooth * Time.deltaTime);
        else
            moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? sprintSpeed : runningSpeed, movementSmooth * Time.deltaTime);
    }


    #endregion


    #region Jump
    // Jump
    bool checkGround()
    {
        IsGrounded = Physics.Raycast(this.transform.position + GetComponent<CapsuleCollider>().center, -Vector3.up, distanceToGround);
        if (!IsGrounded && input.x!=0 || !IsGrounded && input.y!=0)
            IsGrounded = true;
        return IsGrounded;
    }

    void JumpInput() 
    {
        anim.SetBool("IsGrounded", IsGrounded);
        if (Input.GetKeyDown(jumpInput) && IsGrounded)
            Jump();
    }

    void Jump()
    {
        // trigger jump behaviour
        jumpCounter = jumpTimer;
        isJumping = true;

        // trigger jump animations
            anim.CrossFadeInFixedTime("Jump", .1f);
    }

    void ControlJumpBehaviour()
    {
        if (!isJumping) return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            print(1);
            if (jumpCounter <= 0.2f)
                jumpCounter += 0.15f;
            else
                jumpCounter = 0.35f;
        }

        jumpCounter -= Time.deltaTime;
        if (jumpCounter <= 0)
        {
            jumpCounter = 0;
            isJumping = false;
        }
        // apply extra force to the jump height   
        var vel = _rigidbody.velocity;
        vel.y = jumpHeight;
        _rigidbody.velocity = vel;
    }
    #endregion


    #region Read PlayerAction

    void readActionVariables(PlayerAction _Action) 
    {
        int zoomType = 2;
        if (SaveAction == _Action) zoomType = 0;

        SaveAction = _Action;
        _camControll.followSpeed = _Action.cameraFollowSpeed;
        _camControll.CameraZoom(zoomType, _Action.CameraView);
    }

    public void ActionCheck() 
    {
        if( !isSprinting && input.x == 0 || !isSprinting && input.z > 0.2f ||  Magnitude == 0)
        {
            if (!_camControll.lockOn)
                ActionIndex = 0;
        }
        if (_camControll.lockOn)
            ActionIndex = 3;

        readActionVariables(ActionList[ActionIndex]);
    }

#endregion

}
