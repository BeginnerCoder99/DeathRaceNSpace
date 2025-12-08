using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Banner : MonoBehaviour
{

public TMP_Text rollText;
public void WriteBanner(string msg)
    {
        rollText.text = msg;
    }
public void ClearBanner()
    {
        rollText.text = "";
    }
}
