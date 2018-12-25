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
        public String password { get; set; }

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
        private void Button1_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合に強制終了するためのtry-catch文
            try
            {
                //テキストボックスに入力された値を変数に格納
                userId = textBox1.Text;
                String password = textBox2.Text;

                //入力内容をInputCheckerに渡し、入力内容をチェック
                String userIdCheck = userId.DoCheck("ログインID", 1, 15);
                String passwordCheck = password.DoCheck("パスワード", 5, 15);

                //入力内容に問題がなく何も帰ってこなかった場合は次の処理へ
                if (string.IsNullOrWhiteSpace(userIdCheck) && String.IsNullOrWhiteSpace(passwordCheck))
                {

                    BlockChain bc = new BlockChain();
                    //パスワード暗号化の準備、まずログインIDをハッシュ関数化する
                    String userIdHash = bc.CreateHashKey(userId);
                    //パスワードをハッシュ関数に変換する
                    password = bc.CreateHashKey(password);
                    //上記2つの値を連結してハッシュ関数化したものをパスワードとして格納する
                    password = bc.CreateHashKey($"{userIdHash}{password}");

                    //ログインIDとパスワードをログイン用メソッドに渡し、結果をUserInfoDTOに格納
                    UserInfoDTO uiDTO = new UserInfoDTO();
                    UserInfoDAO uiDAO = new UserInfoDAO();
                    uiDTO = uiDAO.Login(userId, password);

                    //値が取れたかどうかチェック
                    if (uiDTO.userId != null)
                    {

                        if (uiDTO.status == 1)
                        {
                            AdminForm af = new AdminForm();
                            af.ShowDialog(this);
                            af.Dispose();
                            textBox1.Text = "";
                            textBox2.Text = "";
                            
                        }
                        else
                        {
                            //ログインIDとパスワード入力欄は空に戻しておく
                            textBox1.Text = "";
                            textBox2.Text = "";

                            //とれていたらスケジュールを呼び出し、ログインIDを渡す
                            ScheduleCalender sc = new ScheduleCalender()
                            {
                                userId = uiDTO.userId
                            };
                            sc.Owner = this;
                            sc.ShowDialog(this);

                            sc.Dispose();
                            this.userId = "";
                        }
                    }
                    //何も取ってこれなかったとき
                    else
                    {
                        //メッセージ表示
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
                ErrorMessage.ApplicationClose();
            }

            

        }

        /// <summary>
        /// 新規登録ボタンが押された場合の動作
        /// </summary>
        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ///何らかの不具合が発生た場合に強制終了するためのtry-catch文
            try
            {
                //新規ユーザー登録フォームを開く
                CreateUser cu = new CreateUser();
                cu.Owner = this;
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
                ErrorMessage.ApplicationClose();
            }
            
        }
    }
}
