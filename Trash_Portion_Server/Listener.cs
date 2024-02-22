//-----------------------------------------------------------
//
// とらっしゅぽーしょん！接続受付 [ Listener.cs ]
// Author:Kenta Nakamoto
// Data 2024/02/08
//
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using MySqlConnector;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Trash_Portion_Server
{
    internal class Listener
    {
        //------------------------------------------------------------------------------
        // フィールド ----------------------------------------

        /// <summary>
        /// プレイヤー人数
        /// </summary>
        private const int playerNum = 2;

        /// <summary>
        /// 接続人数カウント用
        /// </summary>
        private static int connectNo = 0;

        /// <summary>
        /// クライアント情報格納用リスト
        /// </summary>
        private static List<TcpClient> clients = new List<TcpClient>();

        //------------------------------------------------------------------------------
        // メソッド -------------------------------------------

        /// <summary>
        /// メイン処理
        /// </summary>
        /// <param name="args"> ? </param>
        static async Task Main(string[] args)
        {
            await StartListener(20001);
        }

        /// <summary>
        /// 接続受付処理
        /// </summary>
        /// <param name="port"> 接続先ポート番号 </param>
        /// <returns> ? </returns>
        static async Task StartListener(int port)
        {
            // 接続を待つエンドポイントを設定
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            // 接続を受け付けるためのTcpListenerを作成
            TcpListener tcpListener = new TcpListener(localEndPoint);

            // 接続開始
            tcpListener.Start();
            Console.WriteLine("接続受付を開始");

            while (true) 
            {
                try
                {
                    // 接続要求受付＆接続作成
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();   // Accept : 接続受付処理

                    if (connectNo == playerNum)
                    {
                        // リストの初期化
                        clients.Clear();
                        connectNo = 0;
                    }

                    // 接続カウント加算
                    connectNo++;

                    // 接続表示
                    Console.WriteLine("クライアント接続 ({0}P)", connectNo);

                    // クライアントリストに追加
                    clients.Add(tcpClient);

                    if (connectNo == playerNum)
                    {   // プレイヤー人数が揃った時

                        // ゲームサーバー用スレッドの生成・起動
                        Server server = new Server();
                        Thread thread = new Thread(new ParameterizedThreadStart(server.StartSever));
                        thread.Start(clients);
                        Console.WriteLine("スレッド起動", connectNo);
                    }
                }
                catch (Exception ex)
                {
                    // 切断内容の表示
                    Console.WriteLine("切断されました");
                    Console.WriteLine(ex.ToString());

                    /* タイトル移行命令をクライアントに出す */

                    // クライアントリストを初期化
                    clients.Clear();
                }
            }

            //// サーバーからPL番号を受信
            //buffer = new byte[1024];                                               // 送受信データ格納用
            //stream = tcpClient.GetStream();                                        // クライアントのデータ送受信に使うNetworkStreamを取得
            //int length = await stream.ReadAsync(buffer, 0, 1);                     // 受信データのバイト数を取得
            //string recevieString = Encoding.UTF8.GetString(buffer, 0, length);     // 受信データを文字列に変換

            //// スレッドの生成・開始
            //Thread thread = new Thread(new ParameterizedThreadStart(DoWork));
            //thread.Start(tcpClient);

            //for (int j = 0; j < playerNum; j++)
            //{
            //    NetworkStream stream2 = clients[j].GetStream();
            //    byte[] buffer = Encoding.UTF8.GetBytes("完了");        // 文字列をbyteに変換
            //    await stream2.WriteAsync(buffer, 0, buffer.Length);    // レスポンスの送信処理

            //    clients[j].Close();
            //}

            //// エラー発生時
            //Console.WriteLine(ex);
            //Console.ReadLine();
        }
    }
}