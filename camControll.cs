using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;
using Pixeye.Unity;

public class camControll : MonoBehaviour
{
#region Inspector Variables

 #region Input Values
    [Foldout("- Player Input Values")]
        public Vector2 look;
    
    [Foldout("- Player Input Values")]
        public bool lockOn = false;
    
    KeyCode strafeInput = KeyCode.Tab;
    PlayerInput _playerInput;

    #endregion


#region  Camera Controll Values
  [Header("Follow Camera Values")]
    [Foldout("- CameraControll Values")]
        public Transform targetTransform;

    [Foldout("- CameraControll Values")]
        public Transform pivot;

    [Foldout("- CameraControll Values")]
        public float followSpeed = 0.1f, rotateSpeed = 1;
        float lookAngle; 
        float pivotAngle;


  // CameraRange
  [Header("Camera Range Values")]
    [Foldout("- CameraControll Values")]
        public float TopClamp = 70.0f;
    [Foldout("- CameraControll Values")]
        public float BottomClamp = -10.0f;


    // CameraZoom
    [Foldout("- Camera Zoom Values")]
        public float zoom, zoomMultiplier = 4, minZoom = 2, maxZoom = 8, velocirty = 0, smoothTime = 0.25f;

    bool cursorLocked, cursorInputForLook = true, isScoll = false;
    CursorLock _CursorLock;

    #endregion


    #region  Get Target Values

    [Header("Get Target Distance")]
    [Foldout("- GetTarget Values")]
        public float DistanceCheck;

 [Header("Get Target Position")]
    [Foldout("- GetTarget Values")]
        public GameObject saveClosestEnemy;
    [Foldout("- GetTarget Values")]
        public GameObject closestEnemy;
    [Foldout("- GetTarget Values")]
        public GameObject MonsterTargetLocator;

 [Header("Get All Enemy")]
    [Foldout("- GetTarget Values")]
        public GameObject[] allEnemies;
    [Foldout("- GetTarget Values")]
        public List<float> allEmeDis;


    string tagToDetect = "Enemy";

    #endregion


#region Camera Distance & Movement
    [Foldout("- Camera Distance & Movement")]
        public GameObject Cam, camParent;
    [Foldout("- Camera Distance & Movement")]
        public Vector3 CamOffset;
    [Foldout("- Camera Distance & Movement")]
        public float SmoothSpeed;

    #endregion


#region CameraCollider
 [Header("Camera Distance Values")]
    [Foldout("- Camera Collider")]
        public float smooth;
    [Foldout("- Camera Collider")]
        public float minDistance, maxDistance, distance,  distanceOffset;

    [Header("CameraPivot Distance")]
    [Foldout("- Camera Collider")]
        public float normalMagnitude, movementMagnitude;
    [Foldout("- Camera Collider")]
        public Vector3 dollyDir;

    Animator anim;
    float saveDistance;

#endregion


#region CHeck Angle ~ Not Yet
    [Header("Check Angle")]
    [Tooltip("Set Limit of Angle with Camera & Target ")]
    float setAngle;
    float Angle;

    #endregion


#endregion


    private void Awake()
    {
        dollyDir = camParent.transform.localPosition.normalized;
        distance = camParent.transform.localPosition.magnitude;
    }

    void Start()
    {
        lockOn = false; 
        _playerInput = GetComponent<PlayerInput>();
        allEnemies = GameObject.FindGameObjectsWithTag(tagToDetect);

        for (int i = 0; i < allEnemies.Length; i++)
        {
            allEmeDis.Add(Vector3.Distance(transform.position, allEnemies[i].transform.position));
        }

        //zoom
        zoom = Cam.GetComponent<Camera>().fieldOfView;
        anim = Cam.GetComponent<Animator>();
        _CursorLock = GameObject.Find("EventSystem").GetComponent<CursorLock>();

    }

    void Update()
    {
        if (!_CursorLock.isLocked)
            return;

        CameraRotation();
        targetCheck();
        CameraCollision();
        ZoomInput();
    }
   

    #region Camera Controll

    private void CameraRotation()
    {
        Tick(Time.fixedDeltaTime);
        HandleRotation(Time.fixedDeltaTime, look.x, look.y);
    }
    public void Tick(float delta)
    {
        // 相机延迟跟随
        Vector3 targetPosition = Vector3.Lerp(transform.position, targetTransform.transform.position, delta / followSpeed);   

        transform.position = targetPosition;
    }

    public void HandleRotation(float delta, float mouseX, float mouseY)
    {

        if (!lockOn)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            lookAngle += (mouseX * deltaTimeMultiplier) * (rotateSpeed);    //根据鼠标横向输入转化当前镜头横向角度delta /
            pivotAngle +=(mouseY * deltaTimeMultiplier) * (rotateSpeed);   //根据鼠标纵向输入转化当前镜头纵向角度delta /
            pivotAngle = Mathf.Clamp(pivotAngle, BottomClamp, TopClamp);    //限制纵向角度

            Vector3 euler = Vector3.zero;
            euler.y = lookAngle;
            euler.x = pivotAngle;
            Quaternion targetRotation = Quaternion.Euler(euler);

            //让镜头的旋转角赋值为当前所应该转到的横纵角
            transform.rotation = targetRotation;
            pivot.rotation = Quaternion.Lerp(pivot.rotation, targetRotation, delta / 0.25f);

        }
        else
        {
            Vector3 lockPos = MonsterTargetLocator.transform.position;
            Vector3 dir = lockPos - transform.position;
            dir.Normalize();
            dir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(dir);   //镜头目标转向为玩家角色到当前锁定目标的向量的横向平面分量

            Vector3 pivotDir = lockPos - pivot.position;
            pivotDir.Normalize();
            Quaternion pivotTargetRotation = Quaternion.LookRotation(pivotDir);
            Vector3 e = pivotTargetRotation.eulerAngles;
            e.y = 0;    //只考虑垂直转向，即pivot指向玩家角色到当前锁定目标的向量的纵向分量

            //让镜头角度和pivot角度线性变化，实现丝滑转换自由镜头和锁定镜头
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime / 0.25f);
            pivot.localEulerAngles = Vector3.Lerp(pivot.localEulerAngles, e, Time.fixedDeltaTime / 0.25f);

