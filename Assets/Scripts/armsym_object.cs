using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class armsym_object : MonoBehaviour {
    /// <summary>
    /// This class allows a certain GameObject to be gripped by the robot, provided that it has a rigid body component and a mesh collider component.
    /// It also allows for the object to have a semi-reallistic ilusion if the subject decides to throw it. 
    /// 
    /// This script is plug and play - we recommend you adjust manually the parameters of a rigid body  anyway.
    /// 
    /// The GameObject will not interact with a scene without a collider. See the tutorial for more information.
    /// 
    /// </summary>
    private Vector3  P0, P1;
    public Vector3 velocity;

    private void Awake() // Awake guarantees that this happens beforethe initial refill.
    {
        gameObject.tag = "GrippableObject"; // Sets the current gameonject to be of a certain tag. 
        if (gameObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>(); // Add the rigidbody.
            rb.mass = 3;
            rb.angularDrag = 0.57f;
            rb.drag = 0.35f;
            rb.useGravity = true;
            rb.isKinematic = false;


        }

    }

    private void Start()
    {
        // Velocity of block:
        velocity = Vector3.zero;
        P0 = Vector3.zero;
        P1 = Vector3.zero;




    }

    // Update is called once per frame
    void Update () {
        velocity = (P1-P0) / Time.deltaTime;
        P0 = P1;
        P1=transform.position;
    }
}
