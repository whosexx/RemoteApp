using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public enum LogLevel : byte
{
    DEBUG,
    INFO,
    WARN,
    ERROR,
    FATAL,
    OFF,
}

/// <summary>
/// 日志系统
/// </summary>
public sealed class Logger : IDisposable
{
    #region 全局控制
    public static string Dir {get;set;} = "Log";

    /// <summary>
    /// 日志文件后缀
    /// </summary>
    public static string Suffix { get; set; } = ".log";

    /// <summary>
    /// 默认日志文件
    /// </summary>
    public static string Default { get; set; } = "default";
    /// <summary>
    /// 全局是否在命令行输出
    /// </summary>
    public static bool GConsole { get; set; } = true;
    /// <summary>
    /// 全局是否每一次强制flush
    /// </summary>
    public static bool GForceFlush { get; set; } = false;

    /// <summary>
    /// 如果以文件模式写，是否追加
    /// </summary>
    public static bool GIsAppend { get; set; } = false;

    /// <summary>
    /// 写入文件的编码格式
    /// </summary>
    public static Encoding GEncoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// 写日志回调
    /// </summary>
    public static event Action<string, LogLevel, string> WriteLogEvent;

    /// <summary>
    /// 单位B
    /// </summary>
    public static long GMaxSize { get; set; }
    /// <summary>
    /// 最大支持99个文件
    /// </summary>
    public static uint GCount { get; private set; }
    public static void SetCount(uint c)
    {
        if (c > 99)
            throw new ArgumentOutOfRangeException("最大不能超过99.");

        GCount = c;
    }

    private static readonly ConcurrentDictionary<string, Logger> LFactory;
    private static readonly object GMonitor;
    private static LogLevel Level { get; set; } = LogLevel.DEBUG;
    #endregion

    private static System.Threading.Semaphore lsemp;
    private static System.Collections.Concurrent.ConcurrentQueue<(string Name, LogLevel Level, string Log)> LogQ;
    static Logger()
    {
        LFactory = new ConcurrentDictionary<string, Logger>();
        GMonitor = new object();
        GMaxSize = 10 * 1024 * 1024;
        GCount = 5;

        lsemp = new Semaphore(0, int.MaxValue);
        LogQ = new ConcurrentQueue<(string, LogLevel, string)>();
        Task.Run(() => {
            while(true)
            {
                try
                {
                    if (!lsemp.WaitOne(1000))
                        continue;

                    if (!LogQ.TryDequeue(out var log))
                    {
                        while (!LogQ.TryDequeue(out log))
                            Thread.Sleep(1);
                    }

                    Logger.WriteLogEvent?.Invoke(log.Name, log.Level, log.Log);
                }
                catch(Exception e) { Console.WriteLine(e.ToString()); }
            }
        });
    }

    private StreamWriter LWriter { get; set; }
    private object LMonitor { get; set; }
    private string LName { get; set; }
    private int CurrCount { get; set; } = 0;
    private Logger([NotNull] string name)
    {
        this.LMonitor = new object();

        DirectoryInfo dinfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\" + Dir);
        if (!dinfo.Exists)
            dinfo.Create();

        this.LName = name;
        FileInfo finfo = new FileInfo(dinfo.FullName + "\\" + this.LName + Suffix);
        this.LWriter = new StreamWriter(finfo.FullName, Logger.GIsAppend, Logger.GEncoding);
    }

    public long Length
    {
        get { return this.LWriter.BaseStream.Position; }
    }

