using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

class QuirkInfo
{
    public List<char> vowels = new List<char>(new char[] {'A', 'E', 'I', 'O', 'U'});
    public bool portCondition = true;
    public int evenRemainder = 0;
    public int addMultiplier = 1;
    public bool nextIsSwitch = false;
    public bool mayNinth = false;
    public bool unicorn = false;

    public QuirkInfo(int moduleId, KMBombInfo bomb)
    {
        int cnt = 0;
        Debug.LogFormat("[Double Expert #{0}] ------------Quirks------------", moduleId);

        if(bomb.GetIndicators().Count() >= 3)
        {
            cnt++;
            vowels.Add('W');
            vowels.Add('Y');
            Debug.LogFormat("[Double Expert #{0}] Quirk 1 applies.", moduleId);
        }

        if(DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 10)
        {
            cnt++;
            portCondition = false;
            Debug.LogFormat("[Double Expert #{0}] Quirk 2 applies.", moduleId);
        }

        if(bomb.GetSerialNumberLetters().Contains('Y') || bomb.GetSerialNumberLetters().Contains('Z'))
        {
            cnt++;
            evenRemainder = 1;
            Debug.LogFormat("[Double Expert #{0}] Quirk 3 applies.", moduleId);
        }

        if(bomb.GetSolvableModuleNames().Count() > 30)
        {
            cnt++;
            addMultiplier = -1;
            Debug.LogFormat("[Double Expert #{0}] Quirk 4 applies.", moduleId);
        }

        if(DateTime.Now.Day == 1 && DateTime.Now.Month == 4 && (DateTime.Now.Hour * 100 + DateTime.Now.Minute) >= 10 && (DateTime.Now.Hour * 100 + DateTime.Now.Minute) <= 2349)
        {
            cnt++;
            nextIsSwitch = true;
            Debug.LogFormat("[Double Expert #{0}] Quirk 5 applies.", moduleId);
        }

        if(DateTime.Now.Day == 9 && DateTime.Now.Month == 5)
        {
            cnt++;
            mayNinth = true;
            Debug.LogFormat("[Double Expert #{0}] Quirk 6 applies.", moduleId);
        }

        if(DateTime.Now.Day == 9 && DateTime.Now.Month == 4)
        {
            cnt++;
            unicorn = true;
            Debug.LogFormat("[Double Expert #{0}] Quirk 7 applies. Happy National Unicorn Day!", moduleId);
        }

        if(cnt == 0)
        {
            Debug.LogFormat("[Double Expert #{0}] No applicable quirks.", moduleId);
        }
    }
}