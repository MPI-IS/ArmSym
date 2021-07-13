using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The main idea of this script is to provide a boolean variable that hecks if the user is correctly located in the space.
/// </summary>


public class startingpointcollider : MonoBehaviour {
    public bool isrobotthere;
    public GameObject RobotLab;
    private void Start()
    {
        isrobotthere = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name == "Approx Shoulder center" || other.name == "Camera collider")
        {

            isrobotthere = true;
            GetComponent<MeshRenderer>().enabled=false;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Approx Shoulder center"  || other.name == "Camera collider")
        {

            isrobotthere = false;
            //GetComponent<MeshRenderer>().enabled = true;

        }
    }

    private void Update()
    {
        
    }


}
