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
    /// 入力内容のチェックなどに使用するメソッドをまとめるクラス
    /// </summary>
    internal static class InputChecker
    {

        /// <summary>
        /// 空欄、入力内容のチェックなどを行うメソッド。「CreateUser」用
        /// </summary>
        /// <param name="value">渡す値そのもの</param>
        /// <param name="valueType">値の種類(メッセージに表示される)</param>
        /// <param name="minLength">最小文字数</param>
        /// <param name="maxLength">最大文字数</param>
        /// <returns>メッセージを格納したString型の変数</returns>
        internal static String doCheck(String value, String valueType, int minLength, int maxLength)
        {

            //戻すメッセージを初期化
            String result = "";

            //入力可能文字列を指定する(半角英数、半角記号)
            var rg = new Regex(@"[0-9a-zA-Z!-/:-@\[-\`\{-\~]+", RegexOptions.Compiled);

            //空欄チェック
            if (value == "")
            {
                result = $"{valueType}を入力してください";
            }
            //文字数チェック
            else if (value.Length < minLength || value.Length > maxLength)
            {
                result = $"{valueType}は{minLength}文字以上、{maxLength}文字以下で入力してください";
            }
            //入力内容チェック
            else if (!(rg.IsMatch(value)))
            {
                result = $"{valueType}は半角英数、半角記号で入力してください";
            }
            return result;
        }

        /// <summary>
        /// 入力された2つのパスワードが等しいか判別するメソッド
        /// </summary>
        /// <param name="password">パスワード</param>
        /// <param name="reComfirmationPassword">再確認用パスワード</param>
        /// <returns>メッセージを格納したString型の変数</returns>
        internal static String passwordCompare(String password, String reComfirmationPassword,
            String passwordType1, String passwordType2)
        {

            //結果を初期化
            String result = "";

            //2つの値を比較
            if (password != reComfirmationPassword)
            {
                //等しくなければメッセージを格納
                result = $"{passwordType1}と{passwordType2}が一致しません";
            }
            //結果を返す
            return result;
        }

        /// <summary>
        /// 空欄かどうか、入力文字数以下かどうかをチェックするメソッド。「件名」に使用
        /// </summary>
        /// <param name="value">値そのもの</param>
        /// <param name="valueType">値の種類(そのメッセージに表示される)</param>
        /// <param name="maxLength"></param>
        /// <returns>メッセージを格納したString型の変数</returns>
        internal static String doCheck2(String value, String valueType, int maxLength)
        {

            //結果を初期化
            String result = "";

            //空欄チェック
            if (value == "")
            {
                result = $"{valueType}を入力してください";
            }
            //文字数が最大文字数に収まっているかチェック
            else if (value.Length > maxLength)
            {
                result = $"{valueType}は、{maxLength}文字以下で入力してください";
            }
            //結果を戻す
            return result;
        }

        /// <summary>
        /// 入力文字数以下かどうかだけを調べるメソッド。「詳細」に使用
        /// </summary>
        /// <param name="value">値そのもの</param>
        /// <param name="valueType">値の種類(そのメッセージに表示される)</param>
        /// <param name="maxLength"最大文字数></param>
        /// <returns>メッセージを格納したString型の変数</returns>
        internal static String doCheck3(String value, String valueType, int maxLength)
        {
            String result = "";

            if (value.IsOverLength(maxLength))
            {
                result = $"{valueType}は、{maxLength}文字以下で入力してください";
            }
            return result;

        }

        /// <summary>
        /// 渡された値をもとにSHA256のハッシュ関数を作るメソッド
        /// </summary>
        /// <param name="value">渡す値</param>
        /// <returns></returns>
        internal static String createHashKey(String value)
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
        internal static String createHashKey(String value1, String value2)
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


    }
}
