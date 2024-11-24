using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    private Vector3 startPos = new Vector3(25,0.7f, 5);
    [SerializeField] private Transform playerPos;
    public int score = 0;
    public int bestScore = 0;

    // UI
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI bestScoreTextMain;
    [SerializeField] TextMeshProUGUI bestScoreTextResult;

    private void Start()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        SetBestScore();
    }

    // Update is called once per frame
    void Update()
    {
        score = Mathf.RoundToInt(playerPos.position.z - startPos.z);
        scoreText.text = score.ToString();
    }

    private void SetBestScore()
    {
        bestScoreTextMain.text = bestScore.ToString();
        bestScoreTextResult.text = bestScore.ToString();
    }

    public void CheckNewBestScore()
    {
        if(score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", score);
            SetBestScore();
        }
        score = 0;
    }
}
