using UnityEngine;
using DG.Tweening; // 1. Não esqueça de importar o namespace

/// <summary>
/// Crowd member that performs a DOTween jump with a full 360° Y-axis spin when <see cref="Jump"/> is called.
/// Used by <see cref="Crowd"/> to celebrate player scores.
/// </summary>
public class IdlePerson : MonoBehaviour
{
    [Header("Jump Settings")]
    /// <summary>Height of the jump arc.</summary>
    [SerializeField]
     private float jumpPower = 2f;
    /// <summary>Total duration of the jump-and-spin sequence in seconds.</summary>
    [SerializeField]
    private float jumpDuration = 0.5f;


    private bool _isOnGround = true;

    private void Awake()
    {
        DOTween.Init(); 
    }

    /// <summary>Triggers a single jump with a full 360° Y-axis spin if the person is currently on the ground.</summary>
    public void Jump()
    {
        if ( _isOnGround && !DOTween.IsTweening(transform))
        {
            Sequence sequence = DOTween.Sequence();

            _isOnGround = false;


            const int NUMBER_OF_JUMPS = 1;

            sequence.Append(transform.DOJump(transform.position, jumpPower, NUMBER_OF_JUMPS, jumpDuration));

         
            sequence.Join(transform.DORotate(new Vector3(0, 360, 0), jumpDuration, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear));
            
            sequence.OnComplete(() => _isOnGround = true);
        } 
    }
}