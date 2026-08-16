using UnityEngine;

/*
 * 역행 가능한 적들이 가져야 하는 정보
 */

public interface IEmitableTarget
{
    int currentPower { get; }
    Vector2 FacingDir { get; }
    EmitManager emitManager { get; set; }

    void SetFacingDirection(Vector2 dir);
    void SetPower(int power);
    void ResetPower();
    void Flip();
    void RootMovement();
    void UnrootMovement();
}
