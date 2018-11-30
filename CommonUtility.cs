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
            var rg = new Regex(@"^[ぁ-んァ-ヶ亜-熙]+?", RegexOptions.Compiled);

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
            else if (rg.IsMatch(s))
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
