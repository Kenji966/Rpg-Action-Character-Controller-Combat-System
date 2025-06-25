using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using Pixeye.Unity;
using UnityEngine.VFX;

public class AttackSystem : MonoBehaviour
{
    #region Other Class
        movementControll _movementControll; //_movementControll.isRolling = true; //IsGuard //AttackIng = true
        camControll _camControll;
        CursorLock _CursorLock;
        SwordDamagDealer _Sword;
    #endregion

    #region Inspector Variables

    #region Input

    [Header("- Player Input Values")]
        KeyCode AttackInput = KeyCode.JoystickButton2;
        KeyCode SkillInput = KeyCode.JoystickButton3;
        KeyCode EvadeInput = KeyCode.JoystickButton1;
        KeyCode GuardInput = KeyCode.JoystickButton4;

    #endregion

    #region Attack Values

 [Header("- Dash Attack Values")]
    [Foldout("- Attack Values")]
        public float DashAttackRange;
    [Foldout("- Attack Values")]
        public float DashAttackTime, DashAttDistance;

[Header("- Attack Effect Values")]
    [Foldout("- Attack Values")]
        public float AttackMaterialTimes;

    [Foldout("- Attack Values")] 
        public int[] SKillCharge;

 [Header("- Get Signal")]
    [Foldout("- Attack Values")]
        public bool isAttack = true;
        bool DashStart = false;
        bool IsGuard = false;
        bool StopDashAttack = false;

    //Get Correct direction
    Transform PlayerFwDir, CamFwDIr;
    Vector3 Target;

    #endregion

    #region Rolling Variables
    [Foldout("- Rolling Variables")]
        public float delayBetweenPresses = 0.25f;
        bool pressedFirstTime = false;

    [Foldout("- Rolling Variables")]
        public float lastPressedTime, EvadeTime, EvadeRange;

    #endregion

    // Animator && Collider Manager
    #region Hide Variables 

    // Animator Controll
    [Header("- Animator Controll")]
        Animator anim; //DashAniSpeed = 1.025f;


    // Collider Manager
    [Header("- Collider Manager")]
        BoxCollider SwordCollider; //Sword
        List<BoxCollider> IceMagicColl = new List<BoxCollider> { }; //SKill 

    #endregion

    #region Effect Variables

    #region CamEffect

    [Foldout("- Camera Effect")]
        public float[] sCamTimesList;

    [Foldout("- Camera Effect")]
        public List<GameObject> skillCamList;
        bool camSwitching = false;

    // Shake Cam
    Animator camAni; 
    bool shakeCD = false;

    #endregion

    #region VolumeEffect

    [Foldout("- Volume Effect")]
        public Volume DashVolume, DashAttackVolume;

    #endregion

    #region MeshRender & MaterialManager

    [Header("- MeshRender Manager")]
    [Foldout("- MeshRender & Material Manager")]
        public MeshRenderer swordMesh;

    [Foldout("- MeshRender & Material Manager")]
        public SkinnedMeshRenderer PlayerBody, PlayerHair;

    [Header("- Material Manager")]
    [Foldout("- MeshRender & Material Manager")]
        public Material[] PlayerNormalMaterialList;

    [Foldout("- MeshRender & Material Manager")]
        public Material[] PlayerHairNorMaterialList, 
               SwordNorMaterialList, AttacMaterialList, AttackHairMaterialList, AttackSwordMaterialList;

    #endregion

    #region Particle System
     [Foldout("- Particle System Variables")]
        public GameObject[] SwordParSysList;
        GameObject SwordParSys;

    [Foldout("- Particle System Variables")]
        public List<GameObject> SlashEffectList = new List<GameObject>();

    #endregion

    #endregion

    #endregion


    void Start()
    {
        #region Get Normal Variable
        //Get Variable
        _movementControll = GetComponent<movementControll>();
        _camControll = GameObject.Find("CamHolder").GetComponent<camControll>();
        _CursorLock = GameObject.Find("EventSystem").GetComponent<CursorLock>();

        anim = GetComponent<Animator>();
        camAni = GameObject.Find("CamPivot").GetComponent<Animator>();

        #endregion

        #region Get Forword Direction

        PlayerFwDir = transform.Find("----Camera Variables----/PlayerFWDir").transform;
        CamFwDIr = transform.Find("----Camera Variables----/CamFWDIr").transform;
        CamFwDIr.SetParent(null);

        #endregion

        #region Get Skill Collider

        //Sword
        SwordCollider = GameObject.Find("Wakizashi").GetComponent<BoxCollider>();
        _Sword = GetComponent<SwordDamagDealer>(); //SwordCollider.gameObject.
       // sowrdCollider = transform.Find("Root/J_Bip_C_Hips/J_Bip_C_Chest/J_Bip_C_UpperChest/J_Bip_R_Shoulder/J_Bip_R_UpperArm/J_Bip_R_LowerArm/J_Bip_R_Hand/J_Bip_R_Index1/ArmPos/ArmRoate/Wakizashi").GetComponent<BoxCollider>();

        //Ice Skill
        IceMagicColl.Add(transform.Find("IceAttack/Combo3Pos").GetComponent<BoxCollider>());
        IceMagicColl.Add(transform.Find("IceAttack/Ice_DashAtk2Pos").GetComponent<BoxCollider>());

        #endregion

        #region Get Skill Camera

        skillCamList.Add(transform.Find("---SkillCamera---/").GetChild(1).gameObject);
        skillCamList.Add(transform.Find("---SkillCamera---").GetChild(0).gameObject);

        #endregion

        #region Get Effect Variables

        SwordParSys = SwordCollider.gameObject.transform.Find("EnergySword1").gameObject;

        #endregion
    }

