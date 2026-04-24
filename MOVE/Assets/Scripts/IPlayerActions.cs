using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerActions
{
    public void Move(InputAction.CallbackContext context);
    
    public void CheckInput(InputAction.CallbackContext context);
}
