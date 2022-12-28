using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGDefine
{
    [Serializable]
    public enum ENUM_LAYER_TYPE
    {
        /// <summary>
        /// 여기는 Built-In 레이어
        /// </summary>
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,

        /// <summary>
        /// 여기부터 User 커스텀 레이어
        /// </summary>
        Player = 6, // 유저
        Interaction = 7,
        Wall = 8,
        Ground = 9,
    }

    /// <summary>
    /// User 커스텀 태그
    /// </summary>
    [Serializable]
    public enum ENUM_TAG_TYPE
    {
        Ground = 0,
    }

    [Serializable]
    public enum ENUM_TEAM_TYPE
    {
        Defalut = 0,
        Blue = 1,
        Red = 2,

        Max
    }

    /// <summary>
    /// 캐릭터 타입
    /// </summary>
    [Serializable]
    public enum ENUM_CHARACTER_TYPE
    {
        Default = 0,
        Knight = 1,
        Wizard = 2,
        Max
    }

    /// <summary>
    /// Prefabs/UI의 InputPanel, AreaPanel가 가진 조작키의 오브젝트 이름들과 같아야 함
    /// </summary>
    [Serializable]
    public enum ENUM_INPUTKEY_NAME
    {
        Direction = 0,
        Jump = 1,
        Dash = 2,
        Attack = 3,
        Skill1 = 4,
        Skill2 = 5,
        Skill3 = 6,
        Skill4 = 7,
        Max = 8
    }

    /// <summary>
    /// "Resources/Prefabs/Maps/" 경로 안에 같은 이름의 프리팹이 필요
    /// </summary>
    [Serializable]
    public enum ENUM_MAP_TYPE
    {
        ForestMap = 0,
        VolcanicMap = 1,
        Max = 2
    }

    /// <summary>
    /// "Resources/Prefabs/AttackObjects/" 경로 안의 프리팹 이름 리스트
    /// </summary>
    [Serializable]
    public enum ENUM_ATTACKOBJECT_NAME
    {
        Knight_Attack1 = 1,
        Knight_Attack2 = 2,
        Knight_Attack3 = 3,
        Knight_DashSkill_1 = 4,
        Knight_DashSkill_2 = 5,
        Knight_DashSkill_3 = 6,
        Knight_JumpAttack = 7,
        Knight_ThrowSkillObject = 8,
        Knight_SmashSkillObject = 9,
        Knight_SmashSkillObject_1 = 10,
        Knight_SmashSkillObject_2 = 11,
        Knight_SmashSkillObject_3 = 12,

        Wizard_Attack1 = 21,
        Wizard_Attack2 = 22,
        Wizard_ThrowAttackObject = 23,
        Wizard_ThrowJumpAttackObject = 24,
        Wizard_ThunderAttackObject = 25,
        Wizard_ThunderAttackObject_1 = 26,
        Wizard_ThunderAttackObject_2 = 27,
        Wizard_ThunderAttackObject_3 = 28,
    }

    /// <summary>
    /// "Resources/Prefabs/EffectObjects/" 경로 안의 프리팹 이름 리스트
    /// </summary>
    [Serializable]
    public enum ENUM_EFFECTOBJECT_NAME
    {
        Basic_AttackedEffect1 = 0,
        Basic_AttackedEffect2 = 1,
        Basic_AttackedEffect3 = 2,
        Knight_SmokeEffect_Jump = 3,
        Knight_SmokeEffect_Landing = 4,
        Knight_SmokeEffect_Move = 5,
        Wizard_ThunderEffect = 6,
    }

    /// <summary>
    /// "Resources/Sounds/BGM/"
    /// 경로 안에 같은 이름의 Audio Clip 파일이 있어야 함
    /// </summary>
    [Serializable]
    public enum ENUM_BGM_TYPE
    {
        LobbyBGM = 0, // 테스트 위해 임시로 1과 변경
        TestBGM = 1,
        BattleBGM = 2,
        MainBGM = 3, // 테스트 위해 임시로 4와 변경
        TrainingBGM = 4,
    }

    /// <summary>
    /// "Resources/Sounds/SFX/"
    /// 경로 안에 같은 이름의 Audio Clip 파일이 있어야 함
    /// </summary>
    [Serializable]
    public enum ENUM_SFX_TYPE
    {
        win,
        loose,
        walk,
        walkbeep1,
        Attack1,
    }
    
    [Serializable]
    public enum ENUM_PLAYER_STATE
    {
        Idle,
        Move,
        Jump,
        Dash,
        Attack,
        Skill,
        Down,
        Hit,
        Die,

        Max
    }

    /// <summary>
    /// 씬 이름과 같아야 함
    /// Unknown 제외
    /// </summary>
    [Serializable]
    public enum ENUM_SCENE_TYPE
    {
        Unknown,
        Lobby,
        Battle,
        Main,
        Training,
        Debug, // 테스트씬
    }
}

