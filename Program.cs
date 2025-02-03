using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessWatcher
{
    internal class Program
    {
        public enum ProcessEvent
        {
            Unspecified,
            Detected,
            Run_out_of_time,
            Exited
        }
        public enum Operation
        {
            Unspecified,
            Shutdown,
            Shell
        }
        static void Main(string[] args)
        {
            Operation operation = Operation.Unspecified;
            ProcessEvent processEvent = ProcessEvent.Unspecified;
            string shellcommand = "";
            Console.WriteLine("本程序旨在监测特定进程运行情况，并在符合条件时执行设置的操作");
            Console.WriteLine("请输入被监测进程名称(eg. \"notepad\"为notepad.exe):");
            string processname = Console.ReadLine();
        chooseevent:
            Console.WriteLine("请设置要监测的程序行为触发器：1.目标进程终止 2.检测到目标进程 3.进程执行超时");
            switch ((char)Console.ReadKey().Key)
            {
                case '1':
                    processEvent = ProcessEvent.Exited;
                    break;
                case '2':
                    processEvent = ProcessEvent.Detected;
                    break;
                case '3':
                    processEvent = ProcessEvent.Run_out_of_time;
                    break;
                default:
                    Console.WriteLine("错误:无效选项" + Environment.NewLine + "按下任意键重新选择...");
                    Console.ReadKey();
                    goto chooseevent;
                    break;
            }
        chooseoperation:
            Console.WriteLine("请选择当触发器被激活时执行的操作:1.关机 2.执行cmd命令");
            switch ((char)Console.ReadKey().Key)
            {
                case '1':
                    operation = Operation.Shutdown;
                    break;
                case '2':
                    operation = Operation.Shell;
                    Console.WriteLine("请输入要执行的cmd命令(单行):");
                    shellcommand = Console.ReadLine();
                    break;
                default:
                    Console.WriteLine("错误:无效选项" + Environment.NewLine + "按下任意键重新选择...");
                    Console.ReadKey();
                    goto chooseoperation;
                    break;
            }
            Console.Clear();
            Console.WriteLine("----------------");
            Console.WriteLine("配置信息:监测" + processname);
            string eventname;
            switch (processEvent)
            {
                case ProcessEvent.Detected:
                    eventname = "被检测到运行";
                    break;
                case ProcessEvent.Run_out_of_time:
                    eventname = "被检测到运行超时";
                    break;
                case ProcessEvent.Exited:
                    eventname = "被检测到退出时";
                    break;
                default:
                    Console.WriteLine("错误:未指定触发器" + Environment.NewLine + "按下任意键退出程序...");
                    Console.ReadKey();
                    return;
                    break;
            }
            string operationname;
            switch (operation)
            {
                case Operation.Shutdown:
                    operationname = "关机";
                    break;
                case Operation.Shell:
                    operationname = "执行指定cmd命令";
                    break;
                default:
                    Console.WriteLine("错误:未指定执行操作" + Environment.NewLine + "按下任意键退出程序...");
                    Console.ReadKey();
                    return;
                    break;
            }
            Console.WriteLine("在被监测进程" + eventname + "时" + operationname);
            if (!string.IsNullOrEmpty(shellcommand))
            {
                Console.WriteLine("命令内容:" + shellcommand);
            }
            Console.WriteLine("----------------");
            Console.WriteLine("按下任意键开始执行...");
            Console.ReadKey();
            WriteOutput("开始执行");
            Run(processname, processEvent, operation, shellcommand);
        }
        static void PerformOperation(Operation operation)
        {
            switch (operation)
            {
                case Operation.Shutdown:
                    break;
                case Operation.Shell:
                    break;
                default:
                    break;
            }
        }
        static void Run(string processname, ProcessEvent processEvent, Operation operation, string addtionalinfo)
        {
            switch (processEvent)
            {
                case ProcessEvent.Detected:
                    while (true)
                    {
                        (bool, int[]) isfound = IsProcessExists(processname);
                        if (isfound.Item1)
                        {
                            WriteOutput("找到目标进程，ID:" + string.Join(",", isfound.Item2));
                            PerformOperation(operation);
                        }
                        else { WriteOutput("未检测到目标进程"); }
                        Thread.Sleep(10000);
                    }
                    break;
                case ProcessEvent.Run_out_of_time:

                    break;
                case ProcessEvent.Exited:

                    break;

            }


        }
        static (bool, int[]) IsProcessExists(string processname)
        {
            Process[] processes = Process.GetProcessesByName(processname);
            int[] ids = new int[] { };
            for (int i = 0; i < processes.Length; i++)
            {
                ids[i] = processes[i].Id;
            }
            return (processes.Length != 0, ids);
        }
        static void WriteOutput(string content) => Console.WriteLine(DateTime.Now + "    |    " + content);
    }
}
