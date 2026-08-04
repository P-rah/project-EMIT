using System.Collections;
using System.Diagnostics;
using UnityEngine;

public static class DamageCalculation {

    /* INTEGRATION:
     * 이 계산은 공격의 power 부호를 월드 X축 방향으로 보고, 직전 피격 방향(currentPower)과 반대일 때
     * 연계 피해를 주는 로직이다. integration/enemy-state-merge도 같은 10/5 피해값과 비활성화된 궁극기
     * 분기를 갖고 있으므로 중복 구현하지 말고 이 Calculate 본문을 한 벌만 유지한다.
     * 궁극기를 다시 활성화할 때는 Enemy의 궁극기 필드/타이머와 아래의 두 주석 분기를 함께 복구해야 한다.
     * 방향 비교를 Direction2D.IsOpposite로 바꿀 경우 반대 방향 분기에만 사용하고, 같은 방향은 곱이 0보다
     * 큰지 별도로 검사한다. 두 분기 모두 IsOpposite를 사용하면 궁극기 준비 분기가 절대 실행되지 않는다.
     * 피해량 10/5는 현재 양쪽 브랜치의 기준값이며 밸런싱 후 EnemyData 등 데이터 계층으로 옮길 수 있다.
     */

    /*
    필요한 정보
    - 역행 중인지 아닌지
    - 적의 현재 state (넉백을 받고 있는지/그랩을 받고 있는지)

    Level 1: 밀 / 당
        - Emit이 아니고 state가 당/밀 이 아닌 상태에서 밀/당 공격
        Dmg : 1
    
    Level 2: 밀 + 당 / 당 + 밀
        - Emit이 아니고 state가 당/밀 인 상태에서 밀/당 공격
        Dmg : 2
    
    Level 3: 역행 + 역행 반대방향 공격
        - Emit 상태이고 state가 당/밀인 상태에서 밀/당 공격
        Dmg : 4
    
    Level 4: 역행 + (밀 + 당) / 역행 + (당 + 밀)
        - Emit 상태이고 state가 당/밀인 상태에서 직접 당/밀을 한 상태로 밀/당 공격
        Dmg : 20
    */

    public static int Calculate(AttackInfo attack, Enemy enemy)
    {
        int damage = attack.baseDamage;

        //if(enemy.isUltCondition)        //궁극기 조건 충족
        //{
        //    if (enemy.ultPower * attack.power < 0)      //반대방향이면
        //    {
        //        enemy.dmgType = 4;
        //        enemy.cancelUltTimer();
        //        return damage = 20;
        //    }
        //    else enemy.cancelUltTimer();
        //}

        if (EmitManager.Instance.isEmitting)    //역행 중
        {
            if (enemy.currentPower * attack.power < 0)      //반대방향이면
            {
                enemy.dmgType = 3;
                return damage = 10;
            }
            //else if (enemy.currentPower * attack.power > 0)      //같은 방향이면
            //{
            //    enemy.isUltCondition = true;
            //    enemy.ultPower = enemy.currentPower;
            //    enemy.startUltTimer();
            //}
        }
        else
        {
            if (enemy.currentPower * attack.power < 0)      //반대방향이면
            {
                enemy.dmgType = 2;
                return damage = 5;
            }
        }

        enemy.dmgType = 1;
        return damage;
    }
    //    int incomingPower = EmitManager.Instance.isEmitting
    //        ? -attack.power
    //        : attack.power;

    //    int incomingMagnitude = incomingPower < 0 ? -incomingPower : incomingPower;     //공격 방향에 따른 추가 데미지 계산

    //    int storedMagnitude = enemy.currentPower < 0
    //        ? -enemy.currentPower
    //        : enemy.currentPower;       //현재 상태에 따른 추가 대미지 계산

    //    bool hasOppositeDirection = enemy.currentPower * incomingPower < 0;     //서로 반대방향인지 계산

    //    int damage = hasOppositeDirection
    //        ? storedMagnitude + incomingMagnitude
    //        : incomingMagnitude;        //추가 데미지 계산

    //    if (damage < attack.baseDamage)
    //    {
    //        damage = attack.baseDamage;
    //    }

    //    if (EmitManager.Instance.isEmitting)
    //    {
    //        damage += 2;
    //    }

    //    return damage;
    //}
}