    void Update()
    {
        if (!_CursorLock.isLocked)
            return;

        GetInput();
        AniamtorCon();

        Attack();
        LockOnTarget();
        SkillCharging();

        Evade();
        Guard();

        VolumeCon();
    }


    // Input Controll
    void GetInput() 
    {
        AttackInput = KeyCode.JoystickButton2;
        AttackInput = _movementControll.inputType == 1 ? AttackInput : KeyCode.Mouse0;

        SkillInput = KeyCode.JoystickButton3;
        SkillInput = _movementControll.inputType == 1 ? SkillInput : KeyCode.Q;

        EvadeInput = KeyCode.JoystickButton1;
        EvadeInput = _movementControll.inputType == 1 ? EvadeInput : KeyCode.E;

        GuardInput = KeyCode.JoystickButton4;
        GuardInput = _movementControll.inputType == 1 ? GuardInput : KeyCode.LeftControl;
    }

    // Animator Controll
    void AniamtorCon() 
    {
        anim.SetInteger("Skill-A", SKillCharge[0]);
        anim.SetBool("IsGuard", IsGuard);
    }


    void LockOnTarget()
    {
        if (!anim.GetCurrentAnimatorStateInfo(1).IsName("New State") || anim.GetCurrentAnimatorStateInfo(0).IsName("Evade"))
            transform.LookAt(new Vector3(Target.x, transform.position.y, Target.z));
    }


    #region Attack Function

    // Attack Func
    void Attack()
    {

        #region Target Direction

        CamFwDIr.position = PlayerFwDir.position;

        Target = new Vector3(CamFwDIr.position.x, transform.position.y, CamFwDIr.position.z);
        Target = _camControll.lockOn ? GameObject.Find("MonsterTargetLocator").transform.position : Target;

        #endregion


        #region Get Input

        if ( Input.GetKeyDown(AttackInput)) 
        {
            // Return Varible
            _movementControll.stopMove = true;
            StopDashAttack = false;
            shakeCD = false;
            for (int i = 0; i < IceMagicColl.Count; i++) 
                IceMagicColl[i].enabled = true;

            // MaterialChange
            MaterialChange(0);

            // Action
            anim.SetTrigger("Attack");
        }

        #endregion


        #region Dash Attack
        //Dash Attack
        if (anim.GetCurrentAnimatorStateInfo(1).IsName("DashAttack") && !StopDashAttack)
        {
            DashStart = true;

            if (_camControll.lockOn)
                transform.DOMove(Target, DashAttackTime).SetId<Tween>("DashTargetAttack");

            else if(_camControll.saveClosestEnemy != null && Vector3.Distance(this.transform.position, _camControll.saveClosestEnemy.transform.position) < DashAttDistance)
                transform.DOMove(_camControll.saveClosestEnemy.transform.position, DashAttackTime).SetId<Tween>("DashTargetAttack");
           
            else
                transform.DOMove(transform.position + transform.forward * DashAttackRange, DashAttackTime).SetId<Tween>("DashAttack");
          
        }

        #endregion


        #region Cancel Event

        if (DashStart && Vector3.Distance(this.transform.position, Target) < 1.55f) //Input.anyKey && DashStart && isAttack || 
        {
            stopDashAttack();
        }

        if(Input.anyKey && !camSwitching)
        {
            CamSwitch(100);
        }

        #endregion


        #region Get Hit Singal

        if (isAttack)
        {
            if (!shakeCD)
            {
                camAni.Play("AttackCamMovement");
                shakeCD = true;
            }
            DashStart = false;
            SwordCollider.enabled = false;
            for (int i = 0; i < IceMagicColl.Count; i++)
                IceMagicColl[i].enabled = false;

            isAttack = false;
        }

        #endregion

    }

    void SkillCharging() 
    {
        if (!anim.GetCurrentAnimatorStateInfo(1).IsName("skillA-Iku"))
        {
            if (Input.GetKey(SkillInput))
                SKillCharge[0] += 1;
        }

        if (Input.GetKeyUp(SkillInput) || anim.GetCurrentAnimatorStateInfo(1).IsName("skillA-Iku"))
        {
            SKillCharge[0] = 0;
            StartCoroutine(SwordEffChargeHold(0));
        }

    }


    #endregion


