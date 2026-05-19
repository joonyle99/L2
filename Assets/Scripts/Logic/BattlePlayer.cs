using System;
using System.Collections;
using UnityEngine;

public class BattlePlayer : MonoBehaviour
{
    // BattleSimulator가 선계산한 타임라인을 코루틴으로 순서대로 재생
    public void Play(BattleTimeline timeline, Action onComplete)
    {
        StartCoroutine(PlayCoroutine(timeline, onComplete));
    }

    private IEnumerator PlayCoroutine(BattleTimeline timeline, Action onComplete)
    {
        float elapsed = 0f;
        int eventIndex = 0;

        while (eventIndex < timeline.Events.Count)
        {
            elapsed += Time.deltaTime;

            while (eventIndex < timeline.Events.Count && timeline.Events[eventIndex].Time <= elapsed)
            {
                ProcessEvent(timeline.Events[eventIndex]);
                eventIndex++;
            }

            yield return null;
        }

        onComplete?.Invoke();
    }

    private void ProcessEvent(BattleEvent e)
    {
        // TODO: 애니메이션, 이펙트, HP바 갱신 등 시각적 처리
        switch (e.Type)
        {
            case BattleEventType.Attack:
                Debug.Log($"[Battle] {(e.Attacker.IsPlayerSide ? "Player" : "Enemy")} {e.Attacker.Source.Data.name} → {e.Target.Source.Data.name} ({e.Damage} dmg, t={e.Time:F1})");
                break;
            case BattleEventType.Death:
                Debug.Log($"[Battle] {e.Target.Source.Data.name} 사망 (t={e.Time:F1})");
                break;
        }
    }
}
