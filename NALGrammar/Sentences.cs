/*
    Author: Christian Hahm
    Created: May 13, 2022
    Purpose: Enforces Narsese grammar that is used throughout the project
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class Sentence
{
    /*
        sentence::= <statement><punctuation> <tense> %<value>%
    */

    public Term statement;
    public Punctuation punctuation;
    public Stamp stamp;
    public EvidentialValue value;
    public float eternal_expectation;

    //metadata
    public bool needs_to_be_answered_in_output;
    public bool is_from_input;

    public Sentence(Term statement, EvidentialValue value, Punctuation punctuation, int? occurrence_time = null)
    {
        /*

        :param statement:
        :param value: Pass as a tuple for array sentences (overall_truth, list_of_element_truth_values)
        :param punctuation:
        :param occurrence_time:
        */
        Asserts.assert_punctuation(punctuation);
        Asserts.assert_valid_statement(statement);

        this.statement = statement;
        this.punctuation = punctuation;
        this.stamp = new Stamp(this, occurrence_time);
        this.value = value;  // truth-value (for Judgment) || desire-value (for Goal) || null (for Question)

        if (this.punctuation != Punctuation.Question)
        {
            this.eternal_expectation = TruthValueFunctions.Expectation(this.value.frequency, this.value.confidence);
        }
    }

    public override string ToString()
    {
        Asserts.assert(false, "Use helper functions to get string for sentence");
        return "";
    }

    public override int GetHashCode()
    {
        /*
            A Sentence is identified by its ID

            : return: Sentence ID
        */
        return this.stamp.id;
    }

    public bool is_event()
    {
        return this.stamp.occurrence_time != null;
    }

    /// <summary>
    /// Gets the tense of this sentence compared to the given cycle
    /// </summary>
    /// <param name="cycle"></param>
    /// <returns></returns>
    public Tense get_tense(int cycle)
    {
        return this.stamp.get_tense(cycle);
    }

    public bool is_eternal()
    {
        return this.stamp.is_eternal;
    }


    public static Sentence new_sentence_from_string(string sentence_string, int current_cycle_number)
    {
        /*
            :param sentence_string - String of NAL syntax <term copula term>punctuation %frequency;confidence%

            :returns Sentence parsed from sentence_string
        */
        // Find statement start && statement end
        int start_idx = sentence_string.IndexOf(SyntaxUtils.stringValueOf(StatementSyntax.Start));
        Asserts.assert(start_idx != -1, "Statement start character " + SyntaxUtils.stringValueOf(StatementSyntax.Start) + " not found.");

        int end_idx = sentence_string.LastIndexOf(SyntaxUtils.stringValueOf(StatementSyntax.End));
        Asserts.assert(end_idx != -1, "Statement end character " + SyntaxUtils.stringValueOf(StatementSyntax.End) + " not found.");

        // Find sentence punctuation
        int punctuation_idx = end_idx + 1;
        Asserts.assert(punctuation_idx < sentence_string.Length, "No punctuation found.");
        string punctuation_str = Char.ToString(sentence_string[punctuation_idx]);
        Punctuation punctuation = PunctuationMethods.get_punctuation_from_string(punctuation_str);
        Asserts.assert(punctuation != null, punctuation_str + " == not punctuation.");

        // Find Truth Value, if it exists
        int start_truth_val_idx = sentence_string.IndexOf(SyntaxUtils.stringValueOf(StatementSyntax.TruthValMarker), punctuation_idx);
        int middle_truth_val_idx = sentence_string.IndexOf(SyntaxUtils.stringValueOf(StatementSyntax.ValueSeparator), punctuation_idx);
        int end_truth_val_idx = sentence_string.LastIndexOf(SyntaxUtils.stringValueOf(StatementSyntax.TruthValMarker), punctuation_idx);

        bool truth_value_found = !(start_truth_val_idx == -1 || end_truth_val_idx == -1 || start_truth_val_idx == end_truth_val_idx);
        float? freq = null;
        float? conf = null;
        if (truth_value_found)
        {
            // Parse truth value from string
            freq = float.Parse(sentence_string[(start_truth_val_idx + 1)..middle_truth_val_idx]);
            conf = float.Parse(sentence_string[(middle_truth_val_idx + 1)..end_truth_val_idx]);
        }

        // create the statement
        string statement_string = sentence_string[start_idx..(end_idx + 1)];
        Term statement = TermHelperFunctions.simplify(Term.from_string(statement_string));


        // Find Tense, if it exists
        // otherwise mark it as eternal
        Tense tense = Tense.Eternal;
        string[] tenses = Enum.GetNames(typeof(Tense));
        foreach (string t in tenses)
        {
            if (t != SyntaxUtils.stringValueOf(Tense.Eternal))
            {
                int tense_idx = sentence_string.IndexOf(t);
                if (tense_idx != -1)
                {   // found a tense
                    tense = (Tense)SyntaxUtils.enumValueOf(sentence_string[tense_idx..(tense_idx + t.Length)], typeof(Tense));
                    break;
                }
            }
        }


        Sentence sentence = null;
        // make sentence
        if (punctuation == Punctuation.Judgment)
        {
            EvidentialValue value;
            if (freq != null)
            {
                value = new EvidentialValue((float)freq, (float)conf);
            }
            else
            {
                value = new EvidentialValue();
            }
            sentence = new Judgment(statement, value);
        }

        else if (punctuation == Punctuation.Question)
        {
            sentence = new Question(statement);
        }
        else if (punctuation == Punctuation.Goal)
        {
            EvidentialValue value;
            if (freq != null)
            {
                value = new EvidentialValue((float)freq, (float)conf);
            }
            else
            {
                value = new EvidentialValue();
            }
            sentence = new Goal(statement, value);
        }
        else
        {
            Asserts.assert(false, "Error: No Punctuation!");
        }



        if (tense == Tense.Present)
        {
            // Mark present tense event as happening right now!
            sentence.stamp.occurrence_time = current_cycle_number;
        }

        return sentence;
    }

    public StatementTerm get_statement_term()
    {
        return (StatementTerm)this.statement;
    }

    public CompoundTerm get_compound_statement_term()
    {
        return (CompoundTerm)this.statement;
    }

}



