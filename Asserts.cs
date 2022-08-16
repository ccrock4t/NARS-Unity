using UnityEngine;

public static class Asserts
{

    public static void assert(bool expression, string errorMessage)
    {
        try
        {
            UnityEngine.Assertions.Assert.IsTrue(expression);
        }
        catch
        {
            //print message if expression is false
            Debug.LogError(errorMessage);
        }
    }

    public static void assert_sentence_forward_implication(Sentence j)
    {
        /*
            ==>, =/>, =\>
    :param j:
    :return:
    */
        assert(!CopulaMethods.is_symmetric(((StatementTerm)j.statement).get_copula()) && !CopulaMethods.is_first_order(((StatementTerm)j.statement).get_copula()), j.statement.ToString() + " must be a forward implication statement");
    }

    public static void assert_sentence_asymmetric(Sentence j)
    {
        /*
            -->, ==>, =/>, =\>
        :param j:
        :return:
        */
        assert(!CopulaMethods.is_symmetric(((StatementTerm)j.statement).get_copula()), j.statement.ToString() + " must be asymmetric");
    }

    public static void assert_sentence_symmetric(Sentence j)
    {
        /*
            <->,<=>,</>
        :param j:
        :return:
        */
        assert(CopulaMethods.is_symmetric(((StatementTerm)j.statement).get_copula()), j.statement.ToString() + " must be symmetric");
    }

    public static void assert_sentence_equivalence(Sentence j)
    {
        /*
            <=> </>
        :param j:
        :return:
        */
        assert(CopulaMethods.is_symmetric(((StatementTerm)j.statement).get_copula()) && !((StatementTerm)j.statement).is_first_order(), j.statement.ToString() + " must be an equivalence statement");
    }

    public static void assert_sentence_similarity(Sentence j)
    {
        /*
            -->
        :param j:
        :return:
        */
        assert(((StatementTerm)j.statement).get_copula() == Copula.Similarity, j.statement.ToString() + " must be a similarity statement");
    }

    public static void assert_sentence_inheritance(Sentence j)
    {
        /*
            -->
        :param j:
        :return:
        */
        assert(((StatementTerm)j.statement).get_copula() == Copula.Inheritance, j.ToString() + " must be an inheritance statement");
    }

    public static void assert_term(object t)
    {
        assert(t is Term, t.ToString() + " must be a Term");
    }

    public static void assert_compound_term(object t)
    {
        assert(t is CompoundTerm, t.ToString() + " must be a Compound Term");
    }

    public static void assert_valid_statement(object t)
    {
        /*
            A valid statement is either a statementTerm or a higher order compound term(a compound of statements)
        :param t:
        :return:
        */
        assert(t is StatementTerm || (t is CompoundTerm && !TermConnectorMethods.is_first_order((TermConnector)((CompoundTerm)t).connector)), t.ToString() + " term must be a valid Statement");
    }

    public static void assert_statement_term(object t)
    {
        assert(t is StatementTerm, t.ToString() + " must be a Statement Term");
    }


    public static void assert_truth_value(object j)
    {
        assert(j is EvidentialValue, j.ToString() + " must be a EvidentialValue");
    }


    public static void assert_punctuation(object j)
    {
        assert(j is Punctuation, j.ToString() + " must be a Punctuation");
    }


    public static void assert_copula(object j)
    {
        assert(j is Copula, j.ToString() + " must be a Copula");
    }

    public static void assert_concept(object c)
    {
        assert(c is Concept, c.ToString() + " must be a Concept");
    }
}
