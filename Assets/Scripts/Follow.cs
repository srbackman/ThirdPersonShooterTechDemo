using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    CharacterController controller;
    Animator animator;
    [SerializeField] private Transform target;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = target.GetComponent<Animator>();
    }


    void LateUpdate()
    {
        controller.Move(animator.velocity * Time.deltaTime);
    }
}
