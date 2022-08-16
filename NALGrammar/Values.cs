/*
    Author: Christian Hahm
    Created: May 13, 2022
    Purpose: Enforces Narsese grammar that == used throughout the project
*/

using UnityEngine.Assertions;

/*
    <frequency, confidence>
*/
public class EvidentialValue
{
    public float frequency;
    public float confidence;
    public string formatted_string = "";
    public EvidentialValue(float frequency=1.0f, float confidence=0.9f)
    {
        if (confidence >= 1.0f) confidence = 0.9999f;
        if(confidence <= 0.0f) confidence = 0.0001f;
        Asserts.assert(frequency >= 0.0 && frequency <= 1.0, "ERROR: Frequency " + frequency.ToString() + " must be in [0,1]");
        Asserts.assert(confidence >= 0.0 && confidence < 1.0, "ERROR: Confidence must be in (0,1)");
        this.frequency = frequency;
        this.confidence = confidence;
    }

    public string get_formatted_string() {
        if (this.formatted_string.Length == 0)
        {
            this.formatted_string = SyntaxUtils.stringValueOf(StatementSyntax.TruthValMarker)
               + this.frequency.ToString("#.##")
               + SyntaxUtils.stringValueOf(StatementSyntax.ValueSeparator)
               + this.confidence.ToString("#.##")
               + SyntaxUtils.stringValueOf(StatementSyntax.TruthValMarker);
        }
        return this.formatted_string;
    }


    public override string ToString()
    {
        return this.get_formatted_string();
    }
        
}