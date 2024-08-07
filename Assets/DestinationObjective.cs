using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationObjective : MonoBehaviour
{

    public ObjectiveManager objectiveManager;
    public bool destinationReached = false;

    // Start is called before the first frame update
    void Start()
    {
        objectiveManager = FindObjectOfType<ObjectiveManager>();
        objectiveManager.scoreToWin++;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !destinationReached)
        {
            destinationReached = true;
            objectiveManager.IncreaseScore(1);
        }
    }


}
