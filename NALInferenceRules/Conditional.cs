/*
==== ==== ==== ==== ==== ====
==== NAL Inference Rules - Conditional Syllogistic Inference Rules ====
==== ==== ==== ==== ==== ====

    Author: Christian Hahm
    Created: May 16, 2022
    Purpose: Defines the NAL inference rules
            Assumes the given sentences do not have evidential overlap.
            Does combine evidential bases in the Resultant Sentence.
*/

using System.Collections.Generic;
using System.Linq;

public class ConditionalRules
{
    NARS nars;

    public ConditionalRules(NARS nars)
    {
        this.nars = nars;
    }


    public Sentence ConditionalAnalogy(Sentence j1, Sentence j2)
    {
        /*
            Conditional Analogy

            Input:
                j1: Statement (S) <f1, c1> {tense}

                j2: Equivalence Statement (S <=> P)  <f2, c2>
            Evidence:
                F_analogy
            Returns:
                :- Sentence (P <f3, c3>)
        */
        Asserts.assert_sentence_equivalence(j2);

        // Statement
        Term result_statement;
        if (j1.get_statement_term() == j2.get_statement_term().get_subject_term())
        {
            result_statement = j2.get_statement_term().get_predicate_term();
        }
        else if (j1.get_statement_term() == j2.get_statement_term().get_predicate_term())
        {
            result_statement = j2.get_statement_term().get_subject_term();
        }
        else
        {
            Asserts.assert(false, "Error: Invalid inputs to Conditional Analogy: " + j1.ToString() + " and " + j2.ToString());
            return null;
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1,
                                                                     j2,
                                                                     result_statement,
                                                                     this.nars.inferenceEngine.truthValueFunctions.F_Analogy);
    }


