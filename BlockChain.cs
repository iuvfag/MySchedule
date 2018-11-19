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

        UpdateHistoryDTO uhDTO = new UpdateHistoryDTO();

        internal String DTOConnect(UpdateHistoryDTO uhDTO)
        {
            return $"{uhDTO.userId}{uhDTO.scheduleId}{uhDTO.updateType}{uhDTO.updateStartTime}" +
                $"{uhDTO.updateEndingTime}{uhDTO.subject}{uhDTO.detail}{uhDTO.updateTime}{uhDTO.previousHashKey}";
        }

        internal UpdateHistoryDTO Block(UpdateHistoryDTO uhDTO)
        {

            String key = $"{uhDTO.userId}{uhDTO.scheduleId}{uhDTO.updateType}{uhDTO.updateStartTime}" +
                $"{uhDTO.updateEndingTime}{uhDTO.subject}{uhDTO.detail}{uhDTO.updateTime}{uhDTO.previousHashKey}";

            int nonce = GetNonce(key);
            String hashKey = CreateHashKey($"{key}{nonce}");

            uhDTO.nonce = nonce;
            uhDTO.hashKey = hashKey;
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
            var f = CreateHashKey($"{hash}{nonce}").StartsWith("0");
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

