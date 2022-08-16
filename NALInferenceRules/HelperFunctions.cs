/*
==== ==== ==== ==== ==== ====
==== NAL Inference Rules - Helper Functions ====
==== ==== ==== ==== ==== ====

    Author: Christian Hahm
    Created: May 13, 2022
    Purpose: Helper functions to use throughout NARS
*/

using System;
using System.Collections.Generic;

public class HelperFunctions
{
    public NARS nars;
    public HelperFunctions(NARS nars)
    {
        this.nars = nars;
    }

    public string sentence_to_string(Sentence sentence)
    {
        string sentence_string = sentence.statement.ToString() + SyntaxUtils.stringValueOf(sentence.punctuation); // <sentence><punctuation>
        EvidentialValue projectedValue = this.nars.inferenceEngine.get_sentence_value_decayed(sentence);
        //event
        if(sentence.is_event()) sentence_string = sentence_string + " " + SyntaxUtils.stringValueOf(sentence.get_tense(this.nars.current_cycle_number));
        if (sentence is Question) return sentence_string;


        sentence_string = sentence_string + " " + projectedValue.ToString() + " " + SyntaxUtils.stringValueOf(StatementSyntax.ExpectationMarker) + this.nars.inferenceEngine.get_expectation(sentence);
        return sentence_string;
    }



    public (float, float) get_truthvalue_from_evidence(float wp, float w)
    {
        /*
        Input:
            wp: positive evidence w+

            w: total evidence w
        Returns:
            frequency, confidence
    */
        float f; float c;
        if (wp == 0 && w == 0)
        {
            // special case, 0/0
            f = 0;
            if (wp == w)
            {
                f = 1.0f;
            }

        }
        else
        {
            f = wp / w;
        }
        c = get_confidence_from_evidence(w);
        return (f, c);

    }

    public (float, float, float) get_evidence_fromfreqconf(float f, float c)
    {
        /*
    Input:
        f: frequency

        c: confidence
    Returns:
        w+, w, w-
*/
        float wp = this.nars.config.k * f * c / (1 - c);
        float w = this.nars.config.k * c / (1 - c);
        return (wp, w, w - wp);
    }

    public float get_confidence_from_evidence(float w)
    {
        /*
        Input:
            w: Total evidence
        Returns:
            confidence
        */
        return w / (w + this.nars.config.k);
    }


    public Sentence create_resultant_sentence_two_premise
        (Sentence j1,
        Sentence j2,
        Term result_statement,
        TruthValueFunctions.TwoPremiseTruthValueFunction truth_value_function)
    {
        /*
            Creates the resultant sentence between 2 premises, the resultant statement, && the truth function
        :param j1:
        :param j2:
        :param result_statement:
        :param truth_value_function:
        :return: sentence
        */
        result_statement = TermHelperFunctions.simplify(result_statement);

        Sentence result = null;
        Type result_type = premise_result_type(j1, j2);

        if (result_type == typeof(Judgment) || result_type == typeof(Goal))
        {
            // Judgment || Goal
            // Get Truth Value
            bool higher_order_statement = result_statement is StatementTerm && !((StatementTerm)result_statement).is_first_order();

            float f1; float c1; float f2; float c2;
            if (higher_order_statement)
            {
                (f1, c1) = (j1.value.frequency, j1.value.confidence);
                (f2, c2) = (j2.value.frequency, j2.value.confidence);
            }
            else
            {
                EvidentialValue j1_value_decayed = this.nars.inferenceEngine.get_sentence_value_decayed(j1);
                EvidentialValue j2_value_decayed = this.nars.inferenceEngine.get_sentence_value_decayed(j2);

                (f1, c1) = (j1_value_decayed.frequency, j1_value_decayed.confidence);
                (f2, c2) = (j2_value_decayed.frequency, j2_value_decayed.confidence);
            }


            EvidentialValue result_truth = truth_value_function(f1, c1, f2, c2);
            int? occurrence_time = null;

            // if the result == a first-order statement,  || a higher-order compound statement, it may need an occurrence time

            if ((j1.is_event() || j2.is_event()) && !higher_order_statement)
            {
                occurrence_time = this.nars.current_cycle_number;
            }

            if (result_type == typeof(Judgment))
            {
                result = new Judgment(result_statement, result_truth, occurrence_time);
            }
            else if (result_type == typeof(Goal))
            {
                result = new Goal(result_statement, result_truth, occurrence_time);
            }


        }
        else if (result_type == typeof(Question))
        {
            result = new Question(result_statement);
        }


        if (!result.is_event())
        {
            // merge in the parent sentences' evidential bases
            result.stamp.evidential_base.merge_sentence_evidential_base_into_this(j1);
            result.stamp.evidential_base.merge_sentence_evidential_base_into_this(j2);
        }
        else
        {
            // event evidential bases expire too quickly to track

        }

        stamp_and_print_inference_rule(result, nameof(truth_value_function), new Sentence[] { j1, j2 });

        return result;
    }

