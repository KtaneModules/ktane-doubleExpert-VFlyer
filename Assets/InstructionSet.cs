using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class InstructionSet
{
    string text = "";

    LetterAlt letter;
    public bool runtimeLetter = false;

    public Condition cond;
    public List<char> trueRules = new List<char>();
    public List<char> falseRules = new List<char>();

    public InstructionSet()
    { // Base Case;
    }

    public InstructionSet(KMBombInfo bomb, QuirkInfo qi)
    {
        if (rnd.Range(0, 2) == 0)
        {
            int prob = rnd.Range(0, 15);

            if(prob < 6 && bomb.GetSerialNumberLetters().Any()) // Check if the prob is under the value AND there are letters in the serial no.
            {
                letter = new LetterAlt(bomb, qi);
                runtimeLetter = true;

                text = letter.GetText() + " Then, press NEXT.";
            }
            else if (prob < 9)
            {
                trueRules.Add((char)(rnd.Range(0, 26) + 65));

                text = "Apply rule " + trueRules.ElementAt(0) + ". Then, press NEXT.";
            }
            else if (prob < 12)
            {
                trueRules.Add((char) (rnd.Range(0, 26) + 65));
                trueRules.Add((char) (rnd.Range(0, 26) + 65));

                text = "Apply rules " + trueRules.ElementAt(0) + " and " + trueRules.ElementAt(1) + ", in that order. Then, press NEXT.";
            }
            else
            {
                trueRules.Add((char) (rnd.Range(0, 26) + 65));
                trueRules.Add((char) (rnd.Range(0, 26) + 65));
                trueRules.Add((char) (rnd.Range(0, 26) + 65));

                text = "Apply rules " + trueRules.ElementAt(0) + ", " + trueRules.ElementAt(1) + " and " + trueRules.ElementAt(2) + ", in that order. Then, press NEXT.";
            }
        }
        else
        {
            cond = new Condition(bomb, qi);

            if(rnd.Range(0, 2) == 0)
            {
                if(rnd.Range(0, 4) == 0)
                {
                    trueRules.Add((char) (rnd.Range(0, 26) + 65));
                    trueRules.Add((char) (rnd.Range(0, 26) + 65));

                    text = cond.GetText() + "apply rules " + trueRules.ElementAt(0) + " and " + trueRules.ElementAt(1) + ", in that order. Then, press NEXT.";
                }
                else
                {
                    trueRules.Add((char) (rnd.Range(0, 26) + 65));

                    text = cond.GetText() + "apply rule " + trueRules.ElementAt(0) + ". Then, press NEXT.";
                } 
            }
            else
            {
                int prob = rnd.Range(0, 4);
                switch (prob)
                {
                    case 0:
                        {// if prob == 0
                            trueRules.Add((char)(rnd.Range(0, 26) + 65));
                            falseRules.Add((char)(rnd.Range(0, 26) + 65));

                            text = cond.GetText() + "apply rule " + trueRules.ElementAt(0) + ". Otherwise, apply rule " + falseRules.ElementAt(0) + ". Then, press NEXT.";
                            break;
                        }
                    case 1:
                        {// if prob == 1
                            trueRules.Add((char)(rnd.Range(0, 26) + 65));
                            trueRules.Add((char)(rnd.Range(0, 26) + 65));
                            falseRules.Add((char)(rnd.Range(0, 26) + 65));

                            text = cond.GetText() + "apply rules " + trueRules.ElementAt(0) + " and " + trueRules.ElementAt(1) + ", in that order. Otherwise, apply rule " + falseRules.ElementAt(0) + ". Then, press NEXT.";
                            break;
                        }
                    case 2:
                        {// if prob == 2
                            trueRules.Add((char)(rnd.Range(0, 26) + 65));
                            falseRules.Add((char)(rnd.Range(0, 26) + 65));
                            falseRules.Add((char)(rnd.Range(0, 26) + 65));

                            text = cond.GetText() + "apply rule " + trueRules.ElementAt(0) + ". Otherwise, apply rules " + falseRules.ElementAt(0) + " and " + falseRules.ElementAt(1) + ", in that order. Then, press NEXT.";
                            break;
                        }
                    default:
                        {
                            trueRules.Add((char)(rnd.Range(0, 26) + 65));
                            trueRules.Add((char)(rnd.Range(0, 26) + 65));
                            falseRules.Add((char)(rnd.Range(0, 26) + 65));
                            falseRules.Add((char)(rnd.Range(0, 26) + 65));

                            text = cond.GetText() + "apply rules " + trueRules.ElementAt(0) + " and " + trueRules.ElementAt(1) + ", in that order. Otherwise, apply rules " + falseRules.ElementAt(0) + " and " + falseRules.ElementAt(1) + ", in that order. Then, press NEXT.";
                            break;
                        }
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
            {
                trueRules = new List<char>();
                trueRules.Add(letter.GetLetter(keyNumber));
            }
            return trueRules;
        }
        else return falseRules;
    }
}