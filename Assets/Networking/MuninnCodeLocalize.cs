using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MuninnCodeLocalize : MonoBehaviour
{
    public enum HintEnum
    {
        OK = 0,
        参数无效 = 10000,
        服务端错误 = 10001,
        操作失败 = 10002,
        超过最大玩家数量限制 = 20001,
        AppId不匹配 = 20002,
        连接失败 = 20099,
        用户异地登录 = 20101,
        房间已关闭 = 20102,
        房间出现未知错误 = 20103,
        客户端超时 = 20104,
        被房主踢出房间 = 20105,  // 被房主kick了, 这算原因，不算错误
        房主不能踢房主本人 = 20106,  // 房主不能踢房主本人
        踢的用户不在房间内 = 20107, // 踢的用户不在房间内
        非房主不能踢人 = 20108, // 非房主不能踢人

        // 超过限制
        记事贴数量超过限制 = 20300,
        记事贴key不匹配 = 20301,
        未找到该记事贴 = 20302,

        // 客户端定义的错误码[1, 10000)
        AppId无效 = 1000,
        AppSecret无效 = 1001,
        玩家ID无效 = 1002,

        // 这部分是lobby相关的错误码
        Lobby拒绝访问 = 9000,
        Lobby鉴权失败 = 9001,
        Lobby房间已关闭 = 9002,
        Lobby房间未知 = 9003,
        Lobby查询房间失败 = 9004,
        Lobby鉴权丢失 = 9005,
        Lobby创建房间失败 = 9006,
        Lobby创建的房间数超出限制 = 9007,
        Lobby加入房间失败 = 9008,
        Lobby关闭房间失败 = 9009,
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    static public string GetCodeName(uint Code)
    {
        return String.Format("{0}。错误码：{1}", Enum.GetName(typeof(HintEnum), Code), Code);
    }
}
