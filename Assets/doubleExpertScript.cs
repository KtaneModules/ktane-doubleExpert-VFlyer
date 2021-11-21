using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class doubleExpertScript : MonoBehaviour
{
    public KMBombInfo bomb;
    public KMAudio Audio;

    public KMSelectable[] btns;
    public KMSelectable switchBtn;
    public GameObject switchObj;
    public GameObject screenObj;
    public TextMesh display;
    public TextMesh keyword;
    public Material[] screenColors;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved = false;

    bool awoken = false;
    bool submiting = false;
    List<string> keywords;
    int currentKeyword = 0;
    int correctKeyword = 0;



    int startTime;
    DayOfWeek day;

    QuirkInfo qi;
    int keyNumber, startKeyNumber;
    InstructionSet[] sets;
    List<char> appliedRules = new List<char>();

    Coroutine setDisplay;
    Coroutine keywordLoop;

    int currentInstructionSet = 0;
    int latestInstructionSet = 0;

    public string GetSouvenirSubmittedWord()
    {
        if (currentInstructionSet != 0)
            return keywords[correctKeyword];
        return "";
    }
    public int GetSouvenirStartingNumber()
    {
        return startKeyNumber;
    }

    void Awake()
    {
        moduleId = moduleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;

        switchBtn.OnInteract += delegate () { FlipSwitch(); return false; };
        btns[0].OnInteract += delegate () { PrevSet(); return false; };
        btns[1].OnInteract += delegate () { NextSet(); return false; };
    }

    void Activate()
    {
        startTime = (int)(bomb.GetTime() / 60);
        day = DateTime.Now.DayOfWeek;
        CheckQuirks();
        GenerateInstructionSets();
        screenObj.transform.GetComponentInChildren<Renderer>().gameObject.SetActive(true);
        setDisplay = StartCoroutine(DisplaySet(currentInstructionSet));
        awoken = true;
    }

    void FlipSwitch()
    {
        if(!awoken)
            return;

        switchBtn.AddInteractionPunch(.5f);
        switchObj.transform.Rotate(0, 180f, 0);
        Audio.PlaySoundAtTransform("switch", transform);

        if (moduleSolved)
            return;

        if (qi.nextIsSwitch)
        {
            Debug.LogFormat("[Double Expert #{0}] Strike! Quirk 5 applies. Can't flip the switch.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        if (qi.unicorn)
        {
            Debug.LogFormat("[Double Expert #{0}] Flipped the switch. Quirk 7 applies. Module solved.", moduleId);
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            StartCoroutine(SolveAnim());
            return;
        }

        if (submiting)
        {
            if (currentKeyword == correctKeyword)
            {
                Debug.LogFormat("[Double Expert #{0}] Correctly submitted {1}. Module solved.", moduleId, keywords.ElementAt(correctKeyword));
                if (keywordLoop != null) StopCoroutine(keywordLoop);
                moduleSolved = true;
                GetComponent<KMBombModule>().HandlePass();
                StartCoroutine(SolveAnim());
            }
            else
            {
                Debug.LogFormat("[Double Expert #{0}] Strike! Incorrectly submitted {1}. Expected {2}.", moduleId, keywords.ElementAt(currentKeyword), keywords.ElementAt(correctKeyword));
                GetComponent<KMBombModule>().HandleStrike();
                RestartModule();
            }
        }
        else
        {
            CalcInstructionSet(latestInstructionSet);
            StartKeywordSubmission();
        }
    }

    void RestartModule()
    {
        submiting = false;
        keyword.text = "";
        if (keywordLoop != null) StopCoroutine(keywordLoop);

        screenObj.transform.GetComponentInChildren<Renderer>().material = screenColors[0];

        currentInstructionSet = 0;
        latestInstructionSet = 0;
        appliedRules = new List<char>();
        GenerateInstructionSets();

/*        startKeyNumber = rnd.Range(0, 40) + 30;
        keyNumber = startKeyNumber;
        Debug.LogFormat("[Double Expert #{0}] ------------Instruction Sets------------", moduleId);

        sets[0] = new KeyNumberSet(keyNumber);

        Debug.LogFormat("[Double Expert #{0}] Instruction set 1 reads: \"{1}\"", moduleId, sets[0].GetText());

        for (int i = 1; i < sets.Length; i++)
            Debug.LogFormat("[Double Expert #{0}] Instruction set {1} reads: \"{2}\"", moduleId, i + 1, sets[i].GetText());
        */
        setDisplay = StartCoroutine(DisplaySet(currentInstructionSet));
    }

    void PrevSet()
    {
        if(!awoken)
            return;

        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        btns[0].AddInteractionPunch(.5f);
        StartCoroutine(HandleButtonAnim(btns[0].gameObject));
        if (moduleSolved)
            return;

        if (submiting)
            return;

        if (currentInstructionSet <= 0)
            return;

        currentInstructionSet--;
        if (setDisplay != null) StopCoroutine(setDisplay);
        setDisplay = StartCoroutine(DisplaySet(currentInstructionSet));
    }

    void NextSet()
    {
        if(!awoken)
            return;

        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        btns[1].AddInteractionPunch(.5f);
        StartCoroutine(HandleButtonAnim(btns[1].gameObject));
        if (moduleSolved)
            return;

        if (qi.unicorn)
        {
            Debug.LogFormat("[Double Expert #{0}] Strike! Quirk 7 applies. Can't press the NEXT button.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        if (currentInstructionSet == sets.Length - 1)
        {
            if (qi.nextIsSwitch)
            {
                if (submiting)
                {
                    if (currentKeyword == correctKeyword)
                    {
                        Debug.LogFormat("[Double Expert #{0}] Correctly submitted {1}. Module solved.", moduleId, keywords.ElementAt(correctKeyword));
                        if (keywordLoop != null) StopCoroutine(keywordLoop);
                        moduleSolved = true;
                        GetComponent<KMBombModule>().HandlePass();
                        StartCoroutine(SolveAnim());
                    }
                    else
                    {
                        Debug.LogFormat("[Double Expert #{0}] Strike! Incorrectly submitted {1}. Expected {2}.", moduleId, keywords.ElementAt(currentKeyword), keywords.ElementAt(correctKeyword));
                        GetComponent<KMBombModule>().HandleStrike();
                        RestartModule();
                    }
                }
                else
                {
                    CalcInstructionSet(latestInstructionSet);
                    StartKeywordSubmission();
                }
            }

            return;
        }

        if (submiting)
            return;

        currentInstructionSet++;
        if (currentInstructionSet > latestInstructionSet)
        {
            CalcInstructionSet(latestInstructionSet);
            latestInstructionSet = currentInstructionSet;
        }
        if (setDisplay != null) StopCoroutine(setDisplay);
        setDisplay = StartCoroutine(DisplaySet(currentInstructionSet));
    }

    void CalcInstructionSet(int set)
    {
        Debug.LogFormat("[Double Expert #{0}] ------------Instruction Set {1}------------", moduleId, set + 1);

        List<char> rules = sets[set].GetLetters(keyNumber);

        if (sets[set].cond == null)
            Debug.LogFormat("[Double Expert #{0}] Instruction Set {1} has no condition. Applying rule(s) [ {2}].", moduleId, set + 1, GetRules(rules));
        else if (sets[set].CheckCondition(keyNumber))
            Debug.LogFormat("[Double Expert #{0}] Instruction Set {1} condition returned true. Applying rule(s) [ {2}].", moduleId, set + 1, GetRules(rules));
        else
            Debug.LogFormat("[Double Expert #{0}] Instruction Set {1} condition returned false. Applying rule(s) [ {2}].", moduleId, set + 1, GetRules(rules));

        foreach (char rule in rules)
        {
            int ruleCnt = appliedRules.Count();
            if (CheckRule(rule))
            {
                ApplyRuleEffects(rule);
                Debug.LogFormat("[Double Expert #{0}] Rule {1} condition returned true. New Key Number is {2}.", moduleId, rule, keyNumber);
            }
            else if (appliedRules.Count() != ruleCnt)
            {
                Debug.LogFormat("[Double Expert #{0}] Rule {1} condition returned false. Key Number doesn't change.", moduleId, rule, keyNumber);
            }
        }
    }

    string GetRules(List<char> rules)
    {
        string ret = "";

        if (rules.Count() == 0)
            return "None ";

        foreach (char rule in rules)
            ret += rule + " ";

        return ret;
    }

    void Start()
    {
        screenObj.transform.GetComponentInChildren<Renderer>().gameObject.SetActive(false);
    }

    void CheckQuirks()
    {
        qi = new QuirkInfo(moduleId, bomb, startTime);
    }

    void GenerateInstructionSets()
    {
        startKeyNumber = rnd.Range(0, 40) + 30;
        keyNumber = startKeyNumber * 1;

        sets = new InstructionSet[7];

        Debug.LogFormat("[Double Expert #{0}] ------------Instruction Sets------------", moduleId);

        sets[0] = new KeyNumberSet(keyNumber);

        Debug.LogFormat("[Double Expert #{0}] Instruction set 1 reads: \"{1}\"", moduleId, sets[0].GetText());

        for (int i = 1; i < sets.Length; i++)
        {
            sets[i] = new InstructionSet(bomb, qi);
            if (i == sets.Length - 1) sets[i].SetFinalText();
            Debug.LogFormat("[Double Expert #{0}] Instruction set {1} reads: \"{2}\"", moduleId, i + 1, sets[i].GetText());
        }
    }

    char GetRandomChar()
    {
        char[] chars = new char[] { '|', '\\', '!', '"', '@', '#', '£', '$', '§', '%', '&', '/', '{', '(', '[', ')', ']', '=', '}', '?', '\'', '»', '«', '<', '>', '€', ',', ';', '.', ':', '-', '_', '*', '+' };
        return chars.PickRandom();
    }

    bool CheckRule(char rule)
    {
        appliedRules.Add(rule);

        switch (rule)
        {
            case 'A': return (bomb.GetSerialNumber().IndexOfAny(qi.vowels.ToArray()) != -1) || qi.mayNinth;
            case 'B': return bomb.GetBatteryCount() > 3 || qi.mayNinth;
            case 'C': return bomb.IsPortPresent(Port.Parallel) && qi.portCondition;
            case 'D': return bomb.GetPortPlateCount() > 2;
            case 'E': return bomb.GetSerialNumber().IndexOfAny(new char[] { '2', '3', '5', '7' }) != -1;
            case 'F': return bomb.IsPortPresent(Port.StereoRCA) && qi.portCondition;
            case 'G': return appliedRules.Count() != 1 ? qi.vowels.Contains(appliedRules.ElementAt(appliedRules.Count() - 2)) : false;
            case 'H': return keyNumber > 11 || qi.mayNinth;
            case 'I': return bomb.IsIndicatorOn(Indicator.BOB) || qi.mayNinth;
            case 'J': return day == DayOfWeek.Wednesday;
            case 'K': return bomb.GetSolvedModuleNames().Count() < 6;
            case 'L': return Math.Abs(keyNumber) % 2 == qi.evenRemainder;
            case 'M': return !bomb.GetPortPlates().Any((x) => x.Length == 0);
            case 'N': return (bomb.GetSolvableModuleNames().Count() - bomb.GetSolvedModuleNames().Count()) < 5;
            case 'O': return bomb.GetSerialNumber().IndexOfAny("DOUBLE".ToArray()) != -1;
            case 'P': return bomb.IsPortPresent(Port.DVI) && qi.portCondition;
            case 'Q': return bomb.GetSerialNumberLetters().Count() > bomb.GetSerialNumberNumbers().Count();
            case 'R': return GetUniqueDigits() >= 2 || qi.mayNinth;
            case 'S': return GetGreatestPortCount() <= 1 && qi.portCondition;
            case 'T': return appliedRules.Count() != 1 ? "PREVIOUS".ToList().Exists(x => x == appliedRules.ElementAt(appliedRules.Count() - 2)) || qi.mayNinth : false;
            case 'U': return ((int)bomb.GetTime() / 60) % 2 == qi.evenRemainder;
            case 'V': return bomb.GetSolvedModuleNames().Count == 5;
            case 'W': return keyNumber == bomb.GetBatteryCount();
            case 'X': return keyNumber < 12;
            case 'Y': return keyNumber < 65 || qi.mayNinth;
            case 'Z': return keyNumber < 0;
        }

        appliedRules.Remove(rule);
        Debug.LogFormat("[Double Expert #{0}] No rule matches character \"{1}\". Ignoring.", moduleId, rule);
        return false;
    }

    void ApplyRuleEffects(int rule)
    {
        switch (rule)
        {
            case 'A': keyNumber -= (bomb.GetSerialNumberNumbers().Sum() + bomb.GetBatteryCount()) * qi.addMultiplier; break;
            case 'B': keyNumber -= bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count() - 1) * qi.addMultiplier; break;
            case 'C': keyNumber += bomb.GetBatteryHolderCount() * qi.addMultiplier; break;
            case 'D': keyNumber += bomb.GetPortCount() * qi.addMultiplier; break;
            case 'E': keyNumber -= (5 + bomb.GetOffIndicators().Count()) * qi.addMultiplier; break;
            case 'F': keyNumber *= bomb.GetOnIndicators().Count(); break;
            case 'G': keyNumber /= 2; break;
            case 'H': keyNumber -= (int)(bomb.GetTime() / 60) * qi.addMultiplier; break;
            case 'I': keyNumber = 0; break;
            case 'J': keyNumber += 10 * qi.addMultiplier; break;
            case 'K': keyNumber += (bomb.GetSolvableModuleNames().Count() - bomb.GetSolvedModuleNames().Count()) * qi.addMultiplier; break;
            case 'L': if (appliedRules.Count() != 1 && appliedRules.ElementAt(appliedRules.Count() - 2) != 'L') ApplyRuleEffects(appliedRules.ElementAt(appliedRules.Count() - 2)); break;
            case 'M': keyNumber -= bomb.GetPortCount() * qi.addMultiplier; break;
            case 'N': keyNumber += bomb.GetSolvedModuleNames().Count() * qi.addMultiplier; break;
            case 'O': keyNumber *= 2; break;
            case 'P': keyNumber += (bomb.GetPortCount() - bomb.GetPortCount(Port.DVI) * qi.addMultiplier) * qi.addMultiplier; break;
            case 'Q': keyNumber -= GetAlphaPositionSun() * qi.addMultiplier; break;
            case 'R': keyNumber -= bomb.GetSerialNumberNumbers().Sum() * qi.addMultiplier; break;
            case 'S': keyNumber += bomb.GetPortPlateCount() * qi.addMultiplier; break;
            case 'T': keyNumber += GetVowelNumber() * bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count() - 1) * qi.addMultiplier; break;
            case 'U': keyNumber = CalcDigitalRoot(keyNumber); break;
            case 'V': keyNumber += 2 * (bomb.GetModuleNames().Count() == bomb.GetSolvableModuleNames().Count() ? 9 : bomb.GetModuleNames().Count() - bomb.GetSolvableModuleNames().Count()) * qi.addMultiplier; break;
            case 'W': keyNumber -= bomb.GetBatteryHolderCount() * qi.addMultiplier; break;
            case 'X': keyNumber *= -1; break;
            case 'Y': keyNumber += (bomb.GetBatteryCount() + bomb.GetIndicators().Count() + bomb.GetPortCount()) * qi.addMultiplier; break;
            case 'Z': keyNumber = (-keyNumber) + (bomb.GetSolvableModuleNames().Count() - bomb.GetSolvedModuleNames().Count()) * qi.addMultiplier; break;
        }
    }

    int GetUniqueDigits()
    {
        List<int> digits = new List<int>();
        IEnumerable<int> sn = bomb.GetSerialNumberNumbers();

        foreach (int digit in sn)
        {
            if ((digit % 2 == (qi.evenRemainder - 1) * -1) && !digits.Exists(x => x == digit))
                digits.Add(digit);
        }

        return digits.Count();
    }

    int GetGreatestPortCount()
    {
        Port[] ports = { Port.DVI, Port.PS2, Port.Parallel, Port.RJ45, Port.Serial, Port.StereoRCA };
        int cnt = 0;
        for (int i = 0; i < ports.Length; i++)
        {
            if (bomb.GetPortCount(ports[i]) > cnt)
                cnt = bomb.GetPortCount(ports[i]);
        }

        return cnt;
    }

    int GetAlphaPositionSun()
    {
        IEnumerable<char> sn = bomb.GetSerialNumberLetters();
        int sum = 0;

        foreach (char letter in sn)
            sum += letter - 64;

        return sum;
    }

    int GetVowelNumber()
    {
        IEnumerable<char> sn = bomb.GetSerialNumberLetters();
        int cnt = 0;

        foreach (char letter in sn)
        {
            if (qi.vowels.Contains(letter))
                cnt++;
        }

        return cnt;
    }

    int CalcDigitalRoot(int value)
    {
        do
        {
            int result = 0;

            for (; value > 9; value /= 10)
                result += value % 10;

            value = result + value;
        }
        while (value > 9);

        return value;
    }

    void StartKeywordSubmission()
    {
        Debug.LogFormat("[Double Expert #{0}] ------------Keywords------------", moduleId);
        Debug.LogFormat("[Double Expert #{0}] Solving keywords for Key Number = {1}.", moduleId, keyNumber);

        submiting = true;
        screenObj.transform.GetComponentInChildren<Renderer>().material = screenColors[1];
        display.text = "";
        Audio.PlaySoundAtTransform("spark", transform);

        keywords = GetRandomKeywords();
        string correct;

        Debug.LogFormat("[Double Expert #{0}] Available keywords: [ {1} ].", moduleId, keywords.Join(", "));

        correctKeyword = Mathf.Max(0,
            Mathf.Min(Mathf.CeilToInt(keyNumber / 15f), 7)
            );
        /*
        if (keyNumber <= 0) correct = keywords.ElementAt(0);
        else if (keyNumber <= 15) correct = keywords.ElementAt(1);
        else if (keyNumber <= 30) correct = keywords.ElementAt(2);
        else if (keyNumber <= 45) correct = keywords.ElementAt(3);
        else if (keyNumber <= 60) correct = keywords.ElementAt(4);
        else if (keyNumber <= 75) correct = keywords.ElementAt(5);
        else if (keyNumber <= 90) correct = keywords.ElementAt(6);
        else correct = keywords.ElementAt(7);
        */
        correct = keywords.ElementAtOrDefault(correctKeyword);
        Debug.LogFormat("[Double Expert #{0}] Correct keyword: {1}.", moduleId, correct);

        keywords = keywords.Shuffle();
        correctKeyword = keywords.IndexOf(correct);
        keywordLoop = StartCoroutine(KeywordLoop(keywords));
    }

    IEnumerator HandleButtonAnim(GameObject anim) // Animate the buttons
    {
        for (int x = 0; x < 5; x++)
        {
            anim.transform.localPosition += Vector3.down / 500;
            yield return new WaitForSeconds(0.01f);
        }
        for (int x = 0; x < 5; x++)
        {
            anim.transform.localPosition += Vector3.up / 500;
            yield return new WaitForSeconds(0.01f);
        }
        yield break;
    }

    string[][] allPossibleWords = new string[][] {
            new string[] { "Apple", "Delta", "Greek", "Juliett", "Maniac", "Papa", "Single", "Victor", "X-ray", "YMCA", "Zulu" },
            new string[] { "Alpha", "Diamond", "Golf", "Jenga", "Mike", "Pope", "Sierra", "Vow", "Xbox", "Yo-Yo", "Zebra" },
            new string[] { "Banana", "Echo", "Hawaii", "Kilo", "Nutmeg", "Quebec", "Triple", "Violet", "X-file", "Ygor", "Zapra" },
            new string[] { "Beta", "Emerald", "Hotel", "Kenya", "November", "Quiet", "Tango", "Vent Gas", "Xcitebike", "Yeet", "Zebstrika" },
            new string[] { "Cherry", "Foxtrot", "Indigo", "Lima", "Otto", "Romeo", "Ultimate", "Whiskey", "X-men", "Yippy", "Zenoblade" },
            new string[] { "Charlie", "Fluorite", "India", "Lingerie", "Oscar", "Rodeo", "Uniform", "Wires", "X-mas", "Yes", "Zelda" },
            new string[] { "Back", "Define", "High", "Jackal", "Monsplode", "Quiper", "Stunt", "Words", "Xenoblade", "YoVile", "Zen Mode" },
            new string[] { "Cabin", "FedEx", "Gothi", "Kojima", "Nominate", "Prequire", "Tuesday", "Wii", "X01", "Yankee", "Zoo" },
            new string[] { "Chocolate", "Diadem", "Half", "Jakarta", "Not", "Rope", "Thursday", "Warsaw", "X", "Yodeling", "Zero" }
    };

    List<string> GetRandomKeywords()
    {
        List<string> ret = new List<string>();
        for (int x=0;x<allPossibleWords.Length;x++)
        {
            ret.Add(allPossibleWords[x].PickRandom());
        }
        return ret;
    }

    IEnumerator DisplaySet(int set)
    {
        string instr = sets[set].GetText();

        int prob = latestInstructionSet - currentInstructionSet;

        display.text = "";

        string[] words = instr.Split(new char[] { ' ' });
        int charCnt = 0;

        for (int i = 0; i < words.Length; i++)
        {
            for (int j = 0; j < words[i].Length; j++)
            {
                if (rnd.Range(0, 100) < prob)
                {
                    char[] chars = words[i].ToCharArray();
                    chars[j] = GetRandomChar();
                    words[i] = new string(chars);
                }
            }
        }

        for (int i = 0; i < words.Length; i++)
        {
            if (charCnt + words[i].Length > 21)
            {
                display.text += "\n";
                charCnt = 0;
            }
            else if (charCnt != 0)
            {
                display.text += ' ';
                charCnt++;
                yield return new WaitForSeconds(0.01f);
            }

            charCnt += words[i].Length;

            for (int j = 0; j < words[i].Length; j++)
            {
                display.text += words[i][j];
                yield return new WaitForSeconds(0.01f);
            }
        }

        yield return true;
    }

    IEnumerator KeywordLoop(List<string> keywords)
    {
        if (setDisplay != null) StopCoroutine(setDisplay);

        while (true)
        {
            string kw = keywords.ElementAt(currentKeyword);

            for (int j = 0; j < kw.Length; j++)
            {
                if (rnd.Range(0, 6) < sets.Length - latestInstructionSet - 1)
                {
                    char[] chars = kw.ToCharArray();
                    chars[j] = GetRandomChar();
                    kw = new string(chars);
                }
            }

            keyword.text = kw;
            yield return new WaitForSeconds(0.8f);
            currentKeyword++;
            if (currentKeyword >= keywords.Count()) currentKeyword = 0;
        }
    }

    IEnumerator SolveAnim()
    {
        Audio.PlaySoundAtTransform("spark", transform);
        Audio.PlaySoundAtTransform("solve", transform);

        float[] delayTimes = new float[] { 0.5f, 0.1f, 0.5f, 0.1f, 0.4f, 0.2f, 0.5f, 0.3f, 0.2f, 0.3f, 0.1f};
        bool souvDetected = bomb.GetSolvableModuleNames().Contains("Souvenir");
        for (int x = 0; x < delayTimes.Length; x++)
        {
            string scrambledText = "";
            int maxLength = rnd.Range(4, 10);
            for (int y = 0; y < maxLength; y++)
                scrambledText += GetRandomChar();
            keyword.text = souvDetected ? scrambledText : keyword.text;
            yield return new WaitForSeconds(delayTimes[x]);
            screenObj.SetActive(x % 2 != 0);
        }
        keyword.text = "";
    }

    //twitch plays
    // Unused
    /*
    private bool isInputValid(string sn)
    {
        foreach (string s in keywords)
        {
            if (s.EqualsIgnoreCase(sn))
            {
                return true;
            }
        }
        return false;
    }
    */
    // Legacy Force Solve Handling.
    /*
    IEnumerator HandleForceSolve()
    {
        if (!(submiting || qi.nextIsSwitch)) // Quirk 5 condition
            switchBtn.OnInteract();
        else
            while (!submiting)
            {
                btns[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        if (moduleSolved) yield break; // Unicorn Rule, AKA April 9th. Quirk 7.
        do
        {
            yield return new WaitForSeconds(0);
        }
        while (currentKeyword != correctKeyword);
        if (!qi.nextIsSwitch) // Quirk 5 condition
            switchBtn.OnInteract();
        else
            btns[1].OnInteract();
        yield return null;
    }
    */
    IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat("[Double Expert #{0}] A force solve has been issued from TP's handler.", moduleId);
        if (!(submiting || qi.nextIsSwitch)) // Quirk 5 condition
            switchBtn.OnInteract();
        else
            while (!submiting)
            {
                btns[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        if (!moduleSolved) // Unicorn Rule, AKA April 9th. Quirk 7.
        {
            while (currentKeyword != correctKeyword)
            {
                yield return true;
            }
            if (!qi.nextIsSwitch) // Quirk 5 condition
                switchBtn.OnInteract();
            else
                btns[1].OnInteract();
            yield return null;
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "To press the \"previous\" button: \"!{0} prev\" or \"!{0} previous\", To press the \"next\" button: \"!{0} next\", \"Press\" is optional.\n"+
        "To flip/toggle the switch any time: \"!{0} toggle\" or \"!{0} flip\", To flip/toggle the switch on a specific keyword: \"!{0} toggle <word>\" or \"!{0} flip <word>\" This will press 'next' at the given word if corresponding quirk rule applies.\n"+
        "To start over or to reset the module ENTIRELY: \"!{0} reset\" or \"!{0} restart\" This WILL create new numbers and/or sets upon using this command!";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLower();
        string[] parameters = command.Split(' ');
        if (command.RegexMatch(@"^res(et|tart)$"))
        {
            yield return null;
            Debug.LogFormat("[Double Expert #{0}] A reset has been issued viva TP handler.", moduleId);
            RestartModule();
        }
        else if (command.RegexMatch(@"^(press )?prev(ious)?$"))
        {
            yield return null;
            btns[0].OnInteract();
            yield break;
        }
        else if (command.RegexMatch(@"^(press )?next$"))
        {
            yield return null;
            btns[1].OnInteract();
            yield break;
        }
        else if (Regex.IsMatch(parameters[0], @"^(toggle|flip)(\s\w*)*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 1)
            {
                yield return null;
                switchBtn.OnInteract();
                yield break;
            }
            if(!submiting)
            {
                yield return "sendtochaterror I haven't got any words to toggle at yet!";
                yield break;
            }
            string curKeyword = "";
            for (int x = 1; x < parameters.Length; x++)
            {
                curKeyword += " " + parameters[x];
            }
            curKeyword = curKeyword.Trim();
            int attemptsChecked = 0;
            int idxDetect = -1;
            string lastDisplay = keywords[currentKeyword];
            yield return null;
            do
            {
                if (lastDisplay != keywords[currentKeyword])
                {
                    lastDisplay = keywords[currentKeyword];
                    attemptsChecked++;
                    if (keywords[currentKeyword].EqualsIgnoreCase(curKeyword))
                        idxDetect = currentKeyword;
                }
                yield return "trycancel The switch wasn't flipped due to a request to cancel.";
            }
            while (attemptsChecked < 19 && idxDetect != currentKeyword);
            if (attemptsChecked < 19)
            {
                if (qi.nextIsSwitch)
                {
                    btns[1].OnInteract();
                }
                else
                {
                    switchBtn.OnInteract();
                }
            }
            else if (currentInstructionSet < sets.Length - 1 && allPossibleWords.Any(a => a.Any(b => b.EqualsIgnoreCase(curKeyword))))
            {
                yield return "sendtochat \"" + curKeyword + "\" does not appear in the set of words given! Because of an early flip AND trying to guess the correct word, you will be penalized.";
                yield return "unsubmittablepenalty";
            }
            else
            {
                yield return "sendtochat \"" + curKeyword + "\" does not appear in the set of words given! You will not be penalized for finding a word that does not exist.";
            }
            yield break;
        }
    }
}
