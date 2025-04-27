using benjohnson;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public int money;

    // Variables
    [HideInInspector] public int extraCoins; // When picking up coins, also add extracoins

    // Components
    [SerializeField] Counter counter;
    ScaleAnimator anim;

    void Awake()
    {
        extraCoins = 0;

        anim = counter.transform.parent.GetComponent<ScaleAnimator>();
        UpdateCounter();
    }

    void UpdateCounter()
    {
        if (counter == null) return;
        counter.SetText(money.ToString(), 3);
        if (ShopManager.instance != null)
            ShopManager.instance.ReloadPrices();
    }

    public void AddMoney(int value, int count = 1)
    {
        value += extraCoins * count;
        if (value <= 0)
            return;
        money += value;
        PlayerStats.instance.playerStatsDic["coinsCollected"] += value;
        if (Player.instance == null)
            return;
        UpdateCounter();
        anim.SetScale(new Vector2(1.5f, 1.5f));
    }

    public void Buy(int cost)
    {
        money -= cost;
        UpdateCounter();
        anim.SetScale(new Vector2(1.5f, 1.5f));
        SoundManager.instance.PlaySound("Buy");
    }
}
