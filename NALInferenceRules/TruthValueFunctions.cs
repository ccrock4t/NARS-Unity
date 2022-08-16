/*
==== ==== ==== ==== ==== ====
==== NAL Inference Rules - Truth Value Functions ====
==== ==== ==== ==== ==== ====

    Author: Christian Hahm
    Created: May 13, 2022
    Purpose: Defines the NAL inference rules
            Assumes the given sentences do not have evidential overlap.
            Does combine evidential bases in the Resultant Sentence.
*/
public class TruthValueFunctions
{
    NARS nars;

    public TruthValueFunctions(NARS nars)
    {
        this.nars = nars;
    }


    public delegate EvidentialValue TwoPremiseTruthValueFunction(float f1, float c1, float f2, float c2);
    public delegate EvidentialValue OnePremiseTruthValueFunction(float f, float c);

    public EvidentialValue F_Revision(float f1, float c1, float f2, float c2)
    {
        /*
            :return: F_rev: Truth-Value (f,c)
        */
        (float wp1, float w1, float _) = this.nars.helperFunctions.get_evidence_fromfreqconf(f1, c1);
        (float wp2, float w2, float _) = this.nars.helperFunctions.get_evidence_fromfreqconf(f2, c2);
        // compute values of combined evidence
        float wp = wp1 + wp2;
        float w = w1 + w2;
        (float f_rev, float c_rev) = this.nars.helperFunctions.get_truthvalue_from_evidence(wp, w);
        return new EvidentialValue(f_rev, c_rev);
    }

    public EvidentialValue F_Negation(float f, float c)
    {
        /*
            f_neg = 1 - f
            c_neg = c
            :return: F_neg: Truth-Value (f,c)
        */
        return new EvidentialValue(1 - f, c);
    }


    public EvidentialValue F_Conversion(float f, float c)
    {
        /*
            f_cnv = 1
            c_cnv = (f*c)/(f*c+k)
            :return: F_cnv: Truth-Value (f,c)
        */
        // compute values of combined evidence
        float f_cnv = 1.0f;
        float c_cnv = (f * c) / (f * c + this.nars.config.k);
        return new EvidentialValue(f_cnv, c_cnv);
    }


    public EvidentialValue F_Contraposition(float f, float c)
    {
        /*
            wp = 0
            wn = AND(NOT(f), c)
            :return: F_cnt: Truth-Value (f,c)
        */
        //todo
        return new EvidentialValue(f, ExtendedBooleanOperators.band(new float[] { f, c }));
    }


    public EvidentialValue F_Deduction(float f1, float c1, float f2, float c2)
    {
        /*
            f_ded: AND(f1,f2)
            c_ded: AND(f1,f2,c1,c2)

            :return: F_ded: Truth-Value (f,c)
        */
        float f3 = ExtendedBooleanOperators.band(new float[] { f1, f2 });
        float c3 = ExtendedBooleanOperators.band(new float[] { f1, f2, c1, c2 });
        return new EvidentialValue(f3, c3);
    }

    public EvidentialValue F_Analogy(float f1, float c1, float f2, float c2)
    {
        /*
            f_ana: AND(f1,f2)
            c_ana: AND(f2,c1,c2)

            :return: F_ana: Truth-Value (f,c)
        */
        float f_ana = ExtendedBooleanOperators.band(new float[] { f1, f2 });
        float c_ana = ExtendedBooleanOperators.band(new float[] { f2, c1, c2 });
        return new EvidentialValue(f_ana, c_ana);
    }


    public EvidentialValue F_Resemblance(float f1, float c1, float f2, float c2)
    {
        /*
            f_res = AND(f1,f2)
            c_res = AND(OR(f1,f2),c1,c2)

            :return: F_res: Truth-Value (f,c)
        */
        float f_res = ExtendedBooleanOperators.band(new float[] { f1, f2 });
        float c_res = ExtendedBooleanOperators.band(new float[] { ExtendedBooleanOperators.bor(new float[] { f1, f2 }), c1, c2 });

        return new EvidentialValue(f_res, c_res);
    }


