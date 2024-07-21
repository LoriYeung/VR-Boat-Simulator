using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    // Declare
    // step 0(optional): choose accessiblity level, make variable private or public
    // 1: choose data type
    // 2: choose name for variable
    // 3: end it with ";"
    
    public Rigidbody rb;
    

    //Initialise
    // step 1: Add "=" sign
    // 2: choose value
    // 3: end it with ";"

    // Start is called before the first frame update
    void Start()
    {
     Debug.Log("Hello world");   
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 force = new Vector3(0, 10, 0);
        ForceMode mode = ForceMode.Acceleration;
        rb.AddForce(force, mode);


    }
}
