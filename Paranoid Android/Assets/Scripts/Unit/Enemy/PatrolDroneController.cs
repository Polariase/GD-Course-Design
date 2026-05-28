using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolDroneController : UnitController
{
    public int maxHp = 60;
    public int hp;

    public bool isAlert;

    protected override void Awake()
    {
        base.Awake();
        hp = maxHp;
        moveSpeed = 8f;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            animator.SetBool("IsAlert", !animator.GetBool("IsAlert"));
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            animator.SetBool("IsMoving", !animator.GetBool("IsMoving"));
        }
    }
}
