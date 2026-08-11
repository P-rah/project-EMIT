using UnityEngine;

/*
 * 역행 가능한 적들이 가져야 하는 정보
 */

public interface IEmitableTarget
{
    int currentPower { get; }
    Vector2 FacingDir { get; }

    void SetFacingDirection(Vector2 dir);
    void RootMovement();
    void SetPower(int power);
    void Flip();
    void UnrootMovement();
}
