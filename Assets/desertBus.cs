using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;

public class desertBus : MonoBehaviour
{
    public KMNeedyModule needy;
    public KMAudio Audio;

    private bool active;
    public KMSelectable leftButton;
    public KMSelectable rightButton;
    public KMSelectable startButton;
	
	public Texture[] roadPos; //Depends on which lane
	public Texture[] roadNum; //Depends on which position the lane markers are in
	public Texture[] dashTitleSpecial; //Depends on if we need to show the dashboard, title screen, or overtime screen
	
	//roadLane.material.mainTexture = roadPos[roadPosition];
	public MeshRenderer roadLane;
    public MeshRenderer laneMarker;
	public MeshRenderer dashboard;
	
	public MeshRenderer[] miles;
	public MeshRenderer[] timeNums;
	
	public MeshRenderer[] points;
	public MeshRenderer[] hours;
		
	public KMSelectable sphere;
	
	double timeOnRoad;
	
	int roadPosition; //0 = left ditch, 1 = left lane, 2 = right lane, 3 = right ditch
	bool specialPhase; //true = finished the eight-hour shift, false = on road
	
	int roadNumber;
	int score;
	int dashPos;
	bool dashGoingUp;
	
    // Use this for initialization
    void Start()
    {
		roadLane.enabled = false;
		laneMarker.enabled = false;
		removeDigits();
		removeSpecial();
		dashboard.material.mainTexture = dashTitleSpecial[1];	
        needy.OnNeedyActivation += activation;
        needy.OnNeedyDeactivation += OnNeedyDeactivation;
        needy.OnTimerExpired += OnTimerExpired;
        leftButton.OnInteract += pressedLeft;
        rightButton.OnInteract += pressedRight;
        startButton.OnInteract += pressedStart;
    }

