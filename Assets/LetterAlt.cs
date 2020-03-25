using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class LetterAlt
{
    int rule;
    char letter;
    string text;
    bool compactList = rnd.value < .5;

    KMBombInfo bomb;

    public LetterAlt(KMBombInfo bomb, QuirkInfo qi)
    {
        rule = rnd.Range(0, 10);
        this.bomb = bomb;
        IEnumerable<char> letters = bomb.GetSerialNumberLetters();
        switch (rule)
        {
            case 0:
                {
                    int index = rnd.Range(0, letters.Count());

                    letter = letters.ElementAt(index);
                    text = "Apply the rule corresponding to the " + GetOrdinal(index + 1) + " letter of the Serial Number.";
                    break;
                }
            case 1:
                {
                    text = "Apply the rule corresponding to the Xth letter of the Serial Number, where X is the number of solved modules on the bomb, modulo the number of letters in the Serial Number, plus 1.";
                    break;
                }
            case 2:
                {
                    text = "Apply the rule corresponding to the Xth letter of the Serial Number, where X is your current Key Number, modulo the number of letters in the Serial Number, plus 1.";
                    break;
                }
            case 3:
                {
                    int index = bomb.GetSerialNumberNumbers().Sum() % letters.Count();

                    letter = letters.ElementAt(index);
                    text = "Apply the rule corresponding to the Xth letter of the Serial Number, where X is sum of all the Serial Number digits, modulo the number of letters in the Serial Number, plus 1.";
                    break; 
                }
            case 4:
                {
                    int index = qi.startTime % letters.Count();

                    letter = letters.ElementAt(index);
                    text = "Apply the rule corresponding to the Xth letter of the Serial Number, where X is the bomb's starting time (whole minutes), modulo the number of letters in the Serial Number, plus 1.";
                    break; 
                }
            case 5:
                {
                    List<char> sortedLetters = letters.ToList();
                    sortedLetters.Sort();
                    if(rnd.Range(0, 2) == 0)
                    {
                        letter = sortedLetters.ElementAt(0);
                        text = "Apply the rule corresponding to the letter of the Serial Number that comes " + GetOrdinal(1)+" alphabetically.";
                    }
                    else
                    {
                        letter = sortedLetters.ElementAt(letters.Count() - 1);
                        text = "Apply the rule corresponding to the letter of the Serial Number that comes last alphabetically.";
                    }

                    break;
                }
            case 6:
                {
                    int index = bomb.GetModuleNames().Count() % letters.Count();

                    letter = letters.ElementAt(index);
                    text = "Apply the rule corresponding to the Xth letter of the Serial Number, where X is the number of modules on the bomb, modulo the number of letters in the Serial Number, plus 1.";
                    break; 
                }
            case 7:
                {
                    List<string> names = bomb.GetModuleNames();
                    names.Sort();
                    if (names.Count > 0)
                        if (rnd.Range(0, 2) == 0)
                        {
                            letter = names.ElementAt(0)[0];
                            text = "Apply the rule corresponding to the 1st character of the name of the module on the bomb that comes " + GetOrdinal(1) + " alphabetically (if such rule exists).";
                        }
                        else
                        {
                            letter = names.ElementAt(names.Count() - 1)[0];
                            text = "Apply the rule corresponding to the 1st character of the name of the module on the bomb that comes last alphabetically (if such rule exists).";
                        }
                    else
                    {// Implement failsafe to prevent an IndexOutOfBoundsException
                        letter = ' ';
                        text = "Apply no rule in this instruction. Literally.";
                    }
                    break;
                }
            case 8:
                {
                    List<string> solvablenames = bomb.GetSolvableModuleNames().Where(a => a.RegexMatch(@"^[A-Z]|[a-z]")).ToList();
                    solvablenames.Sort();
                    if (solvablenames.Count > 0)
                        if (rnd.Range(0, 2) == 0)
                        {
                            letter = solvablenames.ElementAt(0)[0];
                            text = "Apply the rule corresponding to the 1st character of the name of the solvable module on the bomb that comes 1st alphabetically, excluding modules that start with digits or symbols.";
                        }
                        else
                        {
                            letter = solvablenames.ElementAt(solvablenames.Count() - 1)[0];
                            text = "Apply the rule corresponding to the 1st character of the name of the solvable module on the bomb that comes last alphabetically, excluding modules that start with digits or symbols.";
                        }
                    else
                    {// Implement failsafe to prevent an IndexOutOfBoundsException
                        letter = ' ';
                        text = "Apply no rule in this instruction. Literally.";
                    }
                    break;
                }
            case 9:
                {
                    List<string> modnames = bomb.GetModuleNames().Where(a => a.RegexMatch(@"^[A-Z]|[a-z]")).ToList();
                    modnames.Sort();
                    if (modnames.Count > 0)
                        if (rnd.Range(0, 2) == 0)
                        {
                            letter = modnames.ElementAt(0)[0];
                            text = "Apply the rule corresponding to the 1st character of the name of the module on the bomb that comes 1st alphabetically, excluding modules that start with digits or symbols.";
                        }
                        else
                        {
                            letter = modnames.ElementAt(modnames.Count() - 1)[0];
                            text = "Apply the rule corresponding to the 1st character of the name of the module on the bomb that comes last alphabetically, excluding modules that start with digits or symbols.";
                        }
                    else
                    {// Implement failsafe to prevent an IndexOutOfBoundsException
                        letter = ' ';
                        text = "Apply no rule in this instruction. Literally.";
                    }
                    break;
                }
            case 10:
                {
                    List<string> names = bomb.GetModuleNames();
                    names.Sort();
                    if (names.Count > 0)
                        if (rnd.Range(0, 2) == 0)
                        {
                            letter = names.ElementAt(0)[0];
                            text = "Apply the rule corresponding to the 1st character of the name of the solvable module on the bomb that comes " + GetOrdinal(1) + " alphabetically (if such rule exists).";
                        }
                        else
                        {
                            letter = names.ElementAt(names.Count() - 1)[0];
                            text = "Apply the rule corresponding to the 1st character of the name of the solvable module on the bomb that comes last alphabetically (if such rule exists).";
                        }
                    else
                    {// Implement failsafe to prevent an IndexOutOfBoundsException
                        letter = ' ';
                        text = "Apply no rule in this instruction. Literally.";
                    }
                    break;
                }
            default:
                {
                    letter = ' ';
                    text = "Apply no rule in this instruction. Literally.";
                    break;
                }
        }
    }

    public char GetLetter(int keyNumber)
    {
        int LetSerNumCnt = bomb.GetSerialNumberLetters().Count();

        if (rule == 1)
            return bomb.GetSerialNumberLetters().ElementAt(bomb.GetSolvedModuleNames().Count() % LetSerNumCnt);
        
        if(rule == 2)
            return bomb.GetSerialNumberLetters().ElementAt(keyNumber - LetSerNumCnt*Mathf.FloorToInt((float)keyNumber / LetSerNumCnt));

        return letter;
    }

    public string GetText()
    {
        return text;
    }

    string GetOrdinal(int i)
    {
        switch(i)
        {
            case 1: return compactList ? "1st" : "first";
            case 2: return compactList ? "2nd" : "second";
            case 3: return compactList ? "3rd" : "third";
            case 4: return compactList ? "4th" : "fourth";
            case 5: return compactList ? "5th" : "fifth";
            case 6: return compactList ? "6th" : "sixth";
            default: return "Xth";
        }
    }
}