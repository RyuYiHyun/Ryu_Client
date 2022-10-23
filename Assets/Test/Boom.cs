using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boom : MonoBehaviour
{
    private void Start()
    {
        Destroy(this.gameObject, 1);
    }
}
