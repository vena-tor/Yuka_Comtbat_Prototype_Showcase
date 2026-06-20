using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Slider hpSlider;

    [Header("Display")]
    public bool autoFindPlayerHealth = true;

    private void Awake()
    {
        if (playerHealth == null && autoFindPlayerHealth)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }

        if (hpSlider == null)
        {
            hpSlider = GetComponentInChildren<Slider>();
        }
    }

    private void Start()
    {
        RefreshHpBar();
    }

    private void Update()
    {
        RefreshHpBar();
    }

    private void RefreshHpBar()
    {
        if (playerHealth == null || hpSlider == null)
        {
            return;
        }

        hpSlider.maxValue = playerHealth.MaxHp;
        hpSlider.value = playerHealth.CurrentHp;
    }
}