using TMPro;
using UnityEngine;
using DG.Tweening;

public class ScoreAndStreakSystem : MonoBehaviour
{   
    [Header("UI Elements")]
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI streakText;
    
    [Header("Animation Settings")]
    [SerializeField]
    private float scorePunchScale = 1.2f;
    [SerializeField]
    private float scorePunchDuration = 0.3f;
    [SerializeField]
    private float streakPunchScale = 1.3f;
    [SerializeField]
    private float streakPunchDuration = 0.4f;
    [SerializeField]
    private float streakLoseShakeDuration = 0.5f;
    [SerializeField]
    private float streakLoseShakeStrength = 20f;
    
    [Header("Color Settings")]
    [SerializeField]
    private Color scoreFlashColor = Color.yellow;
    [SerializeField]
    private Color streakLowColor = Color.yellow;
    [SerializeField]
    private Color streakMidColor = new (1f, 0.5f, 0f); // Orange
    [SerializeField]
    private Color streakHighColor = new (1f, 0.3f, 0f); // Bright orange
    [SerializeField]
    private Color streakMissColor = Color.red;
    [SerializeField]
    private Color streakResetColor = Color.white;
    
    [Header("Other Settings")]
    [SerializeField]
    private int streakMidThreshold = 5;
    [SerializeField]
    private int streakHighThreshold = 10;
    [SerializeField]
    private int highStreaksNumber = 5;
    
    private int _scoreValue = 0;
    private int _streakValue = 0;

    private void Awake()
    {
        if (scoreText == null)
        {
            Debug.LogError("Score TextMeshProUGUI reference is not assigned.");
        }

        if (streakText == null)
        {
            Debug.LogError("Streak TextMeshProUGUI reference is not assigned.");
        }
    }

    private void Start()
    {
        // Initialize text
        UpdateScoreText();
        UpdateStreakText();
    }
    
    public void PlayerScored()
    {
        _scoreValue += 1;
        _streakValue += 1;
        
        // Update text
        UpdateScoreText();
        UpdateStreakText();
        
        // Animate score with punch effect
        scoreText.transform.DOKill();
        scoreText.transform.DOPunchScale(Vector3.one * scorePunchScale, scorePunchDuration, 5, 0.5f);
        
        // Animate color flash for score
        scoreText.DOKill();
        scoreText.DOColor(scoreFlashColor, 0.1f).SetLoops(2, LoopType.Yoyo);
        
        // Animate streak with bigger punch effect
        streakText.transform.DOKill();
        streakText.transform.DOPunchScale(Vector3.one * streakPunchScale, streakPunchDuration, 6, 0.5f);
        
        // Animate color based on streak value
        Color streakColor = GetStreakColor(_streakValue);
        streakText.DOKill();
        streakText.DOColor(streakColor, 0.2f).SetLoops(2, LoopType.Yoyo);
        
        // Add rotation for high streaks
        if (_streakValue >= highStreaksNumber)
        {
            streakText.transform.DOPunchRotation(new Vector3(0, 0, 15), streakPunchDuration, 8, 0.5f);
        }
    }
    
    public void PlayerMissed()
    {    
        if (_streakValue > 0){
            _streakValue = 0;
            UpdateStreakText();
            
            // Shake animation for losing streak
            streakText.transform.DOKill();
            streakText.transform.DOShakePosition(streakLoseShakeDuration, streakLoseShakeStrength, 20, 90, false, true);
            
            // Flash red color
            streakText.DOKill();
            streakText.DOColor(streakMissColor, 0.15f).SetLoops(4, LoopType.Yoyo).OnComplete(() =>
            {
                streakText.color = streakResetColor;
            });
            
            // Scale down effect
            streakText.transform.DOScale(0.7f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }
    
    private void UpdateScoreText()
    {
        scoreText.text = $"Pontos: {_scoreValue}";
    }
    
    private void UpdateStreakText()
    {   
        streakText.text = $"Sequência: {_streakValue}";
    }
    
    private Color GetStreakColor(int streak)
    {
        if (streak >= streakHighThreshold)
            return streakHighColor;
        
        if (streak >= streakMidThreshold)
            return streakMidColor;
        
        return streakLowColor;
    }
    
    private void OnDestroy()
    {
        // Clean up tweens when object is destroyed
        scoreText.transform.DOKill();
        scoreText.DOKill();
        streakText.transform.DOKill();
        streakText.DOKill();
    }
}