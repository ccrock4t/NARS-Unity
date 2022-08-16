/*
==== ==== ==== ==== ==== ====
==== NAL Inference Rules - Syllogistic Inference Rules ====
==== ==== ==== ==== ==== ====

    Author: Christian Hahm
    Created: May 16, 2022
    Purpose: Defines the NAL inference rules
            Assumes the given sentences do not have evidential overlap.
            Does combine evidential bases in the Resultant Sentence.
*/

public class SyllogisticRules
{
    NARS nars;

    public SyllogisticRules(NARS nars)
    {
        this.nars = nars;
    }

    public Sentence Deduction(Sentence j1, Sentence j2)
    {
        /*
            Deduction (Strong syllogism)

            -----------------
            Assumes: j1 && j2 do not have evidential overlap

            Input:
                j1: Sentence (M --> P <f1, c1>)

                j2: Sentence (S --> M <f2, c2>)
            Truth Val{
                F_ded
            Returns:
                :- Sentence (S --> P <f3, c3>)
        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_asymmetric(j2);

        // Statement
        StatementTerm result_statement = new StatementTerm(j2.get_statement_term().get_subject_term(), j1.get_statement_term().get_predicate_term(), j1.get_statement_term().get_copula());

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Deduction);
    }



    public Sentence Analogy(Sentence j1, Sentence j2)
    {
        /*
            Analogy (Strong syllogism)

            -----------------
            Assumes: j1 && j2 do not have evidential overlap

            Input:
                j1: Sentence (M --> P <f1, c1>)
                    ||
                j1: Sentence (P --> M <f1, c1>)

                j2: Sentence (S <-> M <f2, c2>)
            Truth Val{
                F_ana
            Returns: (depending on j1)
                :- Sentence (S --> P <f3, c3>)
                    ||
                :- Sentence (P --> S <f3, c3>)

        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_symmetric(j2);

        StatementTerm result_statement;
        // Statement
        if (j1.get_statement_term().get_subject_term() == j2.get_statement_term().get_predicate_term())
        {
            // j1=M-->P, j2=S<->M
            result_statement = new StatementTerm(j2.get_statement_term().get_subject_term(), j1.get_statement_term().get_predicate_term(), j1.get_statement_term().get_copula()); // S-->P
        }

        else if (j1.get_statement_term().get_subject_term() == j2.get_statement_term().get_subject_term())
        {
            // j1=M-->P, j2=M<->S
            result_statement = new StatementTerm(j2.get_statement_term().get_predicate_term(), j1.get_statement_term().get_predicate_term(), j1.get_statement_term().get_copula()); // S-->P
        }
        else if (j1.get_statement_term().get_predicate_term() == j2.get_statement_term().get_predicate_term())
        {
            // j1=P-->M, j2=S<->M
            result_statement = new StatementTerm(j1.get_statement_term().get_subject_term(), j2.get_statement_term().get_subject_term(), j1.get_statement_term().get_copula()); // P-->S
        }
        else if (j1.get_statement_term().get_predicate_term() == j2.get_statement_term().get_subject_term())
        {
            // j1=P-->M, j2=M<->S
            result_statement = new StatementTerm(j1.get_statement_term().get_subject_term(), j2.get_statement_term().get_predicate_term(), j1.get_statement_term().get_copula());  // P-->S
        }
        else
        {
            Asserts.assert(false, "Error: Invalid inputs to nal_analogy: " + j1.ToString() + " && " + j2.ToString());
            return null;
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Analogy);
    }



    public Sentence Resemblance(Sentence j1, Sentence j2)
    {
        /*
            Resemblance (Strong syllogism)

            -----------------
            Assumes: j1 && j2 do not have evidential overlap

            Input:
                j1: Sentence (M <-> P <f1, c1>)
                    ||
                j1: Sentence (P <-> M <f1, c1>)

                j2: Sentence (S <-> M <f2, c2>)
                    ||
                j2: Sentence (M <-> S <f2, c2>)
            Truth Val{
                F_res
            Returns:
                :- Sentence (S <-> P <f3, c3>)
        */
        Asserts.assert_sentence_symmetric(j1);
        Asserts.assert_sentence_symmetric(j2);

        StatementTerm result_statement;
        // Statement
        if (j1.get_statement_term().get_subject_term() == j2.get_statement_term().get_predicate_term())
        {
            // j1=M<->P, j2=S<->M
            result_statement = new StatementTerm(j2.get_statement_term().get_subject_term(),
                                                              j1.get_statement_term().get_predicate_term(),
                                                              j1.get_statement_term().get_copula());  // S<->P
        }
        else if (j1.get_statement_term().get_subject_term() == j2.get_statement_term().get_subject_term())
        {
            // j1=M<->P, j2=M<->S
            result_statement = new StatementTerm(j2.get_statement_term().get_predicate_term(),
                                                              j1.get_statement_term().get_predicate_term(),
                                                              j1.get_statement_term().get_copula());  // S<->P
        }
        else if (j1.get_statement_term().get_predicate_term() == j2.get_statement_term().get_predicate_term())
        {
            // j1=P<->M, j2=S<->M
            result_statement = new StatementTerm(j2.get_statement_term().get_subject_term(),
                                                              j1.get_statement_term().get_subject_term(),
                                                              j1.get_statement_term().get_copula());  // S<->P
        }
        else if (j1.get_statement_term().get_predicate_term() == j2.get_statement_term().get_subject_term())
        {
            // j1=P<->M, j2=M<->S
            result_statement = new StatementTerm(j2.get_statement_term().get_predicate_term(),
                                                              j2.get_statement_term().get_subject_term(),
                                                              j1.get_statement_term().get_copula());  // S<->P
        }
        else
        {
            Asserts.assert(false, "Error: Invalid inputs to nal_resemblance: " + j1.ToString() + " && " + j2.ToString());
            return null;
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Resemblance);
    }

