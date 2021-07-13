using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectsensor : MonoBehaviour {
    public bool colisionidx;
    public List<string> ObjectsAttached = new List<string>();
    public int numattached;

    /// <summary>
    /// OBJECT SENSOR
    /// This script is attached to the colliders in the fingertips, and makes notice if a block is touching the collider. 
    /// ObjectsAttached is a list of all the ids of the objects attached and numattached checks how many objects are attached.
    /// 
    /// This script is also attached to a box called "original compartment". This is part of the refill algorithm, by 
    /// checking how many Objects remain in the original compartment, and refilling it if necessary.
    /// Also  the box called "final comparment" makes us of this to establish a log. 
    /// </summary>
    /// 



    void Start () {
        numattached = 0;
        colisionidx = false;

		
	}

    

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody & other.tag == "GrippableObject")
        {


            colisionidx = true;
            numattached = numattached + 1;
            ObjectsAttached.Add(other.name);
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody & other.tag == "GrippableObject")
        {
            


            numattached = numattached - 1;
            ObjectsAttached.Remove(other.name);
            if (numattached == 0) { colisionidx = false; }
        }

    }
}
