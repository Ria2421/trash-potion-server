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
        /// プレイヤーナンバー
        /// </summary>
        private int playerNo;

        /// <summary>
        /// 送受信サイズ
        /// </summary>
        private const int dataSize = 1024;

        /// <summary>
        /// プレイヤーデータ格納用
        /// </summary>
        private List<UserData> userDatas = new List<UserData>();

        /// <summary>
        /// DB接続情報
        /// </summary>
        //        static MySqlConnectionStringBuilder connectionBuilder = new MySqlConnectionStringBuilder
        //        {
        //#if DEBUG
        //            // デバッグ時はローカルに接続
        //            Server = "localhost",
        //            Database = "quizking",
        //            UserID = "root",
        //            Password = "",
        //#else
        //            // 本始動の時はオンラインに接続
        //            Server = "db-ge-07.mysql.database.azure.com",
        //            Database = "quizking",
        //            UserID = "student",
        //            Password = "Yoshidajobi2023",
        //            SslMode = MySqlSslMode.Required,
        //#endif
        //        };

        //------------------------------------------------------------------------------
        // メソッド -------------------------------------------

        public async void StartSever(object arg)
        {
            // 引数からクライアント情報の受け取り
            List<TcpClient> tcpClients = (List<TcpClient>)arg;

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
            }
#if DEBUG
            // 結果表示
            Console.WriteLine("送信完了");
#endif

            for (int i = 0; i < tcpClients.Count; i++)
            {   // 全プレイヤーから名前を受信

                // サーバーからPL番号を受信待機
                byte[] recvBuffer = new byte[dataSize];                                    // 送受信データ格納用
                NetworkStream stream = tcpClients[i].GetStream();                          // クライアントのデータ送受信に使うNetworkStreamを取得
                int length = await stream.ReadAsync(recvBuffer, 0, recvBuffer.Length);     // 受信データのバイト数を取得

                // 受信データからイベントIDを取り出す
                int eventID = recvBuffer[0];

                // 受信データから文字列を取り出す
                byte[] bufferJson = recvBuffer.Skip(1).ToArray();                              // 1バイト目をスキップ
                string recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);     // 受信データを文字列に変換

                // Jsonデシリアライズ
                UserData userData = JsonConvert.DeserializeObject<UserData>(recevieString);

                // データリストに追加
                userDatas.Add(userData);
            }
#if DEBUG
            // 結果表示
            Console.WriteLine("PLデータ受信完了");
#endif

            // 送信用PLデータリストの作成
            UserDataList userDataList = new UserDataList();
            userDataList.userList = userDatas;

            for (int i = 0; i < tcpClients.Count; i++)
            {   // 全プレイヤーにPLリストを送信

                // 送信データをJSONシリアライズ
                string json = JsonConvert.SerializeObject(userDataList);

                byte[] buffer = Encoding.UTF8.GetBytes(json);                   // 文字列をbyteに変換
                buffer = buffer.Prepend((byte)EventID.UserDataList).ToArray();  // 送信データの先頭にイベントIDを付与
                Array.Resize(ref buffer, dataSize);                             // 送信データの配列数を固定に変更

                // 送信処理
                NetworkStream stream = tcpClients[i].GetStream();
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
#if DEBUG
            // 結果表示
            Console.WriteLine("PLデータ送信完了");
#endif

            for (int i = 0; i < tcpClients.Count; i++)
            {   // 全プレイヤーから完了フラグを受信

                // サーバーから準備完了フラグを受信待機
                byte[] recvBuffer = new byte[dataSize];                                    // 送受信データ格納用
                NetworkStream stream = tcpClients[i].GetStream();                          // クライアントのデータ送受信に使うNetworkStreamを取得
                int length = await stream.ReadAsync(recvBuffer, 0, recvBuffer.Length);     // 受信データのバイト数を取得

                // 受信データからイベントIDを取り出す
                int eventID = recvBuffer[0];

                // 受信データから文字列を取り出す
                byte[] bufferJson = recvBuffer.Skip(1).ToArray();                              // 1バイト目をスキップ
                string recevieString = Encoding.UTF8.GetString(bufferJson, 0, length - 1);     // 受信データを文字列に変換

                // Jsonデシリアライズ
                UserData userData = JsonConvert.DeserializeObject<UserData>(recevieString);

#if DEBUG
                // 準備完了表示
                Console.WriteLine("{0}P準備完了",userData.PlayerNo);
#endif
            }

            for (int i = 0; i < tcpClients.Count; i++)
            {   // 全プレイヤー準備完了フラグを送信

                string complete = "準備完了";

                byte[] buffer = Encoding.UTF8.GetBytes(complete);                // 文字列をbyteに変換
                buffer = buffer.Prepend((byte)EventID.InGameFlag).ToArray();     // 送信データの先頭にイベントIDを付与
                Array.Resize(ref buffer, dataSize);                              // 送信データの配列数を固定に変更

                // 送信処理
                NetworkStream stream = tcpClients[i].GetStream();
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}