    public EvidentialValue F_Abduction(float f1, float c1, float f2, float c2)
    {
        /*
            wp = AND(f1,f2,c1,c2)
            w = AND(f1,c1,c2)

            :return: F_abd: Truth-Value (f,c)
        */
        float wp = ExtendedBooleanOperators.band(new float[] { f1, f2, c1, c2 });
        float w = ExtendedBooleanOperators.band(new float[] { f1, c1, c2 });
        (float f_abd, float c_abd) = this.nars.helperFunctions.get_truthvalue_from_evidence(wp, w);
        return new EvidentialValue(f_abd, c_abd);
    }


    public EvidentialValue F_Induction(float f1, float c1, float f2, float c2)
    {
        /*
        :return: F_ind: Truth-Value (f,c)
        */
        float wp = ExtendedBooleanOperators.band(new float[] { f1, f2, c1, c2 });
        float w = ExtendedBooleanOperators.band(new float[] { f2, c1, c2 });
        (float f_ind, float c_ind) = this.nars.helperFunctions.get_truthvalue_from_evidence(wp, w);
        return new EvidentialValue(f_ind, c_ind);
    }


    public EvidentialValue F_Exemplification(float f1, float c1, float f2, float c2)
    {
        /*
        :return: F_exe: Truth-Value (f,c)
        */
        float wp = ExtendedBooleanOperators.band(new float[] { f1, f2, c1, c2 });
        float w = wp;
        (float f_exe, float c_exe) = this.nars.helperFunctions.get_truthvalue_from_evidence(wp, w);
        return new EvidentialValue(f_exe, c_exe);
    }


    public EvidentialValue F_Comparison(float f1, float c1, float f2, float c2)
    {
        /*
            :return: F_com: Truth-Value (f,c)
        */
        float wp = ExtendedBooleanOperators.band(new float[] { f1, f2, c1, c2 });
        float w = ExtendedBooleanOperators.band(new float[] { ExtendedBooleanOperators.bor(new float[] { f1, f2 }), c1, c2 });
        (float f3, float c3) = this.nars.helperFunctions.get_truthvalue_from_evidence(wp, w);
        return new EvidentialValue(f3, c3);
    }


    public EvidentialValue F_Intersection(float f1, float c1, float f2, float c2)
    {
        /*
        :return: F_int: Truth-Value (f,c)
        */
        float f_int = ExtendedBooleanOperators.band_average(new float[] { f1, f2 });
        float c_int = ExtendedBooleanOperators.band_average(new float[] { c1, c2 });
        return new EvidentialValue(f_int, c_int);
    }


    public EvidentialValue F_Union(float f1, float c1, float f2, float c2)
    {
        /*
        :return: F_uni: Truth-Value (f,c)
        */
        float f3 = ExtendedBooleanOperators.bor(new float[] { f1, f2 });
        float c3 = ExtendedBooleanOperators.band_average(new float[] { c1, c2 });
        return new EvidentialValue(f3, c3);
    }


    public EvidentialValue F_Difference(float f1, float c1, float f2, float c2)
    {
        /*
        :return: F_dif: Truth-Value (f,c)
        */
        float f3 = ExtendedBooleanOperators.band(new float[] { f1, ExtendedBooleanOperators.bnot(f2) });
        float c3 = ExtendedBooleanOperators.band(new float[] { c1, c2 });
        return new EvidentialValue(f3, c3);
    }


    public EvidentialValue F_Projection(float frequency, float confidence, int t_B, int t_T, float decay)
    {
        /*
            Time Projection

            Project the occurrence time of a belief (t_B)
            to another occurrence time (t_T).

            Same frequency, but lower confidence depending on when it occurred.
        */
        if (t_B == t_T) return new EvidentialValue(frequency, confidence);
        int interval = UnityEngine.Mathf.Abs(t_B - t_T);
        float projected_confidence = confidence * UnityEngine.Mathf.Pow(decay, interval);
        return new EvidentialValue(frequency, projected_confidence);
    }


    public EvidentialValue F_Eternalization(float temporal_frequency, float temporal_confidence)
    {
        float eternal_confidence = temporal_confidence / (this.nars.config.k + temporal_confidence);
        return new EvidentialValue(temporal_frequency, eternal_confidence);
    }

    public static float Expectation(float f, float c)
    {
        /*
            Expectation

            -----------------

             Input:
                f: frequency

                c: confidence

             Returns:
                expectation value
        */
        return c * (f - 0.5f) + 0.5f;
    }



}