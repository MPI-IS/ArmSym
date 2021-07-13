using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;



// MPIDataLogger
//
// Logs data in preallocated memory to avoid hitches due to filesystem I/O 
//
// The code in the class 'ArmSym_logger' was originally written by:
// Joachim Tesch, Max Planck Institute for Biological Cybernetics.
// 
// It was further modified with permission for its use in ArmSys by: Samuel Bustamante - Max Planck Institute for Intelligent Systems.
//


public class armsym_logger : MonoBehaviour
{
    private bool IsFirstLineWritten = false;
    public AAExperimentMasterScript ExperimentData;
    string identifier;
    string trial;
    int idx;

    private struct RobotData //Structures are better than classes in allocations. 
    {
        public int idx;
        public float timeSender;
        public float th1;
        public float th2;
        public float th3;
        public float th4;
        public float th5;
        public float th6;
        public float th7;
        public float xposcontroller;
        public float yposcontroller;
        public float zposcontroller;
        public float analogx;
        public float analogy;
        public float xposbaserobot;
        public float yposbaserobot;
        public float zposbaserobot;

    }

    private string logFileName, dirname;
    private int maxRecordingTimeS = 3 * 90 * 60; // 3min at 90Hz // prealocates the memory.
    public int fps = 90;

    private int _maxDataRows = 0;
    private int _currentDataRow = 0;
    private RobotData[] _dataTable = null;

    private StreamWriter _fileWriter = null;


    void Start()
    {

        IsFirstLineWritten = false;
        idx = 0;
    }

    public void init(bool append = false)
    {
        identifier = JsonUtility.FromJson<Subject>(File.ReadAllText("session.json")).identifier; // Gets info from the current session
        trial = ExperimentData.gettrial();
        _maxDataRows = maxRecordingTimeS * fps;

        // Preallocate data table memory
        _dataTable = new RobotData[_maxDataRows];
        clearData(true);

        if (_fileWriter != null)
            _fileWriter.Close();

        if (!ExperimentData.IsPracticeTrial)
        {
            dirname = "./Subjects/" + identifier + "/trial_" + trial + "/";
        }
        else
        {
            dirname = "./Subjects/" + identifier + "/PracticeTrial/";
        }
        logFileName = dirname + "Joint_Data.csv";
        if (!Directory.Exists(dirname)) { Directory.CreateDirectory(dirname); }
        Debug.Log("MPI: Writing data log to: " + logFileName);

        _fileWriter = new StreamWriter(logFileName, append);

        string rowData = "Idx\t"
                        + "Time\t"
                        + "th1\t"
                        + "th2\t"
                        + "th3\t"
                        + "th4\t"
                        + "th5\t"
                        + "th6\t"
                        + "th7\t"
                        + "x_controller_in_space\t"
                        + "y_controller_in_space\t"
                        + "z_controller_in_space\t"
                        + "x_trackpad\t"
                        + "y_trackpad\t"
                        + "x_baserobot_in_space\t"
                        + "y_baserobot_in_space\t"
                        + "z_baserobot_in_space";

        if (!IsFirstLineWritten)
        { // Avoids first row to be written multiple times
            _fileWriter.WriteLine(rowData);
            IsFirstLineWritten = true;
        }

    }

    public void addData(Vector3 controllerposition, Vector3 baserobotposition, Vector2 AnalgousController, float[] kinemang)
    //	public void addData(Vector3 pos, Quaternion rot, Vector3 posFiltered, Quaternion rotFiltered)
    {
        if (_currentDataRow >= (_maxDataRows - 1))
        {
            Debug.LogWarning("Exceeding maximum data logger memory size. Force data dump to filesystem");
            writeData();
            clearData();
        }
        RobotData data = _dataTable[_currentDataRow];
        data.idx = this.idx;
        this.idx = this.idx + 1;

        data.timeSender = Time.time;
        data.th1 = kinemang[0];
        data.th2 = kinemang[1];
        data.th3 = kinemang[2];
        data.th4 = kinemang[3];
        data.th5 = kinemang[4];
        data.th6 = kinemang[5];
        data.th7 = kinemang[6];
        data.xposcontroller = controllerposition.x;
        data.yposcontroller = controllerposition.y;
        data.zposcontroller = controllerposition.z;
        data.analogx = AnalgousController.x;
        data.analogy = AnalgousController.y;
        data.xposbaserobot = baserobotposition.x;
        data.yposbaserobot = baserobotposition.y;
        data.zposbaserobot = baserobotposition.z;

        _dataTable[_currentDataRow] = data;

        _currentDataRow++;
    }

    public void writeData()
    {

        for (int i = 0; i < _currentDataRow; i++)
        {
            RobotData data = _dataTable[i];
            string rowData = "" + data.idx + "\t"
                                + data.timeSender + "\t"
                                + data.th1 + "\t"
                                + data.th2 + "\t"
                                + data.th3 + "\t"
                                + data.th4 + "\t"
                                + data.th5 + "\t"
                                + data.th6 + "\t"
                                + data.th7 + "\t"
                                + data.xposcontroller + "\t"
                                + data.yposcontroller + "\t"
                                + data.zposcontroller + "\t"
                                + data.analogx + "\t"
                                + data.analogy + "\t"
                                + data.xposbaserobot + "\t"
                                + data.yposbaserobot + "\t"
                                + data.zposbaserobot;
            //     Debug.Log(rowData);
            _fileWriter.WriteLine(rowData);
        }
        _fileWriter.Flush();
    }

    public void clearData(bool all = false)
    {
        if (all == true)
            _currentDataRow = _maxDataRows;

        for (int i = 0; i < _currentDataRow; i++)
        {
            RobotData data = _dataTable[i];
            data.idx = 0;
            data.timeSender = 0.0f;
            data.th1 = 0.0f;
            data.th2 = 0.0f;
            data.th3 = 0.0f;
            data.th4 = 0.0f;
            data.th5 = 0.0f;
            data.th6 = 0.0f;
            data.th7 = 0.0f;
            data.xposcontroller = 0.0f;
            data.yposcontroller = 0.0f;
            data.zposcontroller = 0.0f;
            data.analogx = 0.0f;
            data.analogy = 0.0f;
            data.xposbaserobot = 0.0f;
            data.yposbaserobot = 0.0f;
            data.zposbaserobot = 0.0f;

            _dataTable[i] = data;
        }

        _currentDataRow = 0;
    }

    void OnDisable()
    {
        if (_fileWriter != null)
        {
            _fileWriter.Close();
            _fileWriter = null;
        }
    }

}
