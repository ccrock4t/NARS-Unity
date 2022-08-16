/*
    Author: Christian Hahm
    Created: August 10, 2022
    Purpose: Script attached to NARS body. Coordinates NARS system and the body.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NARSAgent : MonoBehaviour
{
    public NARS nars;

    // Start is called before the first frame update
    void Start()
    {
        this.nars = new NARS(this);
    }

    // Update is called once per frame
    void Update()
    {
        this.nars.do_working_cycle();
    }

    public void SendInput(Sentence input_sentence)
    {
        //Debug.Log("Sending input: " + this.nars.helperFunctions.sentence_to_string(input_sentence));
        this.nars.global_buffer.PUT_NEW(input_sentence);
    }

    public void SendMotorOutput(StatementTerm operation)
    {
        string op = operation.get_predicate_term().ToString();
        Invoke(op, 0.1f);
    }

    /// <summary>
    /// motor op
    /// </summary>
    public void Operation()
    {
        // motor operation here
    }

    public int GetCurrentWorkingCycle()
    {
        return nars.current_cycle_number;
    }
}
