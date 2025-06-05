using System.Diagnostics;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UIElements;

using VisualElement = UnityEngine.UIElements.VisualElement;
using Label = UnityEngine.UIElements.Label;
using Debug = UnityEngine.Debug;
using System;

public class GameUIHandler : MonoBehaviour
{
    public GameObject myPlayer = null;
    protected PlayerStats myStats;
    protected EntityAttack myEntityAttack;

    public UIDocument UIDoc;

    private Label m_HealthLabel;
    private VisualElement m_HealthBar;
    private VisualElement m_HealthBarLine;

    private Label m_TargetHealthLabel;
    private VisualElement m_TargetHealthBar;
    private VisualElement m_TargetHealthBarLine;

    public float m_HealthBarWidth = 198f;
    //float m_HeathBarWidth2;
    void Start()
    {
        if (myPlayer != null)
        {
            myStats = myPlayer.GetComponent<PlayerStats>();
            myEntityAttack = myPlayer.GetComponent<EntityAttack>();
        } else
        {
            Debug.LogError("myPlayer not found.");
        }

        m_HealthBar = UIDoc.rootVisualElement.Q<VisualElement>("HealthBar");
        m_HealthLabel = UIDoc.rootVisualElement.Q<Label>("lblHealth");
        m_HealthBarLine = UIDoc.rootVisualElement.Q<VisualElement>("HealthBarLine");

        m_TargetHealthBar = UIDoc.rootVisualElement.Q<VisualElement>("TargetHealthBar");
        m_TargetHealthLabel = UIDoc.rootVisualElement.Q<Label>("TargetlblHealth");
        m_TargetHealthBarLine = UIDoc.rootVisualElement.Q<VisualElement>("TargetHealthBarLine");

        m_HealthBar.style.width = m_HealthBarWidth;
        m_TargetHealthBar.style.width = m_HealthBarWidth;

        HealthBarUpdate();
        
    }

    void Update()
    {
        HealthBarUpdate();
        TargetHealthBarUpdate();
    }

    void HealthBarUpdate()
    {
        int hp = myStats.hp;
        int hpMax = myStats.getHPMax();
        m_HealthLabel.text = $"{hp}/{hpMax}";
        float healthBarWidth = ((float) hp / hpMax) * m_HealthBarWidth;
        m_HealthBarLine.style.width = healthBarWidth;
    }

    void TargetHealthBarUpdate()
    {
        m_TargetHealthBar.visible = false;
        if (myEntityAttack.target == null)
            return;
        
        m_TargetHealthBar.visible = true;
        EntityStats targetStats = myEntityAttack.target.GetComponent<EntityStats>();
        if (targetStats != null)
        {
            int hp = targetStats.hp;
            int hpMax = targetStats.getHPMax();
            m_TargetHealthLabel.text = $"{hp}/{hpMax}";
            float healthBarWidth = ((float)hp / hpMax) * m_HealthBarWidth;
            m_TargetHealthBarLine.style.width = healthBarWidth;
        }
    }
}
