using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra; // This is necessary to use linear algebra while calculating the robot inverse kinematics analysis.
using System.IO;



/// <summary>
/// Here we handle robot gameobjects.
/// </summary>
public class armsym_robot : MonoBehaviour {
    public int controlmode; // Allows to change inverse kinematics in the scenario // 0 is direct kinematics. 1 is inverse kinematics
    public float[]  thd; // I decleare here the joint angles of the robot
    public GameObject joint1, joint2, joint3, joint4, joint5, joint6, joint7; // The joints include 
    public GameObject T1, T2, T3, T4, T5, T6, T7;
    public RobotKinematics WAM = new RobotKinematics(); //WAM is our robot! :-)

    public GameObject robolab; //This is intended to place the base of the robot on the shoulder.
    private GameObject[] robjoints;
    private GameObject[] robangles;
    float[] Angles = new float[7]; // Preallocated to avoid garbage collection

    // Variables for the go-to controller
    private Vector<float> target_pos = null;
    private Vector<float> difference = null;
    private Vector<float> step = null;
    private Vector<float> current_pos = null;
    public bool reached_target;
    private int current_step;
    private float goto_time;

    // /// // /// // /// // /// // /// //

    /// <summary>
    /// Assembly of the robot is performed in the method "start". Here, all the denavit-hartenberg transformations are effectued, and the robos is shrunken. 
    /// Afterwards, a list of methods for the controllers are written. 
    /// 
    /// </summary>

    void Start () {
        robjoints = new GameObject[] { joint1, joint2, joint3, joint4, joint5, joint6, joint7 }; //these are the static joint frames
        robangles = new GameObject[] { T1, T2, T3, T4, T5, T6, T7 }; // These are the transforms on rotations.
        thd = WAM.gethomevalue(); // This guarantees that at the beginning we are at home
        robolab.transform.localScale = WAM.ShrinkageFactor*Vector3.one; // Makes the robot to the scale of schrinkageFactor.
        /////////////////////////////////
        // The following builds our robot 
        for (int Joint = 0; Joint < WAM.ad.Length; Joint++) {
            DHtransform(robjoints[Joint], WAM.ald[Joint], WAM.ad[Joint], WAM.sd[Joint], thd[Joint]); 
        }


    }
    
    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    // General robot methods:

    private void DHtransform(GameObject Q, float al, float a, float s, float th) {

        
        // This is just a normal Haetenberg and Denavit (H-D) transformation. This happens at start: 
        Q.transform.parent.localEulerAngles = new Vector3(0, 0, th);
        Q.transform.localPosition = new Vector3(a, 0, s);
        Q.transform.localEulerAngles = new Vector3(al, 0, 0);

    }


    public float[] GetStatus(GameObject[] Q = null)
    {
        // This method findsThe CurrentRobotAngles
        if (Q == null) { Q = robangles; }

        for (int Joint = 0; Joint < 7; Joint++) { Angles[Joint] = Q[Joint].transform.localEulerAngles.z; }
        return Angles;
    }

    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    // Methods for direct and immediate control

    public void ExecuteAngle(GameObject Q, float angle) // scalar input
    {
        // This method moves a joint to a new angle.
        Q.transform.localEulerAngles = new Vector3(0, 0, angle);
    }

    public void ExecuteAngle(float[] angles, GameObject[] Q=null) // vector input
    {
        // This method moves a joint vector to new angles.
        if (Q == null){Q = robangles;}
        for (int Joint = 0; Joint < 7; Joint++){Q[Joint].transform.localEulerAngles = new Vector3(0, 0, angles[Joint]);}
    }

    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    // Methods for the go-to controller:

    public void SetTarget(float[] kinemang, float total_time = 3) {
        // Sets a rarget for the robot's go-to controller. 
        // Kinemang is a kinemating angle in JOINT SPACE, you need to solve the inverse kinematics before applying any input.
        target_pos = Vector<float>.Build.DenseOfArray(WAM.kinemang);
        current_pos = Vector<float>.Build.DenseOfArray(GetStatus());
        
        difference = target_pos - current_pos;
        for (int Joint = 0; Joint < 7; Joint++)
        {
            if (difference[Joint] > 180){
                difference[Joint] = difference[Joint] - 360;
            }

            if (difference[Joint] < -180){
                difference[Joint] = difference[Joint] + 360;
            }
        }
        goto_time = total_time;
        step = difference / (goto_time*90f); //reach target in 3 seconds
        reached_target = false;
        current_step = 0;
    }


