using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        // Methods
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        public static string GetWindowText(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd) + 1;
            StringBuilder sb = new StringBuilder(len);
            len = GetWindowText(hWnd, sb, len);
            return sb.ToString(0, len);
        }
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

      
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        public static void WindowsToListBox(ListBox el)
        {
            el.Items.Clear();
            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (IsWindowVisible(hWnd) && (GetWindowTextLength(hWnd) != 0))
                {
                    el.Items.Add(GetWindowText(hWnd));
                }
                return true;
            }, IntPtr.Zero);
        }

        //inner enum used only internally
        [Flags]
        private enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }
        //inner struct used only internally
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PROCESSENTRY32
        {
            const int MAX_PATH = 260;
            internal UInt32 dwSize;
            internal UInt32 cntUsage;
            internal UInt32 th32ProcessID;
            internal IntPtr th32DefaultHeapID;
            internal UInt32 th32ModuleID;
            internal UInt32 cntThreads;
            internal UInt32 th32ParentProcessID;
            internal Int32 pcPriClassBase;
            internal UInt32 dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            internal string szExeFile;
        }



        [DllImport("kernel32", SetLastError = false, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr CreateToolhelp32Snapshot([In]UInt32 dwFlags, [In]UInt32 th32ProcessID);

        [DllImport("kernel32", SetLastError = false, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern bool Process32First([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32", SetLastError = false, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern bool Process32Next([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle([In] IntPtr hObject);



        // get the parent process given a pid
        public static Process GetParentProcess(int pid)
        {
            Process.GetProcesses(".");
            Process parentProc = null;
            IntPtr handleToSnapshot = IntPtr.Zero;
            try
            {
                PROCESSENTRY32 procEntry = new PROCESSENTRY32();
                procEntry.dwSize = (UInt32)Marshal.SizeOf(typeof(PROCESSENTRY32));
                handleToSnapshot = CreateToolhelp32Snapshot((uint)SnapshotFlags.Process, 0);
                if (Process32First(handleToSnapshot, ref procEntry))
                {
                    do
                    {
                        if (pid == procEntry.th32ProcessID)
                        {
                            parentProc = Process.GetProcessById((int)procEntry.th32ParentProcessID);
                            break;
                        }
                    } while (Process32Next(handleToSnapshot, ref procEntry));
                }
                else
                {
                    throw new ApplicationException(string.Format("Failed with win32 error code {0}", Marshal.GetLastWin32Error()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
              //  throw new ApplicationException("Can't get the process.", ex);
            }
            finally
            {
                // Must clean up the snapshot object!
                CloseHandle(handleToSnapshot);
            }
            return parentProc;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TreeNode node = new TreeNode();
            // treeView1.Nodes.Add("1","t1");
            string[] mas = new string[10];
            string[] mas1 = new string[10];
            int k = 0; int count = 0;
            string name = "";
            var myProcess = Process.GetProcesses(".");
            string [] names_of_processes=new string[myProcess.Length];

            for (int i = 0; i < myProcess.Length; i++)
            {
                names_of_processes[i] = myProcess[i].ProcessName;
               
            }
            names_of_processes = names_of_processes.Distinct().ToArray();
            for(int i=0;i<names_of_processes.Length;i++)
            {
                Process pr = GetParentProcess(i);
                if (pr != null)
                {
                    mas[k] = pr.ProcessName;
                    mas1[k] = names_of_processes[i];
                    bool flag = treeView1.Nodes.ContainsKey(pr.ProcessName);
                    if (flag==false)
                    {
                        treeView1.Nodes.Add(pr.ProcessName, pr.ProcessName);
                        treeView1.Nodes[i].Nodes.Add(names_of_processes[i], names_of_processes[i]);
                    }
                    else
                    {
                        treeView1.Nodes[pr.ProcessName].Nodes.Add(names_of_processes[i], names_of_processes[i]);
                    }
                        name = pr.ProcessName;
                        k++;
                    }
                else
                {
                    bool flag = treeView1.Nodes.ContainsKey(names_of_processes[i]);
                    if(flag==false)
                    treeView1.Nodes.Add(names_of_processes[i]);
                        
                    } 
                }

            DataGridViewColumn col1 = new DataGridViewColumn();
            DataGridViewColumn col2 = new DataGridViewColumn();
            col1.HeaderText="Процесс";
            col1.Name = "process";
            col2.Name = "sved";

            col2.HeaderText = "Сведения о приоритете";


            dataGridView1.Columns.Add("process", "Процесс");
            dataGridView1.Columns.Add("sved", "Сведения о приоритете");
            dataGridView1.Columns.Add("id", "ID процесса");

            try
            {
                for (int i = 0; i < names_of_processes.Length; i++)
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells["process"].Value = names_of_processes[i];
                    if(names_of_processes[i]=="gajim" ||
                       names_of_processes[i] == "AGSService")
                        dataGridView1.Rows[i].Cells["sved"].Value = "Normal";
                    else
                        dataGridView1.Rows[i].Cells["sved"].Value = myProcess[i].PriorityClass;

                    dataGridView1.Rows[i].Cells["id"].Value = myProcess[i].Id;


                }
            }
            catch(Exception ex)
            {
               // dataGridView1.Rows[3].Visible = false;
              //  MessageBox.Show(ex.Message);
            }

            var openWindowProcesses = System.Diagnostics.Process.GetProcesses(".")
   .Where(p => p.MainWindowHandle != IntPtr.Zero ).ToArray();
            string s = "";
            for(int i=0;i<openWindowProcesses.Length;i++)
            {
                treeView2.Nodes.Add(openWindowProcesses[i].ProcessName);
            }



            dataGridView2.Columns.Add("process", "Окно");
            dataGridView2.Columns.Add("sved", "Сведения об окне");

            try
            {
                for (int i = 0; i < openWindowProcesses.Length; i++)
                {
                    dataGridView2.Rows.Add();
                    dataGridView2.Rows[i].Cells["process"].Value = openWindowProcesses[i].ProcessName;
                    if (openWindowProcesses[i].MainWindowTitle!="")
                        dataGridView2.Rows[i].Cells["sved"].Value = "Visilble";
                    else
                        dataGridView2.Rows[i].Cells["sved"].Value = "UnVisilble";


                }
            }
            catch (Exception ex)
            {
                // dataGridView1.Rows[3].Visible = false;
                //  MessageBox.Show(ex.Message);
            }

        }
    }
}
