using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoatControlsObjective : MonoBehaviour
{
    [Header("Settings")]
    //default: false = sail, true = rudder;
    public bool rudderObjective = false;

    [Header("References")]
    public GameObject TutorialMenu;

    public Image turnLeftStatus;
    public Image turnRightStatus;

    [Header("Objective Status")]
    public bool turnedRight = false;
    public bool turnedLeft = false;

    private Rigidbody boat_rb;
    private float rotation;

    void OnEnable()
    {
        boat_rb.isKinematic = true;

    }

    // Start is called before the first frame update
    void Awake()
    {
        boat_rb = BoatManager.Player.rb;   
    }

    // Update is called once per frame
    void Update()
    {
        CheckCompleted();
    }

    void CheckCompleted()
    {
        if(!rudderObjective)
            rotation = BoatManager.Player.Sail.transform.localEulerAngles.y;
        else
            rotation = BoatManager.Player.Rudder.transform.localEulerAngles.y;

        if(rotation > 180)
            rotation -= 360;

        Debug.Log("Sail Rotation: " + rotation);
        if(rotation >= 45f && !turnedLeft)
        {
            turnLeftStatus.color = Color.green;
            turnedLeft = true;
        }

        if(rotation <= -45 && !turnedRight)
        {
            turnRightStatus.color = Color.green;
            turnedRight = true;
        }

        if(turnedRight && turnedLeft)
        {
            Invoke("DisableMenu", 1f);
            Invoke("ReturnToTutorial", 1.3f);
        }
    }

    void DisableMenu()
    {
        gameObject.SetActive(false);
    }

    void ReturnToTutorial()
    {
        TutorialMenu.SetActive(true);
        boat_rb.isKinematic = false;
        Destroy(this);
    }
}
