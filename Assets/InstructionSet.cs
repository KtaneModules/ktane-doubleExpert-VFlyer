using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

class InstructionSet
{
    static System.Random rnd = new System.Random();

    string text = "";

    public List<char> trueRules = new List<char>();
    public List<char> falseRules = new List<char>();

    public InstructionSet()
    {
        if(rnd.Next() % 2 == 0)
        {
            int prob = rnd.Next() % 10;

            if(prob < 6)
            {

            }
            else if (prob < 9)
            {
                trueRules.Add((char) (rnd.Next() % 26 + 65));
                trueRules.Add((char) (rnd.Next() % 26 + 65));

                text = "Apply rules " + trueRules.ElementAt(0) + " and " + trueRules.ElementAt(1) + ", in that order. Then, press NEXT.";
            }
            else
            {
                trueRules.Add((char) (rnd.Next() % 26 + 65));
                trueRules.Add((char) (rnd.Next() % 26 + 65));
                trueRules.Add((char) (rnd.Next() % 26 + 65));

                text = "Apply rules " + trueRules.ElementAt(0) + ", " + trueRules.ElementAt(1) + " and " + trueRules.ElementAt(2) + ", in that order. Then, press NEXT.";
            }
        }
        else
        {

        }
    }

    public virtual string GetText()
    {
        return text;
    }
}