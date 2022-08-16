/*
==== ==== ==== ==== ==== ====
==== NAL Inference Rules - Extended Boolean Operators ====
==== ==== ==== ==== ==== ====

    Author: Christian Hahm
    Created: May 13, 2022
    Purpose: Defines the NAL inference rules
            Assumes the given sentences do not have evidential overlap.
            Does combine evidential bases in the Resultant Sentence.
*/

public static class ExtendedBooleanOperators
{

    public static float band(float[] argv)
    {
        /*
            Boolean AND

            -----------------

            Input:
                argv: NAL Boolean Values

            Returns:
                argv1*argv2*...*argvn
        */
        float res = 1;
        foreach (float arg in argv)
        {
            res *= arg;
        }
        return res;
    }


    public static float band_average(float[] argv)
    {
        /*
           //modified band geometric average
            Boolean AND, with an exponent inversely proportional to the number of terms being ANDed.

            -----------------

            Input:
                argv: NAL Boolean Values

            Returns:
                (argv1*argv2*...*argvn)^(1/n)
        */

        float res = 1;
        foreach (float arg in argv)
        {
            res *= arg;
        }
        
        float exp = 1 / argv.Length;
        return UnityEngine.Mathf.Pow(res, exp);
    }


    public static float bor(float[] argv)
    {
        /*
            Boolean OR

            -----------------

            Input:
                argv: NAL Boolean Values

            Returns:
                 1-((1-argv1)*(1-argv2)*...*(1-argvn))
        */
        float res = 1;
        foreach (float arg in argv)
        {

            res *= (1 - arg);
        }
        return 1 - res;
    }


    public static float bnot(float arg)
    {
        /*
            Boolean Not

            -----------------

            Input:
                arg: NAL Boolean Value

            Returns:
                1 minus arg
        */
        return 1 - arg;
    }
}