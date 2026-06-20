using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public EnemyHealth enemyHealth;
    public Slider hpSlider;
    public Transform cameraTransform;

    [Header("Follow")]
    public Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

    private void Awake()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }

        if (hpSlider == null)
        {
            hpSlider = GetComponentInChildren<Slider>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Start()
    {
        RefreshHealthBar();
    }

    private void LateUpdate()
    {
        if (enemyHealth == null || hpSlider == null)
        {
            return;
        }

        transform.position = enemyHealth.transform.position + worldOffset;

        if (cameraTransform != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
        }

        RefreshHealthBar();
    }

    private void RefreshHealthBar()
    {
        hpSlider.maxValue = enemyHealth.MaxHp;
        hpSlider.value = enemyHealth.CurrentHp;
    }
}