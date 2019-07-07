using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

class Condition
{
    static System.Random rnd = new System.Random();

    int rule;

    KMBombInfo bomb;
    QuirkInfo qi;

    String text;

    Port port;
    Indicator indicator;
    String word;
    int widgetType1;
    int widgetType2;
    int batteryType1;
    int batteryType2;
    int indicatorType1;
    int indicatorType2;
    int evenOdd;
    int moreLess;
    int lettersDigits;
    int vowelPrime;

    public Condition(KMBombInfo bomb, QuirkInfo qi)
    {
        this.bomb = bomb;
        this.qi = qi;

        rule = rnd.Next() % 27;
        SetRandomPort();
        SetRandomIndicator();
        SetRandomWord();
        widgetType1 = rnd.Next() % 3;
        do {widgetType2 = rnd.Next() % 3;} while(widgetType1 == widgetType2);
        batteryType1 = rnd.Next() % 2;
        batteryType2 = batteryType1 == 1 ? 0 : 1;
        indicatorType1 = rnd.Next() % 2;
        indicatorType2 = indicatorType1 == 1 ? 0 : 1;
        evenOdd = rnd.Next() % 2;
        moreLess = rnd.Next() % 2;
        lettersDigits = rnd.Next() % 2;
        vowelPrime = rnd.Next() % 2;

        switch(rule)
        {
            case 0: text = "If the bomb contains " + GetPortName(true) + " port, "; break;
            case 1: text = "If the bomb has more " + GetWidgetName(widgetType1) + " than " + GetWidgetName(widgetType2) +", "; break;
            case 2: text = "If the bomb has more " + GetBatteryName(batteryType1) + " than " + GetBatteryName(batteryType2) +", "; break;
            case 3: text = "If the bomb has more " + GetIndicatorType(indicatorType1) + " than " + GetIndicatorType(indicatorType2) +", "; break;
            case 4: text = "If the sum of all the Serial Number digits is " + (evenOdd == 0 ? "even" : "odd") + ", "; break;
            case 5: text = "If the sum of all the Serial Number digits is " + (moreLess == 0 ? "more than 15" : "less that 15") + ", "; break;
            case 6: text = "If the current Key Number is " + (evenOdd == 0 ? "even" : "odd") + ", "; break;
            case 7: text = "If the current Key Number is " + (moreLess == 0 ? "more than 50" : "less that 50") + ", "; break;
            case 8: text = "If there are 3 or " + (moreLess == 0 ? "more " : "less ") + GetWidgetName(widgetType1) + " on the bomb, "; break;
            case 9: text = "If the number of " + GetWidgetName(widgetType1) + " on the bomb is " + (evenOdd == 0 ? "even" : "odd") + ", "; break;
            case 10: text = "If the most common port type is " + GetPortName(false) + ", "; break;
            case 11: text = "If there is " + GetIndicatorName() + " indicator on the bomb, "; break;
            case 12: text = "If the number of incurred strikes is " + (evenOdd == 0 ? "even" : "odd") + ", "; break;
            case 13: text = "If there are " + (moreLess == 0 ? "more" : "less") + "solved modules than unsolved modules on the bomb, "; break;
            case 14: text = "If the starting bomb time (whole minutes) is " + (evenOdd == 0 ? "even" : "odd") + ", "; break;
            case 15: text = "If the starting bomb time (whole minutes) is " + (moreLess == 0 ? "more than" : "less than") + " 30, "; break;
            case 16: text = "If the number of modules on the bomb is " + (evenOdd == 0 ? "even" : "odd") + ", "; break;
            case 17: text = "If the number of modules on the bomb is " + (moreLess == 0 ? "more than" : "less than") + " 15, "; break;
            case 18: text = "If the number of " + (lettersDigits == 0 ? "letters" : "digits") + " on the Serial Number is " + (evenOdd == 0 ? "even" : "odd") + ", "; break;
            case 19: text = "If there is a Needy module on the bomb, "; break;
            case 20: text = "If the current Key Number is " + (moreLess == 0 ? "more than" : "less than") + " the sum of all the Serial Number digits, "; break;
            case 21: text = "If the Serial Number contains any character from the word " + word + ", "; break;
            case 22: text = "If there is an empty port plate on the bomb, "; break;
            case 23: text = "If the bomb has duplicate ports, "; break;
            case 24: text = "If there are no " + GetWidgetName(widgetType1) + " on the bomb, "; break;
            case 25: text = "If the Serial Number contains a " + (vowelPrime == 0 ? "vowel" : "prime digit" + " ,"); break;
            case 26: text = "If the number of solved modules on the bomb is " + (moreLess == 0 ? "more than" : "less than") + " " + bomb.GetSolvableModuleNames().Count() + ", "; break;
        }
    }