public class Judgment : Sentence
{
    /*
        judgment ::= <statement>. %<truth-value>%
    */

    public Judgment(Term statement, EvidentialValue value, int? occurrence_time = null) : base(statement, value, Punctuation.Judgment, occurrence_time) { }

}


public class Question : Sentence
{
    /*
        question ::= <statement>? %<truth-value>%
    */

    public Question(Term statement) : base(statement, null, Punctuation.Question)
    {

    }
}


public class Goal : Sentence
{
    /*
        goal ::= <statement>! %<desire-value>%
    */
    bool executed;

    public Goal(Term statement, EvidentialValue value, int? occurrence_time = null) : base(statement, value, Punctuation.Goal, occurrence_time)
    {
        this.executed = false;
    }

}



public class Stamp
{
    /*
        Defines the metadata of a sentence, including
        when it was created, its occurrence time (when is its truth value valid),
        evidential base, etc.
    */
    public static int NEXT_STAMP_ID;

    public bool is_eternal; 
    public int id;
    public int? occurrence_time;
    public Sentence sentence;
    public EvidentialBase evidential_base;
    public string derived_by;
    public Sentence[] parent_premises;
    public bool from_one_premise_inference;

    public Stamp(Sentence this_sentence, int? occurrence_time = null)
    {
        this.id = Stamp.NEXT_STAMP_ID++;
        this.occurrence_time = occurrence_time;
        this.sentence = this_sentence;
        this.evidential_base = new EvidentialBase(this_sentence);
        this.derived_by = null; // none if input task
        this.parent_premises = new Sentence[0];
        this.from_one_premise_inference = false; // == this sentence derived from one-premise inference?
        this.is_eternal = (occurrence_time == null);

    }

    public Tense get_tense(int cycle)
    {
        if (this.occurrence_time == null) return Tense.Eternal;

        if (this.occurrence_time < cycle)
        {
            return Tense.Past;
        }
        else if (this.occurrence_time == cycle)
        {
            return Tense.Present;
        }
        else
        {
            return Tense.Future;
        }
    }
}


public class EvidentialBase : IEnumerable<Sentence>
{
    /*
        Stores history of how the sentence was derived
    */
    Sentence sentence;
    List<Sentence> evidential_base;
    public EvidentialBase(Sentence this_sentence)
    {
        /*
        :param id: Sentence ID
        */
        this.sentence = this_sentence;
        this.evidential_base = new List<Sentence> { this_sentence };  // array of sentences
    }
    public IEnumerator<Sentence> GetEnumerator()
    {
        foreach (var item in this.evidential_base)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)evidential_base).GetEnumerator();
    }

    public bool Contains(Sentence j)
    {
        return this.evidential_base.Contains(j);
    }

    public void merge_sentence_evidential_base_into_this(Sentence sentence)
    {
        /*
            Merge a Sentence's evidential base into this.
            This function assumes the base to merge does not have evidential overlap with this base
            #todo figure out good way to store evidential bases such that older evidence == purged on overflow
        */
        foreach (Sentence e_sentence in sentence.stamp.evidential_base)
        {
            this.evidential_base.Add(e_sentence);
        }


        while (this.evidential_base.Count > NARSConfig.MAX_EVIDENTIAL_BASE_LENGTH)
        {
            this.evidential_base.RemoveAt(0);
        }

    }


    public bool has_evidential_overlap(EvidentialBase other_base)
    {
        /*
            Check does other base has overlapping evidence with this?
            O(M + N)
            https://stackoverflow.com/questions/3170055/test-if-lists-share-any-items-in-python
        */
        if (this.sentence.is_event()) return false;
        return this.evidential_base.Intersect(other_base.evidential_base).Any();
    }


    public static bool may_interact(Sentence j1, Sentence j2)
    {
        /*
            2 Sentences may interact if:
                // Neither is "null"
                // They are not the same Sentence
                // They have not previously interacted
                // One is not in the other's evidential base
                // They do not have overlapping evidential base
        :param j1:
        :param j2:
        :return: Are the sentence allowed to interact for inference
        */
        if (j1 == null || j2 == null) return false;
        if (j1.stamp.id == j2.stamp.id) return false;
        if (j2.stamp.evidential_base.Contains(j1)) return false;
        if (j1.stamp.evidential_base.Contains(j2)) return false;
        if (j1.stamp.evidential_base.has_evidential_overlap(j2.stamp.evidential_base)) return false;
        return true;
    }


}



