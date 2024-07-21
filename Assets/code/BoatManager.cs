using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class BoatManager : MonoBehaviour
{
    [Header("Component References")]
    public Rigidbody rb;

    [Header("Object References")]
    public GameObject Sail;
    public GameObject Centerboard;
    public GameObject Rudder;
    public GameObject SailForcePosition;
    public GameObject CenterboardForcePosition;
    public GameObject RudderForcePosition;
    public GameObject[] BuoyancyForcePositions;


    [Header("Boat State Data")]
    public float SailAngleOfAttack;
    public float CenterboardAngleOfAttack;
    public float RudderAngleOfAttack;
    public float SailUnSignedAngleOfAttack;
    public float CenterboardUnSignedAngleOfAttack;
    public float RudderUnSignedAngleOfAttack;

    public Vector2 ApparentWind;
    public Vector2 ApparentWaterVelocity;

    [Header("Boat Forces")]
    public Vector3 SailLiftForce;
    public Vector3 SailDragForce;
    public Vector3 CenterboardLiftForce;
    public Vector3 CenterboardDragForce;
    public Vector3 RudderLiftForce;
    public Vector3 RudderDragForce;


    [Header("Sail Data")]
    public float SailArea = 7.06f; //In Meter Squared
    public float SailWeight = 59f; //In kg
    public float SailCL;
    public float SailCD;

    [Header("Centerboard Data")]
    public float CenterboardArea = 1f; //In Meter Squared
    public float CenterboardCL;
    public float CenterboardCD;

    [Header("Rudder Data")]
    public float RudderArea;
    public float RudderCL;
    public float RudderCD;

    [Header("Buoyancy Settings")]
    public float DepthBeforeSubmerged = 1;
    public float DisplacementAmount = 3;

    [Header("Player Setting")]
    public bool isPlayer = false;
    public static BoatManager Player;

    [Header("Debugging")]
    public Vector3 ForceDebugOrigin = new Vector3(0, 5, 0);
    public float ForceDebugScale = 0.2f;






    void Awake()
    {
        if (isPlayer == true && Player == null)
            Player = this;
    }



    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Update state data
        RotateSail();
        RotateRudder();
        CalculateApparentWind();
        CalculateApparentWaterVelocity();
        SailAngleOfAttack = CalculateAngleOfAttack(Sail.transform.forward, ApparentWind, out SailUnSignedAngleOfAttack);
        CenterboardAngleOfAttack = CalculateAngleOfAttack(Centerboard.transform.forward, ApparentWaterVelocity, out CenterboardUnSignedAngleOfAttack);
        RudderAngleOfAttack = CalculateAngleOfAttack(Rudder.transform.forward, ApparentWaterVelocity, out RudderUnSignedAngleOfAttack);
        //Calculate Sail lift forcej54
        CalculateSailLiftForce();
        Debug.DrawRay(transform.position + ForceDebugOrigin, SailLiftForce*ForceDebugScale, Color.white);
        //Calculate Sail Drag force
        CalculateSailDragForce();
        Debug.DrawRay(transform.position + ForceDebugOrigin, SailDragForce*ForceDebugScale, Color.grey);
        //Calculate Centerboard force
        CalculateCenterboardLiftForce();
        Debug.DrawRay(transform.position + ForceDebugOrigin, CenterboardLiftForce*ForceDebugScale, Color.white);
        //Calculate Centerboard force
        CalculateCenterboardDragForce();
        Debug.DrawRay(transform.position + ForceDebugOrigin, CenterboardDragForce*ForceDebugScale, Color.grey);
        //Calculate Rudder force
        CalculateRudderLiftForce();
        Debug.DrawRay(Rudder.transform.position + ForceDebugOrigin, CenterboardLiftForce*ForceDebugScale, Color.white);
        //Calculate Rudder force
        CalculateRudderDragForce();
        Debug.DrawRay(Rudder.transform.position + ForceDebugOrigin, CenterboardDragForce*ForceDebugScale, Color.grey);

        Debug.DrawRay(transform.position + ForceDebugOrigin, SailLiftForce + SailDragForce, Color.green);
        Debug.DrawRay(transform.position + ForceDebugOrigin, CenterboardLiftForce + CenterboardDragForce, Color.red);
        Debug.DrawRay(Rudder.transform.position + ForceDebugOrigin, RudderLiftForce + RudderDragForce, new Color(1, 0.5f, 0.025f));
        Debug.DrawRay(transform.position + ForceDebugOrigin, SailLiftForce + SailDragForce + CenterboardLiftForce + CenterboardDragForce + RudderLiftForce + RudderDragForce, Color.yellow);
    }


    void FixedUpdate()
    {
        //Debug.Log(WindManager.instance.CurrentWindForce);
        rb.AddForceAtPosition(SailLiftForce, SailForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(SailDragForce, SailForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(CenterboardLiftForce, CenterboardForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(CenterboardDragForce, CenterboardForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(RudderLiftForce, RudderForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(RudderDragForce, RudderForcePosition.transform.position, ForceMode.Force);
        ApplyBuoyancyForce();
    }

    float CalculateAngleOfAttack(Vector3 ChordLine, Vector2 FluidVelocity, out float UnSignedAngleOfAttack)
    {
        Vector3 FluidVelocity3D = new Vector3(FluidVelocity.x, 0, FluidVelocity.y);
        float DotProduct = Vector3.Dot(-FluidVelocity3D, ChordLine);
        float ProductOfMagnitudes = FluidVelocity3D.magnitude * ChordLine.magnitude;
        UnSignedAngleOfAttack = Mathf.Acos(DotProduct/ProductOfMagnitudes) * Mathf.Rad2Deg;

        Vector3 CrossProduct = Vector3.Cross(-FluidVelocity3D, ChordLine);
        if (CrossProduct.y < 0)
        {
            return -UnSignedAngleOfAttack;
        }
        else 
        {
            return UnSignedAngleOfAttack;
        }
    }

    void CalculateSailCL(float u_alpha)
    {
        if (u_alpha <=0)
        {
            SailCL = 0;
        }
        else if (u_alpha > 0 && u_alpha <= 26.677)
        {
            SailCL = 0.000001408f*Mathf.Pow(u_alpha, 2) + 0.03747f*u_alpha;
        }
        else if (u_alpha > 26.677 && u_alpha <= 39.897)
        {
            SailCL = -0.003787f*Mathf.Pow(u_alpha, 2) + 0.2726f*u_alpha - 3.5765f;
        }
        else if (u_alpha > 39.897 && u_alpha <= 77.517)
        {
            SailCL = -0.01538f*u_alpha + 1.885f;
        }
        else if (u_alpha > 77.517 && u_alpha <= 129.95)
        {
            SailCL = - 0.02273f*u_alpha + 2.4548f;
        }
        else if (u_alpha > 129.95 && u_alpha <= 166.663)
        {
            SailCL = 0.0002557f*Mathf.Pow(u_alpha, 2) - 0.09404f*u_alpha + 7.4035f;
        }
        else if (u_alpha > 166.663 && u_alpha <= 180)
        {
            SailCL = 0.0093746f*Mathf.Pow(u_alpha, 2) - 3.16235f*u_alpha + 265.48677f;
        }
        else
        {
            SailCL = 0;
        } 
    }

    void CalculateSailCD(float u_alpha)
    {
        if (u_alpha <=0)
        {
            SailCD = 0;
        }
        else if (u_alpha >= 0 && u_alpha <= 60.01145)
        {
            SailCD = 0.0001458f*Mathf.Pow(u_alpha, 2) + 0.005417f*u_alpha + 0.05f;
        }
        else if (u_alpha > 60.01145 && u_alpha <= 159.392)
        {
            SailCD = -0.0001255f*Mathf.Pow(u_alpha, 2) + 0.02924f*u_alpha - 0.4026f;
        }
        else if (u_alpha > 159.492 && u_alpha < 180)
        {
            SailCD = -0.0007273f*Mathf.Pow(u_alpha, 2) + 0.1945f*u_alpha - 11.4545f;
        }
        else
        {
            SailCD = 0;
        } 
    }

    void CalculateSailLiftForce()
    {  
        CalculateSailCL(SailUnSignedAngleOfAttack);
        float LiftMagnitude = 0.5f * WindManager.instance.AirDensity * Mathf.Pow(ApparentWind.magnitude, 2) * SailArea * SailCL;
        Vector3 ApparentWind3D = new Vector3(ApparentWind.x, 0, ApparentWind.y);
        Vector3 LiftDirection = Vector3.Cross(ApparentWind3D, Vector3.up).normalized;
        if (SailAngleOfAttack < 0)
        {
            LiftDirection = -LiftDirection;
        }
        SailLiftForce = LiftMagnitude * LiftDirection;
    }

    void CalculateSailDragForce()
    {
        CalculateSailCD(SailUnSignedAngleOfAttack);
        float DragMagnitude = 0.5f * WindManager.instance.AirDensity * Mathf.Pow(ApparentWind.magnitude, 2) * SailArea * SailCD;
        Vector3 ApparentWind3D = new Vector3(ApparentWind.x, 0, ApparentWind.y);
        Vector3 DragDirection = ApparentWind3D.normalized;
        SailDragForce = DragMagnitude * DragDirection;
    }

    void CalculateApparentWind()
    {
        Vector2 BoatVelocity2D = new Vector2(rb.velocity.x, rb.velocity.z);
        ApparentWind = WindManager.instance.CurrentTrueWind - BoatVelocity2D;
    }

    void CalculateApparentWaterVelocity()
    {
        Vector2 BoatVelocity2D = new Vector2(rb.velocity.x, rb.velocity.z);
        ApparentWaterVelocity = -BoatVelocity2D;
    }

    void CalculateCenterboardCL(float u_alpha)
    {
        if (u_alpha < 0)
        {
            CenterboardCL = 0;
        }
        else if (u_alpha <= 15)
        {
            CenterboardCL = 0.7f * Mathf.Sin(1/30f * Mathf.PI * u_alpha);
        }
        else if (u_alpha > 15 && u_alpha <= 40)
        {
            CenterboardCL = 0.1f * Mathf.Sin(1/25f * Mathf.PI * (u_alpha - 5/2f)) + 0.6f;
        }
        else if (u_alpha > 40 && u_alpha <= 90)
        {
            CenterboardCL = -1/12500000f * Mathf.Pow((u_alpha - 40f), 4) + 0.5f;
        }
        else if (u_alpha > 90 && u_alpha <= 140)
        {
            CenterboardCL = -0.7f * Mathf.Sin(1/30f * Mathf.PI * (180 - u_alpha));
        }
        else if (u_alpha > 140 && u_alpha <= 165)
        {
            CenterboardCL = -0.1f * Mathf.Sin(1/25f * Mathf.PI * ((180 - u_alpha) - 5/2f)) - 0.6f;
        }
        else if (u_alpha > 165 && u_alpha <= 180)
        {
            CenterboardCL = 1/12500000f * Mathf.Pow(((180 - u_alpha) - 40f), 4) - 0.5f;
        }
        else
        {
            u_alpha = 0;
        }
    }

    void CalculateCenterboardLiftForce()
    {  
        CalculateCenterboardCL(CenterboardUnSignedAngleOfAttack);
        float LiftMagnitude = 0.5f * WindManager.instance.WaterDensity * Mathf.Pow(ApparentWaterVelocity.magnitude, 2) * CenterboardArea * CenterboardCL;
        Vector3 ApparentWater3D = new Vector3(ApparentWaterVelocity.x, 0, ApparentWaterVelocity.y);
        Vector3 LiftDirection = Vector3.Cross(ApparentWater3D, Vector3.up).normalized;
        if (CenterboardAngleOfAttack < 0)
        {
            LiftDirection = -LiftDirection;
        }
        CenterboardLiftForce = LiftMagnitude * LiftDirection;
    }

    void CalculateCenterboardCD(float u_alpha)
    {
        if (u_alpha < 0)
        {
            CenterboardCD = 0;
        }
        else if (u_alpha > 0 && u_alpha <= 28)
        {
            CenterboardCD = 0.002435f * Mathf.Pow(u_alpha, 2) + 0.01175f * u_alpha;
        }
        else if (u_alpha > 28 && u_alpha <= 53.3333)
        {
            CenterboardCD = 0.01631f * u_alpha + 0.06332f;
        }
        else if (u_alpha > 53.3333 && u_alpha <= 126.6667)
        {
            CenterboardCD = -0.000124f * Mathf.Pow(u_alpha, 2) + 0.02232f * u_alpha + 0.09567f;
        }
        else if (u_alpha > 126.6667 && u_alpha <= 150)
        {
            CenterboardCD = -0.016f * u_alpha + 2.96f;
        }
        else if (u_alpha > 150 && u_alpha <= 180)
        {
            CenterboardCD = 0.0003667f * Mathf.Pow(u_alpha, 2) - 0.1397f * u_alpha + 13.26f;
        }
        else
        {
            CenterboardCD = 0;
        }
    }

    void CalculateCenterboardDragForce()
    {
        CalculateCenterboardCD(CenterboardUnSignedAngleOfAttack);
        float DragMagnitude = 0.5f * WindManager.instance.WaterDensity * Mathf.Pow(ApparentWaterVelocity.magnitude, 2) * CenterboardArea * CenterboardCD;
        Vector3 ApparentCenterboard3D = new Vector3(ApparentWaterVelocity.x, 0, ApparentWaterVelocity.y);
        Vector3 DragDirection = ApparentCenterboard3D.normalized;
        CenterboardDragForce = DragMagnitude * DragDirection;
    }

    void CalculateRudderCL(float u_alpha)
    {
        if (u_alpha < 0)
        {
            RudderCL = 0;
        }
        else if (u_alpha <= 15)
        {
            RudderCL = 0.7f * Mathf.Sin(1/30f * Mathf.PI * u_alpha);
        }
        else if (u_alpha > 15 && u_alpha <= 40)
        {
            RudderCL = 0.1f * Mathf.Sin(1/25f * Mathf.PI * (u_alpha - 5/2f)) + 0.6f;
        }
        else if (u_alpha > 40 && u_alpha <= 90)
        {
            RudderCL = -1/12500000f * Mathf.Pow((u_alpha - 40f), 4) + 0.5f;
        }
        else if (u_alpha > 90 && u_alpha <= 140)
        {
            RudderCL = -0.7f * Mathf.Sin(1/30f * Mathf.PI * (180 - u_alpha));
        }
        else if (u_alpha > 140 && u_alpha <= 165)
        {
            RudderCL = -0.1f * Mathf.Sin(1/25f * Mathf.PI * ((180 - u_alpha) - 5/2f)) - 0.6f;
        }
        else if (u_alpha > 165 && u_alpha <= 180)
        {
            RudderCL = 1/12500000f * Mathf.Pow(((180 - u_alpha) - 40f), 4) - 0.5f;
        }
        else
        {
            u_alpha = 0;
        }
    }

    void CalculateRudderLiftForce()
    {  
        CalculateRudderCL(RudderUnSignedAngleOfAttack);
        float LiftMagnitude = 0.5f * WindManager.instance.WaterDensity * Mathf.Pow(ApparentWaterVelocity.magnitude, 2) * RudderArea * RudderCL;
        Vector3 ApparentWater3D = new Vector3(ApparentWaterVelocity.x, 0, ApparentWaterVelocity.y);
        Vector3 LiftDirection = Vector3.Cross(ApparentWater3D, Vector3.up).normalized;
        if (RudderAngleOfAttack < 0)
        {
            LiftDirection = -LiftDirection;
        }
        RudderLiftForce = LiftMagnitude * LiftDirection;
    }

    void CalculateRudderCD(float u_alpha)
    {
        if (u_alpha < 0)
        {
            RudderCD = 0;
        }
        else if (u_alpha > 0 && u_alpha <= 28)
        {
            RudderCD = 0.002435f * Mathf.Pow(u_alpha, 2) + 0.01175f * u_alpha;
        }
        else if (u_alpha > 28 && u_alpha <= 53.3333)
        {
            RudderCD = 0.01631f * u_alpha + 0.06332f;
        }
        else if (u_alpha > 53.3333 && u_alpha <= 126.6667)
        {
            RudderCD = -0.000124f * Mathf.Pow(u_alpha, 2) + 0.02232f * u_alpha + 0.09567f;
        }
        else if (u_alpha > 126.6667 && u_alpha <= 150)
        {
            RudderCD = -0.016f * u_alpha + 2.96f;
        }
        else if (u_alpha > 150 && u_alpha <= 180)
        {
            RudderCD = 0.0003667f * Mathf.Pow(u_alpha, 2) - 0.1397f * u_alpha + 13.26f;
        }
        else
        {
            RudderCD = 0;
        }
    }

    void CalculateRudderDragForce()
    {
        CalculateRudderCD(RudderUnSignedAngleOfAttack);
        float DragMagnitude = 0.5f * WindManager.instance.WaterDensity * Mathf.Pow(ApparentWaterVelocity.magnitude, 2) * RudderArea * RudderCD;
        Vector3 ApparentRudder3D = new Vector3(ApparentWaterVelocity.x, 0, ApparentWaterVelocity.y);
        Vector3 DragDirection = ApparentRudder3D.normalized;
        RudderDragForce = DragMagnitude * DragDirection;
    }

    void RotateSail()
    {
        float rotationStep = 0.5f;
        Vector3 currentRotation = Sail.transform.localEulerAngles;
        float newYRotation = currentRotation.y;

        if (Input.GetKey(KeyCode.A))
        {
            // Calculate potential new rotation
            newYRotation -= rotationStep;
        }

        if (Input.GetKey(KeyCode.D))
        {
            // Calculate potential new rotation
            newYRotation += rotationStep;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            // Reset rotation
            newYRotation = 0;
        }

        // Adjust newYRotation to ensure it's within the -90 to 90 degree range
        newYRotation = NormalizeAngle(newYRotation);
        newYRotation = Mathf.Clamp(newYRotation, -90, 90);

        // Apply the clamped rotation
        Sail.transform.localEulerAngles = new Vector3(currentRotation.x, newYRotation, currentRotation.z);

    }

    void RotateRudder()
    {
        float rotationStep = 0.5f;
        Vector3 currentRotation = Rudder.transform.localEulerAngles;
        float newYRotation = currentRotation.y;

        if (Input.GetKey(KeyCode.Q))
        {
            // Calculate potential new rotation
            newYRotation -= rotationStep;
        }

        if (Input.GetKey(KeyCode.E))
        {
            // Calculate potential new rotation
            newYRotation += rotationStep;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            // Reset rotation
            newYRotation = 0;
        }

        // Adjust newYRotation to ensure it's within the -90 to 90 degree range
        newYRotation = NormalizeAngle(newYRotation);
        newYRotation = Mathf.Clamp(newYRotation, -90, 90);

        // Apply the clamped rotation
        Rudder.transform.localEulerAngles = new Vector3(currentRotation.x, newYRotation, currentRotation.z);

    }

    float NormalizeAngle(float angle)
    {
        // Normalize an angle to the -180 to 180 range
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }


    void ApplyBuoyancyForce()
    {
        for (int i = 0; i < BuoyancyForcePositions.Length; i++) //i = i + 1
        {
            Vector3 GravityForce = Physics.gravity / BuoyancyForcePositions.Length;
            rb.AddForceAtPosition(GravityForce, BuoyancyForcePositions[i].transform.position, ForceMode.Acceleration);
            Debug.DrawRay(BuoyancyForcePositions[i].transform.position, GravityForce, new Color(1, 0.5f, 0, 1));
            if (BuoyancyForcePositions[i].transform.position.y < 0)
            {
                float DisplacementMultiplier = Mathf.Clamp01(-transform.position.y / DepthBeforeSubmerged) * DisplacementAmount;
                Vector3 BuoyancyForce = new Vector3(0, Mathf.Abs(Physics.gravity.y) * DisplacementMultiplier, 0);
                rb.AddForceAtPosition(BuoyancyForce, BuoyancyForcePositions[i].transform.position, ForceMode.Acceleration);
                Debug.DrawRay(BuoyancyForcePositions[i].transform.position, BuoyancyForce, Color.cyan);
            }
        }
    }



    
}