    public void ReachForTarget() { 
        // Makes a step towards the target. If called without a target, does not do anything
        current_pos = Vector<float>.Build.DenseOfArray(GetStatus());
        if (difference != null && target_pos != null)
        {
            ExecuteAngle((current_pos + step).ToArray());
            current_step = current_step + 1;
            if (current_step >= goto_time*90f) {
                this.reached_target = true;
            }
        }
        else
        {
            this.reached_target = true;
        }
    }

    public void AwayFromTarget()
    {
        // Makes a step away from  the target. If called without a target, does not do anything
        current_pos = Vector<float>.Build.DenseOfArray(GetStatus());
        if (difference != null && target_pos != null)
        {
            if (current_step > 0) {
                ExecuteAngle((current_pos - step).ToArray());
                current_step = current_step - 1;
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    // Methods for the speed controller:

    public void ExecuteSpeed(GameObject Q, float speed) // scalar input
    {
        // This method moves a joint with a certain speed;
        Q.transform.Rotate(Vector3.forward, 20f*speed*Time.deltaTime);
    }

    public void ExecuteSpeed(GameObject[] Q, float[] speeds) // vector input
    {
        // This method moves a joint vector to new angles.
        for (int Joint = 0; Joint < 7; Joint++)
        {
            Q[Joint].transform.Rotate(Vector3.forward, 20f * speeds[Joint] * Time.deltaTime);
        }
    }
}



///////////////////////////////////////////////////////////////////////////     ///////////////////////////////////////////////////////////////////////////
//
//                                                                   End of the class 
// 
///////////////////////////////////////////////////////////////////////////     ///////////////////////////////////////////////////////////////////////////




/// <summary>
/// From here we  have a conversions toolbox wit deg2rad and rad2 deg functions. 
/// This is a multi-class toolbox with just two functions.
/// </summary>
/// 

public class conversions
{ // Method overloading, variables for internal use. This is a small toolbox with functions that might be helpful.
    public static double rad2deg(double b) { return b * 180 / Math.PI; }
    public static double deg2rad(double b) { return b * Math.PI / 180; }
    public static float[] rad2deg(float[] B) {
        for (int j = 0; j < B.Length; j++) { B[j] = B[j] * 180 / (float)Math.PI; }
        return B;
    }
    public static float[] deg2rad(float[] B)
    {
        for (int j = 0; j < B.Length; j++) { B[j] = B[j] * (float)Math.PI / 180; }
        return B;
    }
    public static float rad2deg(float b) { return b * 180 / (float)Math.PI; }
    public static float deg2rad(float b) { return b * (float)Math.PI / 180; }
    public static void rad2deg(ref float[] B)
    {
        for (int j = 0; j < B.Length; j++) { B[j] = B[j] * 180 / (float)Math.PI; }
    }
    public static void deg2rad(ref float[] B)
    {
        for (int j = 0; j < B.Length; j++) { B[j] = B[j] * (float)Math.PI / 180; }
    }

}



/// <summary>
/// From here we write some code for the inverse Kinematics of the robot
/// </summary>

public class RobotKinematics
{
    public float[] ald, ad, sd; // Denavit-Hartenberg parameters of the robot;
    public static float[] home_thd;
    public static Subject Person = new Subject(); 

    public float ShrinkageFactor; // This variable controls the scale of the robot chassis, maintaining the 1-1 movement between controller and arm.
    private Vector<float> Ptool_7;
    public float x, y, z, phi; // D
    public float r, azi, el, l1, l2;
    public float[] kinemang; // Output of the kinematic analysis

    // The following are the variables from the inverse analysis by order of execution. 
    Matrix<float> RROB_VR, R7_F;
    Vector<float> S7_F, A78_F;
    Vector<float> Ptool_F, P7_F, PW_F;


    Matrix<float> TripleProductMat;


    public RobotKinematics()
    {
        // Robot's Hartenberg-Denavit (H-D) parameters:
        ald = new float[] { -(float)Math.PI / 2, (float)Math.PI / 2, -(float)Math.PI / 2, (float)Math.PI / 2, -(float)Math.PI / 2, (float)Math.PI / 2, 0 };
        ald = conversions.rad2deg(ald);
        ad = new float[] { 0, 0, 0.045f, -0.045f, 0, 0, 0 };
        sd = new float[] { 0, 0, 0.55f, 0, 0.3f, 0, 0.06f };

        // Get the user shrinkage factor from configuration:

        string identifier = JsonUtility.FromJson<Subject>(File.ReadAllText("session.json")).identifier;
        string subjdirectory = "Subjects/" + identifier;
        if (!Directory.Exists(subjdirectory) || !File.Exists(subjdirectory + "/identifier.json")) { ShrinkageFactor = (0.73f); } // 2:3 } 

        if(Directory.Exists(subjdirectory) && File.Exists(subjdirectory + "/identifier.json"))
        {
            string jsonidentifier = File.ReadAllText(subjdirectory + "/identifier.json");
            Person = JsonUtility.FromJson<Subject>(jsonidentifier);
            ShrinkageFactor = Person.armlength;

        };

        ShrinkageFactor = ShrinkageFactor / 1.113f; // We pass from cm to percentage, which is the one we need to actually shrink the robot




        // At home we start with: 
        kinemang = gethomevalue();
        Ptool_7 = Vector<float>.Build.DenseOfArray(new float[] { 0, 0, ShrinkageFactor*0.203f }); //  length from shaft to tip of the finger http://support.barrett.com/chrome/site/bhand/280/figure40.png
                                                                                 // 70+58+34+41.5= 203.5 mm =0.203m   
        // Then we have orientation and position at the beginning:
        phi = 0.5f * (float)Math.PI;
        x = 0.429f; // meters
        y = -0.610f; // meters
        z = 0.122f; // meters


        // The following are the variables from the inverse analysis by order of execution. 
        RROB_VR = RoboEngine.Ry(0.523599f).SubMatrix(0, 3, 0, 3); // Transformation between robot and VR system, exactly 30° in Y
        //                         radians, = 15°.
        R7_F = Matrix<float>.Build.DenseIdentity(3);
        Ptool_F = Vector<float>.Build.DenseOfArray(new float[] { x, y, z });
        P7_F = Vector<float>.Build.DenseOfArray(new float[] { 1, 0, 0 });

        PW_F = Vector<float>.Build.DenseOfArray(new float[] { 1,0,0 });

        // It is easy to find out the angle between two vectors, which is used to find the last angle (kinemamg[6]) in the
        // inverse analysisis. Nevertheless, the traditional formula finds the magnitude of this angle, but not its direction.
        // By using a technique derived from the following links, we find out the direction of this angle by finding the determinant
        // of a matrix TripleProductMat on every timeframe.
        // https://math.stackexchange.com/questions/1139218/angle-between-two-3d-vectors-measured-in-a-specific-direction
        // https://en.wikipedia.org/wiki/Triple_product
        TripleProductMat = Matrix<float>.Build.DenseIdentity(3);
    }


    // Inverse kinematics. For more info, please refer to the master's thesis manuscript.

    public void InverseKinematics(float x, float y, float z, float phi, Vector3 forward, Vector3 up)

    {


        Ptool_F[0] = x; Ptool_F[1] = y; Ptool_F[2] = z;


        // The robot is rotated -30 degrees in Y with respect to the VR system.
        // This is done in order to avoid the robot crashing with its first joint, giving it more "antropomorphic" looks.
        // An example of a multi-arm WAM was in youtube on 05/2018: https://www.youtube.com/watch?v=RKYZpxxOc10
        // In the example the shoulders are rotated towards the workspace around 45°. Here, the arm is rotated 30°.
        // Ptool_F (position of the tool), and the orientations are counter-rotated, so the user doesn't feel the rotation.
        // In other words, the kinematic model of the robot "assumes" this rotation. 
        // The orientation of the tool, S7_F, and the axial rotation of the tool, A78_F, also have to be rotated Y. 
        // The matrix RROB_VR helps with this, it is just a 3x3 matrix with a rotation in y.        
        // Further, they are Vector3, so they we convert them into Mathnet.numerics vectors

        Ptool_F = RROB_VR.Multiply(Ptool_F); 
        S7_F = RROB_VR.Multiply(RoboEngine.V3toVf(forward)); // Vector that establish the orientation of the end effector
        A78_F = RROB_VR.Multiply(RoboEngine.V3toVf(up)); // Vector that establish the rotation of the end effector.

        // ShrinkageFactor literally miniaturizes the robot. This mean that there is no longer a 1:1 correspondence between the controller and the end effector position.
        // By making the workspace 1/SF times, we ensure that this 1:1 correspondence is met again.
        Ptool_F = (1 / ShrinkageFactor) * Ptool_F; 


        // The rotation of the tool with respect to the base of the robot can be expressed with the following rotation matrix:
        R7_F.SetColumn(2, S7_F); // Matrix
        R7_F.SetColumn(0, A78_F);
        R7_F.SetColumn(1, RoboEngine.CrossProduct(S7_F, A78_F)); // R7_F is a rotation 3x3 matrix.

        // Since the tool doesn't rotate, the position of the robot end effector depends its length and the rotation matrix.
        P7_F = Ptool_F - R7_F.Multiply(Ptool_7);
        PW_F = P7_F - sd[6] * S7_F; // THis line is: Pw_F=P7_F-s7*S7_F;


        r = RoboEngine.c2s_r(PW_F[0], PW_F[1], PW_F[2]);
        el = RoboEngine.c2s_el(PW_F[2], r);
        azi = (float)Math.Atan2(PW_F[1], PW_F[0]); // Does not need a special function
        l1 = RoboEngine.l2norm(sd[2], ad[2]);
        l2 = RoboEngine.l2norm(sd[4], ad[3]);
        if (r > l1 + l2) { return; } // If the robot cannot reach a specific position, returns the same angles as the last time it found itself.


        double a2 = Math.Acos(((r * r) + (l2 * l2) - (l1 * l1)) / (2 * r * l2));
        double a1 = Math.Asin(l2 * Math.Sin(a2) / l1);
        float dc = (float)(l1 * Math.Cos(a1));
        float rc = (float)(l1 * Math.Sin(a1));

        float th_u = (float)(a2 + Math.Atan(Math.Abs(ad[3]) / sd[4]));
        float th_l = (float)(a1 + Math.Atan(ad[2] / sd[2]));
        float rlj = (float)(rc + (Math.Abs(ad[2]) * Math.Cos(-th_l)));
        float dlj = (float)(dc + (Math.Abs(ad[2]) * Math.Sin(-th_l)));
        Matrix<float> TW_f = RoboEngine.Rz(azi).Multiply(RoboEngine.Ry(((float)Math.PI / 2) - el)).Multiply(RoboEngine.Rz(phi));
        Vector<float> PA_0 = TW_f.SubMatrix(0, 3, 0, 3).Multiply(Vector<float>.Build.DenseOfArray(new float[] { rc, 0, dc }));
        Vector<float> Plj_0 = TW_f.SubMatrix(0, 3, 0, 3).Multiply(Vector<float>.Build.DenseOfArray(new float[] { rlj, 0, dlj }));

        kinemang[0] = (float)Math.Atan2(Plj_0[1], Plj_0[0]);
        kinemang[1] = (float)Math.Acos(Plj_0[2] / sd[2]);

        Vector<float> PA_lj = (1 / ad[2]) * (Plj_0.SubVector(0, 2) - PA_0.SubVector(0, 2));
        Matrix<float> tr_t3_temp = Matrix<float>.Build.DenseOfArray(new float[,] {
            {(float)(Math.Cos(kinemang[0])*Math.Cos(kinemang[1])),-(float)Math.Sin(kinemang[0])},
            {(float)(Math.Sin(kinemang[0])*Math.Cos(kinemang[1])),(float)Math.Cos(kinemang[0])}});


        Vector<float> tr_t3 = tr_t3_temp.Inverse() * PA_lj;

        kinemang[2] = (float)(Math.Atan2(tr_t3[1], tr_t3[0]) + Math.PI);
        kinemang[3] = th_u + th_l;

        var T4_F = Matrix<float>.Build.DenseIdentity(4); // Initialization;
        for (int i = 0; i < 4; i++) { T4_F = T4_F.Multiply(RoboEngine.DHmatrix(ad[i], conversions.deg2rad(ald[i]), sd[i], kinemang[i])); }
        Vector<float> Ptool_4 = T4_F.SubMatrix(0, 3, 0, 3).Transpose().Multiply(Ptool_F - PW_F);
        
        kinemang[5] = (float)(Math.Atan2(Ptool_4[2], RoboEngine.l2norm(Ptool_4[0], Ptool_4[1])) - (Math.PI / 2));


        kinemang[4] = (float)(Math.Atan2(Ptool_4[1], Ptool_4[0]) + Math.PI); 

        
        var R6_F = Matrix<float>.Build.DenseIdentity(4).Multiply(T4_F); // Initialization;
        
        for (int i = 4; i < 6; i++) { R6_F = R6_F.Multiply(RoboEngine.DHmatrix(ad[i], conversions.deg2rad(ald[i]), sd[i], kinemang[i])); }
        var R6_FVec = R6_F.SubMatrix(0, 3, 0, 3).Column(0);
        
        TripleProductMat.SetColumn(0, A78_F);  TripleProductMat.SetColumn(1, R6_FVec);  TripleProductMat.SetColumn(2, S7_F);

        kinemang[6] = (float)Math.Acos(R6_FVec.DotProduct(A78_F) / (RoboEngine.l2norm(R6_FVec) * RoboEngine.l2norm(A78_F)));

        if (TripleProductMat.Determinant() >= 0) { kinemang[6] = -kinemang[6]; };


        conversions.rad2deg(ref kinemang); 


    }

    public float[] gethomevalue() //Delivers home
    {
        return new float[] { -90f,110f,0f,110f,11f,-55f,0f}; 
    }


    static class RoboEngine //toolbox with some useful functions for handling data in robotics. 
                            // Not to be confused with "conversions", which is a multi-class toolbox for 
                            // switching between radians and degrees. 
    {
        public static float c2s_r(float x, float y, float z) {return l2norm(x, y, z); }
        public static float c2s_el(float z, float r) { return (float)(((0.5) * Math.PI) - Math.Acos(z / r)); }
        public static float l2norm(float a, float b) { return (float)(Math.Sqrt((a*a) + (b*b))); } // gets L2norm for 2D vectors
        public static float l2norm(float a, float b, float c) { return (float)(Math.Sqrt((a * a) + (b * b) + (c * c))); } // gets L2norm for 3D vectors
        public static float l2norm(Vector<float> myvec) { // Only if vector is 3D
            float a = myvec[0];
            float b = myvec[1];
            float c = myvec[2];
            return (float)(Math.Sqrt((a * a) + (b * b) + (c * c))); } // gets L2norm for 3D vectors

        public static Vector<float> V3toVf(Vector3 V3){ //Converts Vector3 into Vector from Mathnet numerics
            Vector<float> c = Vector<float>.Build.Dense(3);
            c[0] = V3.x;
            c[1] = V3.y;
            c[2] = V3.z;

            return c;
        }

        public static Vector<float> CrossProduct(Vector<float> a, Vector<float> b) { //Mathematical cross product
            Vector<float> c = Vector<float>.Build.Dense(3);
            c[0] = a[1] * b[2] - a[2] * b[1];
            c[1] = - a[0] * b[2] + a[2] * b[0];
            c[2] = a[0] * b[1] - a[1] * b[0];

            return c;
        }

        public static Matrix<float> DHmatrix(float a, float al, float s, float t) //This returns the transformation matrix of a Denavit-Hartenberg transformation.
                                                                                  //This should not be confused with the function DHtransformation in our Unity class, which transforms a game object with a DH transformation.
        {
            float ct = (float)Math.Cos(t);
            float st = (float)Math.Sin(t);
            float cal = (float)Math.Cos(al);
            float sal = (float)Math.Sin(al);

            return Matrix<float>.Build.DenseOfArray(new float[,] {
            {ct, -st*cal, st*sal, a*ct},
            {st, ct*cal, -ct*sal, a*st},
            {0,sal, cal, s},
            {0,0,0,1}});
        }


        // The following are rotation matrices.
        // The input is an angle q, in RADIANS!!!
        // The output is a matrix in the format of mathnet.numerics, that can be used to applyany transformation. 
        public static Matrix<float> Rx(float q)
        {
            return Matrix<float>.Build.DenseOfArray(new float[,] {
            {1,0,0,0},
            {0,(float)Math.Cos(q),-(float)Math.Sin(q),0},
            {0,(float)Math.Sin(q),(float)Math.Cos(q),0},
            {0,0,0,1}});
        }
        public static Matrix<float> Ry(float q)
        {
            return Matrix<float>.Build.DenseOfArray(new float[,] {
            {(float)Math.Cos(q),0,(float)Math.Sin(q),0},
            {0,1,0,0},
            {-(float)Math.Sin(q),0,(float)Math.Cos(q),0},
            {0,0,0,1}});
        }
        public static Matrix<float> Rz(float q)
        {
            return Matrix<float>.Build.DenseOfArray(new float[,] {
            {(float)Math.Cos(q),-(float)Math.Sin(q),0,0},
            {(float)Math.Sin(q), (float)Math.Cos(q),0,0},
            {0,0,1,0},
            {0,0,0,1}});
        }


        // The last matrix is a pure translation: 
        public static Matrix<float> Trf(float x, float y, float z)
        {
            return Matrix<float>.Build.DenseOfArray(new float[,] {
            {1,0,0,x},
            {0,1,0,y},
            {0,0,1,z},
            {0,0,0,1}});
        }

    }

}
