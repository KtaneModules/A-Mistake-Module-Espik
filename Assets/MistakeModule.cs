using System; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

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
    private int time = 0;
    private String displayTime = "**:**";

    private int lastDigit = 0; // Last digit of the serial number
    private int sumOfDigits = 0; // Sum of the digits of the serial number


    // Ran as bomb loads
    private void Awake() {
        moduleId = moduleIdCounter++;
        Debug.LogFormat("[Mistake #{0}] Uh oh, this isn't supposed to be here.", moduleId);

        // Delegation
        ModuleButton.OnInteract += delegate() {
            PressModule();
            return false;
        };
    }

    // Gets edgework and sets up display
    private void Start() {
        ModuleButton.GetComponent<MeshRenderer>().material = StageTaps[0];
        lastDigit = Bomb.GetSerialNumberNumbers().Last();
        sumOfDigits = Bomb.GetSerialNumberNumbers().Sum();
    }


    // Module is pressed
    private void PressModule() {
        ModuleButton.AddInteractionPunch();

        // Logs time pressed
        time = GetTime();
        Debug.LogFormat("[Mistake #{0}] The module was touched at {1}.", moduleId, displayTime);

        taps++;
        Taps();

        if (taps == 3) {
            StartCoroutine(Pause());
        }
    }

    // Module interaction
    private void Taps() {
        // Current stage
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
        if (!(time % 10 == lastDigit)) {
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
        if (!(time % 60 == sumOfDigits)) {
            Debug.LogFormat("[Mistake #{0}] The module was not touched at the right time! It struck!", moduleId);
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

    // Gets time
    private int GetTime() {
        displayTime = Bomb.GetFormattedTime();
        time = (int) Bomb.GetTime();
        
        return time;
    }


    // Currently unused
    private void Update() {}
}