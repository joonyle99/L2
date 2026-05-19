using TMPro;
using UnityEngine;

public class HeroStats : MonoBehaviour
{
    [SerializeField] private TextMeshPro _attackText;
    [SerializeField] private TextMeshPro _healthText;

    public void SetAttackText(int attack)
    {
        _attackText.text = attack.ToString();
    }
    public void SetHealthText(int health)
    {
        _healthText.text = health.ToString();
    }
}
