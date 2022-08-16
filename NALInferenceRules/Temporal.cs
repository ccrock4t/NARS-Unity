
/*
==== ==== ==== ==== ==== ====
==== NAL Inference Rules - Truth Value Functions ====
==== ==== ==== ==== ==== ====

    Author: Christian Hahm
    Created: May 16, 2022
    Purpose: Defines the NAL temporal inference rules
            Assumes the given sentences do not have evidential overlap.
            Does combine evidential bases in the Resultant Sentence.
*/

using System.Collections.Generic;

public class TemporalRules
{

    NARS nars;

    public TemporalRules(NARS nars)
    {
        this.nars = nars;
    }

    public Sentence TemporalIntersection(Sentence j1, Sentence j2)
    {
        /*
            Temporal Intersection

            Input:
                j1: Event S <f1, c1> {tense}

                j2: Event P <f2, c2> {tense}
            Evidence:
                F_Intersection
            Returns:
                :- Event (S &/ P <f3, c3>)
                :- || Event (P &/ S <f3, c3>)
                :- || Event (S &| P <f3, c3>)
        */
        Asserts.assert(j1.is_eternal() && j2.is_eternal(), "ERROR: Temporal Intersection needs events");
        Sentence result;

        Term j1_statement_term = j1.get_statement_term();
        Term j2_statement_term = j2.get_statement_term();

        if (j1_statement_term == j2_statement_term) return null; // S && S simplifies to S, so no inference to do
                                                                 //if not (not j1_statement_term.is_op() && j2_statement_term.is_op()){ return result  // only care about operations right now

        //todo restore temporal component
        // if j1.stamp.occurrence_time == j2.stamp.occurrence_time{
        //     // j1 &| j2
        //     result_statement = CompoundTerm([j1_statement_term, j2_statement_term],
        //                                                       TermConnector.ParallelConjunction)
        // else if j1.stamp.occurrence_time < j2.stamp.occurrence_time{
        //     // j1 &/ j2
        //     result_statement = CompoundTerm([j1_statement_term, j2_statement_term],
        //                                                       TermConnector.SequentialConjunction,
        //                                                      intervals=[HelperFunctions.convert_to_interval(abs(j2.stamp.occurrence_time - j1.stamp.occurrence_time))])
        // else if j2.stamp.occurrence_time < j1.stamp.occurrence_time{
        //     // j2 &/ j1
        //     result_statement = CompoundTerm([j2_statement_term, j1_statement_term],
        //                                                       TermConnector.SequentialConjunction,
        //                                                      intervals=[HelperFunctions.convert_to_interval(abs(j2.stamp.occurrence_time - j1.stamp.occurrence_time))])
        CompoundTerm result_statement = new CompoundTerm(new List<Term> { j1_statement_term, j2_statement_term }, TermConnector.Conjunction);
        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Intersection);
    }

    public Sentence TemporalInduction(Sentence j1, Sentence j2)
    {
        /*
            Temporal Induction

            Input:
                j1: Event S <f1, c1> {tense}

                j2: Event P <f2, c2> {tense}
            Evidence:
                F_induction
            Returns:
                :- Sentence (S =|> P <f3, c3>)
                :- || Sentence (S =/> P <f3, c3>)
                :- || Sentence (P =/> S <f3, c3>)
        */
        Asserts.assert(j1.is_eternal() && j2.is_eternal(), "ERROR: Temporal Induction needs events");

        Term j1_statement_term = j1.get_statement_term();
        Term j2_statement_term = j2.get_statement_term();

        if (j1_statement_term == j2_statement_term) return null;  // S =/> S simplifies to S, so no inference to do
        if (j2_statement_term.is_op()) return null; // exclude operation consequents

        //todo restore temporal component

        // if j1.stamp.occurrence_time == j2.stamp.occurrence_time{
        //     // j1 =|> j2
        //     result_statement = StatementTerm(j1_statement_term, j2_statement_term,
        //                                                       Copula.ConcurrentImplication)
        // else if j1.stamp.occurrence_time < j2.stamp.occurrence_time{
        //     // j1 =/> j2
        //     result_statement = StatementTerm(j1_statement_term, j2_statement_term,
        //                                                       Copula.PredictiveImplication,
        //                                                       interval=HelperFunctions.convert_to_interval(abs(j2.stamp.occurrence_time - j1.stamp.occurrence_time)))
        // else if j2.stamp.occurrence_time < j1.stamp.occurrence_time{
        //     // j2 =/> j1
        //     result_statement = StatementTerm(j2_statement_term, j1_statement_term,
        //                                                       Copula.PredictiveImplication,
        //                                                       interval=HelperFunctions.convert_to_interval(abs(j2.stamp.occurrence_time - j1.stamp.occurrence_time)))

        StatementTerm result_statement = new StatementTerm(j1_statement_term, j2_statement_term, Copula.Implication);

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Induction);
    }


    public Sentence TemporalComparison(Sentence j1, Sentence j2)
    {
        /*
            Temporal Comparison

            Input:
                A: Event S <f1, c1> {tense}

                B{ Event P <f2, c2> {tense}
            Evidence:
                F_comparison
            Returns:
                :- Sentence (S <|> P <f3, c3>)
                :- || Sentence (S </> P <f3, c3>)
                :- || Sentence (P </> S <f3, c3>)
        */
        Asserts.assert(j1.is_eternal() && j2.is_eternal(), "ERROR: Temporal Comparison needs events");

        Term j1_statement_term = j1.get_statement_term();
        Term j2_statement_term = j2.get_statement_term();

        if (j1_statement_term == j2_statement_term) return null; // S </> S simplifies to S, so no inference to do

        StatementTerm result_statement;
        if (j1.stamp.occurrence_time == j2.stamp.occurrence_time)
            // <|>
            result_statement = new StatementTerm(j1_statement_term, j2_statement_term, Copula.ConcurrentEquivalence);
        else if (j1.stamp.occurrence_time < j2.stamp.occurrence_time)
        {
            // j1 </> j2
            result_statement = new StatementTerm(j1_statement_term, j2_statement_term, Copula.PredictiveEquivalence);
        }
        else// if (j2.stamp.occurrence_time < j1.stamp.occurrence_time)
        {
            // j2 </> j1
            result_statement = new StatementTerm(j2_statement_term, j1_statement_term, Copula.PredictiveEquivalence);
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1,j2,result_statement,this.nars.inferenceEngine.truthValueFunctions.F_Comparison);


    }
}