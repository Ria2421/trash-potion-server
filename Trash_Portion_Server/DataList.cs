//-----------------------------------------------------------
//
//  送受信用データリスト [ DataList.cs ]
//  Author:Kenta Nakamoto
//  Data 2024/02/08
//  Update 2024/02/26
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
    CompleteFlag,     // 準備完了フラグ
    InSelectFlag,     // モード選択画面遷移フラグ
    InGameFlag,       // インゲームフラグ
    MapData,          // マップデータ
    SelectUnit,       // 自ユニット選択
    MoveUnit,         // 自ユニット移動
    PotionGenerate,   // ポーション生成
    PotionComplete,   // 生成成功
    PotionFailure,    // 生成失敗
    PotionThrow,      // 投擲処理
    PotionSetPos,     // 投擲位置
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

    /// <summary>
    /// 完了フラグ
    /// </summary>
    public bool IsReady
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
    public UserData[] userList
    { get; set; }

    //////////////////////
    // 戦績変数追加予定 //
    //////////////////////
}

/// <summary>
/// 送信マップデータ
/// </summary>
class MapData
{
    /// <summary>
    /// 初期タイルデータ格納用
    /// </summary>
    public int[,] tileData;

    /// <summary>
    /// 初期ユニットデータ
    /// </summary>
    public int[,] unitData;
}

/// <summary>
/// 選択ユニットデータ
/// </summary>
class SelectData
{
    /// <summary>
    /// PL番号
    /// </summary>
    public int plNo;

    /// <summary>
    /// 選択ユニットの座標[z]
    /// </summary>
    public int z;

    /// <summary>
    /// 選択ユニットの座標[x]
    /// </summary>
    public int x;
}

/// <summary>
/// 移動先送信データ
/// </summary>
class MoveData
{
    /// <summary>
    /// PL番号
    /// </summary>
    public int plNo;

    /// <summary>
    /// 選択ユニットの座標[z]
    /// </summary>
    public int z;

    /// <summary>
    /// 選択ユニットの座標[x]
    /// </summary>
    public int x;

    /// <summary>
    /// 移動先のX座標
    /// </summary>
    public float posX;

    /// <summary>
    /// 移動先のZ座標
    /// </summary>
    public float posZ;
}

/// <summary>
/// ポーションの設置位置
/// </summary>
class SetPotionData
{
    /// <summary>
    /// 設置位置のX座標
    /// </summary>
    public float posX;

    /// <summary>
    /// 設置位置のZ座標
    /// </summary>
    public float posZ;
}