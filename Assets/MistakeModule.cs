using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class MistakeModule : MonoBehaviour {
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    public KMSelectable ModuleButton;
    public Material[] StageTaps;

    // Logging info
    private static int moduleIdCounter = 1;
    private int moduleId;

    // Solving info
    private int taps = 0;

    private int lastDigit = 0; // Last digit of the serial number
    private int sumOfDigits = 0; // Sum of the digits of the serial number

    // Ran as bomb loads
    private void Awake() {
        moduleId = moduleIdCounter++;
        Debug.LogFormat("[Mistake #{0}] Uh oh, this module isn't supposed to be here.", moduleId);

        // Delegation
        ModuleButton.OnInteract += delegate () {
            ModuleButton.AddInteractionPunch();

            // Logs time pressed
            Debug.LogFormat("[Mistake #{0}] The module was touched at {1}.", moduleId, Bomb.GetFormattedTime());

            taps++;
            Taps();

            if (taps == 3) {
                StartCoroutine(Pause());
            }

            return false;
        };
    }

    // Gets edgework and sets up display
    private void Start() {
        ModuleButton.GetComponent<MeshRenderer>().material = StageTaps[0];
        lastDigit = Bomb.GetSerialNumberNumbers().Last();
        sumOfDigits = Bomb.GetSerialNumberNumbers().Sum();

        // Displays solution in log
        Debug.LogFormat("[Mistake #{0}] First, touch the module anytime.", moduleId);
        Debug.LogFormat("[Mistake #{0}] Second, touch the module when the last digit of the timer is {1}.", moduleId, lastDigit);
        Debug.LogFormat("[Mistake #{0}] Third, touch the module when the seconds sections of the timer is {1}.", moduleId, sumOfDigits);
    }

    // Module interaction
    private void Taps() {
        switch (taps) {
            // First tap
            case 1:
                StageOne();
                break;

            // Second tap
            case 2:
                StageTwo();
                break;

            // Third tap
            case 3:
                StageThree();
                break;

            // Consecutive taps
            default:
                PostSolve();
                break;
        }
    }


    // Stage 1
    private void StageOne() {
        ModuleButton.GetComponent<MeshRenderer>().material = StageTaps[1];
        Audio.PlaySoundAtTransform("MistakeModuleShatter", transform);
        Debug.LogFormat("[Mistake #{0}] The module broke a little!", moduleId);
    }

    // Stage 2
    private void StageTwo() {
        ModuleButton.GetComponent<MeshRenderer>().material = StageTaps[2];
        Audio.PlaySoundAtTransform("MistakeModuleShatter", transform);
        Debug.LogFormat("[Mistake #{0}] The module broke a little more!", moduleId);

        // If the last digit of the bomb timer is not equal to the last digit of the serial number
        if (!(((int) Bomb.GetTime()) % 10 == lastDigit)) {
            Debug.LogFormat("[Mistake #{0}] The module was not touched at the right time! It struck!", moduleId);
            Debug.LogFormat("[Mistake #{0}] We told you to touch it when the last digit of the timer is a {1}!", moduleId, lastDigit);
            GetComponent<KMBombModule>().HandleStrike();
        }
    }

    // Stage 3
    private void StageThree() {
        ModuleButton.GetComponent<MeshRenderer>().material = StageTaps[3];
        Audio.PlaySoundAtTransform("MistakeModuleShatter", transform);
        Debug.LogFormat("[Mistake #{0}] The module broke even more!", moduleId);

        // If the seconds section of the bomb timer are not equal to the sum of the digits of the serial number
        if (!(((int) Bomb.GetTime()) % 60 == sumOfDigits)) {
            Debug.LogFormat("[Mistake #{0}] The module was not touched at the right time! It struck!", moduleId);

            if (sumOfDigits < 10)
                Debug.LogFormat("[Mistake #{0}] We told you to touch it when the sections section of the timer were 0{1}!", moduleId, sumOfDigits);

            else
                Debug.LogFormat("[Mistake #{0}] We told you to touch it when the sections section of the timer were {1}!", moduleId, sumOfDigits);

            GetComponent<KMBombModule>().HandleStrike();
        }
    }

    // Stage post-solve
    private void PostSolve() {
        Audio.PlaySoundAtTransform("MistakeModuleShatter", transform);
        Debug.LogFormat("[Mistake #{0}] The module is already broken enough! It struck!", moduleId);
        GetComponent<KMBombModule>().HandleStrike();
    }

    // Module solving
    private void Solve() {
        Debug.LogFormat("[Mistake #{0}] The module has broken enough! It solved!", moduleId);
        GetComponent<KMBombModule>().HandlePass();
    }

    // Waits for 2 seconds before solving
    private IEnumerator Pause() {
        yield return new WaitForSeconds(2f);
        Solve();
    }


    // Twitch Plays - help from Timwi
#pragma warning disable 414
    private string TwitchHelpMessage = "!{0} touch [any time] | !{0} touch 1 [when last digit is 1] | !{0} touch 05 [when seconds section is 05]";
    private bool ZenModeActive;
#pragma warning restore 414

    // Commands
    private IEnumerator ProcessTwitchCommand(string command) {
        Match m;

        // Stage 1 tap
        if (Regex.IsMatch(command, @"^\s*touch\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) {
            yield return null;
            ModuleButton.OnInteract();
        }

        // Stage 2 tap
        else if ((m = Regex.Match(command, @"^\s*touch\s+(\d)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success) {
            yield return null;
            int value = int.Parse(m.Groups[1].Value);

            // !cancel-able
            while (((int) Bomb.GetTime()) % 10 != value)
                yield return "trycancel";

            ModuleButton.OnInteract();
        }

        // Stage 3 tap
        else if ((m = Regex.Match(command, @"^\s*touch\s+(\d\d)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success) {
            int value = int.Parse(m.Groups[1].Value);

            // If arguement is invalid
            if (value >= 60) {
                yield return "sendtochaterror The number of seconds must be 00-59.";
                yield break;
            }

            // Elevator music calculation
            int currentTime = (int) Bomb.GetTime() % 60;

            int interval =
                ZenModeActive ? (value > currentTime ? value - currentTime : value - currentTime + 60)
                              : (currentTime > value ? currentTime - value : currentTime - value + 60);

            yield return null;

            if (interval > 10)
                yield return "waiting music";

            // !cancel-able
            while (((int) Bomb.GetTime()) % 60 != value)
                yield return "trycancel";

            ModuleButton.OnInteract();
        }
    }
}