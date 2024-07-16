using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI scoreText;

    [SerializeField]
    TextMeshProUGUI comboText;

    // Update is called once per frame
    void Update()
    {
        scoreText.text = $"Score:{ScoreManager.now_score}";
        comboText.text = $"Combo:{ScoreManager.max_combo}";
    }
}
