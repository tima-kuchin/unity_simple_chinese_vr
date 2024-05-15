using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerValueChange : MonoBehaviour
{
    public TMP_Text timeText;

    void Start()
    {
        if (!PlayerPrefs.HasKey("globalTimerValue"))
        {
            PlayerPrefs.SetInt("globalTimerValue", 600);
            DisplayTime(PlayerPrefs.GetInt("globalTimerValue"));
        }
        else
        {
            DisplayTime(PlayerPrefs.GetInt("globalTimerValue"));
        }
    }

    public void AddTime()
    {
        if (PlayerPrefs.GetInt("globalTimerValue") < 5940)
        {
            PlayerPrefs.SetInt("globalTimerValue", PlayerPrefs.GetInt("globalTimerValue") + 60);
        }
        DisplayTime(PlayerPrefs.GetInt("globalTimerValue"));
    }

    public void SubtractTime()
    {
        if (PlayerPrefs.GetInt("globalTimerValue") > 60)
        {
            PlayerPrefs.SetInt("globalTimerValue", PlayerPrefs.GetInt("globalTimerValue") - 60);
        }
        DisplayTime(PlayerPrefs.GetInt("globalTimerValue"));
    }


    public void ResetTimer()
    {
        PlayerPrefs.SetInt("globalTimerValue", 600);
        DisplayTime(PlayerPrefs.GetInt("globalTimerValue"));
    }

    void DisplayTime(int timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

}
