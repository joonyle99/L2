public class BattleSimulator
{
    private const float AttackInterval = 1.5f;
    // 같은 라운드 내에서 플레이어 공격 먼저, 적 공격은 절반 뒤
    private const float EnemyAttackOffset = 0.5f;
    private const int MaxRounds = 100; // 무한루프 방지

    // 선계산: 입력된 양 진영 BattleHero를 라운드 단위로 싸워서 타임라인 반환
    // 원본 BattleHero의 CurrHealth가 소모되므로, 호출 전 복사본을 넘겨야 함
    public BattleTimeline Simulate(BattleHero[] playerSide, BattleHero[] enemySide)
    {
        var timeline = new BattleTimeline();

        for (int round = 0; round < MaxRounds; round++)
        {
            if (!HasAlive(playerSide) || !HasAlive(enemySide)) break;

            float roundTime = round * AttackInterval;

            // 플레이어 → 적
            foreach (var attacker in playerSide)
            {
                if (!IsActive(attacker)) continue;
                var target = FindFirstAlive(enemySide);
                if (target == null) break;
                RecordAttack(timeline, roundTime, attacker, target);
            }

            // 적 → 플레이어 (같은 라운드 안에서 살짝 뒤)
            foreach (var attacker in enemySide)
            {
                if (!IsActive(attacker)) continue;
                var target = FindFirstAlive(playerSide);
                if (target == null) break;
                RecordAttack(timeline, roundTime + EnemyAttackOffset, attacker, target);
            }
        }

        timeline.PlayerWon = HasAlive(playerSide);
        
        return timeline;
    }

    private void RecordAttack(BattleTimeline timeline, float time, BattleHero attacker, BattleHero target)
    {
        int damage = attacker.CurrAttack;
        target.CurrHealth -= damage;

        timeline.Events.Add(new BattleEvent
        {
            Time = time,
            Type = BattleEventType.Attack,
            Attacker = attacker,
            Target = target,
            Damage = damage,
        });

        if (!target.IsAlive)
        {
            timeline.Events.Add(new BattleEvent
            {
                Time = time,
                Type = BattleEventType.Death,
                Target = target,
            });
        }
    }

    private bool HasAlive(BattleHero[] side)
    {
        foreach (var h in side)
            if (h != null && h.IsAlive) return true;
        return false;
    }

    private BattleHero FindFirstAlive(BattleHero[] side)
    {
        foreach (var h in side)
            if (h != null && h.IsAlive) return h;
        return null;
    }

    private bool IsActive(BattleHero h) => h != null && h.IsAlive;
}
