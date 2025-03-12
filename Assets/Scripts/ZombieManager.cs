using Unity.VisualScripting;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public int hp = 100;
    private Animator animator;
    private float dieAnimationLength;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        // Die 애니메이션 클립의 길이를 가져옵니다.
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Z_FallingBack")
            {
                dieAnimationLength = clip.length;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(int pDamaged)
    {
        hp -= pDamaged;
        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        animator.SetTrigger("Die");
        Destroy(gameObject, dieAnimationLength);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
}
