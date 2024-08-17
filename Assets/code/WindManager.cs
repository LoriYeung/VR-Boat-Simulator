using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WindManager : MonoBehaviour
{
    [Header("Wind Override Header")]
    public bool WindOverride = false;
    public Vector2 WindOverrideVector;

    [Header("Settings")]
    public float MinimumMagnitude = 1;
    public float MaximumMagnitude = 10;
    public float MinimumWindTime = 1;
    public float MaximumWindTime = 10;

    [Header("WindVector")]
    public Vector2 CurrentTrueWind = new Vector2();
    public Vector2 NewTrueWind = new Vector2();


    [Header("Timer")]
    public float WindTimer = 0f;
    public float CurrentTime = 0f;
    public static WindManager instance; 

    [Header("Air Settings")]
    public float AirDensity = 1.2257f; 
    public float WaterDensity = 1027f; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (! WindOverride)
        {
            GenerateWindTimer();
            GenerateWind();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (! WindOverride)
        {
            UpdateTimer();
            UpdateCurrentWind();
        }
        else
        {
            CurrentTrueWind = WindOverrideVector;
        }
    }

    public void DownwindOverride()
    {
        WindOverride = true;
        WindOverrideVector = new Vector2(0, 5.5f);
    }

    public void UpwindOverride()
    {
        WindOverride = true;
        WindOverrideVector = new Vector2(0, -5.5f);
    }

    void GenerateWind()
    {
        float NewWindMagnitude = Random.Range(MinimumMagnitude, MaximumMagnitude);
        NewTrueWind.x = Random.Range(-1f, 1f);
        NewTrueWind.y = Random.Range(-1f, 0);
        NewTrueWind = NewTrueWind.normalized * NewWindMagnitude;
    
    }

    void UpdateCurrentWind()
    {
        CurrentTrueWind = Vector2.Lerp(CurrentTrueWind, NewTrueWind, 0.5f * Time.deltaTime);
    }

    void GenerateWindTimer()
    {
        WindTimer = Random.Range(MinimumWindTime, MaximumWindTime);
        CurrentTime = WindTimer;
    }

    void UpdateTimer()
    {
        if(CurrentTime > 0)
        {
            CurrentTime = CurrentTime - Time.deltaTime;
        }
        else
        {
            GenerateWindTimer();
            GenerateWind();
        }
    }

}
