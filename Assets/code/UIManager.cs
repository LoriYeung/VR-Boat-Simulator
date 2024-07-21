using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class UIManager : MonoBehaviour
{
    public GameObject WindVectorUI;
    public GameObject SailDirectionUI;
    public GameObject ApparentWindUI;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWindVectorUI();
        UpdateSailDirectionUI();
        UpdateApparentWindUI();
    }

    void UpdateWindVectorUI()
    {
        float AngleInRad = Mathf.Atan2(WindManager.instance.CurrentTrueWind.y, WindManager.instance.CurrentTrueWind.x);
        WindVectorUI.transform.rotation = Quaternion.Euler(0,0,AngleInRad*Mathf.Rad2Deg);

    }

    void UpdateSailDirectionUI()
    {
        SailDirectionUI.transform.rotation = Quaternion.Euler(0,0,-BoatManager.Player.Sail.transform.localRotation.eulerAngles.y + 90 - BoatManager.Player.transform.localRotation.eulerAngles.y);
    }

    void UpdateApparentWindUI()
    {
        float AngleInRad = Mathf.Atan2(BoatManager.Player.ApparentWind.y, BoatManager.Player.ApparentWind.x);
        ApparentWindUI.transform.rotation = Quaternion.Euler(0,0,AngleInRad*Mathf.Rad2Deg);
    }
}
