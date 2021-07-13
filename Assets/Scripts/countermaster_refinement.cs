using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countermaster_refinement : MonoBehaviour
{
    /// <summary>
    /// Often it happens that we pass the separation (and thus we count a cube), but then the cube falls in the original compartment again. This script makes note of that
    /// and rests this from the global count. 
    /// </summary>
    /// 
    public bool colisionidx;
    public List<string> CubesAttached = new List<string>();
    public List<string> CubesHistory = new List<string>();
    public int numattached;
    public countermaster CounterPlate;
    public datamanagement TrialDataManagement; // Sends a message to save in json. 


    void Start()
    {
        numattached = 0;
        colisionidx = false;


    }



    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody & other.tag == "GrippableObject")
        {


            colisionidx = true;
            numattached = numattached + 1;
            CubesAttached.Add(other.name);

            if (CubesHistory.Contains(other.name))
            {
                if (CounterPlate.HistoryList.Contains(other.name))
                { // If this cube had already tresspased the partition
                    CounterPlate.HistoryList.Remove(other.name);
                    CounterPlate.objectcounting = CounterPlate.objectcounting - 1;
                    TrialDataManagement.ArmSym_Message("Cube_Fell_Back", (float)CounterPlate.objectcounting);
                }
            }
            CubesHistory.Add(other.name);
        }

    }
}