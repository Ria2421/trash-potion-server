//-----------------------------------------------------------
//
//  送受信用データリスト [ DataList.cs ]
// Author:Kenta Nakamoto
// Data 2024/02/08
//
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// イベントID定義 (送受信データの1バイト目に設定)
/// </summary>
public enum EventID
{
    PlayerNo = 1,     // プレイヤーNo
    UserData,         // 名前・No
    UserDataList,     // 全PLのUserDataのリスト
    InGameFlag,       // インゲームフラグ
}

/// <summary>
/// 入力ユーザーデータ
/// </summary>
class UserData
{
    /// <summary>
    /// ユーザー名
    /// </summary>
    public string UserName
    { get; set; }

    /// <summary>
    /// プレイヤーNo
    /// </summary>
    public int PlayerNo
    { get; set; }
}

/// <summary>
/// 全ユーザーのデータリスト
/// </summary>
class UserDataList
{
    /// <summary>
    /// ユーザーデータリスト
    /// </summary>
    public List<UserData> userList
    {  get; set; }

    //////////////////////
    // 戦績変数追加予定 //
    //////////////////////
}