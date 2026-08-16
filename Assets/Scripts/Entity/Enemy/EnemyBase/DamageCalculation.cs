using UnityEngine;

public static class DamageCalculation<T> where T : IDamageable
{
    /*
     * dmgType
     * = 1  기본 데미지
     * = 2  스택의 반대 데미지
     * = 3  역행 반대 데미지
     */

    public static int Calculate(AttackInfo attack, T target)
    {
        int baseDamage = 1;

        if(target.currentPower * attack.power < 0)     //반대방향이면
        {
            if(EmitManager.Instance.isEmitting) target.dmgType = 3;
            else target.dmgType = 2;

            if(target.currentPower > 0) return baseDamage + target.currentPower;
            else return baseDamage - target.currentPower;
        }
        target.dmgType = 1;
        return baseDamage;


        //if (EmitManager.Instance.isEmitting)
        //{
        //    if (target.currentPower * attack.power < 0)      //반대방향이면
        //    {
        //        target.dmgType = 3;
        //        return damage = 10;
        //    }
        //}
        //else    //역행 아님
        //{
        //    if (target.currentPower * attack.power < 0)      //반대방향이면
        //    {
        //        target.dmgType = 2;
        //        return damage = 2;
        //    }
        //}
        //target.dmgType = 1;
        //return damage;
    }
}

