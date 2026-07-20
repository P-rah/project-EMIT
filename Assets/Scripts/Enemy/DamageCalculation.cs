public static class DamageCalculation {

    /*
    필요한 정보
    - 역행 중인지 아닌지
    - 적의 현재 state (넉백을 받고 있는지/그랩을 받고 있는지)

    Level 1: 밀 / 당
        - Emit이 아니고 state가 당/밀 이 아닌 상태에서 밀/당 공격
    
    Level 2: 밀 + 당 / 당 + 밀
        - Emit이 아니고 state가 당/밀 인 상태에서 밀/당 공격
    
    Level 3: 역행 + 역행 반대방향 공격
        - Emit 상태이고 state가 당/밀인 상태에서 밀/당 공격
    
    Level 4: 역행 + (밀 + 당) / 역행 + (당 + 밀)
        - Emit 상태이고 state가 당/밀인 상태에서 직접 당/밀을 한 상태로 밀/당 공격
    */


    public static int Calculate(AttackInfo attack, Enemy enemy)
    {
        int incomingPower = EmitManager.Instance.isEmitting
            ? -attack.power
            : attack.power;
        int incomingMagnitude = incomingPower < 0 ? -incomingPower : incomingPower;
        int storedMagnitude = enemy.currentPower < 0
            ? -enemy.currentPower
            : enemy.currentPower;

        bool hasOppositeDirection = enemy.currentPower * incomingPower < 0;
        int damage = hasOppositeDirection
            ? storedMagnitude + incomingMagnitude
            : incomingMagnitude;

        if (damage < attack.baseDamage)
        {
            damage = attack.baseDamage;
        }

        if (EmitManager.Instance.isEmitting)
        {
            damage += 2;
        }

        return damage;
    }
}

