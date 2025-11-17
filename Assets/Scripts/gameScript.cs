using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class gameScript : MonoBehaviour
{

    [Header("Pieces")]
    public List<Move> pieces;
    [SerializeField] private int playerIndex = 2;
    [SerializeField] private TMP_Dropdown targetDropDown;

    [Header("UI")]
    public Button turnExec;
    public Button turnCont;
    public TMP_Text rollText;
    [SerializeField] private GameObject selectMenu; 
    [SerializeField] private Button waspButton;
    [SerializeField] private Button cannonButton;
    [SerializeField] private Button jetButton;
    [SerializeField] private Button homerButton;

    [Header("Rules")]
    [SerializeField] private int spacesToWin = 100;
    [SerializeField] private float pause = 1.2f;
    //controls how long between pauses
    public bool fastTurn = false;
    //skips confirmations for testing purposes

    [Header("Combat")]
    [SerializeField] private bool combatEnabled = true;
    [SerializeField] private int maxShields = 3;
    [SerializeField] private bool attackBeforeMove = true;
    [SerializeField] private Toggle fastTurnToggle;
    [SerializeField] private Toggle combatToggle;
    

    private bool turnInProgress = false;
    private bool gameOver = false;
    private bool continuePressed = false;
    private int[] spacesMoved;
    private int[] shields;
    private bool[] revive;
    private float basePause;
    private bool pendingCombatEnabled;

    void Start()
    {

        turnExec.gameObject.SetActive(false);
        turnCont.gameObject.SetActive(false);
        if (selectMenu != null) selectMenu.SetActive(true);

        waspButton.onClick.AddListener(() => ChooseShip("Wasp"));
        cannonButton.onClick.AddListener(() => ChooseShip("Cannon"));
        jetButton.onClick.AddListener(() => ChooseShip("Jet"));
        homerButton.onClick.AddListener(() => ChooseShip("Homer"));
        int n = pieces.Count;
        spacesMoved = new int[n];
        shields = new int[n];
        revive = new bool[n];

        for (int i = 0; i < n; i++)
        {
            spacesMoved[i] = 0;
            shields[i] = maxShields;
            revive[i] = false;
        }
        ClearBanner();
        turnExec.onClick.AddListener(() => StartCoroutine(Turn()));
        turnCont.onClick.AddListener(OnContinueClicked);
        turnCont.gameObject.SetActive(false);
        targetDropDown.gameObject.SetActive(false);

        basePause = pause;

        if (fastTurnToggle != null)
        {
            fastTurnToggle.isOn = fastTurn;
            fastTurnToggle.onValueChanged.AddListener(OnFastTurnToggled);
        }
        ApplyFast();

        if (combatToggle != null)
        {
            combatToggle.isOn = combatEnabled;
            combatToggle.onValueChanged.AddListener(OnCombatToggled);
        }
    }

    private void OnCombatToggled(bool isOn)
    {
        if (!turnInProgress)
    {
        combatEnabled = isOn;
        pendingCombatEnabled = isOn;
        Banner($"Combat {(combatEnabled ? "enabled" : "disabled")}");
    }
    else
    {
        pendingCombatEnabled = isOn;
        Banner($"Combat will be {(isOn ? "enabled" : "disabled")} next turn");
    }
    }
    private void OnFastTurnToggled(bool isOn)
    {
        fastTurn = isOn;
        ApplyFast();

    }
    private void ApplyFast()
    {
        
        if (basePause <= 0f) basePause = 1.2f;
        pause = fastTurn ? (basePause / 2f) : basePause;
    }

    private void OnContinueClicked()
    {
        continuePressed = true;
    }

    private IEnumerator Turn()
    {
        if (turnInProgress || gameOver) yield break; // prevent overlapping turns
        turnInProgress = true;

        if (combatEnabled != pendingCombatEnabled)
        {
            combatEnabled = pendingCombatEnabled;
            Banner($"Combat has been {(combatEnabled ? "enabled" : "disabled")} for this turn.");
        }
        yield return new WaitForSeconds(pause);

        turnExec.gameObject.SetActive(false);
        Banner("Starting turn...");
        yield return new WaitForSeconds(pause);
        ClearBanner();
        

        int winnerIndex = -1;

        // For each piece in the game
        for (int i = 0; i < pieces.Count; i++)
        {
            if (gameOver) break;
            Move piece = pieces[i];
            FollowCam.POI = piece.gameObject;

            if (revive[i])
            {
                Banner($"{piece.name} has to restore power");
                yield return new WaitForSeconds(pause);
                shields[i] = maxShields;
                revive[i] = false;
                
                yield return new WaitForSeconds(pause);
                continue;
            }

            if (combatEnabled && attackBeforeMove)
            {
                int targetID = -1;
                if (i == playerIndex)
                {
                    bool done = false;
                    yield return selectTargets(i, sel => { targetID = sel; done = true; });
                    while (!done) yield return null;
                }
                else
                {
                    targetID = PickNearest(i);
                }
                if (targetID != -1)
                {
                    yield return AttackOnce(i, targetID);
                }
                else
                {
                    Banner($"{piece.name} has no valid target");
                    yield return new WaitForSeconds(pause);

                }
            }
            int roll = Random.Range(1, 7);
            int move = roll;
            if (spacesMoved[i] > 8 && spacesMoved[i] < 13)
            {
                move = Mathf.Max(1, roll - 1);
                Banner($"{piece.name} rolled a {roll}! Due to the black holes gravity they've been slowed!");
                yield return new WaitForSeconds(basePause);
                Banner($"{piece.name} is only moving {move} spaces");
                
            }
            else
            {
                Banner($"{piece.name} rolled a {roll}!");
            }

            

            if (!fastTurn)
            {
                continuePressed = false;
                turnCont.gameObject.SetActive(true);
                yield return new WaitUntil(() => continuePressed);
                turnCont.gameObject.SetActive(false);
                
            }
            else
            {
                yield return new WaitForSeconds(pause);
            }

            // Move that piece roll times
            for (int step = 0; step < move; step++)
            {
                piece.MoveForward();
                spacesMoved[i] += 1;
                yield return new WaitForSeconds(pause / 2); // small pause between each step

                if (spacesMoved[i] >= spacesToWin)
                {
                    gameOver = true;
                    winnerIndex = i;
                    break;
                }
            }

            if (gameOver) break;
            yield return new WaitForSeconds(pause);
        }

        if (gameOver && winnerIndex != -1)
        {
            Move winner = pieces[winnerIndex];
            FollowCam.FollowDefault(winner.gameObject);
            Banner($"{winner.name} wins!");
            turnInProgress = false;
            yield break;
        }
        Move leader = pieces[0];
        foreach (Move piece in pieces)
        {
            if (piece.transform.position.x > leader.transform.position.x)
                leader = piece;
        }

        FollowCam.FollowDefault(leader.gameObject);

        int leaderIndex = pieces.IndexOf(leader);
        int leaderSpaces = spacesMoved[leaderIndex];
        Banner($"{leader.name} is in the lead!\n They are at {leaderSpaces} out of {spacesToWin} to win.");
        turnInProgress = false;
        turnExec.gameObject.SetActive(true);
    }

    private List<int> GetTargets(int attackerID)
    {
        var result = new List<int>();
        float attacker = pieces[attackerID].transform.position.x;
        for (int i = 0; i < pieces.Count; i++)
        {
            if (i == attackerID) continue;
            if (pieces[i].transform.position.x > attacker)
                result.Add(i);
        }
        return result;
    }
    private int PickNearest(int attackerID)
    {
        float attacker = pieces[attackerID].transform.position.x;
        int bestID = -1;
        float bestDelta = float.PositiveInfinity;

        for (int i = 0; i < pieces.Count; i++)
        {
            if (i == attackerID) continue;

            float tjx = pieces[i].transform.position.x;
            if (tjx < attacker || tjx == attacker) continue;

            float delta = tjx - attacker;
            if (delta < bestDelta)
            {
                bestDelta = delta;
                bestID = i;
            }
        }
        return bestID;
    }

    private IEnumerator AttackOnce(int attackerID, int targetID)
    {
        var attacker = pieces[attackerID];
        var target = pieces[targetID];

        if (shields[targetID] <= 0)
        {
            Banner($"{target.name} is already powered down and cannot be attacked");
            
        }
        else
        {
            Banner($"{attacker.name} fires at {target.name}!");
            if (!fastTurn)
            {
                continuePressed = false;
                turnCont.gameObject.SetActive(true);
                yield return new WaitUntil(() => continuePressed);
                turnCont.gameObject.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(pause);
            }

            shields[targetID] = shields[targetID] - 1;

            if (shields[targetID] <= 0)
            {
                revive[targetID] = true;
                shields[targetID] = 0;
                Banner($"{target.name}'s shields are down! They will have to spend their next turn powering up!");
            }
            else
            {
                Banner($"{target.name}'s shields are down to {shields[targetID]}");
            }
            yield return new WaitForSeconds(pause);
            
        }
    }
    
    private IEnumerator selectTargets (int attackerID, System.Action<int> onSelected)
    {
        List<int> valid = GetTargets(attackerID);
        if (valid.Count == 0)
        {
            onSelected(-1);
            yield break;
        }

        targetDropDown.ClearOptions();
        var labels = new List<string>();
        foreach (int id in valid)
            labels.Add(pieces[id].name);
        targetDropDown.AddOptions(labels);

        Banner($"{pieces[attackerID].name}: choose target");
        targetDropDown.gameObject.SetActive(true);

        continuePressed = false;

        turnCont.gameObject.SetActive(true);
        yield return new WaitUntil(() => continuePressed);
        turnCont.gameObject.SetActive(false);

        int choice = valid[targetDropDown.value];

        targetDropDown.gameObject.SetActive(false);
        onSelected(choice);
    }
    private void Banner(string msg)
    {
        //rollText.CrossFadeAlpha(0, 0.1f, false);
        rollText.text = msg;
        //rollText.CrossFadeAlpha(0, 0.3f, false);
    }
    private void ClearBanner()
    {
        //yield return new WaitUntil(() => continuePressed);
        rollText.text = "";
    }

    private void ChooseShip(string chosenName)
{
    int idx = pieces.FindIndex(p => p.name == chosenName);
    playerIndex = (idx >= 0) ? idx : -1;

    Banner($"You selected {chosenName}!");
    if (selectMenu != null) selectMenu.SetActive(false);

    turnExec.gameObject.SetActive(true);
    turnCont.gameObject.SetActive(false);
}
}
    

    

