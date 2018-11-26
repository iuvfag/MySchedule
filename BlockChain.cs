using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

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
            //GetNonceメソッドに連結した文字列を渡し、戻り値をint型の変数Nonceに格納
            int nonce = GetNonce(key);
            //Nonceをもとにもう一度ハッシュキーを生成しString型の変数hashKeyに格納
            String hashKey = CreateHashKey($"{key}{nonce}");

            //上記の2つの変数をそれぞれUpdateHistoryDTOクラスのインスタンスの対応するフィールドに格納
            uhDTO.nonce = nonce;
            uhDTO.hashKey = hashKey;
            //DTOクラスを戻す
            return uhDTO;
        }

        internal UpdateHistoryDTO Block(String data, String previousHashKey)
        {
            data = $"{data}{previousHashKey}";
            int nonce = GetNonce(data);
            String hashKey = CreateHashKey($"{data}{nonce}");
            UpdateHistoryDTO uhDTO = new UpdateHistoryDTO()
            {
                nonce = nonce,
                hashKey = hashKey
            };
            return uhDTO;
        }

        /// <summary>
        /// ハッシュキーを渡すと特定のハッシュキーを作るNonceを求めるメソッド
        /// </summary>
        /// <param name="hash">ハッシュキー</param>
        /// <returns></returns>
        internal int GetNonce(String hash)
        {
            //ナンスを入れる変数を初期化しておく
            int nonce = 0;

            //CheckNonceメソッドを呼び出し、結果がfalseの間は回し続けるwhile文を作る
            while (!(CheckNonce(hash, nonce)))
            {
                //その間Nonceは増やし続ける
                nonce++;
            }
            //while文を抜けることができたらNonceを結果として戻す
            return nonce;

            ////パラレルオプションのインスタンス化
            //ParallelOptions options = new ParallelOptions();
            ////キャンセルトークンを使用するためインスタンス化しておく
            //CancellationTokenSource cts = new CancellationTokenSource();
            ////抑制制御のためのオブジェクトインスタンス化(複数のメソッドで同じ変数を並列処理する場合)
            //Object thisLock = new object();
            ////最大スレッド数の設定
            //options.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            ////キャンセルトークンをパラレルにも適応させる
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

