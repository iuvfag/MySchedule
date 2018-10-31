using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        internal static String doCheck(String value, String valueType, int minLength, int maxLength) {

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

        //入力された2つのパスワードが等しいか判別するメソッド
        internal static String passwordCompare(String password, String reComfirmationPassword) {

            //結果を初期化
            String result = "";

            //2つの値を比較
            if (password != reComfirmationPassword)
            {
                //等しくなければメッセージを格納
                result = "パスワードと再確認用パスワードが一致しません";
            }
            //結果を返す
            return result;
        }

        /// <summary>
        /// 空欄かどうか、入力文字数以下かどうかをチェックするメソッド。「件名」に使用
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        internal static String doCheck2(String value, String valueType, int maxLength) {

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
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        internal static String doCheck3(String value, String valueType, int maxLength) {
            String result = "";

            if (value.IsOverLength(maxLength))
            {
                result = $"{valueType}は、{maxLength}文字以下で入力してください";
            }
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
        internal static bool IsOverLength(this String s ,int maxLength)
        {
            return s.Length > maxLength;
        }
    }
    
}
