using benjohnson;
using UnityEngine;

public class StatsLoader : MonoBehaviour
{
    public SpriteRenderer title;
    public Sprite winSprite;
    public Sprite loseSprite;

    public Counter stageCounter;
    public Counter timeCounter;
    public Counter enemiesDefeatedCounter;
    public Counter artifactsDiscoveredCounter;
    public Counter artifactsTriggeredCounter;
    public Counter damageDealtCounter;
    public Counter damageTakenCounter;
    public Counter healthHealedCounter;
    public Counter coinsCollectedCounter;

    void Start()
    {
        LoadStats();
    }

    void LoadStats()
    {
        title.sprite = PlayerStats.instance.win ? winSprite : loseSprite;

        stageCounter.SetText(PlayerStats.instance.stage.ToString(), 3);
        timeCounter.SetText(PlayerStats.instance.time.ToString(), 3);

        enemiesDefeatedCounter.SetText(PlayerStats.instance.playerStatsDic["enemiesDefeated"].ToString(), 3);
        artifactsDiscoveredCounter.SetText(PlayerStats.instance.playerStatsDic["artifactsDiscovered"].ToString(), 3);
        artifactsTriggeredCounter.SetText(PlayerStats.instance.playerStatsDic["artifactsTriggered"].ToString(), 3);
        damageDealtCounter.SetText(PlayerStats.instance.playerStatsDic["damageDealt"].ToString(), 3);
        damageTakenCounter.SetText(PlayerStats.instance.playerStatsDic["damageTaken"].ToString(), 3);
        healthHealedCounter.SetText(PlayerStats.instance.playerStatsDic["healthHealed"].ToString(), 3);
        coinsCollectedCounter.SetText(PlayerStats.instance.playerStatsDic["coinsCollected"].ToString(), 3);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void PlayAgain()
    {
        // Unload endscreen
        SceneManager.instance.UnloadScene(5);
        // Load game
        SceneManager.instance.LoadScene(1);
    }
}
