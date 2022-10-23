using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapRigidbody : MonoBehaviour
{
    [SerializeField] public Rigidbody target = null;

    //[SerializeField] float velocitySensitivity = 0.1f;
    //[SerializeField] float angularVelocitySensitivity = 0.1f;


    [SerializeField] float lerpVelocityAmount = 0.5f;

    [SerializeField] float lerpPositionAmount = 0.5f;

    [SerializeField] float lerpRotationAmount = 0.5f;

    float nextSyncTime;

    public Vector3 targetVelocity;

    public Vector3 targetPosition;

    public Quaternion targetRotation;

    public int targetstate;

    //readonly SyncState previousValue = new SyncState();
    void OnValidate()
    {
        if (target == null)
        {
            target = GetComponent<Rigidbody>();
        }
    }
    const float syncInterval = 0.1f; 

    public void Update()
    {
        if (GetComponent<Player>().id == MainManager.Instance.mainPlayer.id)
        {
            SendToServer();
        }
    }

    void SendToServer()
    {
        float now = Time.time;

        if (now > nextSyncTime)
        {
            //Debug.Log($"현재 시간 - {now}, 지금 이순간 - {nextSyncTime}");
            nextSyncTime = now + syncInterval;
            CmdSendState(target.velocity, target.position, target.rotation, targetstate);
            MainManager.Instance.SendMove(GetComponent<Player>().id, targetVelocity, targetPosition, targetRotation, targetstate);
        }
    }
    void CmdSendState(Vector3 velocity, Vector3 position, Quaternion quaternion, int state)
    {
        target.velocity = velocity;
        target.position = position;
        target.rotation = quaternion;

        targetVelocity = velocity;
        targetPosition = position;
        targetRotation = quaternion;

        targetstate = state;
    }

    void FixedUpdate()
    {

        if (GetComponent<Player>().isJump)
        {
            target.velocity = Vector3.Lerp(target.velocity, new Vector3(targetVelocity.x, target.velocity.y, targetVelocity.z), lerpVelocityAmount);
            target.position = Vector3.Lerp(target.position, new Vector3(targetPosition.x, target.position.y, targetPosition.z), lerpPositionAmount);
        }
        else
        {
            target.velocity = Vector3.Lerp(target.velocity, targetVelocity, lerpVelocityAmount);
            target.position = Vector3.Lerp(target.position, targetPosition, lerpPositionAmount);
        }
        target.rotation = Quaternion.Slerp(target.rotation, targetRotation, lerpRotationAmount).normalized;
        target.position += target.velocity * Time.fixedDeltaTime;
    }



}
