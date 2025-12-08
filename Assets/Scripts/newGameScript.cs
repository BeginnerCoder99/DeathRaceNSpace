using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class newGameScript : MonoBehaviour
{

    [Header("Pieces")]
    public List<ShipBase> pieces;
    [SerializeField] private int playerIndex = 2;
    [SerializeField] private TMP_Dropdown targetDropDown;
    public Banner banner;
    public GameObject firingPrefab;

    [Header("UI")]
    public Button continueButton;
    public Button missileButton;
    public Button boosterButton;
    public Button specialButton;
    [SerializeField]private GameObject selectMenu; 
    [SerializeField]private GameObject menuBanner;
    [SerializeField]private GameObject energy5;
    [SerializeField]private GameObject energy15;
    [SerializeField]private GameObject energy25;
    [SerializeField]private GameObject energy35;
    [SerializeField]private GameObject energy45;
    [SerializeField]private GameObject energy55;
    [SerializeField]private GameObject energy3;
    [SerializeField]private GameObject energy13;
    [SerializeField]private GameObject energy23;
    [SerializeField]private GameObject energy33;
    [SerializeField] private Button waspButton;
    [SerializeField] private Button cannonButton;
    [SerializeField] private Button jetButton;
    [SerializeField] private Button homerButton;
    public TMP_Text statText;
    public TMP_Text energyText;

    [Header("Rules")]
    [SerializeField] private int spacesToWin = 100;
    private float pause;
    //controls how long between pauses
    public bool fastTurn = false;
    //skips confirmations for testing purposes
    [SerializeField] private Toggle fastTurnToggle;
    
    private bool continuePressed = false;
    [SerializeField] private float basePause = 1.2f;
    private int[] position;
    public int leader = 0;
    private bool gameOver = false;
    private int selectedAction = -1;
    private bool actionPressed = false;

    void Start()
    {
    
        
        //All UI elements except select screen turned off
        missileButton.gameObject.SetActive(false);
        boosterButton.gameObject.SetActive(false);
        specialButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        targetDropDown.gameObject.SetActive(false);
        menuBanner.gameObject.SetActive(false);
        banner.ClearBanner();
        energy5.SetActive(false);
        energy15.SetActive(false);
        energy25.SetActive(false);
        energy35.SetActive(false);
        energy45.SetActive(false);
        energy55.SetActive(false);

        energy3.SetActive(false);
        energy13.SetActive(false);
        energy23.SetActive(false);
        energy33.SetActive(false);

        //Turns on select Menu
        if (selectMenu != null) selectMenu.SetActive(true);

        //Adds listener for all the buttons
        waspButton.onClick.AddListener(() => ChooseShip("Wasp"));
        cannonButton.onClick.AddListener(() => ChooseShip("Cannon"));
        jetButton.onClick.AddListener(() => ChooseShip("Jet"));
        homerButton.onClick.AddListener(() => ChooseShip("Homer"));
        continueButton.onClick.AddListener(OnContinueClicked);
        /*missileButton.onClick.AddListener(() => StartCoroutine(OnMissilePressed(playerIndex)));
        boosterButton.onClick.AddListener(() => StartCoroutine(OnBoosterPressed(playerIndex)));
        specialButton.onClick.AddListener(() => StartCoroutine(OnSpecialPressed(playerIndex)));
        */
        SetupButtons();
        //Initializes everything
        position = new int[pieces.Count];
        pause = basePause;

        if (fastTurnToggle != null)
        {
            fastTurnToggle.isOn = fastTurn;
            fastTurnToggle.onValueChanged.AddListener(OnFastTurnToggled);
        }
        ApplyFast();

        for (int i = 0; i<pieces.Count;++i)
            position[i] = 0;

        

    }

    private void SetupButtons()
    {
        missileButton.onClick.AddListener(() => { selectedAction = 0; actionPressed = true; });
        boosterButton.onClick.AddListener(() => { selectedAction = 1; actionPressed = true; });
        specialButton.onClick.AddListener(() => { selectedAction = 2; actionPressed = true; });
    }

    private IEnumerator WaitForPlayerAction()
    {
        actionPressed = false;
        selectedAction = -1;


        while (!actionPressed)
            yield return null;
        
        missileButton.gameObject.SetActive(false);
        boosterButton.gameObject.SetActive(false);
        specialButton.gameObject.SetActive(false);
        
        switch (selectedAction)
        {
            case 0: // Missile
                yield return StartCoroutine(OnMissilePressed(playerIndex));
                break;
            case 1: // Booster
                yield return StartCoroutine(OnBoosterPressed(playerIndex));
                break;
            case 2: // Special
                yield return StartCoroutine(OnSpecialPressed(playerIndex));
                break;
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

    private IEnumerator GameLoop()
    {
        //Have to press continue to start the turn.
        banner.WriteBanner("Press Continue to start the Round.");
        continuePressed = false;
        continueButton.gameObject.SetActive(true);
        yield return new WaitUntil(() => continuePressed);
        continueButton.gameObject.SetActive(false);
        while (!gameOver)
        {
            yield return StartCoroutine(Turn());
        }
    }

    private IEnumerator Turn()
    {
        
        if (gameOver)
            yield break;
        for (int i = 0; i < pieces.Count; ++i)
        {
            statText.text = pieces[i].GetStats();
            FollowCam.POI = pieces[i].gameObject;
            //yield return StartCoroutine(pieces[0].MoveForwardSpaces(99));

            yield return StartCoroutine(pieces[i].StartTurn());

            UpdateEnergyUI(pieces[i]);
            AISuppresion();

            int max = -1;
            int target = 0;

            yield return StartCoroutine(pieces[i].DoTurn());
            position[i] = pieces[i].spacesMoved;

            if (position[i] >= spacesToWin)
            {
                gameOver = true;
                banner.WriteBanner($"{pieces[i].shipName} has won!");
                FollowCam.FollowDefault(pieces[i].gameObject);
                yield break;
            }

            for (int y = 0; y < pieces.Count; ++y)
            {
                if (position[y] > max)
                {
                    max = position[y];
                    leader = y;
                }
            }

            if(i == playerIndex)
            {
                yield return StartCoroutine(pieces[i].playerSelect(i, sel => target = sel));
            }
            else 
            {
                int aiEnergy = pieces[i].GetEnergy();
                if (aiEnergy >= 3 && pieces[i].GetMaxEnergy() == 3)
                {
                    int selection = Random.Range(0,3);
                    if (selection == 0)
                    {
                        yield return StartCoroutine(AIUseMissile(i));
                    }
                    else if(selection == 1)
                    {
                        yield return StartCoroutine(AIUseBooster(i));
                    }
                    else
                    {
                        yield return StartCoroutine(AISpecial(i));
                    }
                }
                else if (aiEnergy >= 5 && pieces[i].GetMaxEnergy() == 5)
                {
                    int selection = Random.Range(0,3);
                    if (selection == 0)
                    {
                        yield return StartCoroutine(AIUseMissile(i));
                    }
                    else if(selection == 1)
                    {
                        yield return StartCoroutine(AIUseBooster(i));
                    }
                    else
                    {
                        yield return StartCoroutine(AISpecial(i));
                    }
                }
                else if (aiEnergy >= 3)
                {
                    int selection = Random.Range(0,2);
                    if (selection == 0)
                    {
                        yield return StartCoroutine(AIUseMissile(i));
                    }
                    else 
                    {
                        yield return StartCoroutine(AIUseBooster(i));
                    }
                }
                else if (aiEnergy >=2)
                {
                    yield return StartCoroutine(AIUseMissile(i));
                }
                
                if (leader != i && position[leader] >= 90)
                    target = leader; 
                else
                    target = pieces[i].randomSelect(i);
            
            }
            if (target != -1)
                {
                    
                    yield return StartCoroutine(pieces[target].ApplyDamage());
                    yield return StartCoroutine(FireProjectile(pieces[i], 20f));
                }

            if (i == playerIndex)
            {
                UpdateEnergyUI(pieces[i]);
                yield return StartCoroutine(WaitForPlayerAction());
            }
            yield return StartCoroutine(buttonPause());

        }
        FollowCam.FollowDefault(pieces[leader].gameObject);
        banner.WriteBanner($"{pieces[leader].shipName} is in the lead!");
        yield return new WaitForSeconds(pause);
        

    }

    private void ChooseShip(string chosenName)
    {
        int idx = pieces.FindIndex(p => p.name == chosenName);
        playerIndex = (idx >= 0) ? idx : -1;
        if (selectMenu != null) selectMenu.SetActive(false);

        //Once selected ship, sets rest of game active
        menuBanner.gameObject.SetActive(true);
        banner.WriteBanner($"You selected {chosenName}!");
        StartCoroutine(GameLoop());

    }
    private IEnumerator AIUseMissile(int shipIndex)
    {
        pieces[shipIndex].SpendEnergy(2);
        
            int target = shipIndex;
            while (target == shipIndex)
            {
                target = Random.Range(0,4);
            }
            banner.WriteBanner($"{pieces[shipIndex].shipName} has activated their missiles!");
            yield return StartCoroutine(pieces[target].ApplyDamage());
            yield return StartCoroutine(FireProjectile(pieces[shipIndex], 20f));
            yield return StartCoroutine(pieces[target].ApplyDamage());
            yield return StartCoroutine(FireProjectile(pieces[shipIndex], 20f));
            
            yield return new WaitForSeconds(pause / 2);
        UpdateEnergyUI(pieces[shipIndex]);
        AISuppresion();
        yield return StartCoroutine(buttonPause());
    }

    private IEnumerator AIUseBooster(int shipIndex)
    {
        pieces[shipIndex].SpendEnergy(3);
        banner.WriteBanner($"{pieces[shipIndex].shipName} has activated their boosters! Moving forward 3 Spaces.");
        yield return StartCoroutine(pieces[shipIndex].MoveForwardSpaces(4));
        UpdateEnergyUI(pieces[shipIndex]);
        AISuppresion();
        yield return StartCoroutine(buttonPause());
    }

    private IEnumerator OnMissilePressed(int shipIndex)
    {
        pieces[shipIndex].SpendEnergy(2);
        UpdateEnergyUI(pieces[shipIndex]);

        int target = -1;
        yield return StartCoroutine(pieces[shipIndex].missilePlayerSelect(shipIndex, sel => target = sel));
        if(target != -1)
        {
            banner.WriteBanner($"{pieces[shipIndex].shipName} has activated their missiles!");
            yield return new WaitForSeconds(pause);
        
            for (int i = 0; i < 2; i++)
            {
                yield return StartCoroutine(pieces[target].ApplyDamage());
                yield return StartCoroutine(FireProjectile(pieces[shipIndex], 20f));
            }
        }

        //yield return StartCoroutine(buttonPause());
        
        
    }

    private IEnumerator OnBoosterPressed(int shipIndex)
    {
        pieces[shipIndex].SpendEnergy(3);
        UpdateEnergyUI(pieces[shipIndex]);

        banner.WriteBanner($"{pieces[shipIndex].shipName} has activated their boosters! Moving forward 3 Spaces.");
        yield return StartCoroutine(pieces[shipIndex].MoveForwardSpaces(3));
        
        //yield return StartCoroutine(buttonPause());
    }

    private IEnumerator  OnSpecialPressed(int shipIndex)
    {
        if(pieces[shipIndex].GetMaxEnergy() == 5)
            pieces[shipIndex].SpendEnergy(5);
        else
            pieces[shipIndex].SpendEnergy(3);
        yield return StartCoroutine(pieces[shipIndex].special());
        //yield return StartCoroutine(buttonPause());
        
    }

    private IEnumerator  AISpecial(int shipIndex)
    {   
        if(pieces[shipIndex].GetMaxEnergy() == 5)
            pieces[shipIndex].SpendEnergy(5);
        else
            pieces[shipIndex].SpendEnergy(3);
        yield return StartCoroutine(pieces[shipIndex].special());
    }

    private void ResetEnergyUI()
    {
        energy5.SetActive(false);
        energy15.SetActive(false);
        energy25.SetActive(false);
        energy35.SetActive(false);
        energy45.SetActive(false);
        energy55.SetActive(false);

        energy3.SetActive(false);
        energy13.SetActive(false);
        energy23.SetActive(false);
        energy33.SetActive(false);

        missileButton.gameObject.SetActive(false);
        boosterButton.gameObject.SetActive(false);
        specialButton.gameObject.SetActive(false);
    }

    private void AISuppresion()
    {
        missileButton.gameObject.SetActive(false);
        boosterButton.gameObject.SetActive(false);
        specialButton.gameObject.SetActive(false);
    }

    private void UpdateEnergyUI(ShipBase ship)
    {
        ResetEnergyUI();

        int maxEnergy = ship.GetMaxEnergy();
        int current = ship.GetEnergy();
        energyText.text = ship.GetEnergy().ToString();

        if (maxEnergy == 5)
        {
            energy5.SetActive(true);
            if (current >= 1) energy15.SetActive(true);
            if (current >= 2) { energy25.SetActive(true); missileButton.gameObject.SetActive(true); }
            if (current >= 3) { energy35.SetActive(true); boosterButton.gameObject.SetActive(true); }
            if (current >= 4) energy45.SetActive(true);
            if (current >= 5) { energy55.SetActive(true); specialButton.gameObject.SetActive(true); }
        }
        else
        {
            energy3.SetActive(true);
            if (current >= 1) energy13.SetActive(true);
            if (current >= 2) { energy23.SetActive(true); missileButton.gameObject.SetActive(true); }
            if (current >= 3) { energy33.SetActive(true); boosterButton.gameObject.SetActive(true); specialButton.gameObject.SetActive(true); }
        }
    }

    public IEnumerator buttonPause()
    {
            if(!fastTurn)
            {
                continuePressed = false;
                continueButton.gameObject.SetActive(true);
                yield return new WaitUntil(() => continuePressed);
                continueButton.gameObject.SetActive(false);
            }
            else
                yield return new WaitForSeconds(pause);
    }

    public IEnumerator FireProjectile(ShipBase ship, float moveDistance)
    {
    if (firingPrefab == null)
    {
        Debug.LogError("Firing prefab not assigned!");
        yield break;
    }

    // Spawn projectile under the ship (positive Z offset for orthographic camera)
    Vector3 spawnPos = ship.transform.position + new Vector3(0, 0, 1f); // adjust 1f as needed
    GameObject projectile = Instantiate(firingPrefab, spawnPos, Quaternion.identity);

    // Move projectile forward in X axis (to the right)
    float elapsed = 0f;
    float duration = pause; 
    Vector3 startPos = projectile.transform.position;
    Vector3 endPos = startPos + new Vector3(moveDistance, 0, 0); // move in +X

    while (elapsed < duration)
    {
        projectile.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
        elapsed += Time.deltaTime;
        yield return null;
    }


    projectile.transform.position = endPos;
    Destroy(projectile);
}






}
    

    

