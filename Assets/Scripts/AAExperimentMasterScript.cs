using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;


/// <summary>
// This script is in charge of running the whole experiment.
// An experiment with ArmSym would consist of a set of trials. In order to keep track of the trials, some variables should be kept between trials.
// ArmSym reloads its main scene on each trial. In order to keep these important varialbles, we make use of the function "dont destroy on load". 
// That function keeps the AAExperimentMasterScript GameObject alive duing the reload of the trials. However, some of the variables are cleaned,
// in particular those belonging to objects. Only static variables are kept. 
// 
/// </summary>
public class AAExperimentMasterScript : MonoBehaviour {
    // These variables don't change during execution:
    public static int trial;
    public static int controlmode_idx; // 0 for prosthetic mimicking, 1 for inverse kinematics, else for user definedsss
    static public int numberoftrials;
    private static string identifier; // identifies the subject
    private static bool firsttrial = true;
    static List<int> trialswithquestionnaire;
    static List<int> trialswithpause;
    static int triallength;

    private static int skin_idx; // This integer allows for a simple way of exchanging skins between the HUMAN MESH (int=0) or the ROBOT MESH (int =1). For more information please check the getrobotmesh method.


    // These variables/function do change between trials
    public bool ReloadScene; // Self explainatory
    public bool IsPracticeTrial = false; // Self explainatory
    string trialdirectory; // The directory of the current trial
    public TrialMasterScript trialmaster; // This will develop the event process of a trial
    public PracticeMasterScript practicemaster; // If we have a practice trial, this will develop the event process
    public GameObject TrialMasterGO; // The GameObject of the string Trialmaster
    public armsym_logger Logger;

    // These variables help with data managment
    public datamanagement TrialInfo;





    public static Subject Person = new Subject(); // Subject is defined on its own string
    void Awake() // a trick by https://forum.unity.com/threads/keeping-variable-data-from-scene-to-scene.42865/
    {

        DontDestroyOnLoad(this.gameObject);     // Make this (AND ONLY THIS!) game object survive when loading the same scene all over again
        
        ReloadScene = false;

        
        if (firsttrial) {
            trial = 0;

            //// Starting saving experimental data
            identifier = JsonUtility.FromJson<Subject>(File.ReadAllText("session.json")).identifier; // Gets info from the current session
            string subjdirectory = "./Subjects/" + identifier; // Finds the folder of the current session
            string path = subjdirectory + "/identifier.json"; // Path at which the data will be saved.
            Person = JsonUtility.FromJson<Subject>(File.ReadAllText(path)); // Loads the  person.
            controlmode_idx = Person.controlmode;
            trialswithpause = Person.trialswithpause;
            trialswithquestionnaire = Person.trialswithquestionnaire;
            numberoftrials = Person.numberoftrials;
            triallength = Person.triallength;
            skin_idx = Person.skin_idx;
            //Neccesary variables
            firsttrial = false; // This is the only variable kept in the whole process
            
            
        }

        if (trial == 0){
            TrialMasterGO.GetComponent<PracticeMasterScript>().enabled = true;
            TrialMasterGO.GetComponent<TrialMasterScript>().enabled = false;
            IsPracticeTrial = true;
        }

        if (trial >= 1) {
            IsPracticeTrial = false;
            practicemaster.enabled = false;
            trialmaster.enabled = true;
        }

        if (trialswithquestionnaire.Contains(trial)) {
            trialmaster.displayquestionnaire = true;
        }

        if (!trialswithquestionnaire.Contains(trial)) 
        {
            trialmaster.displayquestionnaire = false;
        }
        if (trial == numberoftrials) { //Last trial!!
            trialmaster.flushlasttrial = true;
        
        }

        if (trial>numberoftrials){  // ENDS THE GAME
            File.Delete("session.json"); // Deletes data on the session
            Application.Quit();

        }



        if (trial==0) {
            trialdirectory = "./Subjects/" + identifier + "/PracticeTrial";
        }
        else{
            trialdirectory = "./Subjects/" + identifier + "/trial_" + trial.ToString();
            trialmaster.standardtrialtime = triallength;
        } //allocation of the folder

        Logger.init(true);
        




    }


