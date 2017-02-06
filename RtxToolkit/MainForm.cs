using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using RTXCAPILib;
using RTXClient;

namespace RtxToolkit
{
    public partial class MainForm : Form
    {
        RTXClient.RTXAPIClass ObjApi = new RTXAPIClass();
        private RTXCAPILib.IRTXCRoot RTXCRoot;
        public ConfigEntity configEntity = new ConfigEntity();
        private bool formClosing = false;

        public MainForm()
        {
            InitializeComponent();
            //if (GetRTCPath().Length==0)
            //{
            //    MessageBox.Show("你可能安装了假的RTX!!!");
            //}
            configEntity.Load();

            txt_Account.Text = configEntity.Account;
            txt_Port.Text = configEntity.ServerPort.ToString();
            txt_serverAddress.Text = configEntity.ServerAddress;
            txt_Password.Text = configEntity.Password;
            txt_RTXPath.Text = configEntity.RTXPath;
            RTXCRoot = (IRTXCRoot)ObjApi.GetObject("KernalRoot");
        }

        delegate void UpdateStatusDelegate(string status);

        private void UpdateStatus(string status)
        {
            if (this.lbl_status.InvokeRequired)
            {
                while (!this.lbl_status.IsHandleCreated)
                {
                    //解决窗体关闭时出现“访问已释放句柄“的异常
                    if (this.lbl_status.Disposing || this.lbl_status.IsDisposed)
                        return;
                }
                UpdateStatusDelegate d = new UpdateStatusDelegate(UpdateStatus);
                this.lbl_status.Invoke(d, new object[] { status });
            }
            else
            {
                lbl_status.Text = status;
                notifyIcon1.Text = status;
            }
            
        }

        private bool Connect()
        {
            try
            {
                Console.WriteLine("Begin Connecting...");
                RTXCRoot.Login(configEntity.ServerAddress, configEntity.ServerPort, configEntity.Account, configEntity.Password);
                return true;
            }
            catch (COMException ee)
            {
                Console.WriteLine(ee.StackTrace);
                return false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            formClosing = true;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            try
            {
                configEntity.Account = txt_Account.Text;
                configEntity.Password = txt_Password.Text;
                configEntity.ServerAddress = txt_serverAddress.Text ;
                configEntity.ServerPort = int.Parse(txt_Port.Text);
                configEntity.RTXPath = txt_RTXPath.Text;
                configEntity.Save();
                MessageBox.Show("Save Successful!!!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Failed" + ex.ToString());
            }
        }

        private void btn_LocateRTX_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c://";
            openFileDialog.Filter = "RTX文件|RTX.exe";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fName = openFileDialog.FileName;
                txt_RTXPath.Text = fName;
            }
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            if (configEntity.Account.Length == 0 || configEntity.Password.Length == 0)
            {
                MessageBox.Show("no account or password!!!");
                return;
            }

            new Thread(new ThreadStart(() =>
            {
                while (!formClosing)
                {
                    try
                    {
                        if (!isRtxRunning() && configEntity.RTXPath.Length > 0)
                        {
                            System.Diagnostics.Process.Start(configEntity.RTXPath);
                            Thread.Sleep(5000);
                        }
                        if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour <= 21)
                        {
                            //上班时间
                            if (RTXCRoot.MyPresence == RTX_PRESENCE.RTX_PRESENCE_OFFLINE) Connect();
                            if (DateTime.Now.Hour >= 12 && DateTime.Now.Hour < 13)
                            {
                                RTXCRoot.MyPresence = RTX_PRESENCE.RTX_PRESENCE_AWAY;
                            }
                            else
                            {
                                RTXCRoot.MyPresence = RTX_PRESENCE.RTX_PRESENCE_ONLINE;
                            }
                        }
                        else
                        {
                            //下班时间
                            RTXCRoot.MyPresence = RTX_PRESENCE.RTX_PRESENCE_OFFLINE;
                        }

                        UpdateStatus(DateTime.Now.ToString("HH:mm:ss ") + RTXCRoot.MyPresence.ToString().Replace("RTX_PRESENCE_", string.Empty).ToLower());
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        MessageBox.Show(ex.StackTrace);
                        Thread.Sleep(1000);
                    }
                }
            })).Start();
            EnableAllControl(false);

        }

        private string GetRTCPath()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"LinkRTX\shell\open\command");
                return key.GetValue(null).ToString().Replace("Link", string.Empty).Replace("\"", string.Empty).Replace("%", string.Empty).Replace("1", string.Empty).Trim();
            }
            catch (Exception)
            {
                return "";
            }
        }

        private bool isRtxRunning()
        {
            var processList = System.Diagnostics.Process.GetProcessesByName("rtx");
            return (processList.Length > 0);
        }

        private void EnableAllControl(bool enabled)
        {
            foreach (var control in Controls)
            {
                if (control.GetType() == typeof(TextBox))
                {
                    ((TextBox)control).Enabled = enabled;
                }

                if (control.GetType() == typeof(Button))
                {
                    ((Button)control).Enabled = enabled;
                }
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.notifyIcon1.Visible = true;
        }

       
    }
}
