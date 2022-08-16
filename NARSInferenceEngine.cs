/*  Author: Christian Hahm
    Created: May 20, 2022
    Purpose: Given premises, performs proper inference && returns the resultant sentences as Sentences.
*/



using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NARSInferenceEngine
{

    NARS nars;

    public LocalRules localRules;
    public SyllogisticRules syllogisticRules;
    public CompositionRules compositionRules;
    public ConditionalRules conditionalRules;
    public ImmediateRules immediateRules;
    public TemporalRules temporalRules;

    public TruthValueFunctions truthValueFunctions;
    

    public NARSInferenceEngine(NARS nars)
    {
        this.nars = nars;

        this.localRules = new LocalRules(nars);
        this.syllogisticRules = new SyllogisticRules(nars);
        this.compositionRules = new CompositionRules(nars);
        this.conditionalRules = new ConditionalRules(nars);
        this.immediateRules = new ImmediateRules(nars);
        this.temporalRules = new TemporalRules(nars);

        this.truthValueFunctions = new TruthValueFunctions(nars);
    }

    public List<Sentence>? do_semantic_inference_two_premise(Sentence j1, Sentence j2)
    {
        if (!EvidentialBase.may_interact(j1, j2)) return null;

        List<Sentence>? results;

        try
        {
            if (j1 is Goal && j2 is Judgment)
            {
                results = do_semantic_inference_goal_judgment(j1, j2);
            }
            else
            {
                results = do_semantic_inference_two_judgment(j1, j2);
            }
        }
        catch (Exception e)
        {
            Asserts.assert(false, "ERROR: Inference error " + e.ToString() + " between " + j1.ToString() + " && " + j2.ToString());
            return null;
        }

        return results;
    }

    public List<Sentence>? do_semantic_inference_two_judgment(Sentence j1, Sentence j2)
    {
        /*
            Derives a new Sentence by performing the appropriate inference rules on the given semantically related sentences.
            The resultant sentence's evidential base == merged from its parents.

            :param j1: Sentence (Question || Judgment)
            :param j2: Semantically related belief (Judgment)

            :assume j1 && j2 have distinct evidential bases B1 && B2: B1 ⋂ B2 = Ø
                    (no evidential overlap)

            :returns An array of the derived Sentences, || an empty array if the inputs have evidential overlap
        */

        if (this.nars.config.DEBUG) Debug.Log("Trying inference between: " + get_formatted_string(j1) + " && " + get_formatted_string(j2));

        Sentence derived_sentence;

        /*
        ===============================================
        ===============================================
            Pre-Processing
        ===============================================
        ===============================================
        */

        if (j1.value.confidence == 0 || j2.value.confidence == 0)
        {
            if (this.nars.config.DEBUG) Debug.Log("Can't do inference between negative premises");
            return null; // can't do inference with 2 entirely negative premises
        }


        List<Sentence> all_derived_sentences = new List<Sentence>();

        Term j1_statement = j1.statement;
        Term j2_statement = j2.statement;

        // same statement
        if (j1_statement == j2_statement)
        {
            /*
            // Revision
            // j1 = j2
            */
            if (j1 is Question) return all_derived_sentences; // can't do revision with questions

            derived_sentence = this.localRules.Revision(j1, j2);  // S-->P
            add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
            return all_derived_sentences;
        }


        if (j1.value.frequency == 0 || j2.value.frequency == 0)
        {
            if (this.nars.config.DEBUG) Debug.Log("Can't do inference between negative premises");
            return null; // can't do inference with 2 entirely negative premises
        }

        /*
        ===============================================
        ===============================================
            First-order and Higher-Order Syllogistic Rules
        ===============================================
        ===============================================
        */
        //todo arrayterms

        bool swapped = false;
        if (j1.statement is CompoundTerm)
        {
            if (j2.statement is StatementTerm && !j2.get_statement_term().is_first_order())
            {
                if (j2.get_statement_term().get_copula() == Copula.Implication || j2.get_statement_term().get_copula() == Copula.PredictiveImplication)
                {
                    derived_sentence = this.conditionalRules.ConditionalJudgmentDeduction(j2, j1);  // S-->P
                    add_to_derived_sentences(derived_sentence, all_derived_sentences, j2, j1);
                    return all_derived_sentences;
                }
            }
        }

        if (j2.statement is CompoundTerm)
        {
            if (j1.statement is StatementTerm && !j1.get_statement_term().is_first_order())
            {
                if (j1.get_statement_term().get_copula() == Copula.Implication | j1.get_statement_term().get_copula() == Copula.PredictiveImplication)
                {
                    derived_sentence = this.conditionalRules.ConditionalJudgmentDeduction(j1, j2);  // S-->P
                    add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
                    return all_derived_sentences;
                }
            }
        }




        if (j1.statement is StatementTerm && j2.statement is StatementTerm
                && CopulaMethods.is_first_order(j1.get_statement_term().get_copula()) == CopulaMethods.is_first_order(j2.get_statement_term().get_copula()))
        {
            // A --> B and C --> D or // A ==> B and C ==> D

            Term j1_subject_term = j1.get_statement_term().get_subject_term();
            Term j2_subject_term = j2.get_statement_term().get_subject_term();
            Term j1_predicate_term = j1.get_statement_term().get_predicate_term();
            Term j2_predicate_term = j2.get_statement_term().get_predicate_term();
            Copula j1_copula = j1.get_statement_term().get_copula();
            Copula j2_copula = j2.get_statement_term().get_copula();

            // check if the result will lead to tautology
            bool tautology = (j1_subject_term == j2_predicate_term && j1_predicate_term == j2_subject_term) ||
            (j1_subject_term == j2_subject_term && j1_predicate_term == j2_predicate_term
             &&
             ((!CopulaMethods.is_symmetric(j1_copula) && CopulaMethods.is_symmetric(j2_copula))  // S-->P and P<->S will cause tautology
              || (CopulaMethods.is_symmetric(j1_copula) && !CopulaMethods.is_symmetric(j2_copula))));  // S<->P and S-->P will cause tautology

            if (tautology)
            {
                if (this.nars.config.DEBUG) Debug.Log("tautology");
                return all_derived_sentences;  // can't do inference, it will result in tautology
            }

            if (CopulaMethods.is_temporal(j1.get_statement_term().get_copula()) || (j1 is Judgment && j1.is_event()) || (j2 is Judgment && j2.is_event()))
            {
                //dont do semantic inference with temporal
                // todo .. don't do inference with events, it isn't handled gracefully right now
                return all_derived_sentences;
            }
            else if (!CopulaMethods.is_symmetric(j1.get_statement_term().get_copula()) && !CopulaMethods.is_symmetric(j2.get_statement_term().get_copula())){
                if (j1_subject_term == j2_predicate_term || j1_predicate_term == j2_subject_term)
                    /*
                        j1 = M-->P, j2 = S-->M
                    OR swapped premises
                        j1 = S-->M, j2 = M-->P
                    */
                    if (j1_subject_term != j2_predicate_term)
                    {


                        /*
                            j1=S-->M, j2=M-->P

                            Swap these premises
                        */

                        (j1, j2) = (j2, j1);
                    }

                /*
                    j1 = M-->P, j2 = S-->M
                */
                /*
                // Deduction
                */

                derived_sentence = this.syllogisticRules.Deduction(j1, j2);  // S-->P
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Swapped Exemplification
                */
                derived_sentence = this.syllogisticRules.Exemplification(j2, j1);  // P-->S
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

            }
            else if (j1.get_statement_term().get_subject_term() == j2.get_statement_term().get_subject_term())
            {
                /*
                    j1=M-->P
                    j2=M-->S
                */

                /*
                // Induction
                */
                derived_sentence = this.syllogisticRules.Induction(j1, j2);  // S-->P
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Swapped Induction
                */
                derived_sentence = this.syllogisticRules.Induction(j2, j1); // P-->S
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Comparison
                */
                derived_sentence = this.syllogisticRules.Comparison(j1, j2);  // S<->P
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Intensional Intersection || Disjunction
                */
                derived_sentence = this.compositionRules.DisjunctionOrIntensionalIntersection(j1, j2);  // M --> (S | P)
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Extensional Intersection || Conjunction
                */
                derived_sentence = this.compositionRules.ConjunctionOrExtensionalIntersection(j1, j2);  // M --> (S & P)
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Extensional Difference
                */
                derived_sentence = this.compositionRules.ExtensionalDifference(j1, j2);  // M --> (S - P)
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Swapped Extensional Difference
                */
                derived_sentence = this.compositionRules.ExtensionalDifference(j2, j1);  // M --> (P - S)
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
            }
            else if (j1.get_statement_term().get_predicate_term() == j2.get_statement_term().get_predicate_term())
            {
                /*
                    j1 = P-->M
                    j2 = S-->M
                */

                /*
                // Abduction
                */
                derived_sentence = this.syllogisticRules.Abduction(j1, j2);  // S-->P || S==>P
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Swapped Abduction
                */
                derived_sentence = this.syllogisticRules.Abduction(j2, j1);  // P-->S || P==>S
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                if (!CopulaMethods.is_first_order(j1_copula))
                {
                    // two implication statements
                    if (TermConnectorMethods.is_conjunction(j1_subject_term.connector) || TermConnectorMethods.is_conjunction(j2_subject_term.connector))
                    {
                        List<Term> j1_subject_statement_terms;
                        List<Term> j2_subject_statement_terms;

                        if (TermConnectorMethods.is_conjunction(j1_subject_term.connector))
                        {
                            j1_subject_statement_terms = ((CompoundTerm)j1_subject_term).subterms;
                        }
                        else
                        {
                            j1_subject_statement_terms = new List<Term>();
                            j1_subject_statement_terms.Add(j1_subject_term);
                        }

                        if (TermConnectorMethods.is_conjunction(j2_subject_term.connector))
                        {
                            j2_subject_statement_terms = ((CompoundTerm)j2_subject_term).subterms;
                        }
                        else
                        {
                            j2_subject_statement_terms = new List<Term>();
                            j2_subject_statement_terms.Add(j2_subject_term);
                        }

                        IEnumerable<Term> j1_terms_no_j2 = j1_subject_statement_terms.Except(j2_subject_statement_terms);
                        IEnumerable<Term> j2_terms_no_j1 = j2_subject_statement_terms.Except(j1_subject_statement_terms);
                        List<Term> difference_of_subterms = j1_terms_no_j2.Concat(j2_terms_no_j1).ToList();


                        if (difference_of_subterms.Count == 1)
                        {
                            /*
                               At least one of the statement's subjects == conjunctive && differs from the
                               other statement's subject by 1 term
                            */

                            if (j1_subject_statement_terms.Count > j2_subject_statement_terms.Count)

                            {

                                derived_sentence = this.conditionalRules.ConditionalConjunctionalAbduction(j1, j2);  // S
                            }
                            else
                            {
                                derived_sentence = this.conditionalRules.ConditionalConjunctionalAbduction(j2, j1);  // S
                                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
                            }
                        }
                    }
                }
                /*
                // Intensional Intersection Disjunction
                */
                derived_sentence = this.compositionRules.DisjunctionOrIntensionalIntersection(j1, j2);  // (P | S) --> M
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Extensional Intersection Conjunction
                */
                derived_sentence = this.compositionRules.ConjunctionOrExtensionalIntersection(j1, j2);  // (P & S) --> M
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Intensional Difference
                */
                derived_sentence = this.compositionRules.IntensionalDifference(j1, j2);  // (P ~ S) --> M
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);

                /*
                // Swapped Intensional Difference
                */
                derived_sentence = this.compositionRules.IntensionalDifference(j2, j1);  // (S ~ P) --> M
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
                /*
                // Comparison
                */
                derived_sentence = this.syllogisticRules.Comparison(j1, j2);  // S<->P || S<=>P
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
            }

        }
        else if (!CopulaMethods.is_symmetric(j1.get_statement_term().get_copula()) && CopulaMethods.is_symmetric(j2.get_statement_term().get_copula()))
        {
            /*
            // j1 = M-->P || P-->M
            // j2 = S<->M || M<->S
            // Analogy
            */
            derived_sentence = this.syllogisticRules.Analogy(j1, j2);  // S-->P || P-->S
            add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
        }
        else if (CopulaMethods.is_symmetric(j1.get_statement_term().get_copula()) && !CopulaMethods.is_symmetric(j2.get_statement_term().get_copula()))
        {
            /*
            // j1 = M<->S || S<->M
            // j2 = P-->M || M-->P
            // Swapped Analogy
            */
            derived_sentence = this.syllogisticRules.Analogy(j2, j1);  // S-->P || P-->S
            add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
        }
        else if (CopulaMethods.is_symmetric(j1.get_statement_term().get_copula()) && CopulaMethods.is_symmetric(j2.get_statement_term().get_copula()))
        {
            /*
            // j1 = M<->P || P<->M
            // j2 = S<->M || M<->S
            // Resemblance
            */
            derived_sentence = this.syllogisticRules.Resemblance(j1, j2);  // S<->P
            add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
        }
        else if (((j1.statement is StatementTerm) && !j1.get_statement_term().is_first_order()) || ((j2.statement is StatementTerm) && !j2.get_statement_term().is_first_order()))
        {
            // One premise == a higher-order statement
            /*
                j1 = S==>P || S<=>P
                j2 = A-->B || A<->B
                OR
                j1 = A-->B || A<->B
                j2 = S==>P || S<=>P
            */
            if (j2.statement is StatementTerm && !j2.get_statement_term().is_first_order())
            {
                /*
                    j1 = A-->B || A<->B 
                    j2 = S==>P || S<=>P
                */
                // swap sentences so j1 == higher order
                (j1, j2) = (j2, j1);
                swapped = true;


                Asserts.assert(j1.statement is StatementTerm && !j1.get_statement_term().is_first_order(), "ERROR");

                /*
                    j1 = S==>P || S<=>P
                */
                if (CopulaMethods.is_symmetric(j1.get_statement_term().get_copula()) && (j2.statement == j1.get_statement_term().get_subject_term() || j2.statement == j1.get_statement_term().get_predicate_term())){
                    /*
                        j1 = S<=>P
                        j2 = S (e.g A-->B)
                    */
                    //pass
                    // derived_sentence = this.conditionalRules.ConditionalAnalogy(j2, j1)  // P
                    // add_to_derived_sentences(derived_sentence,all_derived_sentences,j1,j2)
                }
                else
                {
                    /*
                        j1 = S==>P
                        j2 = S || P (e.g A-->B)
                    */

                    if (j2.statement == j1.get_statement_term().get_subject_term())
                    {
                        /*
                        j2 = S
                        */
                        // derived_sentence = this.conditionalRules.ConditionalDeduction(j1, j2)  // P
                        // add_to_derived_sentences(derived_sentence,all_derived_sentences,j1,j2)
                        //pass
                    }
                    else if (j2.statement == j1.get_statement_term().get_predicate_term())
                    {
                        /*
                        j2 = P
                        */
                        // j2 = P. || (E ==> P)
                        //pass
                        // derived_sentence = this.conditionalRules.ConditionalJudgmentAbduction(j1, j2)  // S.
                        // add_to_derived_sentences(derived_sentence,all_derived_sentences,j1,j2)
                    }
                    else if (TermConnectorMethods.is_conjunction(j1.get_statement_term().get_subject_term().connector) && !CopulaMethods.is_symmetric(j1.get_statement_term().get_copula())){
                        /*
                        j1 = (C1 && C2 && ..CN && S) ==> P
                        j2 = S
                        */
                        //pass
                        // derived_sentence = this.conditionalRules.ConditionalConjunctionalDeduction(j1,j2)  // (C1 && C2 && ..CN) ==> P
                        // add_to_derived_sentences(derived_sentence,all_derived_sentences,j1,j2)

                    }
                    else if (((j1.statement is CompoundTerm) &&
                    (j2.statement is StatementTerm) &&
                    TermConnectorMethods.is_conjunction(j1.statement.connector)) || ((j2.statement is CompoundTerm) && (j1.statement is StatementTerm) && TermConnectorMethods.is_conjunction(j2.get_compound_statement_term().connector))){
                        /*
                            j1 = (A &/ B)
                            j2 = A
                            OR
                            j1 = A
                            j2 = (A &/ B)
                        */
                        if (j2.statement is CompoundTerm)
                        {
                            /*
                            j1 = A
                            j2 = (A &/ B)
                            */
                            // swap sentences so j1 is the compound
                            (j1, j2) = (j2, j1);
                            swapped = true;

                            /*
                            j1 = (A &/ B)
                            j2 = A
                            */
                            //pass
                        }

                    }
                }
            }
        }

        if (swapped) (j1, j2) = (j2, j1); // restore sentences
        swapped = false;

        /*
            ===============================================
            ===============================================
            Post-Processing
            ===============================================
            ===============================================
        */
        // mark sentences as interacted with each other
        //  j1.mutually_add_to_interacted_sentences(j2)

        if (this.nars.config.DEBUG) Debug.Log("Derived " + all_derived_sentences.Count + " inference results.");


        return all_derived_sentences;
    }

    public List<Sentence>? do_semantic_inference_goal_judgment(Sentence j1, Sentence j2)
    {
        /*
        Derives a new Sentence by performing the appropriate inference rules on the given semantically related sentences.
        The resultant sentence's evidential base == merged from its parents.

        :param j1: Sentence (Goal)
        :param j2: Semantically related belief (Judgment)

        :assume j1 && j2 have distinct evidential bases B1 && B2: B1 ⋂ B2 = Ø
        (no evidential overlap)

        :returns An array of the derived Sentences, or null if the inputs have evidential overlap
        */
        if (this.nars.config.DEBUG) Debug.Log("Trying inference between: " + get_formatted_string(j1) + " && " + get_formatted_string(j2));

        /*
        ===============================================
        ===============================================
        Pre-Processing
        ===============================================
        ===============================================
        */

        if (j1.value.confidence == 0 || j2.value.confidence == 0)
        {
            if (this.nars.config.DEBUG) Debug.Log("Can't do inference between negative premises");
            return null; // can't do inference with 2 entirely negative premises
        }


        List<Sentence> all_derived_sentences = new List<Sentence>();


        Term j1_statement = j1.statement; // goal statement
        StatementTerm j2_statement = j2.get_statement_term();

        Sentence derived_sentence;
        if (!CopulaMethods.is_first_order(j2_statement.get_copula()))
        {
            if (!CopulaMethods.is_symmetric(j2_statement.get_copula()))
            {
                if (j2_statement.get_predicate_term() == j1_statement)
                {
                    // j1 = P!, j2 = S=>P!
                    derived_sentence = this.conditionalRules.ConditionalGoalDeduction(j1, j2);  // :- S! i.e. (P ==> D)
                    add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
                }
                else if (j2_statement.get_subject_term() == j1_statement)
                {
                    // j1 = S!, j2 = (S=>P).
                    derived_sentence = this.conditionalRules.ConditionalGoalInduction(j1, j2);  // :- P! i.e. (P ==> D)
                    add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
                }
            }
        }
        else if (CopulaMethods.is_first_order(j2_statement.get_copula()))
        {
            if (TermConnectorMethods.is_conjunction(j1_statement.connector))
            {
                // j1 = (C &/ S)!, j2 = C. )
                derived_sentence = this.conditionalRules.SimplifyConjunctiveGoal(j1, j2);  // S!
                add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
            }
            else if (j1_statement.connector == TermConnector.Negation)
            {
                // j1 = (--,G)!, j2 = C. )
                if (TermConnectorMethods.is_conjunction(j1.get_compound_statement_term().subterms[0].connector)){
                    // j1 = (--,(A &/ B))!, j2 = A. )
                    derived_sentence = this.conditionalRules.SimplifyNegatedConjunctiveGoal(j1, j2);  // B!
                    add_to_derived_sentences(derived_sentence, all_derived_sentences, j1, j2);
                }
            }
        }
        else
        {
            Asserts.assert(false, "ERROR");
            return null;
        }


        /*
        ===============================================
        ===============================================
        Post-Processing
        ===============================================
        ===============================================
        */

        if (this.nars.config.DEBUG) Debug.Log("Derived " + all_derived_sentences.Count + " inference results.");

        return all_derived_sentences;
    }

    public List<Sentence> do_temporal_inference_two_premise(Sentence A, Sentence B)
    {
        List<Sentence> derived_sentences = new List<Sentence>();

        Sentence derived_sentence;

        derived_sentence = this.temporalRules.TemporalIntersection(A, B); // A &/ B ||  A &/ B || B &/ A
        add_to_derived_sentences(derived_sentence, derived_sentences, A, B);

        derived_sentence = this.temporalRules.TemporalInduction(A, B); // A =|> B || A =/> B || B =/> A
        add_to_derived_sentences(derived_sentence, derived_sentences, A, B);


        /*
        ===============================================
        ===============================================
        Post-Processing
        ===============================================
        ===============================================
        */

        return derived_sentences;
    }


    public List<Sentence>? do_inference_one_premise(Sentence j)
    {
        /*
    Immediate Inference Rules
    Generates beliefs that are equivalent to j but in a different form.

    :param j: Sentence

    :returns An array of the derived Sentences
    */
        List<Sentence> derived_sentences = new List<Sentence>();
        if (j.statement is CompoundTerm || j.stamp.from_one_premise_inference) return derived_sentences; // connectors are too complicated for now
        if (j.get_statement_term().is_first_order()) return derived_sentences; // only higher order
        if (j.get_statement_term().get_subject_term().connector == TermConnector.Negation || j.get_statement_term().get_predicate_term().connector == TermConnector.Negation)
        {
            return derived_sentences;
        }


        if (j is Judgment)
        {
            // Negation (--,(S-->P))
            //  derived_sentence = Immediate.Negation(j)
            //  add_to_derived_sentences(derived_sentence,derived_sentences,j)

            // Conversion (P --> S) or (P ==> S)
            // if ! j.stamp.from_one_premise_inference \
            //         && ! CopulaMethods.is_symmetric(j.statement.get_copula()) \
            //         && j.value.frequency > 0:
            //     derived_sentence = Immediate.Conversion(j)
            //     add_to_derived_sentences(derived_sentence,derived_sentences,j)

            // Contraposition  ((--,P) ==> (--,S))
            if (CopulaMethods.is_implication(j.get_statement_term().get_copula()) && (j.get_statement_term().get_subject_term() is CompoundTerm) && TermConnectorMethods.is_conjunction(j.get_statement_term().get_subject_term().connector))
            {
                Sentence contrapositive = this.immediateRules.Contraposition(j);
                add_to_derived_sentences(contrapositive, derived_sentences, j);
            }

            // contrapositive_with_conversion = Immediate.Conversion(contrapositive)
            // add_to_derived_sentences(contrapositive_with_conversion, derived_sentences, j)

            // Image
            // if (j.statement.get_subject_term(), CompoundTerm) \
            //     && j.statement.get_subject_term().connector == NALSyntax.TermConnector.Product\
            //         && j.statement.get_copula() == NALSyntax.Copula.Inheritance:
            //     derived_sentence_list = Immediate.ExtensionalImage(j)
            //     for derived_sentence in derived_sentence_list:
            //         add_to_derived_sentences(derived_sentence,derived_sentences,j)
            // else if( (j.statement.get_predicate_term(), CompoundTerm) \
            //     && j.statement.get_predicate_term().connector == NALSyntax.TermConnector.Product:
            //     derived_sentence_list = Immediate.IntensionalImage(j)
            //     for derived_sentence in derived_sentence_list:
            //         add_to_derived_sentences(derived_sentence,derived_sentences,j)
        }
        return derived_sentences;
    }


    public void add_to_derived_sentences(Sentence? derived_sentence, List<Sentence> derived_sentence_array, Sentence j1, Sentence? j2 = null)
    {
        /*
            Add derived sentence to array if it meets certain conditions
            :param derived_sentence:
            :param derived_sentence_array:
            :return:
        */
        if (derived_sentence == null) return;  // inference result was not useful
        if (!(derived_sentence is Question) && derived_sentence.value.confidence == 0.0) return; // zero confidence is useless
        derived_sentence_array.Add((Sentence)derived_sentence);

    }

    public string get_formatted_string(Sentence sentence)
    {
        string str = sentence.statement.ToString();
        str += SyntaxUtils.stringValueOf(sentence.punctuation);
        if (sentence.is_event())
        {
            //str = str + " " + SyntaxUtils.stringValueOf(this.get_tense());
        }
        if (sentence.value != null)
        {
            str = str + " " + sentence.value.ToString() + " " + StatementSyntax.ExpectationMarker.ToString() + this.get_expectation(sentence).ToString();
        }
        str = NALSyntax.MARKER_SENTENCE_ID + sentence.stamp.id.ToString() + NALSyntax.MARKER_ID_END + str;
        return str;
    }

    public float get_desirability(Goal sentence)
    {
        return get_expectation(sentence);
    }

    public float get_expectation(Sentence sentence)
    {
        float expectation;
        if (sentence.is_event())
        {
            EvidentialValue time_projected_truth_value = this.get_sentence_value_decayed(sentence);
            expectation = TruthValueFunctions.Expectation(time_projected_truth_value.frequency,
                                                                     time_projected_truth_value.confidence);
        }
        else
        {
            expectation = sentence.eternal_expectation;
        }
        return expectation;
    }

    public bool is_positive(Sentence sentence)
    {
        /*
            :returns: Is this statement true? (does it have more positive evidence than negative evidence?)
        */
        Asserts.assert(!(sentence is Question), "ERROR: Question cannot be positive.");
        bool is_positive = this.get_expectation(sentence) >= this.nars.config.POSITIVE_THRESHOLD;
        return is_positive;
    }

    public bool is_negative(Sentence sentence, float negative_threshold)
    {
        /*
            :returns: Is this statement false? (does it have more negative evidence than positive evidence?)
        */
        Asserts.assert(!(sentence is Question), "ERROR: Question cannot be negative.");
        bool is_negative = this.get_expectation(sentence) < this.nars.config.NEGATIVE_THRESHOLD;
        return is_negative;
    }

    public EvidentialValue get_sentence_value_decayed(Sentence sentence)
    {
        /*
            If this is an event, project its value to the current time
        */
        if (sentence.is_event())
        {
            EvidentialValue present_value = this.nars.inferenceEngine.truthValueFunctions.F_Projection(sentence.value.frequency,
                                                           sentence.value.confidence,
                                                           (int)sentence.stamp.occurrence_time,
                                                           this.nars.current_cycle_number,
                                                           this.nars.config.PROJECTION_DECAY_EVENT);

            return present_value;
        }
        else
        {
            return sentence.value;
        }
    }

}