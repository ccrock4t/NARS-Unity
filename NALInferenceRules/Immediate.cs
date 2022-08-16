/*
==== ==== ==== ==== ==== ====
==== NAL Inference Rules - Immediate Inference Rules ====
==== ==== ==== ==== ==== ====

    Author: Christian Hahm
    Created: May 16, 2022
    Purpose: Defines the NAL inference rules
            Assumes the given sentences do not have evidential overlap.
            Does combine evidential bases in the Resultant Sentence.
*/
using System.Collections.Generic;

public class ImmediateRules
{
    NARS nars;

    public ImmediateRules(NARS nars)
    {
        this.nars = nars;
    }


    public Sentence Negation(Sentence j)
    {
        /*
             Negation

             -----------------

             Input:
               j: Sentence (Statement <f, c>)

             Returns:
        */
        CompoundTerm result_statement = new CompoundTerm(new List<Term>() { j.statement }, TermConnector.Negation);
        return this.nars.helperFunctions.create_resultant_sentence_one_premise(j, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Negation);
    }


    public Sentence Conversion(Sentence j)
    {
        /*
            Conversion Rule

            Reverses the subject && predicate.
            -----------------

            Input:
                j: Sentence (S --> P <f1, c1>)

                must have a frequency above zero, || else the confidence of the conclusion will be zero

            Truth Val{
                w+{ &&(f1,c1)
                w-{ 0
            Returns:
                :- Sentence (P --> S <f2, c2>)
        */
        Asserts.assert_sentence_asymmetric(j);

        // Statement
        StatementTerm result_statement = new StatementTerm(j.get_statement_term().get_predicate_term(),
                                            j.get_statement_term().get_subject_term(),
                                            j.get_statement_term().get_copula());

        return this.nars.helperFunctions.create_resultant_sentence_one_premise(j, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Conversion);
    }


    public Sentence Contraposition(Sentence j)
    {
        /*
        Contraposition
        Inputs{
          j: (S ==> P)

        Frequency must be below one || confidence of conclusion will be zero

        :param j:
        :return: ((--,P) ==> (--,S))
        */
        Asserts.assert_sentence_forward_implication(j);
        // Statement
        CompoundTerm negated_predicate_term = new CompoundTerm(new List<Term>{ j.get_statement_term().get_predicate_term() }, TermConnector.Negation);
        CompoundTerm negated_subject_term = new CompoundTerm(new List<Term> { j.get_statement_term().get_subject_term() }, TermConnector.Negation);

        StatementTerm result_statement = new StatementTerm(negated_predicate_term,
                                            negated_subject_term,
                                            j.get_statement_term().get_copula());

        return this.nars.helperFunctions.create_resultant_sentence_one_premise(j, result_statement, this.nars.inferenceEngine.truthValueFunctions.F_Contraposition);
    }


    public List<Sentence> ExtensionalImage(Sentence j)
    {
        /*
        Extensional Image
        Inputs{
          j: ((*,S,...,P) --> R)

        :param j:
        {Returns: array of
        (S --> (/,R,_,...,P))
        (P --> (/,R,S,...,_))
        ...
        */
        Asserts.assert_sentence_inheritance(j);
        // Statement
        List<Term> statement_subterms = ((CompoundTerm)j.get_statement_term().get_subject_term()).subterms;
        Term R = j.get_statement_term().get_predicate_term();
        return Image(j, statement_subterms, R, TermConnector.ExtensionalImage);
    }

    public List<Sentence> IntensionalImage(Sentence j)
    {
        /*
        Intensional Image
        Inputs{
          j: (R --> (*,S,P))

        :param j:
        {Returns: array of
        ((/,R,_,P) --> S)
        &&
        ((/,R,S,_) --> P)
        */
        Asserts.assert_sentence_inheritance(j);
        List<Sentence> results = new List<Sentence>();
        // Statement
        List<Term> statement_subterms = ((CompoundTerm)j.get_statement_term().get_predicate_term()).subterms;
        Term R = j.get_statement_term().get_subject_term();
        return Image(j, statement_subterms, R, TermConnector.IntensionalImage);
    }

    public List<Sentence> Image(Sentence j, List<Term> statement_subterms, Term R, TermConnector connector)
    {
        List<Sentence> results = new List<Sentence>();
        for (int i1 = 0; i1 < statement_subterms.Count; i1++)
        {
            Term subterm = statement_subterms[i1];

            List<Term> image_subterms = new List<Term> { R };
            for (int i2 = 0; i2 < statement_subterms.Count; i2++)
            {
                if (i1 != i2)
                {
                    image_subterms.Add(statement_subterms[i2]);
                }
                else if (i1 == i2)
                {
                    image_subterms.Add(SyntaxUtils.image_place_holder_term);
                }
            }

            CompoundTerm image_term = new CompoundTerm(image_subterms, TermConnector.ExtensionalImage);
            StatementTerm result_statement = new StatementTerm(subterm, image_term, Copula.Inheritance);

            Sentence result = this.nars.helperFunctions.create_resultant_sentence_one_premise(j, result_statement, null);
            results.Add(result);
        }

        return results;
    }

}