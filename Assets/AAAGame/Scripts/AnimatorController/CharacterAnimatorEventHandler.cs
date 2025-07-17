using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimatorEventHandler : MonoBehaviour
{
    public Animator animator;
    public GameObject handRifle;
    public GameObject upwardRifle;

    static int animator_key_EquipRifle = Animator.StringToHash("EquipRifle");
    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void UnequipRifleEnded()
    {
        handRifle.SetActive(false);
        upwardRifle.SetActive(true);
        animator.SetBool(animator_key_EquipRifle, false);
    }

    public void EquipRifle()
    {
        handRifle.SetActive(true);
        upwardRifle.SetActive(false);
        animator.SetBool(animator_key_EquipRifle, true);
    }
}
