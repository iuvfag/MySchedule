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
        private void button1_Click(object sender, EventArgs e)
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
                ErrorMessage.errorMessage();
            }

        }

        /// <summary>
        /// 登録ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
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
                String userIdCheck = InputChecker.doCheck(userId, "ログインID", 1, 15);
                String passwordCheck = InputChecker.doCheck(password, "パスワード", 5, 15);
                String reConfirmationPasswordCheck = InputChecker.doCheck(reConfirmationPassword, "再確認用パスワード", 5, 15);
                String passwordCompare = InputChecker.passwordCompare(password, reConfirmationPassword);

                //値のチェックの結果何も戻り値がなければ次の処理へ
                if (userIdCheck == "" && passwordCheck == "" && reConfirmationPasswordCheck == "")
                {
                    //入力された2つのパスワードが一致するなら次の処理へ
                    if (passwordCompare == "")
                    {

                        //ログインIDが既に存在するか確認し、問題ないなら次の処理へ
                        if (!(UserInfoDAO.isExsitsUser(userId)))
                        {
                            //ユーザー情報を登録し、登録件数を戻り値として受け取る
                            int result = UserInfoDAO.createUser(userId, password);

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
                    //2つのパスワードが一致しない場合
                    else
                    {
                        //パスワード比較結果の戻り値(メッセージ)表示
                        MessageBox.Show(passwordCompare, "パスワードが正しく入力されていません");
                    }
                }
                /* 値のチェックに問題があった場合、原因(戻り値があるかどうか)によって行う処理を分岐させる */
                else
                {
                    //ログインID入力に問題があった場合
                    if (userIdCheck != "")      //ログインIDチェックの戻り値が空欄でない場合
                    {
                        //ログインIDチェックの戻り値(エラーメッセージ)を表示
                        MessageBox.Show(userIdCheck, "入力内容を確認してください");
                    }
                    //パスワードの入力内容に問題があった場合
                    if (passwordCheck != "")        //パスワードチェックの戻り値が空欄でない場合
                    {
                        //パスワードチェックの戻り値を表示
                        MessageBox.Show(passwordCheck, "入力内容を確認してください");
                    }
                    //再確認パスワードに問題があった場合
                    if (reConfirmationPasswordCheck != "")      //再確認用パスワードチェックの戻り値が空欄でない場合
                    {
                        //再確認用パスワードチェックの戻り値を表示
                        MessageBox.Show(reConfirmationPasswordCheck, "入力内容を確認してください");
                    }
                }
            }
            //何らかの不具合が発生した場合
            catch (Exception ex)
            {
                //例外処理としてErrorMessageクラスの呼び出し
                ErrorMessage.errorMessage();
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
                ErrorMessage.errorMessage();
            }

        }
    }
}
