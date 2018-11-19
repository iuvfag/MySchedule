using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySchedule
{
    public partial class CreateUser : Form
    {
        public CreateUser()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 画面読み込み時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateUser_Load(object sender, EventArgs e)
        {
            //親フォームは隠す
            this.Owner.Hide();
            textBox2.PasswordChar = '*';
            textBox3.PasswordChar = '*';
        }

        /// <summary>
        /// 戻るボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生に強制終了するためのtry-catch文
            try
            {
                //このフォームを閉じる
                this.Close();
            }
            catch (Exception ex)
            {
                //例外処理としてErrorMessageクラスを呼び出す
                ErrorMessage.ApplicationClose();
            }

        }

        /// <summary>
        /// 登録ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button2_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合に強制終了ためのtry-catch文
            try
            {
                //入力内容の受け取り
                String userId = textBox1.Text;
                String password = textBox2.Text;
                String reConfirmationPassword = textBox3.Text;

                //InputCheckerのインスタンス化

                //それぞれの値のチェック
                String userIdCheck = userId.DoCheck("ログインID", 1, 15);
                String passwordCheck = password.DoCheck("パスワード", 5, 15);
                String reConfirmationPasswordCheck = reConfirmationPassword.DoCheck("再確認用パスワード", 5, 15);
                String passwordCompare = password.ValueCompare(reConfirmationPassword,
                    "パスワード", "再確認用パスワード");

                //値のチェックの結果何も戻り値がなければ次の処理へ
                if (String.IsNullOrWhiteSpace(userIdCheck) && String.IsNullOrWhiteSpace(passwordCheck) &&
                    String.IsNullOrWhiteSpace(reConfirmationPasswordCheck) &&
                    String.IsNullOrWhiteSpace(passwordCompare))
                {

                    UserInfoDAO uiDAO = new UserInfoDAO();
                    BlockChain bc = new BlockChain();

                    //パスワード暗号化の準備、まず、ログインIDをハッシュ関数化
                    String userIdHash = bc.CreateHashKey(userId);
                    //パスワードをハッシュ関数化
                    password = bc.CreateHashKey(password);
                    //上記二つを連結してハッシュ関数化したものをパスワードとしてDBに保存する
                    password = bc.CreateHashKey($"{userIdHash}{password}");

                    //ログインIDが既に存在するか確認し、問題ないなら次の処理へ
                    if (!(uiDAO.IsExistsUser(userId)))
                    {
                        //ユーザー情報を登録し、登録件数を戻り値として受け取る
                        int result = uiDAO.CreateUser(userId, password);

                        //登録件数が1件でもあれば
                        if (result > 0)
                        {
                            //メッセージを表示
                            MessageBox.Show("MyScheduleを活用しましょう", "ユーザー登録完了！");
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("登録が行えませんでした");
                        }
                    }
                    //ログインIDが既に使用されている場合
                    else
                    {
                        //メッセージ表示
                        MessageBox.Show("別のログインIDを使用してください", "ログインIDが既に使用されています");
                    }

                }
                /* 値のチェックに問題があった場合、原因(戻り値があるかどうか)によって行う処理を分岐させる */
                else
                {
                    //ログインID入力に問題があった場合(ログインIDチェックの戻り値が空欄でない場合)
                    if (!(String.IsNullOrWhiteSpace(userIdCheck)))
                    {
                        //ログインIDチェックの戻り値(エラーメッセージ)を表示
                        MessageBox.Show(userIdCheck, "入力内容を確認してください");
                    }
                    //パスワードの入力内容に問題があった場合(パスワードチェックの戻り値が空欄でない場合)
                    if (!(String.IsNullOrWhiteSpace(passwordCheck)))
                    {
                        //パスワードチェックの戻り値を表示
                        MessageBox.Show(passwordCheck, "入力内容を確認してください");
                    }
                    //再確認パスワードに問題があった場合(再確認用パスワードチェックの戻り値が空欄でない場合)
                    if (!(String.IsNullOrWhiteSpace(reConfirmationPasswordCheck)))
                    {
                        //再確認用パスワードチェックの戻り値を表示
                        MessageBox.Show(reConfirmationPasswordCheck, "入力内容を確認してください");
                    }
                    //パスワードと再確認用パスワードが一致しない場合
                    if (!(String.IsNullOrWhiteSpace(passwordCompare)))
                    {
                        //パスワード比較の戻り値を表示
                        MessageBox.Show(passwordCompare, "入力内容を確認してください");
                    }
                }
            }
            //何らかの不具合が発生した場合
            catch (Exception ex)
            {
                //例外処理としてErrorMessageクラスの呼び出し
                ErrorMessage.ApplicationClose();
            }

        }

        /// <summary>
        /// このフォームが閉じられた場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateUser_FormClosed(object sender, FormClosedEventArgs e)
        {
            //何らかの不具合が発生した場合強制終了するためのtry-catch文
            try
            {
                //親フォームを表示
                this.Owner.Show();
            }
            //何らかの不具合が発生した場合
            catch (Exception)
            {
                //例外処理としてErrorMessageクラスの呼び出し
                ErrorMessage.ApplicationClose();
            }

        }
    }
}
