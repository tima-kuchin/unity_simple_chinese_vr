using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;


public class MainHandManager : MonoBehaviour
{
    public TMP_Dropdown handDropdown;
    public GameObject rightHand;
    public GameObject leftHand;

    public void Start()
    {
        handDropdown.onValueChanged.AddListener(onDropdownValueChanged);


        if (!PlayerPrefs.HasKey("mainHandPreferences"))
        {
            PlayerPrefs.SetInt("mainHandPreferences", 0);
            Load();
        }
        else
        {
            Load();
        }
        ChangeHand();
    }

    private void Load()
    {
        handDropdown.value = PlayerPrefs.GetInt("mainHandPreferences");
    }

    private void ChangeHand()
    {
        if (PlayerPrefs.GetInt("mainHandPreferences") == 0)
        {
            rightHand.SetActive(true);
            leftHand.SetActive(false);
        }
        else if (PlayerPrefs.GetInt("mainHandPreferences") == 1)
        {
            rightHand.SetActive(false);
            leftHand.SetActive(true);
        }
    }

    void onDropdownValueChanged(int index)
    {
        PlayerPrefs.SetInt("mainHandPreferences", index);
        ChangeHand();
    }
}