    public Sentence ConditionalJudgmentDeduction(Sentence j1, Sentence j2)
    {
        /*
            Conditional Judgment Deduction

            Input:
                j1: Implication Statement (S ==> P) <f2, c2>

                j2: Statement (S) <f1, c1> {tense} (E ==> S)
            Evidence:
                F_deduction
            Returns:
                :- P. :|: <f3, c3> (E ==> P)
        */
        Asserts.assert_sentence_forward_implication(j1);
        Asserts.assert(j2.get_statement_term() == j1.get_statement_term().get_subject_term(), "Error: Invalid inputs to Conditional Judgment Deduction{ "
                                                                + j1.ToString()
                                                                + " and "
                                                                + j2.ToString());
        StatementTerm result_statement = (StatementTerm)j1.get_statement_term().get_predicate_term();  // P

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1,
                                                                     j2,
                                                                     result_statement,
                                                                     this.nars.inferenceEngine.truthValueFunctions.F_Deduction);
    }


    public Sentence ConditionalJudgmentAbduction(Sentence j1, Sentence j2)
    {
        /*
            Conditional Judgment Abduction

            Input:
                j1: Implication Statement (S ==> P) <f2, c2>

                j2: Judgment Event (P) <f1, c1> {tense}, i.e. (E ==> P)
            Evidence:
                F_abduction
            Returns:
                :- S. :|:  <f3, c3> (E ==> S)
        */
        Asserts.assert_sentence_forward_implication(j1);
        Asserts.assert(j2.get_statement_term() == j1.get_statement_term().get_predicate_term(), "Error: Invalid inputs to Conditional Judgment Abduction: "
                                                                  + j1.ToString()
                                                                  + " and "
                                                                  + j2.ToString());

        StatementTerm result_statement = (StatementTerm)j1.get_statement_term().get_subject_term();  // S

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1,
                                                                     j2,
                                                                     result_statement,
                                                                     this.nars.inferenceEngine.truthValueFunctions.F_Abduction);
    }

    public Sentence ConditionalGoalDeduction(Sentence j1, Sentence j2)
    {
        /*
            Conditional Goal Deduction

            Input:
                j1: Goal Event (P) <f1, c1> {tense}, i.e. (P ==> D)

                j2: Implication Statement (S ==> P) <f2, c2>
            Evidence:
                F_deduction
            Returns:
                :- S! <f3, c3> (S ==> D)
        */
        Asserts.assert_sentence_forward_implication(j2);
        Asserts.assert(j1.get_statement_term() == j2.get_statement_term().get_predicate_term(), "Error: Invalid inputs to Conditional Goal Deduction: "
                                                                   + j1.ToString()
                                                                   + " and "
                                                                   + j2.ToString());

        StatementTerm result_statement = (StatementTerm)j2.get_statement_term().get_subject_term();  // S

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1,
                                                                     j2,
                                                                     result_statement,
                                                                     this.nars.inferenceEngine.truthValueFunctions.F_Deduction);
    }

    public Sentence ConditionalGoalInduction(Sentence j1, Sentence j2)
    {
        /*
            Conditional Goal Induction

            Input:
                j1: Goal Event (S!) <f1, c1> {tense}, i.e. (S ==> D)

                j2: Implication Statement (S ==> P) <f2, c2>
            Evidence:
                F_induction
            Returns:
                :- P! <f3, c3> (P ==> D)
        */
        Asserts.assert_sentence_forward_implication(j2);
        Asserts.assert(j1.get_statement_term() == j2.get_statement_term().get_subject_term(), "Error: Invalid inputs to Conditional Goal Induction: "
                                                                + j1.ToString()
                                                                + " and "
                                                                + j2.ToString());

        StatementTerm result_statement = (StatementTerm)j2.get_statement_term().get_predicate_term();  // S

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1,
                                                                     j2,
                                                                     result_statement,
                                                                     this.nars.inferenceEngine.truthValueFunctions.F_Induction);
    }

    public Sentence SimplifyConjunctiveGoal(Sentence j1, Sentence j2)
    {
        /*
            Conditional Goal Deduction

            Input:
                j1: Goal Event (C &/ S)!<f1, c1> , i.e. ((C && S) ==> D)

                j2: Belief (C) <f2, c2> {tense}
            Evidence:
                F_abduction
            Returns:
                :- S! <f3, c3> (S ==> D)
        */
        List<Term> remaining_subterms = new List<Term>(j1.get_statement_term().subterms);
        int found_idx = remaining_subterms.IndexOf(j2.get_statement_term());
        Asserts.assert(found_idx != -1, "Error: Invalid inputs to Simplify conjuctive goal (deduction): "
                    + j1.ToString()
                    + " and "
                    + j2.ToString());


        Term result_statement;
        if (remaining_subterms.Count == 1)
        {
            result_statement = remaining_subterms[0];
        }
        else
        {
            List<int> new_intervals = new List<int>();
            if (j1.get_compound_statement_term().intervals.Count > 0)
            {
                new_intervals = new List<int>(j1.get_compound_statement_term().intervals);
                new_intervals.RemoveAt(found_idx);
            }
            result_statement = new CompoundTerm(remaining_subterms, j1.get_compound_statement_term().connector, new_intervals);
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Deduction);
    }


    public Sentence SimplifyNegatedConjunctiveGoal(Sentence j1, Sentence j2)
    {
        /*
            Conditional Goal Deduction

            Input:
                j1: Goal Event (--,(A &/ B))!<f1, c1> , i.e. ((A &/ B) ==> D)

                j2: Belief A = (S --> P) <f2, c2> {tense}
            Evidence:
                F_abduction
            Returns:
                :- B! <f3, c3> (B ==> D)
        */
        List<Term> remaining_subterms = new List<Term>(((CompoundTerm)j1.get_compound_statement_term().subterms[0]).subterms);
        int found_idx = remaining_subterms.IndexOf(j2.get_statement_term());

        Asserts.assert(found_idx != -1, "Error: Invalid inputs to Simplify negated conjuctive goal (induction): "
                        + j1.ToString()
                        + " and "
                        + j2.ToString());


        remaining_subterms.RemoveAt(found_idx);

        Term result_statement;
        if (remaining_subterms.Count == 1)
        {
            result_statement = new CompoundTerm(remaining_subterms, j1.get_compound_statement_term().connector);
        }
        else
        {
            List<int> new_intervals = new List<int>();
            if (j1.get_compound_statement_term().intervals.Count > 0)
            {
                new_intervals = new List<int>(j1.get_compound_statement_term().intervals);
                new_intervals.RemoveAt(found_idx);
            }
            result_statement = new CompoundTerm(remaining_subterms, j1.get_compound_statement_term().connector, new_intervals);
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Induction);

    }


    /*
        Conditional Conjunctional Rules
        --------------------------------
        Conditional Rules w/ Conjunctions
    */


    public Sentence ConditionalConjunctionalDeduction(Sentence j1, Sentence j2)
    {
        /*
            Conditional Conjunctional Deduction

            Input:
                j1: Conjunctive Implication Judgment ((C1 && C2 && ... CN && S) ==> P) <f2, c2>
                    ||
                    Conjunctive Implication Judgment ((C1 &/ C2 &/ ... CN) ==> P <f2, c2>

                j2: Statement (S) <f1, c1> {tense}
            Evidence:
                F_deduction
            Returns:
                :-  ((C1 && C2 && ... CN) ==> P)  <f3, c3>
        */
        CompoundTerm subject_term;
        if (j1 is Judgment)
        {
            Asserts.assert_sentence_forward_implication(j1);
            subject_term = (CompoundTerm)j1.get_statement_term().get_subject_term();
        }
        else if (j1 is Goal)
        {
            subject_term = j1.get_compound_statement_term();
        }
        else
        {
            Asserts.assert(false, "ERROR");
            return null;
        }

        List<Term> subterms = subject_term.subterms;
        List<Term> subterm_to_remove = new List<Term> { j2.get_statement_term() };

        List<Term> new_subterms = subterms.Except(subterm_to_remove).ToList();  // subtract j2 from j1 subject subterms

        Term new_compound_subject_term;
        if (new_subterms.Count > 1)
        {
            // recreate the conjunctional compound with the new subterms
            new_compound_subject_term = new CompoundTerm(new_subterms, subject_term.connector);
        }
        else if (new_subterms.Count == 1)
        {
            // only 1 subterm, no need to make it a compound
            new_compound_subject_term = new_subterms[0];
        }
        else
        {
            // 0 new subterms
            if (subject_term.subterms.Count > 1)
            {
                new_subterms = new List<Term>(subject_term.subterms);
                new_subterms.RemoveAt(new_subterms.Count - 1);
                new_compound_subject_term = new CompoundTerm(new_subterms, subject_term.connector);
            }
            else
            {
                Asserts.assert(false, "ERROR: Invalid inputs to Conditional Conjunctional Deduction " + j1.ToString() + " && " + j2.ToString());
                return null;
            }
        }

        Term result_statement;
        if (j1 is Judgment)
        {
            result_statement = new StatementTerm(new_compound_subject_term, j1.get_statement_term().get_predicate_term(),
                                                              j1.get_statement_term().get_copula());
        }
        else if (j1 is Goal)
        {
            result_statement = new_compound_subject_term;
        }
        else
        {
            Asserts.assert(false, "ERROR");
            return null;
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1,
                                                                     j2,
                                                                     result_statement,
                                                                     this.nars.inferenceEngine.truthValueFunctions.F_Deduction);
    }


    public Sentence ConditionalConjunctionalAbduction(Sentence j1, Sentence j2)
    {
        /*
            Conditional Conjunctional Abduction

            Input:
                j1: Implication Statement ((C1 && C2 && ... CN && S) ==> P) <f1, c1>

                j2: Implication Statement ((C1 && C2 && ... CN) ==> P) <f2, c2> {tense}
            Evidence:
                F_abduction
            Returns:
                :-  S  <f3, c3>

            #todo temporal
        */

        Asserts.assert_sentence_forward_implication(j1);
        Asserts.assert_sentence_forward_implication(j2);


        CompoundTerm j1_subject_term = (CompoundTerm)j1.get_statement_term().get_subject_term();
        CompoundTerm j2_subject_term = (CompoundTerm)j2.get_statement_term().get_subject_term();


        List<Term> j1_subject_statement_terms;
        if (TermConnectorMethods.is_conjunction(j1_subject_term.connector))
        {
            j1_subject_statement_terms = j1_subject_term.subterms;
        }
        else
        {
            j1_subject_statement_terms = new List<Term> { j1_subject_term };
        }

        List<Term> j2_subject_statement_terms;
        if (TermConnectorMethods.is_conjunction(j2_subject_term.connector))
        {
            j2_subject_statement_terms = j2_subject_term.subterms;
        }
        else
        {
            j2_subject_statement_terms = new List<Term> { j2_subject_term };
        }

        List<Term> difference_of_terms = j1_subject_statement_terms.Except(j2_subject_statement_terms).ToList();

        Asserts.assert(difference_of_terms.Count == 1, "Error, should only have one term in set difference: " + difference_of_terms.ToString());

        StatementTerm result_statement = (StatementTerm)difference_of_terms[0];

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Abduction);
    }
}