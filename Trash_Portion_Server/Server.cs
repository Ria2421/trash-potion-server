//-----------------------------------------------------------
//
// とらっしゅぽーしょん！サーバー [ Server.cs ]
// Author:Kenta Nakamoto
// Data 2024/02/08
//
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Trash_Portion_Server
{
    class Server
    {
        //------------------------------------------------------------------------------
        // フィールド ----------------------------------------

        /// <summary>
        /// 送受信サイズ
        /// </summary>
        const int dataSize = 1024;

        /// <summary>
        /// 全tcpクライアント情報
        /// </summary>
        List<TcpClient> tcpClients;

        /// <summary>
        /// プレイヤーナンバー
        /// </summary>
        int playerNo;

        /// <summary>
        /// 準備完了個数
        /// </summary>
        int readyCnt;

        /// <summary>
        /// JSONファイル格納用
        /// </summary>
        string json;

        /// <summary>
        /// プレイヤーデータ格納用
        /// </summary>
        List<UserData> userDatas = new List<UserData>();

        /// <summary>
        /// ゲーム終了フラグ
        /// </summary>
        bool gameEndFlag;

        //------------------------------------------------------------------------------
        // メソッド -------------------------------------------

        public async void StartSever(object arg)
        {
            // 引数からクライアント情報の受け取り
            tcpClients = (List<TcpClient>)arg;

            /* 接続処理 */

            for (int i = 0; i < tcpClients.Count; i++)
            {   // 全プレイヤーにNoを送信
                playerNo = i + 1;
                byte[] buffer = Encoding.UTF8.GetBytes(playerNo.ToString());      // 文字列をbyteに変換
                buffer = buffer.Prepend((byte)EventID.PlayerNo).ToArray();        // 送信データの先頭にイベントIDを付与
                Array.Resize(ref buffer, dataSize);                               // 送信データの配列数を固定に変更

                // 送信処理
                NetworkStream stream = tcpClients[i].GetStream();
                await stream.WriteAsync(buffer, 0, buffer.Length);

                // ユーザーデータの作成・リストの追加
                UserData userData = new UserData();
                userData.PlayerNo = playerNo;
                userDatas.Add(userData);

                // 結果表示
                Console.WriteLine("{0}PNo送信完了",i+1);
            }
#if DEBUG
            // 結果表示
            Console.WriteLine("PLNo送信完了");
#endif
            //---------------------------//
            // 受け取ったeventID毎の処理 //
            //---------------------------//

            while (true)
            {
                // 監視するソケットのリストを作成
                List<Socket> socketList = new List<Socket>();
                foreach (TcpClient tcpClient in tcpClients)
                {
                    socketList.Add(tcpClient.Client);
                }

                // 各ソケットからの通信を待機 (第4引数はタイムアウトするﾐﾘ秒)
                Socket.Select(socketList, null, null, -1);

                // 受信したデータがあるソケットだけリストに残る。Receive関数で受信
                foreach (Socket socket in socketList)
                {
                    byte[] buffer = new byte[dataSize];
                    int length = socket.Receive(buffer, dataSize, SocketFlags.None);

                    // 受信データからイベントIDを取り出す
                    int eventID = buffer[0];

                    switch (eventID)
                    {
                        //--------------------------------------------//
                        // 名前のデータを受信し、全クライアントに送る //
                        case (int)EventID.UserData:  

                            //- 受信処理 -//

                            // 受信データから文字列を取り出す
                            byte[] bufferJson = buffer.Skip(1).ToArray();                               // 1バイト目をスキップ
                            string recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);  // 受信データを文字列に変換

                            // Jsonデシリアライズ
                            UserData userData = JsonConvert.DeserializeObject<UserData>(recevieString);

                            // データリストの該当PLNoに名前を保存
                            userDatas[userData.PlayerNo - 1].UserName = userData.UserName;
#if DEBUG
                            // 結果表示
                            Console.WriteLine("{0}P名受信完了",userData.PlayerNo);
#endif
                            //- 送信処理 -//

                            // 送信データをJSONシリアライズ
                            json = JsonConvert.SerializeObject(userDatas[userData.PlayerNo - 1]);

                            // 全クライアント送信処理
                            SendAllClients(json, (int)EventID.UserData);

                            break;

                        // ---------------------------------------------//
                        // 準備完了フラグを受信し、全クライアントに送る //
                        case (int)EventID.CompleteFlag:  

                            //- 受信処理 -//

                            // 受信データから文字列を取り出す
                            bufferJson = buffer.Skip(1).ToArray();                               // 1バイト目をスキップ
                            recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);  // 受信データを文字列に変換

                            // Jsonデシリアライズ
                            userData = JsonConvert.DeserializeObject<UserData>(recevieString);

                            // データリストの該当PLNoに完了フラグを保存
                            userDatas[userData.PlayerNo - 1].IsReady = userData.IsReady;

                            readyCnt++;  // 準備完了フラグの個数をカウント
#if DEBUG
                            // 結果表示
                            Console.WriteLine("{0}P準備完了", userData.PlayerNo);
#endif
                            //- 送信処理 -//

                            // 送信データをJSONシリアライズ
                            json = JsonConvert.SerializeObject(userDatas[userData.PlayerNo - 1]);

                            // 全クライアント送信処理
                            SendAllClients(json, (int)EventID.CompleteFlag);

                            break;

                        // -------------------------------------------------------------------//
                        // 1Pからインゲームフラグを受信し、全クライアントにマップデータを送る //
                        case (int)EventID.InGameFlag:

                            //- 送信処理 -//

                            // 送信データをJSONシリアライズ
                            json = "";

                            // 全クライアント送信処理
                            SendAllClients(json, (int)EventID.InGameFlag);
#if DEBUG
                            Console.WriteLine("ゲーム開始");
#endif
                            break;

                        // -----------------------------------------------------------------------//
                        // 選択情報を受信し、全クライアントに選択されたユニットのタイル情報を送る //
                        case (int)EventID.SelectUnit:

                            //- 受信処理 -//

                            // 受信データから文字列を取り出す
                            bufferJson = buffer.Skip(1).ToArray();                               // 1バイト目をスキップ
                            recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);  // 受信データを文字列に変換

                            //- 送信処理 -//

                            // 全クライアント送信処理
                            SendAllClients(recevieString, (int)EventID.SelectUnit);
#if DEBUG
                            Console.WriteLine("選択処理受信");
#endif
                            break;

                        // -----------------------------------------------------------------//
                        // 移動情報を受信し、全クライアントに移動対象のいるタイル情報を送る //
                        case (int)EventID.MoveUnit:

                            //- 受信処理 -//

                            // 受信データから文字列を取り出す
                            bufferJson = buffer.Skip(1).ToArray();                               // 1バイト目をスキップ
                            recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);  // 受信データを文字列に変換

                            //- 送信処理 -//

                            // 全クライアント送信処理
                            SendAllClients(recevieString, (int)EventID.MoveUnit);
#if DEBUG
                            Console.WriteLine("移動処理受信");
#endif
                            break;

                        // ---------------------------------------------------------------//
                        // プレイヤーNoを受信し、全クライアントにポーション生成情報を送る //
                        case (int)EventID.PotionGenerate:

                            //- 受信処理 -//

                            // 受信データから文字列(PLNo)を取り出す
                            bufferJson = buffer.Skip(1).ToArray();                               // 1バイト目をスキップ
                            recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);  // 受信データを文字列に変換

                            // Jsonデシリアライズ
                            int plNo = JsonConvert.DeserializeObject<int>(recevieString);

                            //- 送信処理 -//

                            // 全クライアント送信処理
                            SendAllClients(recevieString, (int)EventID.PotionGenerate);
#if DEBUG
                            Console.WriteLine(plNo + "Pポーション生成開始");
#endif
                            break;

                        // ---------------------------------------------------------//
                        // プレイヤーNoを受信し、全クライアントに生成成功情報を送る //
                        case (int)EventID.PotionComplete:

                            //- 受信処理 -//

                            // 受信データから文字列(PLNo)を取り出す
                            bufferJson = buffer.Skip(1).ToArray();                               // 1バイト目をスキップ
                            recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);  // 受信データを文字列に変換

                            // Jsonデシリアライズ
                            plNo = JsonConvert.DeserializeObject<int>(recevieString);

                            //- 送信処理 -//

                            // 全クライアント送信処理
                            SendAllClients(recevieString, (int)EventID.PotionComplete);