            pivotAngle = 0;     //清空当前的纵向角度，使得解锁镜头时镜头不会出现突变
            lookAngle = transform.eulerAngles.y;   //使读入的镜头横向角度始终为当前角度值，同样是为了使得解锁镜头时镜头不会出现突变
        }

    }
    // Set CameraDistance
    void setCameraDistance()
    {
        if (lockOn)
        {
            Vector3 newVec3 = camParent.transform.position + CamOffset;
            Cam.transform.position = Vector3.Slerp(Cam.transform.position, newVec3, Time.deltaTime * SmoothSpeed);
        }
        else
            Cam.transform.position = Vector3.Slerp(Cam.transform.position, camParent.transform.position, Time.deltaTime * SmoothSpeed);
    }

    // Zoom Camera
    void ZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            zoom -= scroll * zoomMultiplier;
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            isScoll = true;
            CameraZoom(1, zoom);
        }
    }

    public void CameraZoom(int zoomType, float fieldOfView) 
    {
        int smoothSpeed = 1;

        if (fieldOfView == Cam.GetComponent<Camera>().fieldOfView)
            return;
    

        switch (zoomType) 
        {
            case 0:
                if(isScoll)
                    return;
                break;
            case 1:
                smoothSpeed = 10;
                break;
            case 2:
                isScoll = false;
                break;
        }

        Vector3 SlerpVector = new Vector3(0, 0, Cam.GetComponent<Camera>().fieldOfView);
        SlerpVector = Vector3.Slerp(SlerpVector, new Vector3(0, 0, fieldOfView), smoothTime * smoothSpeed);

        Cam.GetComponent<Camera>().fieldOfView = SlerpVector.z;
    }

    void CameraCollision() 
    {
        Vector3 desiredCameraPos = camParent.transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;
        Debug.DrawRay(camParent.transform.position, desiredCameraPos, Color.red);
       
        if (Physics.Linecast(camParent.transform.parent.position, desiredCameraPos, out hit))
        {
            saveDistance = distance;
            camParent.transform.localPosition = Vector3.Lerp(camParent.transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);
          
            distance = Mathf.Clamp(hit.distance * distanceOffset, minDistance, maxDistance);
        }
        else
        {   
            setCameraDistance();
            distance = saveDistance;
        }

    }

    Vector3 Between(Vector3 v1, Vector3 v2, float percentage)
    {
        return (v2 - v1) * percentage + v1;
    }

    #endregion


    #region LockOnTarget

    void targetCheck()
    {
        closestEnemy = ClosetEnemy(lockOn);
        anim.SetBool("Targeting", lockOn);
       // attCon.getTarget = lockOn;

        if (Input.GetKeyDown(strafeInput) || Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            if (!lockOn && closestEnemy != null)
                lockOn = true;
            else if (lockOn)
                lockOn = false;
        }

        if (saveClosestEnemy != null)
            MonsterTargetLocator.transform.position = saveClosestEnemy.transform.position;
        else
            MonsterTargetLocator.transform.position = Vector3.zero;

        if (!lockOn)
            saveClosestEnemy = null;

        if (lockOn && saveClosestEnemy == null)
            saveClosestEnemy = closestEnemy;

    }

    GameObject ClosetEnemy(bool isLock)
    {
        GameObject closestEnemy = null;

        switch (lockOn)
        {
            case true:

                for (int i = 0; i < allEnemies.Length; i++)
                {
                    allEmeDis[i] = Vector3.Distance(transform.position, allEnemies[i].transform.position);

                    if (allEmeDis[i] > DistanceCheck && allEnemies[i] == saveClosestEnemy)
                    {
                        saveClosestEnemy = null;
                        closestEnemy = null;
                        lockOn = false;
                      //  attCon.getTarget = false;
                    }
                }
                break;

            case false:
                for (int i = 0; i < allEnemies.Length; i++)
                {
                    float distanceHere = Vector3.Distance(transform.position, allEnemies[i].transform.position);
                    allEmeDis[i] = distanceHere;

                    if (allEmeDis[i] == allEmeDis.Min() && allEmeDis[i] <= DistanceCheck)
                    {
                        closestEnemy = allEnemies[i];
                    }
                    else if (allEmeDis[i] == allEmeDis.Min() && allEmeDis[i] > DistanceCheck)
                    {
                        saveClosestEnemy = null;
                    }
                }
                break;
        }
        return closestEnemy;
    }

    #endregion


    #region getInput

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook)
        {
            LookInput(value.Get<Vector2>());
        }
    }
    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    #endregion


}


