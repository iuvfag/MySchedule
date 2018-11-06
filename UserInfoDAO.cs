using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace MySchedule
{
    /// <summary>
    /// ユーザー情報をDBとやり取りするクラス
    /// 
    /// メソッドは
    /// ①ログイン用メソッド
    /// ②ユーザー登録用メソッド
    /// ③ログインIDが既に存在するか確認するメソッド
    /// </summary>
    static class UserInfoDAO
    {

        //DBへの接続準備
        static NpgsqlConnection con = new NpgsqlConnection(@"Server=localhost;
                                                    Port=5432;
                                                    UserId=postgres;
                                                    Password=postgres;
                                                    DataBase=myschedule");
        //のちに使用するためインスタンス化しておく
        static NpgsqlCommand cmd = new NpgsqlCommand();

        /// <summary>
        /// ①ログイン用メソッド(結果としてログインIDを格納したString型の変数を返す)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="password">パスワード</param>
        /// <returns>ログインIDを格納したString型の変数</returns>
        internal static String login(String userId, String password)
        {

            //結果を初期化
            String result = "";
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT * FROM user_info WHERE user_id = @userId AND password = @password";
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter("@password", password));
            //上記の分で@にした部分にそれぞれ値を格納する

            //接続
            con.Open();

            //例外処理のためtry-catchする
            try
            {
                using (var reader = cmd.ExecuteReader())    //SQL文をもとにデータを読み取るため使用
                {
                    while (reader.Read() == true)   //データが読み取れているなら
                    {
                        result = reader.GetString(1);   //「user_id」のカラムからとってきた値をresultに格納
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());     //例外が発生した場合メッセージ表示
                throw;
            }
            finally
            {
                con.Close();        //最終的に接続は閉じておく
            }

            //パラメーターに格納した値をremoveする！
            cmd.Parameters.Remove("@userId");
            cmd.Parameters.Remove("@password");

            return result;      //resultを返す
        }

        /// <summary>
        /// ②ユーザー登録用メソッド(結果として0か1を格納したint型の変数を返す)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="password">パスワード</param>
        /// <returns>更新件数を格納したint型の変数</returns>
        internal static int createUser(String userId, String password)
        {

            //結果を初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "INSERT INTO user_info (user_id, password, registration_date) " +
                "VALUES (@userId, @password, now())";

            //SQl文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter("@password", password));

            //接続
            con.Open();

            try
            {
                result = cmd.ExecuteNonQuery();     //SQLの実行結果(更新件数)をそのままresultに格納
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());     //例外処理
                throw;
            }
            finally
            {
                con.Close();    //最終的に接続は閉じておく
            }

            //パラメーターに格納した値をremoveする！
            cmd.Parameters.Remove("@userId");
            cmd.Parameters.Remove("@password");

            return result;      //resultを返す
        }

        /// <summary>
        /// ③ログインIDが既に存在するか確認するメソッド
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <returns>bool型の変数</returns>
        internal static bool isExsitsUser(String userId)
        {

            //結果を初期化
            bool result = false;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT * FROM user_info WHERE user_id = @userId";

            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));

            //接続
            con.Open();

            try
            {
                using (var reader = cmd.ExecuteReader())    //SELECT文なのでデータを読み取っていく
                {
                    if (reader.Read() == true)      //読み取れたら
                    {
                        result = true;      //結果はtrue
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());     //例外処理
                throw;
            }
            finally
            {
                con.Close();        //最終的に接続は閉じておく
            }

            //パラメータの中身をremoveしておく！
            cmd.Parameters.Remove("@userId");

            return result;          //結果を戻す
        }
    }
}
