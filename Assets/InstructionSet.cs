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

    LetterAlt letter;
    public bool runtimeLetter = false;

    public Condition cond;
    public List<char> trueRules = new List<char>();
    public List<char> falseRules = new List<char>();

    public InstructionSet() {}

    public InstructionSet(KMBombInfo bomb, QuirkInfo qi)
    {
        if(rnd.Next() % 2 == 0)
        {
            int prob = rnd.Next() % 10;

            if(prob < 6)
            {
                letter = new LetterAlt(bomb, qi);
                runtimeLetter = true;

                text = letter.GetText() + " Then, press NEXT.";
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
            cond = new Condition(bomb, qi);

            if(rnd.Next() % 2 == 0)
            {
                if(rnd.Next() % 4 == 0)
                {
                    trueRules.Add((char) (rnd.Next() % 26 + 65));
                    trueRules.Add((char) (rnd.Next() % 26 + 65));

                    text = cond.GetText() + "apply rules " + trueRules.ElementAt(0) + " and " + trueRules.ElementAt(1) + ", in that order. Then, press NEXT.";
                }
                else
                {
                    trueRules.Add((char) (rnd.Next() % 26 + 65));

                    text = cond.GetText() + "apply rule " + trueRules.ElementAt(0) + ". Then, press NEXT.";
                } 
            }
            else
            {
                int prob = rnd.Next() % 4;

                if(prob == 0)
                {
                    trueRules.Add((char) (rnd.Next() % 26 + 65));
                    falseRules.Add((char) (rnd.Next() % 26 + 65));

                    text = cond.GetText() + "apply rule " + trueRules.ElementAt(0) + ". Otherwise, apply rule " + falseRules.ElementAt(0) + ". Then, press NEXT.";
                }
                else if (prob == 1)
                {
                    trueRules.Add((char) (rnd.Next() % 26 + 65));
                    trueRules.Add((char) (rnd.Next() % 26 + 65));
                    falseRules.Add((char) (rnd.Next() % 26 + 65));

                    text = cond.GetText() + "apply rules " + trueRules.ElementAt(0) + " and " + trueRules.ElementAt(1) + ", in that order. Otherwise, apply rule " + falseRules.ElementAt(0) + ". Then, press NEXT.";
                }
                else if (prob == 2)
                {
                    trueRules.Add((char) (rnd.Next() % 26 + 65));
                    falseRules.Add((char) (rnd.Next() % 26 + 65));
                    falseRules.Add((char) (rnd.Next() % 26 + 65));

                    text = cond.GetText() + "apply rule " + trueRules.ElementAt(0) + ". Otherwise, apply rules " + falseRules.ElementAt(0) + " and " + falseRules.ElementAt(1) + ", in that order. Then, press NEXT.";
                }
                else
                {
                    trueRules.Add((char) (rnd.Next() % 26 + 65));
                    trueRules.Add((char) (rnd.Next() % 26 + 65));
                    falseRules.Add((char) (rnd.Next() % 26 + 65));
                    falseRules.Add((char) (rnd.Next() % 26 + 65));

                    text = cond.GetText() + "apply rules " + trueRules.ElementAt(0) + " and " + trueRules.ElementAt(1) + ", in that order. Otherwise, apply rules " + falseRules.ElementAt(0) + " and " + falseRules.ElementAt(1) + ", in that order. Then, press NEXT.";
                }
            }
        }
    }

    public virtual string GetText()
    {
        return text;
    }

    public bool CheckCondition(int keyNumber)
    {
        if(cond == null)
            return true;

        return cond.CheckCondition(keyNumber);
    }

    public void SetFinalText()
    {
        text = text.Split(new [] {" Then,"}, StringSplitOptions.None).ElementAt(0) + " Then, flip the switch.";
    }

    public List<char> GetLetters(int keyNumber)
    {
        if(CheckCondition(keyNumber))
        {
            if(runtimeLetter)
                trueRules.Add(letter.GetLetter(keyNumber));
            return trueRules;
        }
        else return falseRules;
    }
}