    public void Reload()
    {
        string name = Dir + "\\" + this.LName + Suffix;
        FileInfo finfo = new FileInfo(name);

        string dname = Dir + "\\" + this.LName + "." + CurrCount.ToString("D2") + Suffix;
        FileInfo dest = new FileInfo(dname);
        if (dest.Exists)
        {
            if ((dest.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                dest.Attributes = FileAttributes.Normal;
            try { dest.Delete(); } catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        lock (this.LMonitor)
        {            
            this.LWriter.Close();            
            try { finfo.MoveTo(dest.FullName); }
            catch (Exception e) { Console.WriteLine(e.ToString()); }            

            this.LWriter = new StreamWriter(name, false, Logger.GEncoding);
            CurrCount++;
            if (CurrCount >= GCount)
                CurrCount = 0;
        }
    }
    
    public static void SetLogLevel(LogLevel level = LogLevel.DEBUG)
    {
        Logger.Level = level;
        if (level == LogLevel.OFF)
            Clear();
    }

    public static Logger Register([NotNull] string file)
    {
        try { FileInfo finfo = new FileInfo(file); }
        catch { throw new ArgumentException("参数有误，不正确！"); }

        if (LFactory.TryGetValue(file, out Logger log))
            return log;

        log = new Logger(file);
        lock (Logger.GMonitor)         
            LFactory.TryAdd(file, log);

        return log;
    }

    public static void UnRegister([NotNull] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        Logger log = null;
        lock (Logger.GMonitor)
            while (!LFactory.TryRemove(name, out log)) ;

        log?.Dispose();
    }

    public static void FlushLogger()
    {
        foreach (var p in Logger.LFactory.Values.ToList())
            p.Flush();
    }

    /// <summary>
    /// 关闭除了默认日志以外的所有日志
    /// </summary>
    public static void Clear()
    {
        foreach (var p in Logger.LFactory.Values.ToList())
            p.Dispose();
        
        lock (Logger.GMonitor)
            Logger.LFactory.Clear();
    }

    private static Logger Get([NotNull] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        LFactory.TryGetValue(name, out Logger log);
        return log;
    }

    public static void Debug(string log, bool console = false) => Logger.WriteLine(log, Logger.Default, console, LogLevel.DEBUG);
    public static void Info(string log, bool console = false) => Logger.WriteLine(log, Logger.Default, console, LogLevel.INFO);
    public static void Warn(string log, bool console = false) => Logger.WriteLine(log, Logger.Default, console, LogLevel.WARN);
    public static void Error(string log, bool console = false) => Logger.WriteLine(log, Logger.Default, console, LogLevel.ERROR);
    public static void Fatal(string log, bool console = false) => Logger.WriteLine(log, Logger.Default, console, LogLevel.FATAL);

    public static void Debug(string log, string file, bool console = false) => Logger.WriteLine(log, file, console, LogLevel.DEBUG);
    public static void Info(string log, string file, bool console = false) => Logger.WriteLine(log, file, console, LogLevel.INFO);
    public static void Warn(string log, string file, bool console = false) => Logger.WriteLine(log, file, console, LogLevel.WARN);
    public static void Error(string log, string file, bool console = false) => Logger.WriteLine(log, file, console, LogLevel.ERROR);
    public static void Fatal(string log, string file, bool console = false) => Logger.WriteLine(log, file, console, LogLevel.FATAL);

    public static void WriteLine(string log, LogLevel level) => Logger.WriteLine(log, Logger.Default, false, level);
    public static void WriteLine(string log, LogLevel level, bool console) => Logger.WriteLine(log, Logger.Default, console, level);
    public static void WriteLine(string log, bool console) => Logger.WriteLine(log, Logger.Default, console, LogLevel.DEBUG);
    public static void WriteLine(string log, string logfile = null, bool console = false, LogLevel level = LogLevel.DEBUG, bool onlyconsole = false)
    {
        if (Logger.Level >= LogLevel.OFF || level < Logger.Level)
            return;

        if (string.IsNullOrWhiteSpace(logfile))
            logfile = Default;

        Logger linstance = Logger.Get(logfile);
        if (linstance == null)
            linstance = Logger.Register(logfile);

        if (linstance != null)
            linstance.WriteLog(log, level, console, onlyconsole);
        else
            throw new Exception("日志写入出错了！");
    }


    /// <summary>
    /// 写入一行日志
    /// </summary>
    /// <param name="log"></param>
    public void WriteLog(string logLine, LogLevel level, bool console = true, bool onlyconsole = false)
    {
        string top = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss][");
        string last = "][" + logLine + "]";

        try
        {
            lock (this.LMonitor)
            {
                if (console && GConsole)
                {
                    ConsoleColor color;
                    switch (level)
                    {
                        case LogLevel.DEBUG:
                        color = ConsoleColor.Cyan;
                        break;
                        case LogLevel.INFO:
                        color = ConsoleColor.Green;
                        break;
                        case LogLevel.WARN:
                        color = ConsoleColor.Yellow;
                        break;
                        case LogLevel.ERROR:
                        color = ConsoleColor.Red;
                        break;
                        case LogLevel.FATAL:
                        color = ConsoleColor.DarkRed;
                        break;
                        default:
                        color = ConsoleColor.White;
                        break;
                    }

                    Console.Write(top);
                    Console.ForegroundColor = color;
                    Console.Write(level);
                    Console.ResetColor();
                    Console.WriteLine(last);
                    if (onlyconsole)
                        return;
                }

                var log = top + level + last;
                if (this.LWriter != null)
                {
                    try
                    {
                        this.LWriter.WriteLine(log);
                        if (this.Length >= GMaxSize)
                            this.Reload();

                        if (GForceFlush)
                            this.LWriter.Flush();
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                }
            }
        }
        finally
        {
            LogQ.Enqueue((this.LName, level, logLine));
            lsemp.Release();
        }
    }

    public void Flush()
    {
        lock (this.LMonitor)
            this.LWriter?.Flush();
    }

    ~Logger()
    {
        this.Dispose(false);
    }

    private bool IsDisposed { get; set; } = false;
    private void Dispose(bool isdispose)
    {
        if (this.IsDisposed)
            return;

        if (isdispose)
        {
            lock (this.LMonitor)
            {
                if (this.IsDisposed)
                    return;

                if (this.LWriter != null)
                {
                    try { this.LWriter.Dispose(); }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    this.LWriter = null;
                }
            }
        }

        this.IsDisposed = true;
    }

    /// <summary>
    /// 关闭日志文件流
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);        
    }
}