    public Sentence create_resultant_sentence_one_premise
        (Sentence j,
        Term result_statement,
        TruthValueFunctions.OnePremiseTruthValueFunction truth_value_function,
        EvidentialValue? result_truth = null)
    {
        /*
            Creates the resultant sentence for 1 premise, the resultant statement, && the truth function
            if truth function == null, uses j's truth-value
            :param j:
            :param result_statement:
            :param truth_value_function:
            :param result_truth: Optional truth result
            :return:
        */
        result_statement = TermHelperFunctions.simplify(result_statement);
        Type result_type = j.GetType();
        if (result_type == typeof(Judgment) || result_type == typeof(Goal))
        {
            // Get Truth Value
            if (result_truth == null)
            {
                if (truth_value_function == null)
                {
                    result_truth = j.value; //NALGrammar.Values.EvidentialValue(j.value.frequency,j.value.confidence)
                }
                else
                {
                    result_truth = truth_value_function(j.value.frequency, j.value.confidence);
                }
            }
        }

        Sentence result = null;
        if (result_type == typeof(Judgment))
        {
            result = new Judgment(result_statement, result_truth, j.stamp.occurrence_time);
        }
        else if (result_type == typeof(Goal))
        {
            result = new Goal(result_statement, result_truth, j.stamp.occurrence_time);
        }
        else if (result_type == typeof(Question))
        {
            result = new Question(result_statement);
        }


        if (truth_value_function == null)
        {
            stamp_and_print_inference_rule(result, nameof(truth_value_function), j.stamp.parent_premises);
        }
        else
        {
            stamp_and_print_inference_rule(result, nameof(truth_value_function), new Sentence[] { j });
        }

        return result;
    }

    public void stamp_and_print_inference_rule(Sentence sentence, string inference_rule, Sentence[] parent_sentences)
    {
        if (inference_rule == null)
        {
            sentence.stamp.derived_by = "Structural Transformation";
        }
        else
        {
            sentence.stamp.derived_by = inference_rule;
        }


        List<Sentence> parent_premises = new List<Sentence>();

        foreach (Sentence parent in parent_sentences)
        {
            parent_premises.Add(parent);
        }

        sentence.stamp.parent_premises = parent_premises.ToArray();

    }

    public Type premise_result_type(Sentence j1, Sentence j2)
    {
        /*
        Given 2 sentence premises, determine s the type of the resultant sentence
        */
        if (!(j1 is Judgment))
        {
            return j1.GetType();
        }
        else if (!(j2 is Judgment))
        {
            return j2.GetType();
        }
        else
        {
            return typeof(Judgment);
        }
    }

    public int convert_to_interval(int working_cycles)
    {
        /*
        return interval from working cycles
        */
        //round(Config.INTERVAL_SCALE*math.sqrt(working_cycles))
        return working_cycles; //round(math.log(Config.INTERVAL_SCALE * working_cycles)) + 1 #round(math.log(working_cycles)) + 1 ##round(5*math.log(0.05*(working_cycles + 9))+4)
             }

    public int convert_from_interval(int interval)
    {
        /*
        return working cycles from interval
        */
        //round((interval/Config.INTERVAL_SCALE) ** 2)
        return interval; //round(math.exp(interval) / Config.INTERVAL_SCALE) #round(math.exp(interval))  // round(math.exp((interval-4)/5)/0.05 - 9);
    }


    public int interval_weighted_average(int interval1, int interval2, float weight1, float weight2)
    {
        return (int)((interval1 * weight1 + interval2 * weight2) / (weight1 + weight2));
    }

    public int get_unit_evidence()
    {
        return 1 / (1 + this.nars.config.k);
    }
}