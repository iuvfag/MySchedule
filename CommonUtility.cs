using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace MySchedule
{
    /// <summary>
    /// 入力内容のチェックなどに使用するメソッド、値の暗号化メソッドなど
    /// 今回のプロジェクトで使用するその他のメソッドをまとめるクラス
    /// </summary>
    internal class CommonUtility
    {
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
        /// <summary>
        /// 渡された2つの値を連結してSHA256のハッシュ関数を作るメソッド
        /// </summary>
        /// <param name="value1">渡す値①</param>
        /// <param name="value2">渡す値②</param>
        /// <returns></returns>
        internal String CreateHashKey(String value1, String value2)
        {
            byte[] hash = null;
            var bytes = Encoding.Unicode.GetBytes($"{value1}{value2}");

            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                hash = sha256.ComputeHash(bytes);
            }

            String result = String.Join("", hash.Select(x => x.ToString("X")));
            return result;
        }


        /// <summary>
        /// データベース登録用のハッシュキー生成のためのメソッド(編集履歴テーブル用)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="scheduleId">スケジュールID</param>
        /// <param name="updateType">更新した内容</param>
        /// <param name="updateStartTime">更新したスケジュールの開始時刻</param>
        /// <param name="updateEndingTime">更新したスケジュールの終了時刻</param>
        /// <param name="subject">件名</param>
        /// <param name="detail">詳細</param>
        /// <returns>作成したハッシュキーを格納したString型の変数</returns>
        internal String CreateHashKey(String userId, int scheduleId, String updateType, DateTime updateStartTime,
            DateTime updateEndingTime, String subject, String detail, DateTime updateTime, String previousHashKey)
        {

            //引数として渡された値をベースにハッシュ関数を生成
            //まず連結
            String key = $"{userId}{scheduleId}{updateType}{updateStartTime}{updateEndingTime}{subject}{detail}" +
                $"{updateTime}{previousHashKey}";

            //のちに値を入れる配列
            byte[] hash = null;
            //連結した値をバイト変換する
            var bytes = Encoding.Unicode.GetBytes(key);

            //SHA256という形式でハッシュ関数を作る
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                //バイト変換した値でハッシュ変換する
                hash = sha256.ComputeHash(bytes);
            }
            //結果を戻す
            String result = String.Join("", hash.Select(x => x.ToString("X")));
            return result;
        }

        /// <summary>
        /// データベース登録用のハッシュキー生成のためのメソッド(スケジュール情報テーブル用)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endingTime">終了時刻</param>
        /// <param name="subject">件名</param>
        /// <param name="detail">詳細</param>
        /// <returns>作成したハッシュキーを格納したString型の変数</returns>
        internal String CreateHashKey(String userId, DateTime startTime, DateTime endingTime, String subject,
            String detail)
        {
            //引数として渡された値をベースにハッシュ関数を生成
            //まず連結する
            String key = $"{userId}{startTime}{endingTime}{subject}{detail}";

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
            //結果を戻す
            String result = String.Join("", hash.Select(x => x.ToString("X")));
            return result;
        }

    }

    /// <summary>
    /// 拡張メソッド
    /// クラスとメソッドはstaticにしておき、
    /// メソッドの引数にはthisを付ける(これが拡張する型になる)
    /// 
    /// こうするとString型で宣言した変数が
    /// このメソッドを呼び出すことが可能となる
    /// 
    /// ex)
    /// result.IsOverLength(int)　など
    /// </summary>
    internal static class StringExtension
    {
        /// <summary>
        /// その文字列の長さが引数として渡した文字数より小さいかどうかを判定するメソッド
        /// </summary>
        /// <param name="s">String型の変数「s」(拡張メソッドとしてString型の変数すべてに使用可能)</param>
        /// <param name="maxLength">最大文字数</param>
        /// <returns>bool型の変数</returns>
        internal static bool IsOverLength(this String s, int maxLength)
        {
            return s.Length > maxLength;
        }

        internal static bool IsUnderOrOverLength(this String s, int minLength, int maxLength)
        {
            return s.Length < minLength || s.Length > maxLength;
        }

        /// <summary>
        /// 空欄、入力内容のチェックなどを行うメソッド。「CreateUser」用
        /// </summary>
        /// <param name="s">渡す値そのもの</param>
        /// <param name="valueType">値の種類(メッセージに表示される)</param>
        /// <param name="minLength">最小文字数</param>
        /// <param name="maxLength">最大文字数</param>
        /// <returns>メッセージを格納したString型の変数</returns>
        internal static String DoCheck(this String s, String valueType, int minLength, int maxLength)
        {

            //戻すメッセージを初期化
            String result = "";

            //入力可能文字列を指定する(半角英数、半角記号)
            var rg = new Regex(@"[0-9a-zA-Z!-/:-@\[-\`\{-\~]+", RegexOptions.Compiled);

            //空欄チェック
            if (String.IsNullOrWhiteSpace(s))
            {
                result = $"{valueType}を入力してください";
            }
            //文字数チェック
            else if (s.IsUnderOrOverLength(minLength, maxLength))
            {
                result = $"{valueType}は{minLength}文字以上、{maxLength}文字以下で入力してください";
            }
            //入力内容チェック
            else if (!(rg.IsMatch(s)))
            {
                result = $"{valueType}は半角英数、半角記号で入力してください";
            }
            return result;
        }

        /// <summary>
        /// 空欄かどうか、入力文字数以下かどうかをチェックするメソッド。「件名」に使用
        /// </summary>
        /// <param name="s">値そのもの</param>
        /// <param name="valueType">値の種類(そのメッセージに表示される)</param>
        /// <param name="maxLength"></param>
        /// <returns>メッセージを格納したString型の変数</returns>
        internal static String DoCheck2(this String s, String valueType, int maxLength)
        {

            //結果を初期化
            String result = "";

            //空欄チェック
            if (String.IsNullOrWhiteSpace(s))
            {
                result = $"{valueType}を入力してください";
            }
            //文字数が最大文字数に収まっているかチェック
            else if (s.IsOverLength(maxLength))
            {
                result = $"{valueType}は、{maxLength}文字以下で入力してください";
            }
            //結果を戻す
            return result;
        }

        /// <summary>
        /// 入力文字数以下かどうかだけを調べるメソッド。「詳細」に使用
        /// </summary>
        /// <param name="s">値そのもの</param>
        /// <param name="valueType">値の種類(そのメッセージに表示される)</param>
        /// <param name="maxLength"最大文字数></param>
        /// <returns>メッセージを格納したString型の変数</returns>
        internal static String DoCheck3(this String s, String valueType, int maxLength)
        {
            String result = "";

            if (s.IsOverLength(maxLength))
            {
                result = $"{valueType}は、{maxLength}文字以下で入力してください";
            }
            return result;

        }

        /// <summary>
        /// 入力された2つのパスワードが等しいか判別するメソッド
        /// </summary>
        /// <param name="password">パスワード</param>
        /// <param name="reComfirmationPassword">再確認用パスワード</param>
        /// <returns>メッセージを格納したString型の変数</returns>
        internal static String ValueCompare(this String s, String reComfirmationPassword,
            String passwordType1, String passwordType2)
        {

            //結果を初期化
            String result = "";

            //2つの値を比較
            if (s != reComfirmationPassword)
            {
                //等しくなければメッセージを格納
                result = $"{passwordType1}と{passwordType2}が一致しません";
            }
            //結果を返す
            return result;
        }

    }
}
