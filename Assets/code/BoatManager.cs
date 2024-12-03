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

    [Header("Boat Control Settings")]
    public float SailSensitivity = 5f;
    public float RudderSensitivity = 5f;

    float currentRudderSensitivity;
    float currentSailSensitivity;

    public float controlSpeedMultiplier = 1.5f;

    [Header("Object References")]
    public GameObject Sail;
    public GameObject Centerboard;
    public GameObject Rudder;
    public GameObject Tiller;
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
        //sensitivity of control
        currentRudderSensitivity = RudderSensitivity;
        currentSailSensitivity = SailSensitivity;
    }

    // Update is called once per frame
    void Update()
    {
        //Update state data
        IncreaseControlSpeed(); //hold shift key to make controls faster
        RotateSail();
        RotateRudder();

        //calculate fuild
        CalculateApparentWind();
        CalculateApparentWaterVelocity();

        //calculate all Angle Of Attack
        SailAngleOfAttack = CalculateAngleOfAttack(Sail.transform.forward, ApparentWind, out SailUnSignedAngleOfAttack);
        CenterboardAngleOfAttack = CalculateAngleOfAttack(Centerboard.transform.forward, ApparentWaterVelocity, out CenterboardUnSignedAngleOfAttack);
        RudderAngleOfAttack = CalculateAngleOfAttack(Rudder.transform.forward, ApparentWaterVelocity, out RudderUnSignedAngleOfAttack);

        //Calculate Sail forces
        CalculateSailLiftForce();
        Debug.DrawRay(transform.position + ForceDebugOrigin, SailLiftForce*ForceDebugScale, Color.white);
        CalculateSailDragForce();
        Debug.DrawRay(transform.position + ForceDebugOrigin, SailDragForce*ForceDebugScale, Color.grey);
        Debug.DrawRay(transform.position + ForceDebugOrigin, SailLiftForce + SailDragForce, Color.green);

        //Calculate Centerboard forces
        CalculateCenterboardLiftForce();
        Debug.DrawRay(transform.position + ForceDebugOrigin, CenterboardLiftForce*ForceDebugScale, Color.white);
        CalculateCenterboardDragForce();
        Debug.DrawRay(transform.position + ForceDebugOrigin, CenterboardDragForce*ForceDebugScale, Color.grey);
        Debug.DrawRay(transform.position + ForceDebugOrigin, CenterboardLiftForce + CenterboardDragForce, Color.red);

        //Calculate Rudder forces
        CalculateRudderLiftForce();
        Debug.DrawRay(Rudder.transform.position + ForceDebugOrigin, CenterboardLiftForce*ForceDebugScale, Color.white);
        CalculateRudderDragForce();
        Debug.DrawRay(Rudder.transform.position + ForceDebugOrigin, CenterboardDragForce*ForceDebugScale, Color.grey);
        Debug.DrawRay(Rudder.transform.position + ForceDebugOrigin, RudderLiftForce + RudderDragForce, Color.blue);

        //Resultant force line
        Debug.DrawRay(transform.position + ForceDebugOrigin, SailLiftForce + SailDragForce + CenterboardLiftForce + CenterboardDragForce + RudderLiftForce + RudderDragForce, Color.yellow);
    }


    void FixedUpdate()
    {
        //Apply forces to object position
        rb.AddForceAtPosition(SailLiftForce, SailForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(SailDragForce, SailForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(CenterboardLiftForce, CenterboardForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(CenterboardDragForce, CenterboardForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(RudderLiftForce, RudderForcePosition.transform.position, ForceMode.Force);
        rb.AddForceAtPosition(RudderDragForce, RudderForcePosition.transform.position, ForceMode.Force);
    }

    float CalculateAngleOfAttack(Vector3 ChordLine, Vector2 FluidVelocity, out float UnSignedAngleOfAttack)
    {
        //Calculate magnitude of Angle Of Attack
        Vector3 FluidVelocity3D = new Vector3(FluidVelocity.x, 0, FluidVelocity.y);
        float DotProduct = Vector3.Dot(-FluidVelocity3D, ChordLine);
        float ProductOfMagnitudes = FluidVelocity3D.magnitude * ChordLine.magnitude;
        UnSignedAngleOfAttack = Mathf.Acos(DotProduct/ProductOfMagnitudes) * Mathf.Rad2Deg;

        //Decide which side the angle is on
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

    void CalculateSailLiftForce()
    {  
        //Calculate the lift direction and magnitude
        CalculateSailCL(SailUnSignedAngleOfAttack);
        float LiftMagnitude = 0.5f * WindManager.instance.AirDensity * Mathf.Pow(ApparentWind.magnitude, 2) * SailArea * SailCL;
        Vector3 ApparentWind3D = new Vector3(ApparentWind.x, 0, ApparentWind.y);
        Vector3 LiftDirection = Vector3.Cross(ApparentWind3D, Vector3.up).normalized;
        if (SailAngleOfAttack < 0)
        {
            LiftDirection = -LiftDirection;
        }

        //calculate the lift vector
        SailLiftForce = LiftMagnitude * LiftDirection;
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

    void CalculateSailDragForce()
    {
        //calculate magnitude and direction of drag
        CalculateSailCD(SailUnSignedAngleOfAttack);
        float DragMagnitude = 0.5f * WindManager.instance.AirDensity * Mathf.Pow(ApparentWind.magnitude, 2) * SailArea * SailCD;
        Vector3 ApparentWind3D = new Vector3(ApparentWind.x, 0, ApparentWind.y);
        Vector3 DragDirection = ApparentWind3D.normalized;

        //calculate the drag vector
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
        //calculate magnitude and direction of lift
        CalculateCenterboardCL(CenterboardUnSignedAngleOfAttack);
        float LiftMagnitude = 0.5f * WindManager.instance.WaterDensity * Mathf.Pow(ApparentWaterVelocity.magnitude, 2) * CenterboardArea * CenterboardCL;
        Vector3 ApparentWater3D = new Vector3(ApparentWaterVelocity.x, 0, ApparentWaterVelocity.y);
        Vector3 LiftDirection = Vector3.Cross(ApparentWater3D, Vector3.up).normalized;
        if (CenterboardAngleOfAttack < 0)
        {
            LiftDirection = -LiftDirection;
        }

        //calculate drag vector
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
        //calculate magnitude and direction of drag
        CalculateCenterboardCD_Test(CenterboardUnSignedAngleOfAttack);
        float DragMagnitude = 0.5f * WindManager.instance.WaterDensity * Mathf.Pow(ApparentWaterVelocity.magnitude, 2) * CenterboardArea * CenterboardCD;
        Vector3 ApparentCenterboard3D = new Vector3(ApparentWaterVelocity.x, 0, ApparentWaterVelocity.y);
        Vector3 DragDirection = ApparentCenterboard3D.normalized;

        //calculate drag vector
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
        //calculate magnitude and direction of lift
        CalculateRudderCL(RudderUnSignedAngleOfAttack);
        float LiftMagnitude = 0.5f * WindManager.instance.WaterDensity * Mathf.Pow(ApparentWaterVelocity.magnitude, 2) * RudderArea * RudderCL;
        Vector3 ApparentWater3D = new Vector3(ApparentWaterVelocity.x, 0, ApparentWaterVelocity.y);
        Vector3 LiftDirection = Vector3.Cross(ApparentWater3D, Vector3.up).normalized;
        if (RudderAngleOfAttack < 0)
        {
            LiftDirection = -LiftDirection;
        }

        //calculate lift vector
        RudderLiftForce = LiftMagnitude * LiftDirection;
    }

    void CalculateRudderCD_Test(float u_alpha)
    {
        if (u_alpha < 0)
        {
            RudderCD = 0;
        }
        else if (u_alpha > 0 && u_alpha <= 10)
        {
            RudderCD = 0.01f*u_alpha;
        }
        else if (u_alpha > 10 && u_alpha <= 30)
        {
            RudderCD = 0.025f*u_alpha - 0.15f;
        }
        else if (u_alpha > 30 && u_alpha <= 50)
        {
            RudderCD = 0.015f*u_alpha + 0.015f;
        }
        else if (u_alpha > 50 && u_alpha <= 60)
        {
            RudderCD = 0.01f*u_alpha + 0.4f;
        }
        else if (u_alpha > 60 && u_alpha <= 80)
        {
            RudderCD = 0.005f*u_alpha + 0.7f;
        }
        else if (u_alpha > 80 && u_alpha <= 100)
        {
            RudderCD = 1.1f;
        }
        else if (u_alpha > 100 && u_alpha <= 120)
        {
            RudderCD = -0.005f*u_alpha + 1.6f;
        }
        else if (u_alpha > 120 && u_alpha <= 140)
        {
            RudderCD = -0.01f*u_alpha + 2.2f;
        }
        else if (u_alpha > 140 && u_alpha <= 170)
        {
            RudderCD = -0.02333333f*u_alpha + 4.06666667f;
        }
        else if (u_alpha > 170 && u_alpha <= 180)
        {
            RudderCD = -0.01f*u_alpha + 1.8f;
        }
        else if (u_alpha > 180)
        {
            RudderCD = 0;
        }
    }

    void CalculateCenterboardCD_Test(float u_alpha)
    {
        if (u_alpha < 0)
        {
            CenterboardCD = 0;
        }
        else if (u_alpha > 0 && u_alpha <= 10)
        {
            CenterboardCD = 0.01f*u_alpha;
        }
        else if (u_alpha > 10 && u_alpha <= 30)
        {
            CenterboardCD = 0.025f*u_alpha - 0.15f;
        }
        else if (u_alpha > 30 && u_alpha <= 50)
        {
            CenterboardCD = 0.015f*u_alpha + 0.015f;
        }
        else if (u_alpha > 50 && u_alpha <= 60)
        {
            CenterboardCD = 0.01f*u_alpha + 0.4f;
        }
        else if (u_alpha > 60 && u_alpha <= 80)
        {
            CenterboardCD = 0.005f*u_alpha + 0.7f;
        }
        else if (u_alpha > 80 && u_alpha <= 100)
        {
            CenterboardCD = 1.1f;
        }
        else if (u_alpha > 100 && u_alpha <= 120)
        {
            CenterboardCD = -0.005f*u_alpha + 1.6f;
        }
        else if (u_alpha > 120 && u_alpha <= 140)
        {
            CenterboardCD = -0.01f*u_alpha + 2.2f;
        }
        else if (u_alpha > 140 && u_alpha <= 170)
        {
            CenterboardCD = -0.02333333f*u_alpha + 4.06666667f;
        }
        else if (u_alpha > 170 && u_alpha <= 180)
        {
            CenterboardCD = -0.01f*u_alpha + 1.8f;
        }
        else if (u_alpha > 180)
        {
            CenterboardCD = 0;
        }
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
        //calculate magnitude and direction of drag
        CalculateRudderCD_Test(RudderUnSignedAngleOfAttack);
        float DragMagnitude = 0.5f * WindManager.instance.WaterDensity * Mathf.Pow(ApparentWaterVelocity.magnitude, 2) * RudderArea * RudderCD;
        Vector3 ApparentRudder3D = new Vector3(ApparentWaterVelocity.x, 0, ApparentWaterVelocity.y);
        Vector3 DragDirection = ApparentRudder3D.normalized;

        //calculate drag vector
        RudderDragForce = DragMagnitude * DragDirection;
    }

    void IncreaseControlSpeed() 
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            currentRudderSensitivity = RudderSensitivity * controlSpeedMultiplier;
            currentSailSensitivity = SailSensitivity * controlSpeedMultiplier;
        }
        else
        {
            currentRudderSensitivity = RudderSensitivity;
            currentSailSensitivity = SailSensitivity;
        }
    }

    void RotateSail()
    {
        float rotationStep = 0.5f;
        Vector3 currentRotation = Sail.transform.localEulerAngles;
        float newYRotation = currentRotation.y;

        if (Input.GetKey(KeyCode.A))
        {
            //calculate new rotation
            newYRotation -= rotationStep * currentSailSensitivity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            //calculate new rotation
            newYRotation += rotationStep * currentSailSensitivity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            //reset rotation
            newYRotation = 0;
        }

        //adjust newYRotation to ensure it's within the -90 to 90 degree range
        newYRotation = NormalizeAngle(newYRotation);
        newYRotation = Mathf.Clamp(newYRotation, -90, 90);

        //apply the clamped rotation
        Sail.transform.localEulerAngles = new Vector3(currentRotation.x, newYRotation, currentRotation.z);

    }

    void RotateRudder()
    {
        //Rudder.transform.eulerAngles = new Vector3(0, Tiller.transform.eulerAngles.y, 0);
    }

    float NormalizeAngle(float angle)
    {
        //normalize an angle to the -180 to 180 range
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    void ApplyBuoyancyForce()
    {
        for (int i = 0; i < BuoyancyForcePositions.Length; i++)
        {
            //Calculate and apply gravity force
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
