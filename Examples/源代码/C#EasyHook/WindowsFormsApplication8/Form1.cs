﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Reflection;
using ClassLibrary1;
using EasyHook;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormEasyHook
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        public Form1()
        {
            InitializeComponent();
        }

        private bool RegGACAssembly()
        {
            var dllName = "EasyHook.dll";
            var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName);
            //配置文件中必须加loadFromRemoteSources开启节点，否则无法加载程序集
            Assembly assembly = Assembly.LoadFrom(dllPath);
            if (!RuntimeEnvironment.FromGlobalAccessCache(assembly))
            {
                new System.EnterpriseServices.Internal.Publish().GacInstall(dllPath);
                Thread.Sleep(100);
            }

             dllName = "ClassLibrary1.dll";
             dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName);
            new System.EnterpriseServices.Internal.Publish().GacRemove(dllPath);
            if (!RuntimeEnvironment.FromGlobalAccessCache(Assembly.LoadFrom(dllPath)))
            {
                new System.EnterpriseServices.Internal.Publish().GacInstall(dllPath);
                Thread.Sleep(100);
            }

            return true;
        }

        private static bool InstallHookInternal(int processId)
        {
            try
            {
                var parameter = new HookParameter
                {
                    Msg = "已经成功注入目标进程",
                    HostProcessId = RemoteHooking.GetCurrentProcessId()
                };

                RemoteHooking.Inject(
                    processId,
                    InjectionOptions.Default,
                    typeof(HookParameter).Assembly.Location,
                    typeof(HookParameter).Assembly.Location,
                    string.Empty,
                    parameter
                );
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }

            return true;
        }

        private static bool IsWin64Emulator(int processId)
        {
            var process = Process.GetProcessById(processId);
            if (process == null)
                return false;

            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
            {
                bool retVal;

                return !(IsWow64Process(process.Handle, out retVal) && retVal);
            }

            return false; // not on 64-bit Windows Emulator
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var p = Process.GetProcessById(int.Parse(textBox1.Text));
            if (p == null)
            {
                MessageBox.Show("指定的进程不存在!");
                return;
            }

            if (IsWin64Emulator(p.Id) != IsWin64Emulator(Process.GetCurrentProcess().Id))
            {
                var currentPlat = IsWin64Emulator(Process.GetCurrentProcess().Id) ? 64 : 32;
                var targetPlat = IsWin64Emulator(p.Id) ? 64 : 32;
                MessageBox.Show(string.Format("当前程序是{0}位程序，目标进程是{1}位程序，请调整编译选项重新编译后重试！", currentPlat, targetPlat));
                return;
            }

            RegGACAssembly();
            InstallHookInternal(p.Id);
        }

        private void btnGet_Click(object sender, EventArgs e)
        {
            string name = txtProcessName.Text;
            Process[] process = Process.GetProcessesByName(name);
            if (process.Length > 0)
            {
                textBox1.Text = process.FirstOrDefault().Id.ToString();
            }
            else
            {
                MessageBox.Show("该进程不存在！");
            }
        }
    }
}
