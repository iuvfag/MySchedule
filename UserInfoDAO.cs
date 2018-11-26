using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Transactions;
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
    /// ④パスワード再設定用のメソッド
    /// ⑤テーブルの全レコードを取得するメソッド(管理者用)
    /// </summary>
    class UserInfoDAO
    {

        //DBへの接続準備
        NpgsqlConnection con = new NpgsqlConnection(@"Server=localhost;
                                                    Port=5432;
                                                    UserId=postgres;
                                                    Password=postgres;
                                                    DataBase=myschedule");
        //のちに使用するためインスタンス化しておく
        NpgsqlCommand cmd = new NpgsqlCommand();

        /// <summary>
        /// ①ログイン用メソッド(結果としてログインID,statusを格納したUserInfoDTOを返す)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="password">パスワード</param>
        /// <returns>ログインIDを格納したString型の変数</returns>
        internal UserInfoDTO Login(String userId, String password)
        {

            //結果を初期化
            UserInfoDTO uiDTO = new UserInfoDTO();
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT user_id, status FROM user_info WHERE user_id = @userId AND password = @password";
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
                        uiDTO.userId = reader.GetString(0);    //取得した最初の値「user_id」をUserInfoDTOに格納
                        uiDTO.status = reader.GetInt32(1);      //取得した次の値「status」をUserInfoDTOに格納
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

            return uiDTO;      //resultを返す
        }

        /// <summary>
        /// ②ユーザー登録用メソッド(結果として0か1を格納したint型の変数を返す)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="password">パスワード</param>
        /// <returns>更新件数を格納したint型の変数</returns>
        internal int CreateUser(String userId, String password)
        {

            //結果を初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "INSERT INTO user_info (user_id, password, registration_date, update_date, status) " +
                "VALUES (@userId, @password, now(), now(), 0)";

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
        /// ③ログインIDが既に存在するか確認するメソッド(ログインIDのみで判定)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <returns>bool型の変数</returns>
        internal bool IsExistsUser(String userId)
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

        /// <summary>
        /// ④ログインIDが既に存在するか確認するメソッド(ログインIDとパスワードで判定)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal bool IsExistsUser(String userId, String password)
        {
            //結果を初期化
            bool result = false;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT * FROM user_info WHERE user_id = @userId AND password = @password";

            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter("@password", password));

            //接続開始
            con.Open();

            try
            {
                using (var reader = cmd.ExecuteReader())    //リーダーの呼び出し
                {
                    while (reader.Read())       //リーダーが読み取れたら
                    {
                        result = true;      //結果はtrue
                    }
                }
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }
            //パラメーターの値はremoveしておく！
            cmd.Parameters.Remove("@userId");
            cmd.Parameters.Remove("@password");

            //結果を戻す
            return result;
        }


        /// <summary>
        /// ⑤パスワード再設定用のメソッド
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="password">パスワード</param>
        /// <param name="newPassword">新しいパスワード</param>
        /// <returns></returns>
        internal int ResetPassword(String userId, String password, String newPassword)
        {
            //結果の初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "UPDATE user_info SET password = @newPassword, update_date = now() " +
                "WHERE user_id = @userId AND password = @password";

            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userid", userId));
            cmd.Parameters.Add(new NpgsqlParameter("@password", password));
            cmd.Parameters.Add(new NpgsqlParameter("@newPassword", newPassword));

            con.Open();

            try
            {
                //結果をresultに格納
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }

            //パラメーターの値はremoveしておく！
            cmd.Parameters.Remove("@userId");
            cmd.Parameters.Remove("@password");
            cmd.Parameters.Remove("@newPassword");

            //結果を戻す
            return result;
        }

        /// <summary>
        /// ⑤テーブルの全レコードを取得するメソッド(管理者用)
        /// </summary>
        /// <returns>DBの値を格納したデータテーブル</returns>
        internal DataTable GetAllUserInfo()
        {
            //DataSet、DataTableの初期化
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            cmd.Connection = con;

            //SQL文の作成
            String sql = "SELECT id as ユーザー管理ID, user_id as ログインID, password as パスワード," +
                "registration_date as 登録日時, status as ステータス FROM user_info;";

            //接続開始
            con.Open();

            try
            {
                //SQL文を実行し、データセットに値を入れるために必要
                //データを格納する準備
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, con);

                ds.Reset();             //データ格納前にリセットしておく
                da.Fill(ds);            //データ格納
                dt = ds.Tables[0];      //今回は最初のテーブルに入っているためインデックス[0]を指定
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }
            //データテーブルを戻す
            return dt;
        }

    }
}
