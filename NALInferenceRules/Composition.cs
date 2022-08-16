/*
==== ==== ==== ==== ==== ====
==== NAL Inference Rules - Composition Inference Rules ====
==== ==== ==== ==== ==== ====

    Author: Christian Hahm
    Created: May 16, 2022
    Purpose: Defines the NAL inference rules
            Assumes the given sentences do not have evidential overlap.
            Does combine evidential bases in the Resultant Sentence.
*/

using System.Collections.Generic;
using static TruthValueFunctions;

public class CompositionRules
{
    NARS nars;

    public CompositionRules(NARS nars)
    {
        this.nars = nars;
    }


    public Sentence DisjunctionOrIntensionalIntersection(Sentence j1, Sentence j2)
    {
        /*
            First Order: Intensional Intersection (Strong Inference)
            Higher Order: Disjunction

            Assumes: j1 && j2 do not have evidential overlap
            -----------------

            Input:
                j1: Sentence (T1 --> M <f1, c1>) (Sentence (T1 ==> M <f1, c1>))
                and
                j2: Sentence (T2 --> M <f2, c2>) (Sentence (T2 ==> M <f2, c2>))

                OR

                j1: Sentence (M --> T1 <f1, c1>) (Sentence (M ==> T1 <f1, c1>))
                and
                j2: Sentence (M --> T2 <f2, c2>) (Sentence (M ==> T2 <f2, c2>))
            Evidence:
                F_int

                OR

                F_uni
            Returns:
                :- Sentence ((T1 | T2) --> M) (Sentence ((T1 || T2) --> M))
                OR
                :- Sentence (M --> (T1 | T2)) (Sentence (M --> (T1 || T2)))
        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_asymmetric(j2);

        // Statement
        CompoundTerm compound_term;
        StatementTerm result_statement;
        TermConnector connector;
        Copula copula;
        if (j1.get_statement_term().is_first_order() && j2.get_statement_term().is_first_order())
        {
            connector = TermConnector.IntensionalIntersection;
            copula = Copula.Inheritance;
        }
        else
        {
            // higher-order, could be temporal
            // todo temporal disjunction
            connector = TermConnector.Disjunction;
            copula = Copula.Implication;
        }

        // Statement
        TwoPremiseTruthValueFunction result_truth_function = null;
        if (j1.get_statement_term().get_predicate_term() == j2.get_statement_term().get_predicate_term())
        {
            // j1: Sentence(T1 --> M < f1, c1 >)
            // j2: Sentence(T2 --> M < f2, c2 >)

            // don't compound terms which are already compound
            // this reduces complexity.
            // todo better simplifying of syntactically complex results
            if (j1.get_statement_term().get_subject_term() is CompoundTerm || j2.get_statement_term().get_subject_term() is CompoundTerm) return null;

            compound_term = new CompoundTerm(new List<Term> { j1.get_statement_term().get_subject_term(), j2.get_statement_term().get_subject_term() }, connector);  // (T1 | T2)
            result_statement = new StatementTerm(compound_term, j1.get_statement_term().get_predicate_term(), copula);  // ((T1 | T2) --> M)

            if (!(j1 is Question)) result_truth_function = this.nars.inferenceEngine.truthValueFunctions.F_Intersection;


        }
        else if (j1.get_statement_term().get_subject_term() == j2.get_statement_term().get_subject_term())
        {
            // j1: Sentence(M --> T1 < f1, c1 >)
            // j2: Sentence(M --> T2 < f2, c2 >)
            if (j1.get_statement_term().get_predicate_term() is CompoundTerm || j2.get_statement_term().get_predicate_term() is CompoundTerm)
            {
                // don't compound terms which are already compound
                // this reduces complexity.
                // todo better simplifying of syntactically complex results
                return null;
            }

            compound_term = new CompoundTerm(new List<Term> { j1.get_statement_term().get_predicate_term(), j2.get_statement_term().get_predicate_term() }, connector);  // (T1 | T2)

            result_statement = new StatementTerm(j1.get_statement_term().get_subject_term(), compound_term, copula);  // (M --> (T1 | T2))

            if (!(j1 is Question))
            {
                result_truth_function = this.nars.inferenceEngine.truthValueFunctions.F_Union;
            }

        }
        else
        {
            Asserts.assert(false, "ERROR: Invalid inputs to Intensional Intersection");
            return null;
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, result_truth_function);
    }


    public Sentence ConjunctionOrExtensionalIntersection(Sentence j1, Sentence j2)
    {
        /*
            First-Order{ Extensional Intersection (Strong Inference)
            Higher-Order{ Conjunction

            Assumes: j1 && j2 do not have evidential overlap
            -----------------

            Input:
                j1: Sentence (T1 --> M <f1, c1>) (Sentence (T1 ==> M <f1, c1>))
                &&
                j2: Sentence (T2 --> M <f2, c2>) (Sentence (T2 ==> M <f2, c2>))

                OR

                j1: Sentence (M --> T1 <f1, c1>) (Sentence (M ==> T1 <f1, c1>))
                &&
                j2: Sentence (M --> T2 <f2, c2>) (Sentence (M ==> T2 <f2, c2>))
            Evidence:
                F_uni

                OR

                F_int
            Returns:
                Sentence ((T1 & T2) --> M) || Sentence ((T1 && T2) ==> M)

                ||

                Sentence (M --> (T1 & T2)) || Sentence (M ==> (T1 && T2))
        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_asymmetric(j2);

        // Statement
        CompoundTerm compound_term;
        StatementTerm result_statement;
        TermConnector connector;
        Copula copula;
        if (j1.get_statement_term().is_first_order() && j2.get_statement_term().is_first_order())
        {
            connector = TermConnector.ExtensionalIntersection; // &
            copula = Copula.Inheritance;
        }
        else
        {
            // higher-order, could be temporal
            connector = TermConnector.Conjunction; // &&
            copula = Copula.Implication;
        }

        TwoPremiseTruthValueFunction result_truth_function = null;
        if (j1.get_statement_term().get_predicate_term() == j2.get_statement_term().get_predicate_term())
        {
            // j1: Sentence(T1 --> M < f1, c1 >)
            // j2: Sentence(T2 --> M < f2, c2 >)

            // don't compound terms which are already compound
            // this reduces complexity.
            // todo: better simplifying of syntactically complex results
            if (j1.get_statement_term().get_subject_term() is CompoundTerm || j2.get_statement_term().get_subject_term() is CompoundTerm) return null;

            compound_term = new CompoundTerm(new List<Term> { j1.get_statement_term().get_subject_term(), j2.get_statement_term().get_subject_term() }, connector);  // (T1 & T2)
            result_statement = new StatementTerm(compound_term, j1.get_statement_term().get_predicate_term(), copula);  // ((T1 & T2) --> M)

            if (!(j1 is Question)) result_truth_function = this.nars.inferenceEngine.truthValueFunctions.F_Union;

        }
        else if (j1.get_statement_term().get_subject_term() == j2.get_statement_term().get_subject_term())
        {
            // j1: Sentence(M --> T1 < f1, c1 >)
            // j2: Sentence(M --> T2 < f2, c2 >)

            // don't compound terms which are already compound
            // this reduces complexity.
            // todo{ better simplifying of syntactically complex results
            if (j1.get_statement_term().get_predicate_term() is CompoundTerm || j2.get_statement_term().get_predicate_term() is CompoundTerm) return null;

            compound_term = new CompoundTerm(new List<Term> { j1.get_statement_term().get_predicate_term(), j2.get_statement_term().get_predicate_term() }, connector);  // (T1 & T2)
            result_statement = new StatementTerm(j1.get_statement_term().get_subject_term(), compound_term, copula);  // (M --> (T1 & T2))

            if (!(j1 is Question)) result_truth_function = this.nars.inferenceEngine.truthValueFunctions.F_Intersection;

        }
        else
        {
            Asserts.assert(false, "ERROR: Invalid inputs to Extensional Intersection");
            return null;
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, result_truth_function);
    }


    public Sentence IntensionalDifference(Sentence j1, Sentence j2)
    {
        /*
            Intensional Difference (Strong Inference)

            Assumes: j1 && j2 do not have evidential overlap
            -----------------

            Input:
                j1: Sentence (T1 --> M <f1, c1>)
                &&
                j2: Sentence (T2 --> M <f2, c2>)
            Evidence:
                F_difference
            Returns:
                :- Sentence ((T1 ~ T2) --> M)
        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_asymmetric(j2);
        Asserts.assert(j1.get_statement_term().get_predicate_term() == j2.get_statement_term().get_predicate_term(), "Error:");

        // don't compound terms which are already compound
        // this reduces complexity.
        // todo{ better simplifying of syntactically complex results
        if (j1.get_statement_term().get_subject_term() is CompoundTerm || j2.get_statement_term().get_subject_term() is CompoundTerm) return null;

        CompoundTerm compound_term = new CompoundTerm(new List<Term> { j1.get_statement_term().get_subject_term(), j2.get_statement_term().get_subject_term() }, TermConnector.IntensionalDifference);  // (T1 ~ T2)
        StatementTerm result_statement = new StatementTerm(compound_term, j1.get_statement_term().get_predicate_term(), Copula.Inheritance);  // ((T1 ~ T2) --> M)
        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Difference);
    }


    public Sentence ExtensionalDifference(Sentence j1, Sentence j2)
    {
        /*
            Extensional Difference (Strong Inference)

            Assumes: j1 && j2 do not have evidential overlap
            -----------------
            Input:
                j1: Sentence (M --> T1 <f1, c1>)
                &&
                j2: Sentence (M --> T2 <f2, c2>)
            Evidence:
                F_difference
            Returns:
                :- Sentence (M --> (T1 - T2))
        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_asymmetric(j2);
        Asserts.assert(j1.get_statement_term().get_subject_term() == j2.get_statement_term().get_subject_term(), "Error:");

        // don't compound terms which are already compound
        // this reduces complexity.
        // todo: better simplifying of syntactically complex results
        if (j1.get_statement_term().get_predicate_term() is CompoundTerm || j2.get_statement_term().get_predicate_term() is CompoundTerm) return null;

        CompoundTerm compound_term = new CompoundTerm(new List<Term> { j1.get_statement_term().get_predicate_term(), j2.get_statement_term().get_predicate_term() }, TermConnector.ExtensionalDifference);
        StatementTerm result_statement = new StatementTerm(j1.get_statement_term().get_subject_term(), compound_term, Copula.Inheritance);  // (M --> (T1 - T2))

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Difference);
    }
}