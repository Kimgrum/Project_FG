using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ĳ������ ������Ʈ�� �����ϱ� ���� ����ϴ� �Ķ���� ���
/// </summary>
/// 

public class CharacterParam { }

public class CharacterMoveParam : CharacterParam
{
    public Vector2 inputVec;
    public bool isRun;

    public CharacterMoveParam(Vector3 inputVec, bool isRun)
    {
        this.inputVec = inputVec;
        this.isRun = isRun;
    }
}