#if DEBUG
                            Console.WriteLine(plNo + "P生成成功");
#endif
                            break;

                        // ---------------------------------------------------------//
                        // プレイヤーNoを受信し、全クライアントに生成失敗情報を送る //
                        case (int)EventID.PotionFailure:

                            //- 受信処理 -//

                            // 受信データから文字列(PLNo)を取り出す
                            bufferJson = buffer.Skip(1).ToArray();                               // 1バイト目をスキップ
                            recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);  // 受信データを文字列に変換

                            // Jsonデシリアライズ
                            plNo = JsonConvert.DeserializeObject<int>(recevieString);

                            //- 送信処理 -//

                            // 全クライアント送信処理
                            SendAllClients(recevieString, (int)EventID.PotionFailure);
#if DEBUG
                            Console.WriteLine(plNo + "P生成失敗");
#endif
                            break;

                        case (int)EventID.PotionThrow:

                            recevieString = "";

                            // 全クライアント送信処理
                            SendAllClients(recevieString, (int)EventID.PotionThrow);

                            break;

                        case (int)EventID.PotionSetPos:

                            //- 受信処理 -//

                            // 受信データから文字列(設置位置)を取り出す
                            bufferJson = buffer.Skip(1).ToArray();                               // 1バイト目をスキップ
                            recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);  // 受信データを文字列に変換

                            //- 送信処理 -//

                            // 全クライアント送信処理
                            SendAllClients(recevieString, (int)EventID.PotionSetPos);

                            break;

                        case (int)EventID.GameEnd:

                            // ゲーム終了フラグをtrueに
                            gameEndFlag = true;

                            Console.WriteLine("ゲーム終了");

                            break;

                        // ---------- //
                        // デフォルト //
                        default: 
                            break;
                    }
                }

                if (readyCnt == userDatas.Count)
                { // PL全員が準備完了になった時

                    // InSelectFlag表示
                    Console.WriteLine("選択画面移行");

                    // 送信データをJSONシリアライズ
                    json = "";

                    // 全クライアント送信処理
                    SendAllClients(json,(int)EventID.InSelectFlag);

                    readyCnt = 0;
                }

                if(gameEndFlag)
                {
                    // whileループから抜けてスレッド終了
                    break;
                }
            }
        }

        /// <summary>
        /// 全クライアントデータ送信処理
        /// </summary>
        /// <param name="json"></param>
        private async void SendAllClients(string json,int eventID) 
        {
            for (int i = 0; i < tcpClients.Count; i++)
            {   // 全プレイヤーにJSONファイルを送信

                byte[] buffer = Encoding.UTF8.GetBytes(json);        // JSONをbyteに変換
                buffer = buffer.Prepend((byte)eventID).ToArray();    // 送信データの先頭にイベントIDを付与
                Array.Resize(ref buffer, dataSize);                  // 送信データの配列数を固定に変更

                // 送信処理
                NetworkStream stream = tcpClients[i].GetStream();
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}