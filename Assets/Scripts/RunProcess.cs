using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using System;
using System.Diagnostics;
using static UnityEngine.Rendering.BoolParameter;

public class RunProcess : MonoBehaviour
{
    private float timeRemaining;
    private bool isRunning;
    private bool isPaused;

    public TMP_Text timeText;

    public GameObject pauseMenu;
    public GameObject setingsMenu;
    public GameObject gameView;
    public GameObject startRunButton;
    public GameObject resetRunButton;
    public GameObject timerButtonGroup;


    void Start()
    {
        StopTimer();
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isRunning && !isPaused)
        {

            startRunButton.SetActive(false);
            resetRunButton.SetActive(true);
            
            if (timeRemaining >= 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                StopTimer();
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            setingsMenu.SetActive(false);

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void StartTimer()
    {
        timeRemaining = GetDisplayTime();
        isRunning = true;
        timerButtonGroup.SetActive(false);
    }


    private void PauseTimer()
    {
        isPaused = true;
        PlayerPrefs.SetFloat("SavedTime", timeRemaining);
    }

    private void ResumeTimer()
    {
        isPaused = false;
        timeRemaining = PlayerPrefs.GetFloat("SavedTime");
    }


    public void AddTime()
    {
        if (timeRemaining < 5940)
        {
            timeRemaining += 60;
        }
        DisplayTime(timeRemaining);
    }

    public void SubtractTime()
    {
        if (timeRemaining > 60 )
        {
            timeRemaining -= 60;
        }
        DisplayTime(timeRemaining);
    }

    public void ResetTimer()
    {
        timeRemaining = 60;
        DisplayTime(timeRemaining);
    }

    public void StopTimer()
    {
        isRunning = false;
        isPaused = false;
        if (PlayerPrefs.HasKey("SavedTime"))
        { 
            PlayerPrefs.DeleteKey("SavedTime"); 
        }
        ResetTimer();
        startRunButton.SetActive(true);
        resetRunButton.SetActive(false);
        timerButtonGroup.SetActive(true);
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    float GetDisplayTime()
    {
        string[] parts = timeText.text.Split(':');

        if (parts.Length == 2)
        {
            if (int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds))
            {
                timeRemaining = minutes*60 + seconds;
            }
        }
        return timeRemaining;
    }
    public void PauseGame()
    {
        PauseTimer();
        isPaused = true;
        pauseMenu.SetActive(true);
        gameView.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        ResumeTimer();
        isPaused = false;
        pauseMenu.SetActive(false);
        gameView.SetActive(true);
        Time.timeScale = 1f;
    }
}