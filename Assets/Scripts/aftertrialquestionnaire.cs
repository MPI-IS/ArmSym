using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This script is attached to the embodiment+stress  formular within the game.
public class aftertrialquestionnaire : MonoBehaviour {
    public bool isquestionnairestarted; // public, set to false

    ///////
    // questionidx keeps track of current questions.
    private int questionidx;
    ///////

    // General allocations
    public Text questiontext; // the text on the question canvas
    public GameObject experimentmaster; // in order to finish a trial.
    public datamanagement TrialDataManagement; // Sends a message to save in json. 
    private float RefractoryPeriod; //If more than one second hasn't passed, disallows questionnaire to pass question. 
    public List<QuestionareEntry> AllQuestions = new List<QuestionareEntry> { };
    int NumQuestions;

    // We have two kinds of questions. One of them is a 7-degree likert scale. The other one is a slider that ranges from  "None" to "As bad as it gets", a visual alanlogue scale.

    // For likert:
    public GameObject AnswerLikert; // this is the slider from which we get the answer.
    public GameObject Likert7Question; // Cointains the part of the questionnaire dedicated to embodiment.

    // For slider 
    public GameObject SliderQuestion; //contains the part of te questionnaire dedicated to stress.
    public GameObject AnswerSlider; // This is a different slider
                                    // An example of this kind of questionnaire is  called VAS:
                                    // https://academic.oup.com/occmed/article/62/8/600/1439531
    // NOTE: if islikert is false, the question is a slider/VAS type.

    // Use this for initialization
    void Start () {
        questionidx = 0;

        /////////////////////////
        /////////////////////////
        /// Add your questions here!!
        AllQuestions.Add(new QuestionareEntry { question= "Indicate on this slide how stressed you feel right now:", islikert=false});
        AllQuestions.Add(new QuestionareEntry { question = " I felt like the robot was moving like my arm", islikert = true });
        AllQuestions.Add(new QuestionareEntry { question = "Indicate on this slide how potato you feel right now: ", islikert = false });
        AllQuestions.Add(new QuestionareEntry { question = "My interaction with ArmSym is clear and understandable", islikert = true });

        AllQuestions.Add(new QuestionareEntry { question = "I felt like if I had three arms", islikert = true });
        AllQuestions.Add(new QuestionareEntry { question = " I felt like the robot was my arm", islikert = true });
        AllQuestions.Add(new QuestionareEntry { question = " The robot started to change shape, color and appearance, and started to look like my arm", islikert = true });

        ///
        /////////////////////////
        /////////////////////////
        // Don't modify the code from here on

        NumQuestions = AllQuestions.Count;
        RefractoryPeriod = Time.time; // We initialize here this variable, which is going to be useful in the button press method.
        SetQuestion(AllQuestions[0]);
    }

    
    public void SetQuestion(QuestionareEntry Q) {
        // Every time the button is presed, it calls this method, which reloads the interface for a new question. It calls the method "set question likert" or "set question slider" depending on the tipe of the question defined when questionaire entries are raised.
        if (Q.islikert) { SetQuestionLikert(Q.question); }
        else { SetQuestionSlider(Q.question); }

    }

    public void SetQuestionLikert(string s)
    {
        AnswerLikert.GetComponent<Slider>().value = 4f;
        this.questiontext.text = s;
        SliderQuestion.SetActive(false);
        Likert7Question.SetActive(true);
    }

    public void SetQuestionSlider(string s)
    {
        AnswerSlider.GetComponent<Slider>().value = 50;
        this.questiontext.text = s;
        SliderQuestion.SetActive(true);
        Likert7Question.SetActive(false);
    }

    public void FlushAnswer(bool likert = true) {
        // Fluihes the trial to the data management system" 
        if (likert) { TrialDataManagement.ArmSym_Message("Question: " + (questionidx+1).ToString() + " " + AllQuestions[questionidx].question, (int)AnswerLikert.GetComponent<Slider>().value); }
        else { TrialDataManagement.ArmSym_Message("Question: " + (questionidx+1).ToString()+ " "+AllQuestions[questionidx].question, AnswerSlider.GetComponent<Slider>().value); }
     }

    public void finishtrial() {
        // We need to finish the current trial!!
        experimentmaster.GetComponent<AAExperimentMasterScript>().ReloadScene = true;
    }

    
    public void buttonpressed(){// Every time the "continue" button is pressed, this method runs:
        if ((Time.time - RefractoryPeriod) > 1)
        {
            FlushAnswer(AllQuestions[questionidx].islikert);
            questionidx = questionidx + 1;

            if (questionidx == NumQuestions) {
                finishtrial();
             }
            else
            {
                RefractoryPeriod = Time.time;
                SetQuestion(AllQuestions[questionidx]);


            }
            


        }



           
    }

    public class QuestionareEntry {
        // The user can later add instances of this class to the questionnaire list in the start method
        public string question;
        public bool islikert; // if its not likert, it's a slider (VAS)
    }

}
