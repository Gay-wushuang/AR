using UnityEngine;

public class AstronautAnimationController : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetIdle()
    {
        _animator.SetBool("IsWalk", false);
        _animator.SetBool("IsFloat", false);
        _animator.SetBool("IsWave", false);
    }

    public void SetWalk()
    {
        _animator.SetBool("IsWalk", true);
        _animator.SetBool("IsFloat", false);
        _animator.SetBool("IsWave", false);
    }

    public void SetFloat()
    {
        _animator.SetBool("IsWalk", false);
        _animator.SetBool("IsFloat", true);
        _animator.SetBool("IsWave", false);
    }

    public void SetWave()
    {
        _animator.SetBool("IsWalk", false);
        _animator.SetBool("IsFloat", false);
        _animator.SetBool("IsWave", true);
    }

    public void PlayAnimation(string animationName)
    {
        switch (animationName.ToLower())
        {
            case "idle":
                SetIdle();
                break;
            case "walk":
                SetWalk();
                break;
            case "float":
                SetFloat();
                break;
            case "wave":
                SetWave();
                break;
            default:
                Debug.LogWarning($"Animation '{animationName}' not found!");
                break;
        }
    }
}
