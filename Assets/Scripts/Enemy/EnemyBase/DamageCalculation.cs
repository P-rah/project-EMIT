public static class DamageCalculation {
    /*
     * 현재 오브젝트의 대미지 정보에 따른 대미지 계산 클래스
     * 상세 수치는 공통으로 적용되고 고정이므로 하드코딩 하였음
     */

    public static int Calculate(AttackInfo attack, Enemy enemy)
    {
        int damage = 1;

        if (EmitManager.Instance.isEmitting)    //역행 중
        {
            if (enemy.currentPower * attack.power < 0)      //반대방향이면
            {
                enemy.dmgType = 3;
                return damage = 10;
            }
        }
        else    //역행 아님
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
}

