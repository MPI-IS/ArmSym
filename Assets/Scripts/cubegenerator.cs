using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubegenerator : MonoBehaviour {

    /// <summary>
    /// This script generates the pouring box where the cubes fall.
    /// 
    /// CubeIdx is a standard cube placed on the exact origin of the workspace. It is instanciated as many times as
    /// numberofcubes states, in a random color between red, blue or green. The original cube is set inactive at the end, 
    /// meaning it dissapears. 
    /// Box gameobject is used to find out the position of the box. In theory it should work if the box is translated, 
    /// but it is not robust enough to support box rotations. 
    /// 
    /// ColliderHelper was brought in as a solution for the following problem. Unity's collision system has a bug, in which 
    /// objects which travel fast do not stop when they touch a thin collider. Colliderhelper is a cube with a box collider,
    /// which appears to be much more robust that our custom box's own mesh collider. 
    /// ColliderHelper is active at the beginning, and once all blocks fall, it is set inactive, in order for it not to intrfere
    /// with the experiment. It is set inactive exactly 0.7 seconds after pouting. 
    /// 
    /// 
    /// Cubegenerator further checks when the original compartment is running empty of cubes, thus refilling it. The number of
    /// cubes will never exceed totalcubes. 
    /// 
    /// If PracticeTable is active, then it blocks down the refill algorithm. 
    /// </summary>
    public GameObject CubeIdx, box, ColliderHelper, OriginalCompartment;
    private objectsensor OC_cm;
    private float TimeSinceRefill, TotalBlocks;
    private bool IsHelperDeattached; // Checks if colliderHelper is deatached 
    private int numberofcubes, RefillQuantity;
    public AAExperimentMasterScript ExperimentMasterScript;
    void Start () {
        RefillQuantity = 100; // Refills 50 blocks every time.

        TotalBlocks = 0; // Allows to avoi having two blocks with the same name at the same time.
        TimeSinceRefill = refill(0, RefillQuantity, CubeIdx, box, TotalBlocks);
        TotalBlocks = TotalBlocks + RefillQuantity;


        OC_cm = OriginalCompartment.GetComponent<objectsensor>();
        IsHelperDeattached = false;
    }
    void Update()
    {
        // The following loop deataches the "collider helper"
        if (!IsHelperDeattached) { // Nested, since IsHelperDeattached happens much less usually, and then avoids unnecessary evaluations.
            if (Time.time - TimeSinceRefill > 0.7) {
                ColliderHelper.SetActive(false); // Inactivates the helper, it is not necessary. 
                IsHelperDeattached = true; // Sets this boolean variable in order to have a counting.

            }

        }

        // The following loop refills RefillQuantity cubes if the number of cubes is lower than 10. 
        if (OC_cm.numattached < 10 & ExperimentMasterScript.IsPracticeTrial==false) { // Practice table is used as a proxy for practice time, in which we don't require a refill
            if (Time.time - TimeSinceRefill > 0.7){
                TimeSinceRefill=refill(0, RefillQuantity, CubeIdx, box, TotalBlocks);
                TotalBlocks = TotalBlocks + RefillQuantity;
                ColliderHelper.SetActive(true);
                IsHelperDeattached = false;
            }

        }

    }
    /////////////////////////////
    /// <summary>
    ///The refill scheme aims to imitate a filling box as stated in the paper by Mathiowetz et. al. 
    /// https://ajot.aota.org/article.aspx?articleid=1884839
    /// Ideally, blocks are aligned in a "Pouring" box on top of the box, with specific dimensions, and then fall.  .
    /// </summary>
    
    static float refill(int count, int numberofcubes, GameObject CubeIdx, GameObject box, float TotalBlocks) {
                
        CubeIdx.SetActive(true); // This is the original cube.
        for (int k = 0; k < 6; k++) // k is the amount of rows of cubes within the pouring box.
        {
            for (int j = 0; j < 6; j++) // Each row is a square grid of 6x6 cubes, denoted by i and j.
            {
                for (int i = 0; i < 6; i++)
                {


                    var newcube = Instantiate(CubeIdx, new Vector3(box.transform.position.x - 0.0635f + 0.0254f * i, box.transform.position.y + 0.6f + 0.0254f * k, box.transform.position.z - 0.1955f + 0.0254f * j), Quaternion.identity);
                    var randomnumber = Random.Range(0f, 4f);
                    // kudos to https://answers.unity.com/questions/353015/how-to-instantiate-a-prefab-and-change-its-color.html
                    // The following alleatorily paints the cube in four colours.
                    if (randomnumber < 1) { newcube.GetComponent<Renderer>().material.color = Color.yellow; }
                    if (randomnumber >= 1 & randomnumber < 2) { newcube.GetComponent<Renderer>().material.color = Color.red; }
                    if (randomnumber >= 2 & randomnumber < 3) { newcube.GetComponent<Renderer>().material.color = Color.blue; }
                    if (randomnumber >= 3) { newcube.GetComponent<Renderer>().material.color = Color.green; }
                    newcube.name = "Block" + (TotalBlocks+count).ToString(); // Each instance of the cube has an individual name.
                    // This name is impotant, it allows the hand to grasp!!

                    count = count + 1;
                    if (count == numberofcubes) { break; } // This nested loop never completes, because it breaks soon.
                }
                if (count == numberofcubes) { break; }

            }
            if (count == numberofcubes) { break; }
        }
        CubeIdx.SetActive(false); // This is the original cube.
        return Time.time;


    }

}

