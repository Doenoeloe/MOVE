using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class EnemyVisuals : MonoBehaviour
{
    [Header("State Colors")]
    public Color telegraphColor = new Color(1f, 0.6f, 0f, 1f);
    public Color attackColor    = new Color(1f, 0.1f, 0.1f, 1f);
    public Color staggerColor   = new Color(0.4f, 0.4f, 1f, 1f);

    [Header("Hit Flash")]
    public float hitFlashDuration = 0.08f;

    private EnemyAI  _ai;
    private Renderer _renderer;
    private Color    _baseColor;

    void Awake()
    {
        _ai       = GetComponent<EnemyAI>();
        _renderer = GetComponentInChildren<Renderer>();

        if (_renderer != null)
            _baseColor = _renderer.material.color;
    }

    public void SetForState(EnemyAI.AIState state)
    {
        switch (state)
        {
            case EnemyAI.AIState.Telegraph: SetColor(telegraphColor); break;
            case EnemyAI.AIState.Attacking: SetColor(attackColor);    break;
            case EnemyAI.AIState.Stagger:   SetColor(staggerColor);   break;
            case EnemyAI.AIState.Recover:   SetColor(Color.gray);     break;
            case EnemyAI.AIState.Dead:      SetColor(Color.black);    break;
            default:                        SetColor(_baseColor);     break;
        }
    }

    public void FlashHit()
    {
        CancelInvoke(nameof(RestoreAfterFlash));
        SetColor(Color.white);
        Invoke(nameof(RestoreAfterFlash), hitFlashDuration);
    }
    
    void RestoreAfterFlash() => SetForState(_ai.CurrentState);

    void SetColor(Color c)
    {
        if (_renderer != null)
            _renderer.material.color = c;
    }
}