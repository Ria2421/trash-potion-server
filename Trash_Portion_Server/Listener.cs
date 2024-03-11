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
                    if (connectNo == playerNum)
                    {
                        // リストの初期化
                        clients = new List<TcpClient>();
                        connectNo = 0;
                    }

                    // 接続要求受付＆接続作成
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();   // Accept : 接続受付処理

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
        }
    }
}