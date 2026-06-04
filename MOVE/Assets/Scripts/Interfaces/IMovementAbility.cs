using UnityEngine;

public interface IMovementAbility
{
    bool IsActive { get; }

    /// <summary>Called every frame. Return true to suppress default movement.</summary>
    bool TryExecute(Vector3 cameraRelativeInput, float deltaTime);

    void Cancel();
}