    public Sentence Abduction(Sentence j1, Sentence j2)
    {
        /*
            Abduction (Weak syllogism)

            -----------------
            Assumes: j1 && j2 do not have evidential overlap

            Input:
                j1: Sentence (P --> M <f1, c1>)

                j2: Sentence (S --> M <f2, c2>)
            Evidence:
                F_abd
            Returns:
                :- Sentence (S --> P <f3, c3>)
        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_asymmetric(j2);

        // Statement
        StatementTerm result_statement = new StatementTerm(j2.get_statement_term().get_subject_term(),
                                                          j1.get_statement_term().get_subject_term(),
                                                          j1.get_statement_term().get_copula());
        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1,j2,result_statement,this.nars.inferenceEngine.truthValueFunctions.F_Abduction);
    }


    public Sentence Induction(Sentence j1, Sentence j2)
    {
        /*
            Induction (Weak syllogism)

            -----------------
            Assumes: j1 && j2 do not have evidential overlap

            Input:
                j1: Sentence (M --> P <f1, c1>)

                j2: Sentence (M --> S <f2, c2>)
            Evidence:
                F_ind
            Returns:
                :- Sentence (S --> P <f3, c3>)
        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_asymmetric(j2);

        // Statement
        StatementTerm result_statement = new StatementTerm(j2.get_statement_term().get_predicate_term(), j1.get_statement_term().get_predicate_term(), j1.get_statement_term().get_copula());

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Induction);
    }


    public Sentence Exemplification(Sentence j1, Sentence j2)
    {
        /*
            Exemplification (Weak syllogism)

            -----------------
            Assumes: j1 && j2 do not have evidential overlap

            Input:
                j1: Sentence (P --> M <f1, c1>)

                j2: Sentence (M --> S <f2, c2>)
            Evidence:
                F_exe
            Returns:
                :- Sentence (S --> P <f3, c3>)
        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_asymmetric(j2);

        // Statement
        StatementTerm result_statement = new StatementTerm(j2.get_statement_term().get_predicate_term(), j1.get_statement_term().get_subject_term(), j1.get_statement_term().get_copula());
        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Exemplification);
    }

    public Sentence Comparison(Sentence j1, Sentence j2)
    {
        /*
            Comparison (Weak syllogism)

            -----------------
            Assumes: j1 && j2 do not have evidential overlap

            Input:
                j1: Sentence (M --> P <f1, c1>)
                j2: Sentence (M --> S <f2, c2>)

                ||

                j1: Sentence (P --> M <f1, c1>)
                j2: Sentence (S --> M <f2, c2>)
            Evidence:
                F_com
            Returns:
                :- Sentence (S <-> P <f3, c3>)
        */
        Asserts.assert_sentence_asymmetric(j1);
        Asserts.assert_sentence_asymmetric(j2);

        Copula copula;
        if (j1.get_statement_term().is_first_order())
        {
            copula = Copula.Similarity;
        }
        else
        {
            copula = Copula.Equivalence;
        }

        StatementTerm result_statement;
        // Statement
        if (j1.get_statement_term().get_subject_term() == j2.get_statement_term().get_subject_term())
        {
            // M --> P && M --> S

            result_statement = new StatementTerm(j2.get_statement_term().get_predicate_term(),
                                                              j1.get_statement_term().get_predicate_term(),
                                                              copula);
        }
        else if (j1.get_statement_term().get_predicate_term() == j2.get_statement_term().get_predicate_term())
        {
            // P --> M && S --> M
            result_statement = new StatementTerm(j2.get_statement_term().get_subject_term(),
                                                              j1.get_statement_term().get_subject_term(),
                                                              copula);
        }
        else
        {
            Asserts.assert(false, "Error: Invalid inputs to nal_comparison: " + j1.ToString() + " && " + j2.ToString());
            return null;
        }

        return this.nars.helperFunctions.create_resultant_sentence_two_premise(j1, j2, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Comparison);
    }

}