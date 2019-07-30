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
    List<String> keywords;
    int currentKeyowrd = 0;
    int correctKeyword = 0;

    int startTime;
    DayOfWeek day;

    QuirkInfo qi;
    int keyNumber;
    InstructionSet[] sets;
    List<char> appliedRules = new List<char>();

    Coroutine setDisplay;
    Coroutine keywordLoop;

    int currentInstructionSet = 0;
    int latestInstructionSet = 0;

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
            if (currentKeyowrd == correctKeyword)
            {
                Debug.LogFormat("[Double Expert #{0}] Submitted {1}. Module solved.", moduleId, keywords.ElementAt(correctKeyword));
                if (keywordLoop != null) StopCoroutine(keywordLoop);
                moduleSolved = true;
                GetComponent<KMBombModule>().HandlePass();
                StartCoroutine(SolveAnim());
            }
            else
            {
                Debug.LogFormat("[Double Expert #{0}] Strike! Submitted {1}. Expected {2}.", moduleId, keywords.ElementAt(currentKeyowrd), keywords.ElementAt(correctKeyword));
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

        keyNumber = rnd.Range(0, 40) + 30;

        Debug.LogFormat("[Double Expert #{0}] ------------Instruction Sets------------", moduleId);

        sets[0] = new KeyNumberSet(keyNumber);

        Debug.LogFormat("[Double Expert #{0}] Instruction set 1 reads: \"{1}\"", moduleId, sets[0].GetText());

        for (int i = 1; i < sets.Length; i++)
            Debug.LogFormat("[Double Expert #{0}] Instruction set {1} reads: \"{2}\"", moduleId, i + 1, sets[i].GetText());

        currentInstructionSet = 0;
        latestInstructionSet = 0;
        setDisplay = StartCoroutine(DisplaySet(currentInstructionSet));
    }

    void PrevSet()
    {
        if(!awoken)
            return;

        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        btns[0].AddInteractionPunch(.5f);

        if (moduleSolved)
            return;

        if (submiting)
            return;

        if (currentInstructionSet == 0)
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
                    if (currentKeyowrd == correctKeyword)
                    {
                        Debug.LogFormat("[Double Expert #{0}] Submitted {1}. Module solved.", moduleId, keywords.ElementAt(correctKeyword));
                        if (keywordLoop != null) StopCoroutine(keywordLoop);
                        moduleSolved = true;
                        GetComponent<KMBombModule>().HandlePass();
                        StartCoroutine(SolveAnim());
                    }
                    else
                    {
                        Debug.LogFormat("[Double Expert #{0}] Strike! Submitted {1}. Expected {2}.", moduleId, keywords.ElementAt(currentKeyowrd), keywords.ElementAt(correctKeyword));
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

    String GetRules(List<char> rules)
    {
        String ret = "";

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
        keyNumber = rnd.Range(0, 40) + 30;

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
        return chars[rnd.Range(0, chars.Length)];
    }

    bool CheckRule(char rule)
    {
        appliedRules.Add(rule);

        switch (rule)
        {
            case 'A': return (bomb.GetSerialNumber().IndexOfAny(qi.vowels.ToArray()) != -1) || qi.mayNinth;
            case 'B': return bomb.GetBatteryCount() > 3 || qi.mayNinth;
            case 'C': return bomb.IsPortPresent(Port.Parallel) && qi.portCondition;
            case 'D': return bomb.GetPortPlateCount() > 2 && qi.portCondition;
            case 'E': return bomb.GetSerialNumber().IndexOfAny(new char[] { '2', '3', '5', '7' }) != -1;
            case 'F': return bomb.IsPortPresent(Port.StereoRCA) && qi.portCondition;
            case 'G': return appliedRules.Count() != 1 ? qi.vowels.Contains(appliedRules.ElementAt(appliedRules.Count() - 2)) : false;
            case 'H': return keyNumber > 11 || qi.mayNinth;
            case 'I': return bomb.IsIndicatorPresent(Indicator.BOB) || qi.mayNinth;
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
        Debug.LogFormat("[Double Expert #{0}] No rule matches character {1}. Ignoring.", moduleId, rule);
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
            case 'P': keyNumber += (bomb.GetPortCount() - bomb.GetPortCount(Port.DVI)) * qi.addMultiplier; break;
            case 'Q': keyNumber -= GetAlphaPositionSun() * qi.addMultiplier; break;
            case 'R': keyNumber -= bomb.GetSerialNumberNumbers().Sum() * qi.addMultiplier; break;
            case 'S': keyNumber += bomb.GetPortPlateCount() * qi.addMultiplier; break;
            case 'T': keyNumber += GetVowelNumber() * bomb.GetSerialNumberNumbers().ElementAt(bomb.GetSerialNumberNumbers().Count() - 1) * qi.addMultiplier; break;
            case 'U': keyNumber = CalcDigitalRoot(keyNumber); break;
            case 'V': keyNumber += (bomb.GetModuleNames().Count() - bomb.GetSolvableModuleNames().Count() == 0 ? 9 : bomb.GetModuleNames().Count() - bomb.GetSolvableModuleNames().Count()) * qi.addMultiplier; break;
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
            sum += (int)letter - 64;

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
        String correct;

        Debug.LogFormat("[Double Expert #{0}] Available keywords are [ {1}].", moduleId, GetKeywords(keywords));

        if (keyNumber <= 0) correct = keywords.ElementAt(0);
        else if (keyNumber <= 15) correct = keywords.ElementAt(1);
        else if (keyNumber <= 30) correct = keywords.ElementAt(2);
        else if (keyNumber <= 45) correct = keywords.ElementAt(3);
        else if (keyNumber <= 60) correct = keywords.ElementAt(4);
        else if (keyNumber <= 75) correct = keywords.ElementAt(5);
        else if (keyNumber <= 90) correct = keywords.ElementAt(6);
        else correct = keywords.ElementAt(7);

        Debug.LogFormat("[Double Expert #{0}] Correct keyword is {1}.", moduleId, correct);

        keywords = keywords.OrderBy(x => rnd.Range(0, 1000)).ToList();
        correctKeyword = keywords.IndexOf(correct);
        keywordLoop = StartCoroutine(KeywordLoop(keywords));
    }

    List<String> GetRandomKeywords()
    {
        List<String> ret = new List<string>();

        ret.Add(new String[] { "Apple", "Delta", "Greek", "Juliett", "Maniac", "Papa", "Single", "Victor", "X-ray", "YMCA", "Zulu" }.ToList().OrderBy(x => rnd.Range(0, 1000)).ElementAt(0));
        ret.Add(new String[] { "Alpha", "Diamond", "Golf", "Jenga", "Mike", "Pope", "Sierra", "Vow", "Xbox", "Yo-Yo", "Zebra" }.ToList().OrderBy(x => rnd.Range(0, 1000)).ElementAt(0));
        ret.Add(new String[] { "Banana", "Echo", "Hawaii", "Kilo", "Nutmeg", "Quebec", "Triple", "Violet", "X-file", "Ygor", "Zapra" }.ToList().OrderBy(x => rnd.Range(0, 1000)).ElementAt(0));
        ret.Add(new String[] { "Beta", "Emerald", "Hotel", "Kenya", "November", "Quiet", "Tango", "Vent Gas", "Xcitebike", "Yeet", "Zebstrika" }.ToList().OrderBy(x => rnd.Range(0, 1000)).ElementAt(0));
        ret.Add(new String[] { "Cherry", "Foxtrot", "Indigo", "Lima", "Otto", "Romeo", "Ultimate", "Whiskey", "X-men", "Yippy", "Zenoblade" }.ToList().OrderBy(x => rnd.Range(0, 1000)).ElementAt(0));
        ret.Add(new String[] { "Charlie", "Fluorite", "India", "Lingerie", "Oscar", "Rodeo", "Uniform", "Wires", "X-mas", "Yes", "Zelda" }.ToList().OrderBy(x => rnd.Range(0, 1000)).ElementAt(0));
        ret.Add(new String[] { "Back", "Define", "High", "Jackal", "Monsplode", "Quiper", "Stunt", "Words", "Xenoblade", "YoVile", "Zen Mode" }.ToList().OrderBy(x => rnd.Range(0, 1000)).ElementAt(0));
        ret.Add(new String[] { "Cabin", "FedEx", "Gothi", "Kojima", "Nominate", "Prequire", "Tuesday", "Wii", "X01", "Yankee", "Zoo" }.ToList().OrderBy(x => rnd.Range(0, 1000)).ElementAt(0));
        ret.Add(new String[] { "Chocolate", "Diadem", "Half", "Jakarta", "Not", "Rope", "Thursday", "Warsaw", "X", "Yodeling", "Zero" }.ToList().OrderBy(x => rnd.Range(0, 1000)).ElementAt(0));

        return ret;
    }

    String GetKeywords(List<string> keywords)
    {
        String ret = "";

        foreach (String kw in keywords)
            ret += kw + ", ";

        return ret;
    }

    IEnumerator DisplaySet(int set)
    {
        String instr = sets[set].GetText();

        int prob = latestInstructionSet - currentInstructionSet;

        display.text = "";

        String[] words = instr.Split(new char[] { ' ' });
        int charCnt = 0;

        for (int i = 0; i < words.Length; i++)
        {
            for (int j = 0; j < words[i].Length; j++)
            {
                if (rnd.Range(0, 12) < prob)
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

    IEnumerator KeywordLoop(List<String> keywords)
    {
        if (setDisplay != null) StopCoroutine(setDisplay);

        while (true)
        {
            String kw = keywords.ElementAt(currentKeyowrd);

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
            currentKeyowrd++;
            if (currentKeyowrd == keywords.Count()) currentKeyowrd = 0;
        }
    }

    IEnumerator SolveAnim()
    {
        Audio.PlaySoundAtTransform("spark", transform);
        Audio.PlaySoundAtTransform("solve", transform);

        yield return new WaitForSeconds(0.5f);
        screenObj.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        screenObj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        screenObj.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        screenObj.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        screenObj.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        screenObj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        screenObj.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        screenObj.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        screenObj.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        screenObj.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        screenObj.SetActive(false);
    }

    //twitch plays
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

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} prev/previous [Presses the button 'previous'] | !{0} next [Presses the button 'next'] | !{0} toggle [Flips the switch] | !{0} toggle <word> [Flips the switch at the specified word (Will press 'next' at word if corresponding quirk rule applies)] | !{0} reset [Resets the module, ENTIRELY (new numbers/sets)]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*reset\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Debug.LogFormat("[Double Expert #{0}] Twitch Plays reset called!", moduleId);
            RestartModule();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*toggle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            switchBtn.OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*prev\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*previous\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            btns[0].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*next\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            btns[1].OnInteract();
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*toggle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if(parameters.Length == 2 || parameters.Length == 3)
            {
                if(parameters.Length == 3)
                {
                    string builder = parameters[1]+" "+parameters[2];
                    if((builder.EqualsIgnoreCase("Vent Gas") && isInputValid(builder)) || (builder.EqualsIgnoreCase("Zen Mode") && isInputValid(builder)))
                    {
                        yield return null;
                        while (!keywords[currentKeyowrd].EqualsIgnoreCase(builder)) yield return "trycancel The switch wasn't flipped due to a request to cancel.";
                        if (qi.nextIsSwitch)
                        {
                            btns[1].OnInteract();
                        }
                        else
                        {
                            switchBtn.OnInteract();
                        }
                    }
                }
                else if (isInputValid(parameters[1]))
                {
                    yield return null;
                    while (!keywords[currentKeyowrd].EqualsIgnoreCase(parameters[1])) yield return "trycancel The switch wasn't flipped due to a request to cancel.";
                    if (qi.nextIsSwitch)
                    {
                        btns[1].OnInteract();
                    }
                    else
                    {
                        switchBtn.OnInteract();
                    }
                }
                else
                {
                    if(currentInstructionSet < sets.Length-1)
                    {
                        yield return "unsubmittablepenalty";
                        yield return "That word does not appear in the cycle of words! Because of an early flip, the unsubmittable penalty was applied.";                    }
                    else
                    {
                        yield return "sendtochat That word does not appear in the cycle of words!";
                    }
                }
            }
            yield break;
        }
    }
}