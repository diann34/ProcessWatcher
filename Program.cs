using System;
using System.Diagnostics;
using System.Threading;

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
            string additionalinfo = "";
            TimeSpan delayms = TimeSpan.FromMilliseconds(5000);
            Console.WriteLine("本程序旨在监测特定进程运行情况，并在符合条件时执行设置的操作");
            Console.WriteLine("请输入被监测进程名称(eg. \"notepad\"为notepad.exe):");
            string processname = Console.ReadLine();
            /*
            Console.WriteLine("请输入监测间隔时长(单位:ms)(若无效则设置为默认值5000):");
            if (uint.TryParse(Console.ReadLine(), out uint delay))
            {
                if (delay <= 0) { Console.WriteLine("输入无效，已设置为默认值5000"); delayms = TimeSpan.FromMilliseconds(5000); }
                else { delayms = TimeSpan.FromMilliseconds(delay); }
            }
            else { Console.WriteLine("输入无效，已设置为默认值5000"); delayms = TimeSpan.FromMilliseconds(5000); }
            */
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
                    Console.WriteLine("请设置超时时间(单位:ms)(若无效则设置为默认值60000):");
                    if (uint.TryParse(Console.ReadLine(), out uint timeout))
                    {
                        if (timeout <= 0) { Console.WriteLine("输入无效，已设置为默认值"); additionalinfo = "60000"; }
                        else { additionalinfo = timeout.ToString(); }
                    }
                    else
                    {
                        Console.WriteLine("输入无效，已设置为默认值"); additionalinfo = "60000";
                    }
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
                sc:
                    operation = Operation.Shell;
                    Console.WriteLine("请输入要执行的cmd命令(单行):");
                    shellcommand = Console.ReadLine();
                    if (string.IsNullOrEmpty(shellcommand))
                    {
                        Console.WriteLine("错误:输入不能为空！" + Environment.NewLine + "按下任意键重新选择按下任意键重新输入...");
                        Console.ReadKey(false);
                        goto sc;
                    }
                    break;
                default:
                    Console.WriteLine("错误:无效选项" + Environment.NewLine + "按下任意键重新选择...");
                    Console.ReadKey(false);
                    goto chooseoperation;
                    break;
            }
            Console.Clear();
            Console.WriteLine("----------------");
            Console.WriteLine("配置信息:监测" + processname);
            string eventname="";
            switch (processEvent)
            {
                case ProcessEvent.Detected:
                    eventname = "被检测到运行";
                    break;
                case ProcessEvent.Run_out_of_time:
                    eventname = "被检测到运行超时(从开始监测起)";
                    break;
                case ProcessEvent.Exited:
                    eventname = "被检测到退出";
                    break;
                default:
                    Console.WriteLine("错误:未指定触发器" + Environment.NewLine + "按下任意键退出程序...");
                    Console.ReadKey(false);
                    Environment.Exit(Environment.ExitCode);
                    break;
            }
            string operationname="";
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
                    Console.ReadKey(false);
                    Environment.Exit(Environment.ExitCode);
                    break;
            }
            Console.WriteLine("在被监测进程" + eventname + "时" + operationname + ",每隔" + delayms.TotalMilliseconds + "ms检测一次");
            if (processEvent == ProcessEvent.Run_out_of_time)
            {
                Console.WriteLine("超时时长:" + additionalinfo);
            }
            if (operation == Operation.Shell)
            {
                Console.WriteLine("命令内容:" + shellcommand);
            }
            Console.WriteLine("----------------");
            Console.WriteLine("按下任意键开始执行...");
            Console.ReadKey();
            WriteOutput("开始执行");
            Run(processname, processEvent, operation, delayms, shellcommand, additionalinfo);
        }
        static void PerformOperation(Operation operation, string additionalinfo = null)
        {
            WriteOutput("满足条件，开始执行指定操作");
            switch (operation)
            {
                case Operation.Shutdown:
                    Process.Start("shutdown","-s -t 60");
                    break;
                case Operation.Shell:
                    Process.Start("cmd","/c "+additionalinfo);
                    break;
                default:
                    Console.WriteLine("未知要执行的操作！"+Environment.NewLine+"按下任意键退出...");
                    Console.ReadKey(false);
                    Environment.Exit(Environment.ExitCode);
                    break;
            }
            Console.WriteLine("操作已执行完毕！"+Environment.NewLine+"按下任意键退出...");
            Console.ReadKey(false);
            Environment.Exit(Environment.ExitCode);
        }
        static void Run(string processname, ProcessEvent processEvent, Operation operation, TimeSpan delay, string shellcommand, string additionalinfo = null)
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
                            PerformOperation(operation,shellcommand);
                        }
                        else { WriteOutput("未检测到目标进程"); }
                        Thread.Sleep((int)delay.TotalMilliseconds);
                    }
                    break;
                case ProcessEvent.Run_out_of_time:
                    if (!IsProcessExists(processname).Item1)
                    {
                        Console.WriteLine("未检测到指定进程！程序将退出且不执行操作"+Environment.NewLine+"按下任意键退出...");
                        Console.ReadKey(false);
                        Environment.Exit(Environment.ExitCode);
                    }

                    DateTime startTime = DateTime.Now;
                    TimeSpan timeout = TimeSpan.FromMilliseconds(uint.Parse(additionalinfo));
                    bool operationPerformed = false;

                    while (true)
                    {
                        var check = IsProcessExists(processname);
                        if (!check.Item1)
                        {
                            break;
                        }

                        TimeSpan elapsed = DateTime.Now - startTime;
                        if (elapsed >= timeout)
                        {
                            operationPerformed = true;
                            PerformOperation(operation, shellcommand);
                            break;
                        }

                        WriteOutput($"进程已运行 {elapsed.TotalMilliseconds}ms / {timeout.TotalMilliseconds}ms");
                        Thread.Sleep((int)delay.TotalMilliseconds);
                    }

                    if (!operationPerformed)
                    {
                        WriteOutput("进程已退出，程序将退出而不执行操作" + Environment.NewLine + "按下任意键退出...");
                        Console.ReadKey(false);
                        Environment.Exit(Environment.ExitCode);
                    }
                    break;
                case ProcessEvent.Exited:
                    if (!IsProcessExists(processname).Item1) { Console.WriteLine("未检测到指定进程！程序将退出且不执行操作" + Environment.NewLine + "按下任意键退出..."); Console.ReadKey(false); Environment.Exit(Environment.ExitCode); }
                    while (true)
                    {
                        (bool, int[]) isfound = IsProcessExists(processname);
                        if (isfound.Item1)
                        {
                            WriteOutput("找到目标进程，ID:" + string.Join(",", isfound.Item2));
                        }
                        else
                        {
                            PerformOperation(operation,shellcommand);
                        }
                        Thread.Sleep((int)delay.TotalMilliseconds);
                    }
                    break;

            }


        }
        static (bool, int[]) IsProcessExists(string processname)
        {
            Process[] processes = Process.GetProcessesByName(processname);
            int[] ids = new int[processes.Length];
            for (int i = 0; i < processes.Length; i++)
            {
                ids[i] = processes[i].Id;
            }
            return (processes.Length != 0, ids);
        }
        static void WriteOutput(string content) => Console.WriteLine(DateTime.Now + "    |    " + content);
    }
}
