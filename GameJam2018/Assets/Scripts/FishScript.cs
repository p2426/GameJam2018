﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour {

    public FishManager FM;
    public bool DEBUG = true;
    public int ID;
    public Vector3 Speed;
    public Vector3 MaxSpeed;
    private Vector3 _steering;
    private float turnSpeed;

    public LineRenderer LR_CoM;
    public LineRenderer LR_CoR;
    public LineRenderer LR_AvoidFish;
    public LineRenderer LR_Steering;

    public Vector3 CoM;
    int CoMCount;

    public Vector3 CoR;
    int CoRCount;

    public float AvoidFishVisionRatio = 0.2f;
    public Vector3 AvoidFish;
    public float ClosestFishRange;

    public float AvoidTankVisionRatio = 0.2f;
    public Vector3 AvoidBottomTank;
    public float ClosestBottomTankRange = 2f;


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update ()
    {
        // Reset counters and references
        // CentreOfMass
        CoM = Vector3.zero;
        CoMCount = 0;
        // Centre of Rotation
        CoR = Vector3.zero;
        CoRCount = 0;
        // Avoid 1 (Fish)
        AvoidFish = Vector3.zero;
        ClosestFishRange = 10000000; // good enough for now, tie to tank size eventually
        turnSpeed = FM.TurnRate;

        // Populates CoM, CoR and AvoidFish
        ScanFish();

        // Populates AvoidShark
        ScanSharks();

        // Determines TankEdge Avoidance
        ScanTank();

        //double start = Time.realtimeSinceStartup;

        // TO DO .... AVOID1 - Fish impact ---- DONE
        // TO DO .... AVOID2 - Nearest Shark
        // TO DO .... AVOID3 - Tank Edge
        // TO DO .... AVOID4 - Tank Bottom / Top

        // TO DO .... Apply Weighting to each
        // FIRST Pass weighting

        _steering = (CoM * FM.CoMWeight) + (CoR * FM.CoRWeight) - (AvoidFish * FM.AvoidFishWeight);
        _steering -= (AvoidBottomTank * FM.TankAvoidWeight);

        //Debug.Log("Time to Search " + (Time.realtimeSinceStartup - start));


        //Quaternion target = Quaternion.Euler(CoR.x, CoR.y, CoR.z);
        // Dampen towards the target rotation
        //transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * FM.TurnRate);
        //transform.Rotate(transform.up, FM.TurnRate * Time.deltaTime);

        //transform.rotation = Quaternion.LookRotation(_steering, Vector3.up);

        Quaternion Steer = Quaternion.Euler(_steering) * transform.rotation;
        //transform.rotation = Quaternion.Lerp(transform.rotation, Steer, Time.deltaTime * 25);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_steering, transform.up), Time.deltaTime);

        transform.Translate(transform.forward * FM.Speed * Time.deltaTime, Space.World);

        if (DEBUG) UpdateLineRenderers();

    }

    private void ScanFish()
    {
        foreach (FishScript FS in FM.Fish)
        {
            if (FS.ID != ID) // don't compare on self
            {
                float range = Vector3.Distance(transform.position, FS.transform.position);
                if (range < FM.Vision)
                {
                    if (range <= FM.Vision * AvoidFishVisionRatio)
                    {
                        if (range <= ClosestFishRange)
                        {
                            // AvoidFish
                            ClosestFishRange = range;
                            AvoidFish = FS.transform.position - transform.position;
                        }
                    }
                    else
                    {
                        // CentreOfMass
                        CoM += (FS.transform.position - transform.position);
                        CoMCount++;
                        // Centre of Rotation
                        CoR += (FS.transform.forward - transform.forward);
                        CoRCount++;
                    }
                }
            }
            if (CoMCount > 0)
            {
                CoM.Normalize();
            }
            if (CoRCount > 0)
            {
                CoR.Normalize();
            }
            AvoidFish.Normalize();
        }
    }

    private void ScanSharks()
    {

    }

    private void ScanTank()
    {
        Vector3 BottomTankPos = new Vector3(transform.position.x, 0, transform.position.z);
        float range = Vector3.Distance(transform.position, BottomTankPos);
        if (range < FM.Vision)
        {
            if (range <= FM.Vision * AvoidTankVisionRatio)
            {
                if (range <= ClosestBottomTankRange)
                {
                    AvoidBottomTank = BottomTankPos - transform.position;
                }
            }
        }

    }

    private void UpdateLineRenderers()
    {
        // CoM Orange
        LR_CoM.SetPosition(0, transform.position);
        LR_CoM.SetPosition(1, transform.position + (CoM * FM.CoMWeight));
        // CoR Green
        LR_CoR.SetPosition(0, transform.position);
        LR_CoR.SetPosition(1, transform.position + (CoR * FM.CoRWeight));
        // FishAvoid Red
        LR_AvoidFish.SetPosition(0, transform.position);
        LR_AvoidFish.SetPosition(1, transform.position - (AvoidFish * FM.AvoidFishWeight));
        // Final Steering Black
        LR_Steering.SetPosition(0, transform.position);
        LR_Steering.SetPosition(1, transform.position + _steering);
    }



}