    // Evade Func
    void Evade() 
    {
        if (Input.GetKeyDown(EvadeInput) && anim.GetBool("IsSprinting"))
        {
            anim.SetTrigger("isEvade");

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Evade"))
        {
            transform.DOMove(transform.position + transform.forward * EvadeRange, EvadeTime).SetId<Tween>("EvadeMove");
        }

        //double click check func

        //    if (pressedFirstTime) // we've already pressed the button a first time, we check if the 2nd time is fast enough to be considered a double-press
        //    {
        //        bool isDoublePress = Time.time - lastPressedTime <= delayBetweenPresses;

        //        if (isDoublePress)
        //        {
        //            pressedFirstTime = false;
        //         //   anim.SetTrigger("isEvade");
        //        }
        //    }
        //    else // we've not already pressed the button a first time
        //    {
        //        pressedFirstTime = true; // we tell this is the first time
        //    }

        //    lastPressedTime = Time.time;
        //}

        //if (pressedFirstTime && Time.time - lastPressedTime > delayBetweenPresses) // we're waiting for a 2nd key press but we've reached the delay, we can't consider it a double press anymore
        //{
        //    // note that by checking first for pressedFirstTime in the condition above, we make the program skip the next part of the condition if it's not true,
        //    // thus we're avoiding the "heavy computation" (the substraction and comparison) most of the time.
        //    // we're also making sure we've pressed the key a first time before doing the computation, which avoids doing the computation while lastPressedTime is still uninitialized
        //    pressedFirstTime = false;
        //}
    }
   }

    // Guard Func
    void Guard() 
    {
        if (Input.GetKeyDown(GuardInput))
        {
         //   IsGuard = !IsGuard;
        }
    }



    #region Effect Function

    void VolumeCon()
    {
        // Dash Volume Effect
        DashVolume.enabled = anim.GetBool("IsSprinting");
    }

    void MaterialChange(int i)
    {
        switch (i)
        {
            case 0: // Combo Attack

                PlayerBody.materials = AttacMaterialList;
                PlayerHair.materials = AttackHairMaterialList;
                swordMesh.materials = AttackSwordMaterialList;
                CancelInvoke("returnMaterial");
                Invoke("returnMaterial", AttackMaterialTimes);
                break;
        }
    }


    public void returnMaterial()
    {
        PlayerBody.materials = PlayerNormalMaterialList;
        PlayerHair.materials = PlayerHairNorMaterialList;
        swordMesh.materials = SwordNorMaterialList;
        CancelInvoke("returnMaterial");
    }

    #endregion


    #region Animation Trigger Func

    public void stopDashAttack()
    {
        StopDashAttack = true;
        DashStart = false;
        DOTween.PauseAll();
    }

    public void CanDealDamage(int i) 
    {
        switch(i)
        {
            case 0:
                _Sword.CanDealDamage = true;
                break;
            case 1:
                _Sword.CanDealDamage = false;
                break;
        }
    }

    public void IceDamage(int i)
    {
        switch (i)
        {
            case 0:
                _Sword.iceAttack = true;
                break;
            case 1:
                _Sword.iceAttack = false;
                break;
        }
    }

    public IEnumerator CamSwitch(int i)
    {
      if (_camControll.lockOn)
        switch (i)
        {
            case 100:
                camSwitching = false;
                skillCamList[0].SetActive(false);
                _camControll.Cam.SetActive(true);
                break;


            case 0:
                camSwitching = true;
                skillCamList[i].SetActive(true);
                _camControll.Cam.SetActive(false);
                yield return new WaitForSeconds(sCamTimesList[0]);
                
                camSwitching = false;
                _camControll.Cam.SetActive(true);
                skillCamList[0].SetActive(false);
                break;

        }
    }

    public IEnumerator SwordEffectTrigger(float i)
    {
        StopCoroutine(SwordEffChargeHold(i));
        StopCoroutine(SwordEffectTrigger(i));
        

        ParticleSystem[] SowrdFa = SwordParSys.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem child in SowrdFa) 
        {
            child.Clear();
            child.Play();
        }

        yield return new WaitForSeconds(i);

    }

    public IEnumerator SwordEffChargeHold(float i)
    {
        StopCoroutine(SwordEffChargeHold(i));
        StopCoroutine(SwordEffectTrigger(i));

        List<ParticleSystem> SwordPS = new List<ParticleSystem>();
        SwordPS.Add(SwordParSys.GetComponentInChildren<ParticleSystem>());

        switch (i) 
        {
            case 0:

                foreach (var ps in SwordPS)
                {
                    ps.Play();
                }

                yield return new WaitForSeconds(1.5f);
                SwordParSys.SetActive(false);

                break;
            case 1:

                SwordParSys.SetActive(true);

                yield return new WaitForSeconds(0.5f);

                foreach (var ps in SwordPS)
                {
                    ps.Pause();
                }

                break;
        }

       
    }

    public IEnumerator SlashEffectFuncd(int i)
    {
        SlashEffectList[i].SetActive(false);
        SlashEffectList[i].SetActive(true);

        yield return new WaitForSeconds(1.5f);
        SlashEffectList[i].SetActive(false);
    }

    #endregion

}
