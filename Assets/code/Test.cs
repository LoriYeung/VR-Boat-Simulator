using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform tillerHandle;
    public Transform rudder;

    void Update() {
        rudder.eulerAngles = new Vector3(0, tillerHandle.eulerAngles.y, 0);
    }
}
