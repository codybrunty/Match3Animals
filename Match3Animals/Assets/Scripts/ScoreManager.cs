using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour{

    public Text scoreText;
    public int score;
    public int displayScore;
    public float scoreAddUpDuration;
    private Coroutine addUpCo;

    private void Start() {
        UpdateScoreDisplay();
    }

    public void AddToScore(Animal animal, float bonusMulti) {
        score += (int)(animal.scoreValue * bonusMulti);
        if (addUpCo != null) { StopCoroutine(addUpCo); }
        addUpCo = StartCoroutine(AnimateScore());
        //Debug.Log("x"+bonusMulti.ToString()+" "+(int)(animal.scoreValue * bonusMulti));
    }

    public void UpdateScoreDisplay() {
        scoreText.text = "Score: "+ displayScore.ToString();
    }

    IEnumerator AnimateScore() {
        int startScore = displayScore;
        for (float t = 0f; t < scoreAddUpDuration; t+=Time.deltaTime) {
            displayScore = (int)Mathf.Lerp(startScore, score, t / scoreAddUpDuration);
            UpdateScoreDisplay();
            yield return null;
        }
        displayScore = score;
        UpdateScoreDisplay();
    }

}
