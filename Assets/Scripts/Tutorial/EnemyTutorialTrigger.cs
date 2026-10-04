using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTutorialTrigger : MonoBehaviour
{
    private TutorialManager tutorialManager;
    [Header("참조 연결")]
    [SerializeField] private EnemyBase enemy;
    [SerializeField] private TutorialBase stepData;

    [Header("조건 목록 (모두 만족 시 실행)")]
    [SerializeField]
    private List<TutorialCondition> conditions = new List<TutorialCondition>();

    private bool hasTriggered = false;

    private void Awake()
    {
        if (enemy == null) enemy = GetComponent<EnemyBase>();
    }
    private void Start()
    {
        tutorialManager = TutorialManager.Instance;
    }

    private void Update()
    {
        if (hasTriggered || stepData == null) return;

        bool allConditionsMet = conditions.Count > 0 && conditions.All(c => c.IsMet(enemy));

        if (allConditionsMet)
        {

            bool started = tutorialManager.TryStartTutorial(stepData);
            if (started)
            {
                hasTriggered = true;
            }
        }
    }
}