    private void Update()
    {


        if (ReloadScene == true) {

            SaveJSONwithdata();
            

            ReloadThisScene(ref ReloadScene, ref trial);
        }


    }

    private void ReloadThisScene(ref bool ReloadScene, ref int trial) {

        Logger.writeData();
        Logger.clearData(true);

        if (trialswithpause.Contains(trial))
        {
            trial++;
            SceneManager.LoadScene("PauseMenu");
        }
        else
        {
            trial++;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    private void SaveJSONwithdata() {
        //TrialInfo.trialdata.Totalcubes = trialmaster.trialscore;
        TrialInfo.ArmSym_Message("Total_Score", trialmaster.trialscore);
        
        if (!Directory.Exists(trialdirectory)) { Directory.CreateDirectory(trialdirectory); }
        using (FileStream fs = new FileStream(trialdirectory+"/trialdata.json", FileMode.Create))
        
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(JsonUtility.ToJson(TrialInfo.trialdata));
                writer.Close();
                writer.Dispose();
            }

            fs.Close();
            fs.Dispose();

        }

    }

    public string gettrial() {

        return trial.ToString();

    }

    /// <summary>
    // The method GetRenderPieces can be accessed by any other instance in the game. What it does is that it retrieves all the Game objects that belong to the current 
    // GameObject mesh, with a certain skin chosen, and hides the meshes for any other game objects in a way that robot and human skins cannot coexist in the same runtime.
    //
    // It is important to recall that the robot will have active rigid bodies in some fingers, and that these will be kept active. We are only replacing the 3D model that is 
    // rendered and displayed.    
    /// </summary>

    public GameObject[] GetRenderPieces(int? idx = null)
    {
        if (!idx.HasValue) { // the default choice is skin_idx, which is a field that belongs to ExperimentMaster (the present class), but can be also overloaded. 
            idx = AAExperimentMasterScript.skin_idx;

        }
        Debug.Log(idx);
        GameObject[] RenderPieces;
        if (idx == 0) // 0 means that the human 3D model will be used.
        {
            RenderPieces = GameObject.FindGameObjectsWithTag("Human3DModel");
            foreach (var renderpiece in GameObject.FindGameObjectsWithTag("Robot3DModel")) { if (renderpiece.GetComponent<Renderer>()) { renderpiece.GetComponent<Renderer>().enabled = false; } }; // The pieces of the robot are dissapeared.
            foreach (var renderpiece in GameObject.FindGameObjectsWithTag("Phalange")) { if (renderpiece.GetComponent<Renderer>()) { renderpiece.GetComponent<Renderer>().enabled = false; } }; // The pieces of the finger are dissapeared.


        }

        else // means that the robot 3D model will be used.
        {
            // We have two types of robot tags, one for the robots and one for the fingers.
            // C# seems to make it difficult to concatenate arrays, so we used a trick from https://stackoverflow.com/questions/6028556/how-to-append-some-object-arrays-in-c.
            GameObject[] A = GameObject.FindGameObjectsWithTag("Robot3DModel");
            GameObject[] B = GameObject.FindGameObjectsWithTag("Phalange");

            var temporal = new List<GameObject>();
            temporal.AddRange(A);
            temporal.AddRange(B);

            RenderPieces = temporal.ToArray();
            foreach (var renderpiece in GameObject.FindGameObjectsWithTag("Human3DModel")) { if (renderpiece.GetComponent<Renderer>()) { renderpiece.GetComponent<Renderer>().enabled = false; } }; // The pieces of the finger are dissapeared.


        }

        return RenderPieces;

    }

}
