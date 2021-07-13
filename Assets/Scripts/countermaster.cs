using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countermaster : MonoBehaviour {
    /// <summary>
    /// This is a modified version of the script:
    /// CubeCollidermaster. 
    /// It is in charge of counting how many objects has the user transfered. 
    /// 
    /// 
    /// IMPORTANT NOTES:
    /// -By definition, even if the cube falls in the floor, the intent should be counted as good. Also by definition, if the user takes two cubes at the same time, then
    /// only one is counted. Only if the cube falls back in the original comparment, the intent is discounted. This is doune in countermaster_refinement
    /// 
    /// </summary>
    /// 
    public List<string> HistoryList = new List<string>();
    public int objectcounting;
    public datamanagement TrialDataManagement; // Sends a message to save in json. 
    private float refractorytime;
    public TrialMasterScript TrialTiming;

    // Use this for initialization
    void Start () {

        objectcounting = 0;
        refractorytime = Time.time;
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (ExperimentClockUtils.trialphase==1) // Only if this happens during a trial
        {
            if (other.attachedRigidbody & other.tag == "GrippableObject") // If it's a block or graspable object...
            {

                if (Time.time - refractorytime > 0.25 & !HistoryList.Contains(other.name)) // this avoids multiple cubes entering at the same time or cubes entering twice. 
                {
                    refractorytime = Time.time;
                    objectcounting = objectcounting + 1;
                    HistoryList.Add(other.name);
                    TrialDataManagement.ArmSym_Message("BarrierCross", (float)objectcounting);
                }
            }
        }
    }
       
}
