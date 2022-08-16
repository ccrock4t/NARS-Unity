/*
==== ==== ==== ==== ==== ====
==== NAL Inference Rules - Local Inference Rules ====
==== ==== ==== ==== ==== ====

    Author: Christian Hahm
    Created: May 17, 2022
    Purpose: Defines the NAL inference rules
            Assumes the given sentences do not have evidential overlap.
            Does combine evidential bases in the Resultant Sentence.
*/

using System.Collections.Generic;

public class LocalRules
{
    NARS nars;

    public LocalRules(NARS nars)
    {
        this.nars = nars;
    }


    public Sentence Revision(Sentence j1, Sentence j2)
    {
        /*
            Revision Rule

            Assumes: j1 and j2 do not have evidential overlap
            -----------------

            Revises two instances of the same statement / sentence with different truth values.

            Input:
              j1: Sentence (Statement <f1, c1>)

              j2: Sentence (Statement <f2, c2>)
            Returns:
              :- Sentence (Statement <f3, c3>)
        */
        Asserts.assert(j1.statement.get_term_string() == j2.statement.get_term_string(), "Cannot revise sentences for 2 different statements");

        Term statement = j1.statement;
        Term result_statement;
        if (statement is CompoundTerm && ((CompoundTerm)statement).connector == TermConnector.SequentialConjunction)
        {
            List<int> new_intervals = new List<int>();
            for (int i = 0; i < ((CompoundTerm)statement).intervals.Count; i++)
            {
                int j1_interval = j1.get_compound_statement_term().intervals[i];
                int j2_interval = j2.get_compound_statement_term().intervals[i];
                int new_interval = this.nars.helperFunctions.interval_weighted_average(j1_interval,
                                                          j2_interval,
                                                          j1.value.confidence,
                                                          j2.value.confidence);
                new_intervals.Add(new_interval);
            }
            result_statement = new CompoundTerm(j1.get_statement_term().subterms, ((CompoundTerm)statement).connector, new_intervals);
        }
        else
        {
            result_statement = j1.get_statement_term();
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1,
                                                                     j2,
                                                                     result_statement,
                                                                     this.nars.inferenceEngine.truthValueFunctions.F_Revision);
    }




    public Sentence Choice(Sentence j1, Sentence j2, bool only_confidence=false)
    {
        /*
             Choice Rule

             -----------------

             Choose the better answer (according to the choice rule) between 2 different sentences.
             If the statements are the same, the statement with the highest confidence is chosen.
             If they are different, the statement with the highest expectation is chosen.

             Input:
               j1: Sentence (Statement <f1, c1>)

               j2: Sentence (Statement <f2, c2>)

             Returns:
               j1 or j2, depending on which is better according to the choice rule
        */
        float decay;
        if (j1 is Goal)
        {
            decay = this.nars.config.PROJECTION_DECAY_DESIRE;
        }
        else
        {
            decay = this.nars.config.PROJECTION_DECAY_EVENT;
        }

        // Truth Value
        EvidentialValue j1_value = this.nars.inferenceEngine.get_sentence_value_decayed(j1);
        EvidentialValue j2_value = this.nars.inferenceEngine.get_sentence_value_decayed(j2);
        (float f1, float c1, float f2, float c2) = (j1_value.frequency, j1_value.confidence, j2_value.frequency, j2_value.confidence);

        Sentence best;
        // Make the choice
        if (only_confidence || j1.get_statement_term() == j2.get_statement_term())
        {
            if (c1 >= c2)
            {
                best = j1;
            }
            else
            {
                best = j2;
            }
        }
        else
        {
            float e1 = TruthValueFunctions.Expectation(f1, c1);
            float e2 = TruthValueFunctions.Expectation(f2, c2);
            if (e1 >= e2)
            {
                best = j1;
            }
            else
            {
                best = j2;
            }
        }

        return best;
    }


    public bool Decision(Goal j)
    {
        /*
             Decision Rule

             -----------------

             Make the decision to purse a desire based on its expected desirability

             Input:
               f: Desire-value frequency
               c: Desire-value confidence

             Returns:
               true or false, whether to pursue the goal
        */
        float desirability = this.nars.inferenceEngine.get_desirability(j);
        return desirability > this.nars.config.T;
    }

    public Sentence Eternalization(Sentence j)
    {
        /*
            Eternalization
            :param j:
            :return: Eternalized form of j
        */
        Sentence result;
        if (j is Judgment)
        {
            EvidentialValue result_truth = this.nars.inferenceEngine.truthValueFunctions.F_Eternalization(j.value.frequency, j.value.confidence);
            result = new Judgment(j.statement, result_truth, null);
        }
        else if (j is Goal)
        {
            EvidentialValue result_truth = this.nars.inferenceEngine.truthValueFunctions.F_Eternalization(j.value.frequency, j.value.confidence);
            result = new Goal(j.statement, result_truth, null);
        }
        else
        {
            Asserts.assert(false, "Error");
            return null;
        }

        result.stamp.evidential_base.merge_sentence_evidential_base_into_this(j);

        return result;
    }

    public Sentence Projection(Sentence j, int occurrence_time)
    {
        /*
            Projection.

            Returns a event j projected to the given occurrence time.

            :param j:
            :param occurrence_time: occurrence time to project j to
            :return: Projected form of j
        */
        float decay = this.nars.config.PROJECTION_DECAY_EVENT;
        if (j is Goal)
        {
            decay = this.nars.config.PROJECTION_DECAY_DESIRE;
        }
        EvidentialValue result_truth = this.nars.inferenceEngine.truthValueFunctions.F_Projection(j.value.frequency,
                                                    j.value.confidence,
                                                    (int)j.stamp.occurrence_time,
                                                    occurrence_time,
                                                    decay);

        Sentence result;
        if (j is Judgment)
        {
            result = new Judgment(j.statement, result_truth, occurrence_time);
        }
        else if (j is Goal)
        {
            result = new Goal(j.statement, result_truth, occurrence_time);
        }
        else
        {
            Asserts.assert(false, "error");
            return null;
        }

        result.stamp.evidential_base.merge_sentence_evidential_base_into_this(j);

        return result;
    }

    public EvidentialValue Value_Projection(Sentence j, int occurrence_time)
    {
        /*
            Projection; only returns an evidential value as opposed to an entire new sentence

            Returns j's truth value projected to the given occurrence time.

            :param j:
            :param occurrence_time{ occurrence time to project j to
            :return: project value of j
        */
        float decay = this.nars.config.PROJECTION_DECAY_EVENT;
        if (j is Goal)
        {
            decay = this.nars.config.PROJECTION_DECAY_DESIRE;
        }
        EvidentialValue result_truth = this.nars.inferenceEngine.truthValueFunctions.F_Projection(j.value.frequency,
                                                    j.value.confidence,
                                                    (int)j.stamp.occurrence_time,
                                                    occurrence_time,
                                                    decay);

        return result_truth;
    }
}