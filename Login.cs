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

    public partial class Login : Form
    {

        public String userId { get; set; }

        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            //呼び出された時点で空欄にしておく
            textBox1.Text = "";
            textBox2.Text = "";
            textBox2.PasswordChar = '*';
            this.userId = "";
            
        }
        /// <summary>
        /// 「ログイン」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合に強制終了するためのtry-catch文
            try
            {
                //テキストボックスに入力された値を変数に格納
                userId = textBox1.Text;
                String password = textBox2.Text;

                //入力内容をInputCheckerに渡し、入力内容をチェック
                String userIdCheck = InputChecker.doCheck(userId, "ログインID", 1, 15);
                String passwordCheck = InputChecker.doCheck(password, "パスワード", 5, 15);

                //入力内容に問題がなく何も帰ってこなかった場合は次の処理へ
                if (string.IsNullOrWhiteSpace(userIdCheck) && String.IsNullOrWhiteSpace(passwordCheck))
                {

                    //ログインIDとパスワードをログイン用メソッドに渡し、結果をresultに格納
                    String result = UserInfoDAO.login(userId, password);

                    //値が取れたかどうかチェック
                    if (result != "")
                    {
                        //とれていたらスケジュールを呼び出し、ログインIDを渡す
                        ScheduleCalender sc = new ScheduleCalender();
                        sc.userId = result;
                        sc.ShowDialog(this);

                        //スケジュールを閉じた場合はログインIDとパスワード入力欄は空に戻しておく
                        textBox1.Text = "";
                        textBox2.Text = "";
                        sc.Dispose();
                        this.userId = "";
                    }
                    else
                    {
                        //何も取ってこれなかったとき
                        MessageBox.Show("ログインIDとパスワードの組み合わせが正しくありません", "入力内容を確認してください");
                    }
                }
                //入力内容に問題があった場合
                else
                {
                    //ログインID入力欄に関するのメッセージ表示
                    if (!(String.IsNullOrWhiteSpace(userIdCheck)))
                    {
                        MessageBox.Show(userIdCheck, "入力内容を確認してください");
                    }
                    //パスワード入力欄に関するのメッセージ表示
                    if (!(String.IsNullOrWhiteSpace(passwordCheck)))
                    {
                        MessageBox.Show(passwordCheck, "入力内容を確認してください");
                    }
                }
            }
            //何らかの不具合が発生した場合
            catch (Exception ex)
            {
                //例外処理としてErrorMessageクラスを呼び出す
                ErrorMessage.errorMessage();
            }

            

        }

        /// <summary>
        /// 新規登録ボタンが押された場合の動作
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ///何らかの不具合が発生た場合に強制終了するためのtry-catch文
            try
            {
                //新規ユーザー登録フォームを開く
                CreateUser cu = new CreateUser();
                cu.ShowDialog(this);
                cu.Dispose();

                //入力欄を空欄に戻す
                textBox1.Text = "";
                textBox2.Text = "";
            }
            //何らかの不具合が発生した場合
            catch (Exception ex)
            {
                //例外処理としてErrorMessageクラスを呼び出す
                ErrorMessage.errorMessage();
            }
            
        }
    }
}
