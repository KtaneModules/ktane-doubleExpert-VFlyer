using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

class QuirkInfo
{
    public int startTime;
    
    public List<char> vowels = new List<char>(new char[] {'A', 'E', 'I', 'O', 'U'});
    public bool portCondition = true;
    public int evenRemainder = 0;
    public int addMultiplier = 1;
    public bool nextIsSwitch = false;
    public bool mayNinth = false;
    public bool unicorn = false;

    public QuirkInfo(int moduleId, KMBombInfo bomb, int startTime)
    {
        this.startTime = startTime;

        int cnt = 0;
        Debug.LogFormat("[Double Expert #{0}] ------------Quirks------------", moduleId);

        if(bomb.GetIndicators().Count() >= 3 && bomb.GetOffIndicators().Count() == 0)
        {
            cnt++;
            vowels.Add('W');
            vowels.Add('Y');
            Debug.LogFormat("[Double Expert #{0}] Quirk 1 applies. W and Y must be included as vowels. (At least 3 indicators, none of those are lit.)", moduleId);
        }

        if(DateTime.Now.Hour >= 8 && DateTime.Now.Hour <= 10)
        {
            cnt++;
            portCondition = false;
            Debug.LogFormat("[Double Expert #{0}] Quirk 2 applies. Port type/port rules will always return false, port plate rules are unaffected. (Generated within 8 - 10 AM)", moduleId);
        }

        if(bomb.GetSerialNumberLetters().Contains('X') || bomb.GetSerialNumberLetters().Contains('Z'))
        {
            cnt++;
            evenRemainder = 1;
            Debug.LogFormat("[Double Expert #{0}] Quirk 3 applies. Odd and even must be switched in both the manual and the module. (\"X\" or \"Z\" in the serial number.)", moduleId);
        }

        if(bomb.GetSolvableModuleNames().Count() > 30)
        {
            cnt++;
            addMultiplier = -1;
            Debug.LogFormat("[Double Expert #{0}] Quirk 4 applies. Add and subtract must be switched in both the manual and the module. (More than 30 solvable modules present.)", moduleId);
        }

        if(DateTime.Now.Day == 1 && DateTime.Now.Month == 4 && (DateTime.Now.Hour * 100 + DateTime.Now.Minute) >= 10 && (DateTime.Now.Hour * 100 + DateTime.Now.Minute) <= 2349)
        {
            cnt++;
            nextIsSwitch = true;
            Debug.LogFormat("[Double Expert #{0}] Quirk 5 applies. Don't flip the switch at all. Use the \"NEXT\" button to flip and submit instead. (Generated on April 1st within 0:10 - 23:49 of the 24hr clock)", moduleId);
        }

        if(DateTime.Now.Day == 9 && DateTime.Now.Month == 5)
        {
            cnt++;
            mayNinth = true;
            Debug.LogFormat("[Double Expert #{0}] Quirk 6 applies. Rules A, B, D, H, I, R, T, Y must be performed even if the condition return false. (Generated on May 9th)", moduleId);
        }

        if(DateTime.Now.Day == 9 && DateTime.Now.Month == 4)
        {
            cnt++;
            unicorn = true;
            Debug.LogFormat("[Double Expert #{0}] Quirk 7 applies. Happy National Unicorn Day! Just flip the switch and solve it. (Generated on April 9th)", moduleId);
        }

        if(cnt == 0)
        {
            Debug.LogFormat("[Double Expert #{0}] No applicable quirks.", moduleId);
        }
    }
}