    public String GetText()
    {
        return text;
    }

    public bool CheckCondition()
    {
        return true;
    }

    void SetRandomPort()
    {
        switch(rnd.Next() % 6)
        {
            case 0: port = Port.DVI; break;
            case 1: port = Port.PS2; break; 
            case 2: port = Port.Parallel; break;
            case 3: port = Port.RJ45; break;
            case 4: port = Port.Serial; break;
            case 5: port = Port.StereoRCA; break;
        }

    }

    String GetPortName(bool type)
    {
        switch(port)
        {
            case Port.DVI: return (type ? "a " : "") + "DVI-D";
            case Port.PS2: return (type ? "a " : "") + "PS/2";
            case Port.Parallel: return (type ? "a " : "") + "Parrallel";
            case Port.RJ45: return (type ? "an " : "") + "RJ-45";
            case Port.Serial: return (type ? "a " : "") + "Serial";
            case Port.StereoRCA: return (type ? "a " : "") + "Stereo RCA";
        }

        return "";
    }

    void SetRandomIndicator()
    {
        switch(rnd.Next() % 11)
        {
            case 0: indicator = Indicator.BOB; break;  
            case 1: indicator = Indicator.CAR; break;
            case 2: indicator = Indicator.CLR; break;
            case 3: indicator = Indicator.FRK; break; 
            case 4: indicator = Indicator.FRQ; break; 
            case 5: indicator = Indicator.IND; break; 
            case 6: indicator = Indicator.MSA; break; 
            case 7: indicator = Indicator.NSA; break; 
            case 8: indicator = Indicator.SIG; break;
            case 9: indicator = Indicator.SND; break;
            case 10: indicator = Indicator.TRN; break;
        }
    }

    String GetIndicatorName()
    {
        switch(indicator)
        {
            case Indicator.BOB: return "a BOB";
            case Indicator.CAR: return "a CAN";
            case Indicator.CLR: return "a CLR";
            case Indicator.FRK: return "an FRK";
            case Indicator.FRQ: return "an FRQ";
            case Indicator.IND: return "an IND";
            case Indicator.MSA: return "an MSA";
            case Indicator.NSA: return "an NSA";
            case Indicator.SIG: return "a SIG";
            case Indicator.SND: return "a SND";
            case Indicator.TRN: return "a TRN";
        }

        return "";
    }

    void SetRandomWord()
    {
        switch(rnd.Next() % 13)
        {
            case 0: word = "BOMB"; break;
            case 1: word = "EXPERT"; break;
            case 2: word = "STRIKE"; break;
            case 3: word = "MANUAL"; break;
            case 4: word = "SWITCH"; break;
            case 5: word = "FLIP"; break;
            case 6: word = "NEXT"; break;
            case 7: word = "PREVIOUS"; break;
            case 8: word = "QUIRK"; break;
            case 9: word = "KEYWORD"; break;
            case 10: word = "RULE"; break;
            case 11: word = "SCREEN"; break;
            case 12: word = "DOUBLE"; break;
        }
    }

    String GetWidgetName(int widgetType)
    {
        switch(widgetType)
        {
            case 0: return "batteries";
            case 1: return "indicators";
            case 2: return "ports";
        }

        return "";
    }

    String GetBatteryName(int batteryType)
    {
        switch(batteryType)
        {
            case 0: return "AA batteries";
            case 1: return "D batteries";
        }

        return "";
    }

    String GetIndicatorType(int indicatorType)
    {
        switch(indicatorType)
        {
            case 0: return "lit indicators";
            case 1: return "unlit indicators";
        }

        return "";
    }
}