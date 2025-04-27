using benjohnson;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : Singleton<PlayerStats>
{
    public bool win;
    public int stage;
    public int time;

    public Dictionary<string, int> playerStatsDic = new Dictionary<string, int>();
    //public int enemiesDefeated;
    //public int artifactsDiscovered;
    //public int artifactsTriggered;
    //public int damageDealt;
    //public int damageTaken;
    //public int healthHealed;
    //public int coinsCollected;

    public void ResetStats()
    {
        win = false;
        stage = 0;
        time = 0;

        string[] keys = { "enemiesDefeated", "artifactsDiscovered", "artifactsTriggered", "damageDealt", "damageTaken", "healthHealed", "coinsCollected" };
        foreach (string key in keys)
        {
            if (playerStatsDic.ContainsKey(key))
            {
                playerStatsDic[key] = 0;
            }
            else
            {
                playerStatsDic.Add(key, 0);
            }

        }

        //enemiesDefeated = 0;
        //artifactsDiscovered = 0;
        //artifactsTriggered = 0;
        //damageDealt = 0;
        //damageTaken = 0;
        //healthHealed = 0;
        //coinsCollected = 0;
    }
}
