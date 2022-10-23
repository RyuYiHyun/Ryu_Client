using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    private void LateUpdate()
    {
        if (target != null)
        {
            //target.position
            //transform.position = Vector3.Lerp(transform.position, MainManager.Instance.mainPlayer.currPos + offset, Time.deltaTime * Player.moveSpeed);
            transform.position = target.position + offset;
        }
    }
}
