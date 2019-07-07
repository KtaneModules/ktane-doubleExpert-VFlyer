using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

class LetterAlt
{
    int rule;
    char letter;
    String text;

    KMBombInfo bomb;

    static System.Random rnd = new System.Random();

    public LetterAlt(KMBombInfo bomb, QuirkInfo qi)
    {
        rule = rnd.Next() % 8;
        this.bomb = bomb;

        switch(rule)
        {
            case 0:
            {
                IEnumerable<char> letters = bomb.GetSerialNumberLetters();
                int index = rnd.Next() % letters.Count();

                letter = letters.ElementAt(index);
                text = "Apply the rule that corresponds to the " + GetOrdinal(index + 1) + " letter of the Serial Number.";
                break;
            }
            case 1:
            {
                text = "Apply the rule that corresponds to the letter X of the Serial Number, where X is the number of solved modules on the bomb, modulo the number of letters in the Serial Number, plus 1.";
                break;
            }
            case 2:
            {
                text = "Apply the rule that corresponds to the letter X of the Serial Number, where X is your current Key Number, modulo the number of letters in the Serial Number, plus 1.";
                break;
            }
            case 3:
            {
                IEnumerable<char> letters = bomb.GetSerialNumberLetters();
                int index = bomb.GetSerialNumberNumbers().Sum() % letters.Count();

                letter = letters.ElementAt(index);
                text = "Apply the rule that corresponds to the letter X of the Serial Number, where X is sum of all the Serial Number digits, modulo the number of letters in the Serial Number, plus 1.";
                break; 
            }
            case 4:
            {
                IEnumerable<char> letters = bomb.GetSerialNumberLetters();
                int index = qi.startTime % letters.Count();

                letter = letters.ElementAt(index);
                text = "Apply the rule that corresponds to the letter X of the Serial Number, where X is the bomb's starting time (whole minutes), modulo the number of letters in the Serial Number, plus 1.";
                break; 
            }
            case 5:
            {
                List<char> letters = bomb.GetSerialNumberLetters().ToList();
                letters.Sort();

                if(rnd.Next() % 2 == 0)
                {
                    letter = letters.ElementAt(0);
                    text = "Apply the rule that corresponds to the letter of the Serial Number that comes first alphabetically.";
                }
                else
                {
                    letter = letters.ElementAt(letters.Count() - 1);
                    text = "Apply the rule that corresponds to the letter of the Serial Number that comes last alphabetically.";
                }

                break;
            }
            case 6:
            {
                IEnumerable<char> letters = bomb.GetSerialNumberLetters();
                int index = bomb.GetModuleNames().Count() % letters.Count();

                letter = letters.ElementAt(index);
                text = "Apply the rule that corresponds to the letter X of the Serial Number, where X is the number of modules on the bomb, modulo the number of letters in the Serial Number, plus 1.";
                break; 
            }
            case 7:
            {
                List<String> names = bomb.GetModuleNames();
                names.Sort();

                if(rnd.Next() % 2 == 0)
                {
                    letter = names.ElementAt(0)[0];
                    text = "Apply the rule that corresponds to the first character of the name of the module on the bomb that comes first alphabetically (if such rule exists).";
                }
                else
                {
                    letter = names.ElementAt(names.Count() - 1)[0];
                    text = "Apply the rule that corresponds to the first character of the name of the module on the bomb that comes last alphabetically (if such rule exists).";
                }

                break;
            }
        }
    }

    public char GetLetter(int keyNumber)
    {
        if(rule == 1)
            return bomb.GetSerialNumberLetters().ElementAt(bomb.GetSolvedModuleNames().Count() % bomb.GetSerialNumberLetters().Count());
        
        if(rule == 2)
            return bomb.GetSerialNumberLetters().ElementAt(keyNumber % bomb.GetSerialNumberLetters().Count());

        return letter;
    }

    public String GetText()
    {
        return text;
    }

    String GetOrdinal(int i)
    {
        switch(i)
        {
            case 1: return "first";
            case 2: return "second";
            case 3: return "third";
            case 4: return "fourth";
            case 5: return "fifth";
            case 6: return "sixth";
        }

        return "";
    }
}