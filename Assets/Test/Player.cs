using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // 이속, 회전 속도
    public int id;
    public const float rotateSpeed = 50.0f;
    public const float jumpPower = 10.0f;
    public float moveSpeed = 20.0f;
    public Rigidbody rigbody;
    public Transform trans;
    public GameObject boomPrefab;
    public Transform Gun;


    public KeyCode shootKey = KeyCode.Z;

    public bool isJump = false;
    public bool isDodge = false;

    public Animator animaotr;
    private void Awake()
    {
        rigbody = GetComponent<Rigidbody>();
        animaotr = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        //trans.position = Vector3.Lerp(trans.position, currPos, Time.deltaTime * moveSpeed);
        //trans.rotation = Quaternion.Slerp(trans.rotation, currRot, Time.deltaTime * rotateSpeed);

        if (id == MainManager.Instance.mainPlayer.id)
        {
            GetInput();
            Move();
            //Turn();
            TurnRotate();
            Jump();
            Dodge();
            if (Input.GetKeyDown(shootKey))
            {
                Fire();
            }
            if(GetComponent<LeapRigidbody>().targetstate != 3)
            {
                if (walkDown && runDown)
                {
                    GetComponent<LeapRigidbody>().targetstate = 1;
                }
                else if (runDown)
                {
                    GetComponent<LeapRigidbody>().targetstate = 2;
                }
                else
                {
                    GetComponent<LeapRigidbody>().targetstate = 0;
                }
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            rigbody.velocity = Vector3.zero;
            rigbody.angularVelocity = Vector3.zero;
            animaotr.SetBool("IsJump", false);
            GetComponent<LeapRigidbody>().targetstate = 4;
            isJump = false;
        }
    }
    #region 플레이어 컨트롤 변수

    Vector3 move;
    Vector3 dodge;
    bool walkDown;
    bool runDown;
    bool jumpDown;

    //public PacketMoveData m_mainPlayerData;
    #endregion


    #region 플레이어 컨트롤 함수
    void GetInput()
    {
        move.x = Input.GetAxis("Horizontal");
        move.z = Input.GetAxis("Vertical");
        walkDown = Input.GetKey(KeyCode.LeftShift);
        jumpDown = Input.GetKeyDown(KeyCode.Space);

    }
    

    void Move()
    {
        Vector3 lookDir = move.normalized;
        if(isDodge)
        {
            move = dodge;
            return;
        }
        runDown = lookDir != Vector3.zero ? true : false;

        if (walkDown)
        {
            //mainPlayerPos = Vector3.Lerp(mainPlayerPos, mainPlayerPos + lookDir, Time.deltaTime * Player.moveSpeed * 0.3f);
            rigbody.velocity = new Vector3(lookDir.x * moveSpeed / 3, rigbody.velocity.y, lookDir.z * moveSpeed / 3);
        }
        else
        {
            //mainPlayerPos = Vector3.Lerp(mainPlayerPos, mainPlayerPos + lookDir, Time.deltaTime * Player.moveSpeed);
            rigbody.velocity = new Vector3(lookDir.x * moveSpeed, rigbody.velocity.y, lookDir.z * moveSpeed);
        }
        //mainPlayerPos += lookDir * Player.moveSpeed * Time.deltaTime;
    }
    void Turn()
    {

        if (move.normalized != Vector3.zero)
        {
            //mainPlayerRot = Quaternion.LookRotation(lookDir);
            rigbody.rotation = Quaternion.LookRotation(move.normalized);
        }
    }
    void TurnRotate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            Debug.DrawLine(ray.origin, hit.point);
            Vector3 lookRotation = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            transform.LookAt(lookRotation);
            rigbody.rotation = transform.rotation;
        }
    }
    void Jump()
    {
        if (jumpDown && (move == Vector3.zero)  && !isJump && !isDodge)
        {
            //rigbody.AddForce(Vector3.up * Player.jumpPower, ForceMode.Impulse);
            //animaotr.SetBool("IsJump", true);
            //animaotr.SetTrigger("doJump");
            isJump = true;
            MainManager.Instance.SendJump(id);
        }
    }
    void Dodge()
    {
        if (jumpDown && (move != Vector3.zero) && !isJump && !isDodge)
        {
            //moveSpeed *= 2;
            //animaotr.SetTrigger("doDodge");
            dodge = move;
            isDodge = true;
            MainManager.Instance.SendDodge(id);
        }
    }


    public IEnumerator DodgeEnd()
    {
        yield return new WaitForSeconds(0.6f);
        moveSpeed = 20.0f;
        isDodge = false;
    }
    void Fire()
    {
        MainManager.Instance.SendFire(id, Gun.position, Gun.right);
    }
    public float power = 500.0f;
    public void MakeFire(Vector3 gunPosition, Vector3 gunRight)
    {
        GameObject temp = GameObject.Instantiate(boomPrefab, gunPosition, Quaternion.identity);
        temp.SetActive(true);
        temp.GetComponent<Rigidbody>().useGravity = false;
        temp.GetComponent<Rigidbody>().AddForce(gunRight * power);
    }

    #endregion
}
