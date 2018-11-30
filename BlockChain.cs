using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;

namespace MySchedule
{
    class BlockChain
    {

        public String hashKey { get; set; }
        public String previousHashKey { get; set; }
        public int nonce { get; set; }

        //UpdateHistoryDTOクラスをインスタンス化しておく
        UpdateHistoryDTO uhDTO = new UpdateHistoryDTO();

        /// <summary>
        /// UpdateHistoryDTOクラスのインスタンスを渡すと中の要素を文字列として連結するメソッド
        /// </summary>
        /// <param name="uhDTO">UpdateHistoryDTOクラスのインスタンス</param>
        /// <returns></returns>
        internal String DTOConnect(UpdateHistoryDTO uhDTO)
        {
            return $"{uhDTO.userId}{uhDTO.scheduleId}{uhDTO.updateType}{uhDTO.updateStartTime}" +
                $"{uhDTO.updateEndingTime}{uhDTO.subject}{uhDTO.detail}{uhDTO.updateTime}{uhDTO.previousHashKey}";
        }

        /// <summary>
        /// DTOクラスをもとにブロックチェーンに必要なハッシュキーとNonceを生成しインスタンスに格納して返すメソッド
        /// </summary>
        /// <param name="uhDTO"></param>
        /// <returns></returns>
        internal UpdateHistoryDTO Block(UpdateHistoryDTO uhDTO)
        {
            ///文字列の連結
            String key = $"{uhDTO.userId}{uhDTO.scheduleId}{uhDTO.updateType}{uhDTO.updateStartTime}" +
                $"{uhDTO.updateEndingTime}{uhDTO.subject}{uhDTO.detail}{uhDTO.updateTime}{uhDTO.previousHashKey}";
            var task = GetNonce(key);
            //GetNonceメソッドに連結した文字列を渡し、戻り値をint型の変数Nonceに格納
            int nonce = task.Result;
            //Nonceをもとにもう一度ハッシュキーを生成しString型の変数hashKeyに格納
            String hashKey = CreateHashKey($"{key}{nonce}");

            //上記の2つの変数をそれぞれUpdateHistoryDTOクラスのインスタンスの対応するフィールドに格納
            uhDTO.nonce = nonce;
            uhDTO.hashKey = hashKey;
            //DTOクラスを戻す
            return uhDTO;
        }

        internal ScheduleInfoDTO Block(ScheduleInfoDTO siDTO)
        {
            //まず、引数をすべて連結
            String key = $"{siDTO.userId}{siDTO.startTime}{siDTO.endingTime}{siDTO.subject}{siDTO.detail}";

            //のちに値を入れる配列
            byte[] hash = null;
            //連結した値をバイト変換する
            var bytes = Encoding.Unicode.GetBytes(key);
            //SHA256という形式でハッシュ変換する
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                //バイト変換した値でハッシュ変換する
                hash = sha256.ComputeHash(bytes);
            }
            //結果を戻すs
            siDTO.hashKey = String.Join("", hash.Select(x => x.ToString("X")));
            return siDTO;
        }

        /// <summary>
        /// ハッシュキーを渡すと特定のハッシュキーを作るNonceを求めるメソッド
        /// 時間がかかるため、並列処理を行う
        /// </summary>
        /// <param name="hash">ハッシュキー</param>
        /// <returns>Nonceなどを格納したTask<T>(値を返すことのできる非同期操作)</returns>
        internal async Task<int> GetNonce(String hash)
        {
            //パラレルオプションのインスタンス化
            ParallelOptions options = new ParallelOptions();
            //キャンセルトークンを使用するためインスタンス化しておく
            CancellationTokenSource cts = new CancellationTokenSource();
            //最大スレッド数の設定
            options.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            //キャンセルトークンをパラレルにも適応させる
            options.CancellationToken = cts.Token;
            //ナンスの初期化
            int nonce = 0;

            //処理中のタスクを待機させるためawaitを使用
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    //複数スレッドによる処理の実行
                    Parallel.For(0, 100000, options, (i, state) =>
                    {
                        //CheckNonceの結果がfalseの間
                        while (!(CheckNonce(hash, i)))
                        {
                            //iを増やし続ける
                            i++;
                            //どこかのスレッドで結果が出て、処理の必要がなくなった場合
                            if (state.ShouldExitCurrentIteration)
                            {
                                //処理の終了
                                return;
                            }
                            //CheckNonceがtrueになった場合
                            if (CheckNonce(hash, i))
                            {
                                //iをNonceに格納
                                nonce = i;
                                //処理を停止するトークンを発行
                                cts.Cancel();
                            }

                        }

                    });
                }
                catch (OperationCanceledException)
                {


                }
            });
            //ナンスを入れる変数を初期化しておく

            //CheckNonceメソッドを呼び出し、結果がfalseの間は回し続けるwhile文を作る

            //while文を抜けることができたらNonceを結果として戻す
            return nonce;

            //ParallelOptions options = new ParallelOptions();
            //CancellationTokenSource cts = new CancellationTokenSource();
            ////抑制制御のためのオブジェクトインスタンス化(複数のメソッドで同じ変数を並列処理する場合)
            //Object thisLock = new object();
            //options.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            //options.CancellationToken = cts.Token;

            //以下が試験的に制作した処理など
            //String hash = textBox11.Text;
            //int nonce = 0;
            ////重くなるため並列処理させる
            //await Task.Factory.StartNew(() =>
            //{
            ////既定の数だけ並列処理を行う
            //    Parallel.For(0, 100000, (i, state) =>
            //    {
            ////抑制制御のためのlock
            //        lock (thisLock)
            //        {
            //            nonce = GetNonce(hash, i);
            //            cts.Cancel();     //キャンセル指示
            //            state.Break();    //処理の中止
            //            MessageBox.Show(nonce.ToString(), GetHashCode($"{hash}{nonce}"));
            //        }
            //    });

            //});

        }

        /// <summary>
        /// 渡された値とNonceをハッシュ変換したものが特定の値になるかどうかboolで返すメソッド
        /// </summary>
        /// <param name="hash">渡す値</param>
        /// <param name="nonce">Nonce</param>
        /// <returns></returns>
        internal bool CheckNonce(String hash, int nonce)
        {
            //変数fにGetHashCodeメソッドの結果を格納
            //今回はハッシュ値の条件として値が「0」で始まるものを探す
            var f = CreateHashKey($"{hash}{nonce}").StartsWith("00");
            //fを結果として戻す
            return f;
        }

        /// <summary>
        /// 渡された値をもとにSHA256のハッシュ関数を作るメソッド
        /// </summary>
        /// <param name="value">渡す値</param>
        /// <returns></returns>
        internal String CreateHashKey(String value)
        {
            byte[] hash = null;
            var bytes = Encoding.Unicode.GetBytes(value);

            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                hash = sha256.ComputeHash(bytes);
            }
            String result = String.Join("", hash.Select(x => x.ToString("X")));
            return result;
        }


    }
}

