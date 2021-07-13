using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class datamanagement : MonoBehaviour {
    /// <summary>
    /// Data management comoponent that serves to store the data from a given trial.
    /// 
    /// The class DataManagementInstance allows us to create an object called 'trialdata'. This object is the value that 
    /// is stored EVERY TRIAL on a json file, and can be run in Python or Matlab.
    /// 
    /// In order to save data, simply invoke this component and call the method ArmSym_Message, which automatically asigns a timestamp.
    /// When you call the method, you must always specify a string, and you may add an aditional float with extra information, which is saved as 0f if you don't need
    /// to add any aditional info. 
    /// </summary>
    
    public DataManagementInstance trialdata = new DataManagementInstance();


    void Start()
    {
        ArmSym_Message("Scene awake");
    }
    public void ArmSym_Message(string message, float? val_message=null) {
        trialdata.Times.Add(Time.time);
        trialdata.Message.Add(message);
        if (!val_message.HasValue)
        {
            trialdata.Val_message.Add(0f);

        }
        else {
            trialdata.Val_message.Add(val_message.Value);
        }
        
    }
}


public class DataManagementInstance
{
    /// <summary>
    /// Our Data management class has three array components. The constructor is empty because the arrays are empty. 
    /// </summary>
    public List<float> Times = new List<float>();
    public List<string> Message = new List<string>();
    public List<float> Val_message = new List<float>();



    public DataManagementInstance(){

    }


}

