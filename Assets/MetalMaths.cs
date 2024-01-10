using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MetalMaths : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public Renderer[] mrends;
    public Renderer[] symbols;
    public Material[] metals;
    public List<KMSelectable> buttons;
    public Transform[] bpos;
    public Renderer[] phibits;
    public Renderer[] unitselects;
    public TextMesh[] disps;

    private Color c;
    private readonly bool[,] phisegs = new bool[10, 4] { { true, false, false, false}, { false, true, false, false}, { false, false, true, false}, { false, true, true, false}, { false, true, false, true}, { false, false, true, true}, { false, true, true, true}, { true, true, false, false}, { true, false, true, false}, { true, false, true, true} };
    private int ratio;
    private int op;
    private List<int> frec = new List<int> { 0, 1 };
    private bool gen;
    private int[] x;
    private int[] y;
    private string e;
    private string f;
    private int[] p = new int[2];
    private string eval;
    private int[] vals = new int[6];
    private string[] reps = new string[6];
    private int unit = -1;
    private bool push = true;
    private bool[] spin = new bool[2];
    private int dsel;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        ratio = Random.Range(1, 10);
        Debug.LogFormat("[Mathemetallics #{0}] The base is the {1} ratio.", moduleID, new string[] { "", "Golden", "Silver", "Bronze", "Copper", "Nickel", "Aluminium", "Iron", "Tin", "Lead" }[ratio]);
        c = metals[ratio + 8].color;
        unitselects[0].material = metals[18];
        foreach (Renderer m in mrends)
            m.material = metals[ratio - 1];
        for (int i = 0; i < 9; i++)
            if (i != ratio - 1)
                symbols[i].enabled = false;
        for (int i = 1; i < 4; i++)
            phibits[i].enabled = false;
        for (int i = 1; i < Mathf.Max(13 - ratio, 6); i++)
            frec.Add(ratio * frec[i] + frec[i - 1]);
        op = Random.Range(0, ratio < 4 ? 4 : 2);
        module.OnActivate += delegate () { StartCoroutine(Activate()); };
        Debug.Log(string.Join(", ", frec.Select(x => x.ToString()).ToArray()));
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!moduleSolved && unit >= 0 && !push)
                {
                    push = true;
                    switch (b)
                    {
                        case 2:
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                            if (disps[unit + 4].text.Length < 10)
                                disps[unit + 4].text += dsel.ToString();
                            break;
                        case 3:
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                            unitselects[unit].material = metals[18];
                            unit ^= 1;
                            unitselects[unit].material = metals[ratio + 8];
                            break;
                        case 4:
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                            disps[unit + 4].text = "";
                            break;
                        case 5:
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                            Debug.LogFormat("[Mathemetallics #{0}] Submitted {1}.{2}", moduleID, disps[4].text, disps[5].text);
                            if (disps[4].text == reps[4] && disps[5].text == reps[5])
                            {
                                moduleSolved = true;
                                module.HandlePass();
                                for (int i = 0; i < 7; i++)
                                    if (i == 4 || i == 5)
                                    {
                                        disps[i].text = reps[i];
                                        disps[i].color = c;
                                    }
                                    else
                                        disps[i].text = "";
                                for (int i = 0; i < 2; i++)
                                    unitselects[i].material = metals[18];
                                StartCoroutine(Spin(true, true));
                                Audio.PlaySoundAtTransform("Solve", transform);
                            }
                            else
                            {
                                disps[4].text = "";
                                disps[5].text = "";
                                module.HandleStrike();
                                Audio.PlaySoundAtTransform("Bang", transform);
                            }
                            break;
                        default:
                            spin[0] = true;
                            StartCoroutine(Spin(b == 0, false));
                            break;
                    }
                    if (b > 1)
                        StartCoroutine(Push(b - 1));
                }
                return false;
            };
            if(b < 2)
            {
                button.OnInteractEnded += delegate { spin[0] = false; };
            }
        }
    }

    private IEnumerator Activate()
    {
        gen = false;
        if (op > 1)
        {
            x = Generate(3, 4);
            while (gen)
                yield return null;
            e = eval;
            y = Generate(2, 5);
            while (gen)
                yield return null;
            f = eval;
            StartCoroutine(Multiply(e, f));
            p[0] = (x[1] * (ratio * y[1] + y[0])) + (x[0] * y[1]);
            p[1] = (x[1] * y[1]) + (x[0] * y[0]);
        }
        else
        {
            x = Generate(Mathf.Max(8 - ratio, 4), Mathf.Max(8 - ratio, 4));
            while (gen)
                yield return null;
            e = eval;
            y = Generate(Mathf.Max(7 - ratio, 4), Mathf.Max(7 - ratio, 4));
            while (gen)
                yield return null;
            f = eval;
            StartCoroutine(Add(e, f));
            p[0] = x[1] + y[1];
            p[1] = x[0] + y[0];
        }
    }

    private void Complete()
    { 
        vals[2] = y[1];
        vals[3] = y[0];
        reps[2] = string.Join("", f.TakeWhile(k => k != '.').Select(k => k.ToString()).ToArray());
        reps[3] = string.Join("", f.SkipWhile(k => k != '.').Skip(1).Select(k => k.ToString()).ToArray());
        if (op % 2 == 0)
        {
            vals[0] = x[1];
            vals[1] = x[0];
            reps[0] = string.Join("", e.TakeWhile(k => k != '.').Select(k => k.ToString()).ToArray());
            reps[1] = string.Join("", e.SkipWhile(k => k != '.').Skip(1).Select(k => k.ToString()).ToArray());
            vals[4] = p[0];
            vals[5] = p[1];
            reps[4] = string.Join("", eval.TakeWhile(k => k != '.').Select(k => k.ToString()).ToArray());
            reps[5] = string.Join("", eval.SkipWhile(k => k != '.').Skip(1).Select(k => k.ToString()).ToArray());
        }
        else
        {
            vals[4] = x[1];
            vals[5] = x[0];
            reps[4] = string.Join("", e.TakeWhile(k => k != '.').Select(k => k.ToString()).ToArray());
            reps[5] = string.Join("", e.SkipWhile(k => k != '.').Skip(1).Select(k => k.ToString()).ToArray());
            vals[0] = p[0];
            vals[1] = p[1];
            reps[0] = string.Join("", eval.TakeWhile(k => k != '.').Select(k => k.ToString()).ToArray());
            reps[1] = string.Join("", eval.SkipWhile(k => k != '.').Skip(1).Select(k => k.ToString()).ToArray());
        }
        for (int i = 0; i < 4; i++)
            disps[i].text = reps[i];
        disps[6].text = "+-*/"[op].ToString();
        Debug.LogFormat("[Mathemetallics #{0}] The displayed equation is {1}.{2} {3} {4}.{5} = ?", moduleID, reps[0], reps[1], "+-x/"[op], reps[2], reps[3]);
        Debug.LogFormat("[Mathemetallics #{0}] Converting the top screen yields {1}\u03c1 + {2}", moduleID, vals[0], vals[1]);
        Debug.LogFormat("[Mathemetallics #{0}] Converting the middle screen yields {1}\u03c1 + {2}", moduleID, vals[2], vals[3]);
        Debug.LogFormat("[Mathemetallics #{0}] The solution to the equation is {1}\u03c1 + {2}", moduleID, vals[4], vals[5]);
        Debug.LogFormat("[Mathemetallics #{0}] Enter {1}.{2} on the bottom screen.", moduleID, reps[4], reps[5]);
        StartCoroutine("Startup");        
    }

    private int[] Generate(int l, int d)
    {
        int r = 0;
        int[] x = new int[2];
        eval = "";
        for (int i = l; i > 0; i--)
        {
            r = Random.Range(0, ratio + 1);
            x[0] += r * frec[i - 1];
            x[1] += r * frec[i];
            eval += r.ToString();
        }
        r = Random.Range(0, ratio + 1);
        x[0] += r;
        eval += r.ToString();
        eval += ".";
        for (int i = 1; i < d; i++)
        {
            int s = (2 * (i % 2)) - 1;
            r = Random.Range(0, ratio + 1);
            x[1] += s * r * frec[i];
            x[0] += -s * r * frec[i + 1];
            eval += r.ToString();
        }
        StartCoroutine(Minimise(eval, false));
        return x;
    }

    private int[] Digitise(string v)
    {
        int[] d = new int[20];
        int[] l = new int[2] { 10 - v.TakeWhile(k => k != '.').Count(), 11 - v.SkipWhile(k => k != '.').Count() };
        for (int i = 0; i < l[0]; i++)
            v = "0" + v;
        for (int i = 0; i < l[1]; i++)
            v += "0";
        v = v.Remove(10, 1);
        Debug.Log(v);
        for (int i = 0; i < 20; i++)
            d[i] = v[i] - '0';
        return d;
    }

    private IEnumerator Minimise(string v, bool c)
    {
        gen = true;
        int[] d = Digitise(v);
        int[] d2 = new int[20];
        bool failure = false;
        while (Enumerable.Range(0, 20).Any(x => d[x] != d2[x]))
        {
            for (int i = 0; i < 20; i++)
                d2[i] = d[i];
            for (int i = 0; i < 18; i++)
                if (d[i + 1] >= ratio && d[i + 2] > 0)
                {
                    d[i]++;
                    d[i + 1] = 0;
                    d[i + 2]--;
                }
            if (d[0] >= ratio && d[1] > 0)
            {
                failure = true;
                break;
            }
            yield return null;
        }
        if (failure)
        {
            Debug.Log("Minimisation Failed: Try Again!");
            StartCoroutine(Activate());
            yield break;
        }
        else 
        {
            string[] r = new string[2];
            r[0] = string.Join("", d.Take(10).SkipWhile(x => x == 0).Select(x => x.ToString()).ToArray());
            r[1] = string.Join("", d.Reverse().Take(10).SkipWhile(x => x == 0).Reverse().Select(x => x.ToString()).ToArray());
            if (r[0].Length < 1)
                r[0] = "0";
            if (r[1].Length > 0)
                r[1] = "." + r[1];
            eval = r[0] + r[1];
            gen = false;
            if (c)
                Complete();
        }
    }

    private IEnumerator Add(string x, string y)
    {
        gen = true;
        string z = "";
        int[] d = Digitise(x);
        int[] d2 = Digitise(y);
        for (int i = 0; i < 20; i++)
            d[i] += d2[i];
        Debug.Log(x + " + " + y + " = " + string.Join("", d.Select(k => k.ToString()).ToArray()));
        bool failure = false;
        for (int i = 0; i < 20; i++)
            d2[i] = d[i];
        while (d.Any(k => k > ratio))
        {
            for (int i = 1; i < 18; i++)
                if (d[i] > ratio)
                {
                    d2[i - 1]++;
                    d2[i] -= ratio + 1;
                    d2[i + 1] += ratio - 1;
                    d2[i + 2]++;
                }
            if (d2[0] > ratio || d2[18] > ratio || d2[19] > ratio)
            {
                failure = true;
                break;
            }
            else
                for (int i = 0; i < 20; i++)
                    d[i] = d2[i];
            Debug.Log(string.Join("", d.Select(k => k.ToString()).ToArray()));
            yield return null;
        }
        if (failure)
        {
            Debug.Log("Addition Failed: Try Again!");
            eval = "";
            StartCoroutine(Activate());
            yield break;
        }
        else
        {
            for (int i = 0; i < 10; i++)
                z += d[i].ToString();
            z += ".";
            for (int i = 10; i < 20; i++)
                z += d[i].ToString();
            StartCoroutine(Minimise(z, true));
            yield break;
        }
    }

    private IEnumerator Multiply(string x, string y)
    {
        gen = true;
        string z = "";
        int[] d = Digitise(x);
        int[] d2 = Digitise(y);
        int[] d3 = new int[20];
        for (int i = 0; i < 20; i++)
        {
            if (d[i] < 1)
                continue;
            for (int j = 0; j < 20; j++)
            {
                if (i + j < 9)
                    continue;
                else if (i + j >= 20)
                    break;
                d3[i + j - 9] += d[i] * d2[j];
            }
        }
        Debug.Log(x + " x " + y + " = " + string.Join("", d3.Select(k => k.ToString()).ToArray()));
        bool failure = false;
        int[] d4 = new int[20];
        for (int i = 0; i < 20; i++)
            d4[i] = d3[i];
        while (d3.Any(k => k > ratio))
        {
            for (int i = 1; i < 18; i++)
                if (d3[i] > ratio)
                {
                    d4[i - 1]++;
                    d4[i] -= ratio + 1;
                    d4[i + 1] += ratio - 1;
                    d4[i + 2]++;
                }           
            if (d4[0] > ratio || d4[18] > ratio || d4[19] > ratio)
            {
                failure = true;
                break;
            }
            else
                for (int i = 0; i < 20; i++)
                    d3[i] = d4[i];
            Debug.Log(string.Join("", d3.Select(k => k.ToString()).ToArray()));
            yield return null;
        }
        if (failure)
        {
            Debug.Log("Multiplication Failed: Try Again!");
            eval = "";
            StartCoroutine(Activate());
            yield break;
        }
        else
        {
            for (int i = 0; i < 10; i++)
                z += d3[i].ToString();
            z += ".";
            for (int i = 10; i < 20; i++)
                z += d3[i].ToString();
            StartCoroutine(Minimise(z, true));
            yield break;
        }
    }

    private IEnumerator Startup()
    {
        if (moduleID == moduleIDCounter)
            Audio.PlaySoundAtTransform("Start", transform);
        float e = 0;
        while(e < 1)
        {
            e += Time.deltaTime;
            Color s = c * e;
            foreach (TextMesh t in disps)
                t.color = s;
            yield return null;
        }
        for (int i = 0; i < 7; i++)
            StartCoroutine("Flicker", i);
        push = false;
        unit = 0;
        unitselects[0].material = metals[ratio + 8];
    }

    private IEnumerator Flicker(int i)
    {
        while (!moduleSolved)
        {
            yield return new WaitForSeconds(Random.Range(0f, 0.5f));
            float a = Random.Range(0.75f, 1f);
            disps[i].color = a * c;
            yield return null;
            disps[i].color = c;
        }
    }

    private IEnumerator Spin(bool r, bool g)
    {
        float v = g ? 0.5f : 6;
        int s = r ? 1 : -1;
        do
        {
            if (!g)
                Audio.PlaySoundAtTransform("Spin", bpos[0]);
            float p = bpos[0].localEulerAngles.z;
            int f = spin[1] ? 0 : 4;
            dsel += r ? 1 : ratio;
            dsel %= ratio + 1;
            for (int i = 0; i < 4; i++)
                phibits[f + i].enabled = g ? true : phisegs[dsel, i];
            float e = 0;
            while (e < 1)
            {
                e += Time.deltaTime * v;
                bpos[0].localEulerAngles = new Vector3(0, 0, r ? Mathf.Lerp(p, p + 180, e) : Mathf.Lerp(p, p - 180, e));
                yield return null;
            }
            bpos[0].localEulerAngles = new Vector3(0, 0, p + 180 * s);
            spin[1] ^= true;
        } while (spin[0]);
        push = false;
    }

    private IEnumerator Push(int i)
    {
        float e = 0;
        Vector3 p = bpos[i].localPosition;
        while(e < 1)
        {
            e += Time.deltaTime * 8f;
            bpos[i].localPosition = new Vector3(p.x, Mathf.Lerp(-0.0047f, -0.0027f, e), p.z);
            yield return null;
        }
        bpos[i].localPosition = p;
        push = false;
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} enter # [Types digits onto selected screen] | !{0} switch [Changes selected screen] | !{0} clear [Removes digits from selected screen] | !{0} submit";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if(commands.Length > 2)
        {
            yield return "sendtochaterror!f Invalid command length.";
            yield break;
        }
        yield return null;
        switch (commands[0])
        {
            case "switch":
                buttons[3].OnInteract();
                yield break;
            case "clear":
                buttons[4].OnInteract();
                yield break;
            case "submit":
                buttons[5].OnInteract();
                yield break;
            case "enter":
                if(disps[unit + 4].text.Length > 9)
                {
                    yield return "sendtochaterror!f Screen must be cleared before further entry.";
                    yield break;
                }
                if(commands[1].Any(x => !"0123456789".Contains(x.ToString())))
                {
                    yield return "sendtochaterror!f Only numeric digits may be entered onto a screen.";
                    yield break;
                }
                if(commands[1].Any(x => x - '0' > ratio))
                {
                    yield return "sendtochaterror!f Only digits in the range 0 - " + ratio + " are present.";
                    yield break;
                }
                int[] r = new int[commands[1].Length];
                commands[1] = dsel.ToString() + commands[1];
                for (int i = 0; i < r.Length; i++)
                {
                    int g = (commands[1][i + 1] - '0') - (commands[1][i] - '0');
                    g += ratio + 1;
                    g %= ratio + 1;
                    r[i] = (g > (ratio + 1) / 2) ? 1 : 0;
                }
                for(int i = 0; i < r.Length; i++)
                {
                    if(dsel == commands[1][i + 1] - '0')
                    {
                        yield return null;
                        buttons[2].OnInteract();
                        while (push)
                            yield return null;
                        continue;
                    }
                    yield return null;
                    buttons[r[i]].OnInteract();
                    Debug.Log("Spin");
                    yield return null;
                    while (dsel != commands[1][i + 1] - '0')
                        yield return null;
                    buttons[r[i]].OnInteractEnded();
                    while (push)
                        yield return null;
                    buttons[2].OnInteract();
                    if(disps[unit + 4].text.Length > 9)
                    {
                        yield return "sendtochat!f Entry limit reached.";
                        yield break;
                    }
                    while (push)
                        yield return null;
                }
                yield break;
            default:
                yield return "sendtochaterror!f Invalid command.";
                yield break;
        }
    }


}
