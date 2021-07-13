using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

// Armsym_configurationscene
// This app... launches the app.
// Creates an identifier for the subject and sets the data.
// The subject in VR does NOT see this, only the experimenter in the computer. 

public class armsym_configurationscene : MonoBehaviour {
    public InputField Identifier, gender, age, armlength;
    public Dropdown ExperimentalModeDropdown;
    public Toggle Skin_Toggle;

    public Text ExceptionTextExists; // Comes into play if an identifier already exists, creating an exception that avoids losing data.
    public Text ExceptionTextFill;  // If user hasn't filled the data, throws exception

    Subject Person = new Subject();
    launcherclass defaultLauncher = new launcherclass();


    private Experimental_Condition Condition = new Experimental_Condition();
    private List<Experimental_Condition> ListOfConditions= new List<Experimental_Condition>();
    private List<string> NameOfConditions= new List<string>();

    private void Start()
    {

        foreach (string file in System.IO.Directory.GetFiles(Application.streamingAssetsPath+"/Experimental_conditions"))
        {  //this loops through the files for experimental conditions  Kudos for the line to https://answers.unity.com/questions/16433/get-list-of-all-files-in-a-directory.html
            if (file.Contains(".meta")) { continue;  } //Ignores .meta files
            try
            {
                Condition = JsonUtility.FromJson<Experimental_Condition>(File.ReadAllText(file)); // Loads the  person.
                ListOfConditions.Add(Condition);
                NameOfConditions.Add(Condition.Display_name);

            }

            catch (Exception e) {
                Debug.Log("Error with one of the files. ArmSym has ignored an experimental condition file. The error is: "+e);
                continue;

            }
            

        }
        ExperimentalModeDropdown.AddOptions(NameOfConditions);
    }


    public void onbuttonpress() {

        if (Identifier.text == "" || gender.text == "" || age.text == "") {
            ExceptionTextFill.gameObject.SetActive(true);
            throw new Exception("Please fill all the fields!!"); }



        if (armlength.text == "")
        {
            Person.armlength = (0.73f);

        }
        else {
            Person.armlength = float.Parse(armlength.text)/100f;
        }
        Person.identifier = Identifier.text;
        Person.age = int.Parse(age.text);
        Person.gender = gender.text;
        

        // Condition has all the information!!
        Condition = ListOfConditions[ExperimentalModeDropdown.value];
        Person.numberoftrials = Condition.numberoftrials;
        Person.trialswithquestionnaire = Condition.trialswithquestionnaire;
        Person.trialswithpause = Condition.trialswithpause;
        Person.controlmode = Condition.controlmode;
        Person.triallength = Condition.triallength;
        if (Skin_Toggle.isOn)
        {
            Person.skin_idx = 0; // human!!
        }
        else {
            Person.skin_idx = 1; // robot
        }


        WriteSubjecttotext();
        WriteSessionNametotext();





        SceneManager.LoadScene("LaboratoryScene");
    }


    void WriteSessionNametotext() {
        using (FileStream fs = new FileStream("./session.json", FileMode.Create))
        {
            defaultLauncher.identifier = Identifier.text;




            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(JsonUtility.ToJson(defaultLauncher));
                writer.Close();
                writer.Dispose();

            }

            fs.Close();
            fs.Dispose();
        }

    }

    void WriteSubjecttotext()
    { // Is called once 
        string json = JsonUtility.ToJson(Person);
        string subjdirectory = "./Subjects/" + Person.identifier;
        if (Directory.Exists(subjdirectory)) {
            ExceptionTextExists.gameObject.SetActive(true);
            throw new Exception("This identifier already exists"); } // Makes sure that the app doesnt overwrite data.

        if (!Directory.Exists(subjdirectory)) { Directory.CreateDirectory(subjdirectory); }
        string path = subjdirectory + "/identifier.json"; // Path at which the data will be saved.
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(json);
                writer.Close();
                writer.Dispose();
            }

            fs.Close();
            fs.Dispose();
        }
    }

}


// 
public class launcherclass
{

    public string identifier;

    public launcherclass()
    {

    }


}

// 
public class Subject
{
    public string identifier;
    public float armlength;
    public string gender;
    public int age;
    public int controlmode;
    public List<int> trialswithquestionnaire;
    public List<int> trialswithpause;
    public int numberoftrials;
    public int triallength;
    public int skin_idx; // it's a bool, but since users may add their own, we set it as int.


    public Subject()
    {

    }


}


public class Experimental_Condition
{
    public string Display_name;
    public int controlmode;
    public List<int> trialswithquestionnaire;
    public List<int> trialswithpause;
    public int numberoftrials;
    public int triallength;

    public Experimental_Condition()
    {

    }


}