using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {


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



        [DllImport("kernel32", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr CreateToolhelp32Snapshot([In]UInt32 dwFlags, [In]UInt32 th32ProcessID);

        [DllImport("kernel32", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern bool Process32First([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern bool Process32Next([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32", SetLastError = true)]
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
               // throw new ApplicationException("Can't get the process.", ex);
            }
            finally
            {
                // Must clean up the snapshot object!
                CloseHandle(handleToSnapshot);
            }
            return parentProc;
        }

        // get the specific parent process
      //  public static Process CurrentParentProcess
      //  {
            //get{
              //  return GetParentProcess(Process.GetProcesses("."));
          //  }
       // }
 



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TreeNode node = new TreeNode();
           // treeView1.Nodes.Add("1","t1");
            string [] mas=new string[10];
            string[] mas1 = new string[10];
            int k = 0; int count = 0;
            string name="";
            var myProcess = Process.GetProcesses(".");
            for(int i=0;i<myProcess.Length;i++)
            {
               
                treeView1.Nodes.Add(myProcess[i].ProcessName);
                Process pr = GetParentProcess(i);
                if (pr != null)
                {
                    mas[k] = pr.ProcessName;
                    mas1[k] = myProcess[i].ProcessName;
                  //  treeView1.Nodes[i].Remove();
                    //if (k == 0)
                    //{
                      
                    //    treeView1.Nodes.Add(pr.ProcessName);
                    //    treeView1.Nodes[i].Nodes.Add(myProcess[i].ProcessName);
                    //    k++;
                    //}
                    //else
                    //{
                    //    if (mas[k - 1] != mas[k] && k < mas.Length)
                    //    {
                    //        treeView1.Nodes[i].Remove();
                    //        treeView1.Nodes.Add(pr.ProcessName);
                    //        treeView1.Nodes[i].Nodes.Add(myProcess[i].ProcessName);
                    //        k++;
                    //    }
                    //}
                    name=pr.ProcessName;
                    k++;
                }
              //  int j = treeView1.Nodes.Count();
                string s = treeView1.Nodes[i].Name;
                 if(s==name)
                 {
                       treeView1.Nodes[i].Nodes.Add(myProcess[i].ProcessName);
                 }
          

            }
            //for (int j = 0; j < mas.Length; j++)
            //{
            //    if(mas[j]!=null & mas1[j]!=null)
            //        if(treeView1.Nodes)
            //}

           // MessageBox.Show("Parent Proc. ID: {0}, Parent Proc. name: {1}"+ pr.Id+" "+ pr.ProcessName);

        }
    }
}