    void FixedUpdate()
    {
        if (active)
        {
			timeOnRoad = timeOnRoad + Time.fixedDeltaTime;
			
			if ((int)(timeOnRoad * 81) % 81 >= 54)
			{
				roadNumber = 2;
			}
			else if ((int)(timeOnRoad * 81) % 81 >= 27)
			{
				roadNumber = 1;
			}			
			else
			{
				roadNumber = 0;
			}
				
			if (!specialPhase)
            //transform.Rotate(Vector3.up * needy.GetNeedyTimeRemaining());
			{
				if (timeOnRoad >= 28800) //28800
				{
					//Wow, you kept the bus going for eight hours!
					score++;
					if (score > 99)
					{
						score = 99;
					}
					specialPhase = true;				
					roadLane.enabled = false;
					laneMarker.enabled = false;
					removeDigits();
					showSpecial();
					needy.SetNeedyTimeRemaining(60.1f);
					timeOnRoad = 0;
					dashboard.material.mainTexture = dashTitleSpecial[2];
				}
				else if (needy.GetNeedyTimeRemaining() < 75.1 && roadPosition == 1)
				{
					roadPosition = 2;
					needy.SetNeedyTimeRemaining(75.1f);
					sphere.transform.localPosition = new Vector3(handlePosition(), .015f, 0);
					roadLane.material.mainTexture = roadPos[roadPosition];
					laneMarker.material.mainTexture = roadNum[((roadPosition * 3) + roadNumber)];
				}
				else if (needy.GetNeedyTimeRemaining() < 30.1 && roadPosition == 2)
				{
					GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.NeedyActivated, transform);
					roadPosition = 3;
					needy.SetNeedyTimeRemaining(30.1f);
					sphere.transform.localPosition = new Vector3(handlePosition(), .015f, 0);
					roadLane.material.mainTexture = roadPos[roadPosition];
					laneMarker.material.mainTexture = roadNum[((roadPosition * 3) + roadNumber)];
				}
				doClock();
				doMiles();
				laneMarker.material.mainTexture = roadNum[((roadPosition * 3) + roadNumber)];
			}
			else
			{
				if (timeOnRoad >= 60)
				{
					//needed to press start in 60 seconds, so you get a strike :(
					needy.OnStrike();
					active = false;
					removeSpecial();
					needy.HandlePass();
					specialPhase = false;
					timeOnRoad = 0;
				}
			}
        }

    }
    void activation()
    {
        active = true;
		roadLane.enabled = true;
		laneMarker.enabled = true;
		dashboard.enabled = true;
		showDigits();
		needy.SetNeedyTimeRemaining(120.1f);
		roadPosition = 1;
		timeOnRoad = 0;
		roadNumber = 0;				
		roadLane.material.mainTexture = roadPos[roadPosition];
		dashboard.material.mainTexture = dashTitleSpecial[0];
		laneMarker.material.mainTexture = roadNum[((roadPosition * 3) + roadNumber)];
		sphere.transform.localPosition = new Vector3(handlePosition(), .015f, 0);
    }
    protected void OnNeedyDeactivation()
    {
        active = false;
    }
    protected void OnTimerExpired()
    {
        if (active)
		{
				needy.OnStrike();
				roadLane.enabled = false;
				removeDigits();
				laneMarker.enabled = false;
				dashboard.material.mainTexture = dashTitleSpecial[1];				
				active = false;
				needy.HandlePass();
		}
        active = false;
    }
	
	void doClock()
	{
		if (!specialPhase)
		{		
			var timeString = DateTime.Now.ToString("HHmm");
			for (int pN = 0; pN < 4; pN++)
			{
				timeNums[3 - pN].GetComponentInChildren<TextMesh>().text = timeString.Substring(pN, 1);
			}
		}

	}
	
	void doMiles()
	{
		if (!specialPhase)
		{
			var xyzMiles = (int)(timeOnRoad / 8);
			var milesString = "" + xyzMiles;
			if (xyzMiles < 10)
			{
				milesString = "000" + milesString;
			}
			else if (xyzMiles < 100)
			{
				milesString = "00" + milesString;
			}
			else if (xyzMiles < 100)
			{
				milesString = "0" + milesString;
			}
			for (int pN = 0; pN < 4; pN++)
			{
				miles[3 - pN].GetComponentInChildren<TextMesh>().text = milesString.Substring(pN, 1);
			}
			//Debug.Log(xyzMiles);			
		}

	}
	
	protected bool pressedLeft()
	{
		if (active && !specialPhase)
		{
			if (roadPosition == 0)
			{
				needy.OnStrike();
				roadLane.enabled = false;
				laneMarker.enabled = false;
				removeDigits();
				dashboard.material.mainTexture = dashTitleSpecial[1];					
				active = false;
				needy.HandlePass();
			}
			else
			{
				roadPosition--;
				if (roadPosition == 0)
				{
					needy.SetNeedyTimeRemaining(30.09f);
					//left ditch
				}
				else if (roadPosition == 1)
				{
					needy.SetNeedyTimeRemaining(120.1f);
					//left lane
				}
				else if (roadPosition == 2)
				{
					needy.SetNeedyTimeRemaining(75.09f);
					//right lane
				}
				roadLane.material.mainTexture = roadPos[roadPosition];
				laneMarker.material.mainTexture = roadNum[((roadPosition * 3) + roadNumber)];
				sphere.transform.localPosition = new Vector3(handlePosition(), .015f, 0);
			}
		}

		return false;
	}

	protected bool pressedRight()
	{
		if (active && !specialPhase)
		{
			if (roadPosition == 3)
			{
				needy.OnStrike();
				removeDigits();
				roadLane.enabled = false;
				laneMarker.enabled = false;
				dashboard.material.mainTexture = dashTitleSpecial[1];		
				active = false;
				needy.HandlePass();
			}
			else
			{
				roadPosition++;
				if (roadPosition == 3)
				{
					needy.SetNeedyTimeRemaining(30.09f);
					//right ditch
				}
				else if (roadPosition == 2)
				{
					needy.SetNeedyTimeRemaining(75.09f);
					//right lane
				}
				else if (roadPosition == 1)
				{
					needy.SetNeedyTimeRemaining(120.1f);
					//left lane
				}
				roadLane.material.mainTexture = roadPos[roadPosition];
				laneMarker.material.mainTexture = roadNum[((roadPosition * 3) + roadNumber)];
				sphere.transform.localPosition = new Vector3(handlePosition(), .015f, 0);
			}
		}
		return false;
	}
	
	float handlePosition()
	{
		return -.06f + (.04f * roadPosition);
	}

	void removeSpecial()
	{
			points[0].GetComponentInChildren<TextMesh>().text = "";
			hours[0].GetComponentInChildren<TextMesh>().text = "";
			hours[1].GetComponentInChildren<TextMesh>().text = "";
	}
	void showSpecial()
	{
			hours[0].GetComponentInChildren<TextMesh>().text = "00";
			var scoreString = "000000";
			if (score < 10)
			{
				scoreString = "0000000";
			}			
			points[0].GetComponentInChildren<TextMesh>().text = scoreString + (score % 10);
			hours[1].GetComponentInChildren<TextMesh>().text = "" + (score * 8);
	}
	void removeDigits()
	{
		for (int dN = 0; dN < 4; dN++)
		{
			miles[dN].GetComponentInChildren<TextMesh>().text = "";
			timeNums[dN].GetComponentInChildren<TextMesh>().text = "";
		}
	}
	void showDigits()
	{
		for (int dN = 0; dN < 4; dN++)
		{
			//miles[dN].enabled = true;
			//timeNums[dN].enabled = true;
		}
	}

	protected bool pressedStart()
	{
		if (active && specialPhase)
		{
			
			timeOnRoad = 0;
			//get back to gameplay
			specialPhase = false;
			roadLane.enabled = true;
			laneMarker.enabled = true;
			removeSpecial();
			showDigits();
			roadPosition = 1;
			dashboard.material.mainTexture = dashTitleSpecial[0];
			roadLane.material.mainTexture = roadPos[roadPosition];
			laneMarker.material.mainTexture = roadNum[((roadPosition * 3) + roadNumber)];
			needy.SetNeedyTimeRemaining(120.1f);
			Debug.Log("Fucking start");
		}
		else
		{
			GetComponent<KMAudio>().PlaySoundAtTransform("beep", transform);
		}
		return false;
	}
}