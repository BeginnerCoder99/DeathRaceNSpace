using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipBase : MonoBehaviour
{
    public virtual string shipName => "Default";
    public virtual int speedStat => 2;
    public virtual int maxEnergy => 5;
    public virtual int maxShields => 4;

    [Header ("Combat")]
    public int shields;
    public int energy;
    public bool revive = false;
    public TMP_Dropdown targetDropDown;
    public Transform shieldTransform;
    public Renderer shieldRenderer;
    public float shieldOffsetPerLevel = 0.2f;

    [Header ("Turn")]
    public bool fastTurn = false;
    public float basePause = 1.2f;
    public float pause;
    public Toggle fastTurnToggle;
    
    
    [Header ("UI Elements")]
    private bool continuePressed = false;
    public Button continueButton;
    public int spacesMoved = 0;
    public Banner banner;
    public Move moveClass;


    void Start()
    {
        shieldRenderer = shieldTransform.GetComponent<Renderer>();
        shieldRenderer.material = new Material(shieldRenderer.material);
        
        shields = maxShields;
        energy = maxEnergy;
        UpdateShieldVisual();

        if (fastTurnToggle)
        {
            fastTurnToggle.isOn = fastTurn;
            fastTurnToggle.onValueChanged.AddListener(OnFastTurnToggled);
        }
        ApplyFast();

        if (continueButton != null)
            continueButton.onClick.AddListener(() => continuePressed = true);
    }

    private void UpdateShieldVisual()
    {
        float offset = 0f;
        if (shields == 4) offset = 0.8f;
        if (shields == 3) offset = 0.6f;
        if (shields == 2) offset = 0.4f;
        if (shields == 1) offset = 0.2f;
        if (shields == 0) offset = 0f;
        Vector2 currentOffset = shieldRenderer.material.mainTextureOffset;
        currentOffset.x = offset;
        shieldRenderer.material.mainTextureOffset = currentOffset;
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

    public IEnumerator StartTurn()
    {
        banner.WriteBanner($"{shipName} is starting their turn...");
        yield return new WaitForSeconds(pause);

        if (spacesMoved > 17 && spacesMoved < 27  )
        {
            banner.WriteBanner($"{shipName} energy is drained by the solar storm");
            if (energy < 1)
            {   energy = 0;
                banner.WriteBanner($"{shipName} doesn't have any energy to drain, losing shields.");
                if  (shields > 1)
                {
                    shields--;
                    UpdateShieldVisual();
                }
                else
                {
                    shields = 0;
                    UpdateShieldVisual();
                    revive = true;
                    banner.WriteBanner($"{shipName} ran out of power.");
                }
            }
            else
                energy--;
        }
        else
        {
            if(energy < maxEnergy) energy++;
        }
    }
    public IEnumerator DoTurn ()
    {

        if (!fastTurn)
        {
            continuePressed = false;
            continueButton.gameObject.SetActive(true);
            yield return new WaitUntil(() => continuePressed);
            continueButton.gameObject.SetActive(false);
                
        }

        else
        {
            yield return new WaitForSeconds(pause);
        }

        if (revive)
        {
            banner.WriteBanner($"{shipName} is restoring power...");
            yield return new WaitForSeconds(pause);
            revive = false;
            shields = maxShields;
            UpdateShieldVisual();
            
            continuePressed = false;
            continueButton.gameObject.SetActive(true);
            yield return new WaitUntil(() => continuePressed);
            continueButton.gameObject.SetActive(false);
            yield break;
        }

        yield return StartCoroutine(move());

        if (!fastTurn)
        {
            continuePressed = false;
            continueButton.gameObject.SetActive(true);
            yield return new WaitUntil(() => continuePressed);
            continueButton.gameObject.SetActive(false);
                
        }

        else
        {
            yield return new WaitForSeconds(pause);
        }

    }
    public IEnumerator move()
    {
        int roll = Random.Range(1, 7);
        int move = roll+speedStat;
        if (spacesMoved+move > 8 && spacesMoved+move < 13)
        {
            move = move -1;
            banner.WriteBanner($"{shipName} rolled a {roll}! Due to the black holes gravity they've been slowed!");
            yield return new WaitForSeconds(pause);
            banner.WriteBanner($"{shipName} is only moving {move} spaces");
        }
        else if (spacesMoved > 8 && spacesMoved < 13)
        {
            move = move -2;
            banner.WriteBanner($"{shipName} rolled a {roll}! Due to the black holes gravity they've been slowed!");
            yield return new WaitForSeconds(pause);
            banner.WriteBanner($"{shipName} is only moving {move} spaces");
        }
        else
        {
            banner.WriteBanner($"{shipName} rolled a {roll}! Moving {move} spaces.");
        }

        for (int i = 0; i < move; ++i)
        {
            moveClass.MoveForward();
            spacesMoved += 1;
            yield return new WaitForSeconds(pause/2);
            //Need to do followcam for the movement.
        }
    }
    public int randomSelect (int shipIndex)
    {
        //Gives us the ship index, randomly choose player to attack
        int chosen = shipIndex;
        while (chosen == shipIndex)
        {
            chosen = Random.Range(0,4);
        }
        return chosen;
    }

    

    public IEnumerator playerSelect (int shipIndex, System.Action<int> onSelected)
    {
        
        List<string> shipNames = new List<string>
        {
            "Wasp",
            "Cannon",
            "Jet",
            "Homer"
        };

        List<string> dropDown = new List<string>();
        for (int i = 0; i < shipNames.Count; i++)
        {
            if (i != shipIndex)
                dropDown.Add(shipNames[i]);
        }
        dropDown.Add("Recharge");
    
        targetDropDown.ClearOptions();
        targetDropDown.AddOptions(dropDown);

        banner.WriteBanner($"{shipName}: choose target");
        targetDropDown.gameObject.SetActive(true);

        continuePressed = false;
        continueButton.gameObject.SetActive(true);

        yield return new WaitUntil(() => continuePressed);

        continueButton.gameObject.SetActive(false);
        targetDropDown.gameObject.SetActive(false);

        int choice = targetDropDown.value;

        if (choice == dropDown.Count-1)
        {
            energy+=2;
            if (energy > maxEnergy)
                energy = maxEnergy;
            banner.WriteBanner($"{shipName}: has chosen to recharge! They are at {energy} energy");
            onSelected(-1);
            yield break;
        }

        if (choice >= shipIndex)
            choice = choice+1;

        
        onSelected(choice);


    }

    public IEnumerator missilePlayerSelect (int shipIndex, System.Action<int> onSelected)
    {
        
        List<string> shipNames = new List<string>
        {
            "Wasp",
            "Cannon",
            "Jet",
            "Homer"
        };

        List<string> dropDown = new List<string>();
        for (int i = 0; i < shipNames.Count; i++)
        {
            if (i != shipIndex)
                dropDown.Add(shipNames[i]);
        }
    
        targetDropDown.ClearOptions();
        targetDropDown.AddOptions(dropDown);

        banner.WriteBanner($"{shipName}: choose target");
        targetDropDown.gameObject.SetActive(true);

        continuePressed = false;
        continueButton.gameObject.SetActive(true);

        yield return new WaitUntil(() => continuePressed);

        continueButton.gameObject.SetActive(false);
        targetDropDown.gameObject.SetActive(false);

        int choice = targetDropDown.value;

        if (choice >= shipIndex)
            choice = choice+1;

        
        onSelected(choice);

    }

    public IEnumerator ApplyDamage()
    {
        if (shields <= 0)
        {
            shields = 0;
            banner.WriteBanner($"{shipName} shields are already down.");

        }
        else if (shields == 1)
        {
            shields--;
            if(shields <= 0)
            {
                revive = true;
                banner.WriteBanner($"{shipName}'s shields are down! They'll have to recharge next turn.");
                shields = 0;
            }
        }
        else
        {
            shields--;
            banner.WriteBanner($"{shipName} has taken damage! Shields are down to {shields}!");
            
        }

        UpdateShieldVisual();
        yield return new WaitForSeconds(pause);


    }

    public string GetStats()
    {
        return $"SPEED:+{speedStat}\nSHIELDS:{shields}\nMAX SHIELDS:{maxShields}";
        //CURRENT ENERGY:{maxShields}\nMAX ENERGY:{maxEnergy}\n";
    }
    
    public int GetEnergy()
    {
        return energy;
    }
    public int GetMaxEnergy()
    {
        return maxEnergy;
    }
    public void SpendEnergy(int amount)
    {
        energy -= amount;
        if (energy < 0) energy = 0;
    }

    public IEnumerator MoveForwardSpaces(int spaces)
    {
        for (int i = 0; i < spaces; i++)
        {
            moveClass.MoveForward();
            spacesMoved++;
            yield return new WaitForSeconds(pause / 20);
        }
    }

    public IEnumerator special()
    {
        
        banner.WriteBanner($"{shipName} has activated Nitro!");
        yield return StartCoroutine(MoveForwardSpaces(10));
        if (!fastTurn)
        {
            continuePressed = false;
            continueButton.gameObject.SetActive(true);
            yield return new WaitUntil(() => continuePressed);
            continueButton.gameObject.SetActive(false);
                
        }
        else
        {
            yield return new WaitForSeconds(pause);
        }
    }

}
