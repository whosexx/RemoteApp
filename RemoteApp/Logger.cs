using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 日志系统
/// </summary>
public class Logger : IDisposable
{
    public enum LogLevel:byte
    {
        ALL = 0,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL,
        OFF,
    }

    #region 全局控制
    private const string LPath = "Log";
    private const string Suffix = ".log";
    public const string Default = "default";

    private static ConcurrentDictionary<string, Logger> LFactory = new ConcurrentDictionary<string, Logger>();
    private static object GMonitor = new object();
    private static bool IsAppend = false;
    private static Encoding Encoding = Encoding.UTF8;
    private static LogLevel Level { get; set; } = LogLevel.DEBUG;
    #endregion

    private StreamWriter LWriter { get; set; }
    private object LMonitor { get; set; }
    private string LName { get; set; }    
    private Logger(string name)
    {
        this.LMonitor = new object();

        DirectoryInfo dinfo = new DirectoryInfo(LPath);
        if (!dinfo.Exists)
            dinfo.Create();

        this.LName = name;
        FileInfo finfo = new FileInfo(dinfo.FullName + "\\" + this.LName + Suffix);
        this.LWriter = new StreamWriter(finfo.FullName, Logger.IsAppend, Logger.Encoding);
    }

    private bool IsDisposed = false;
    ~Logger()
    {
        //if (!this.IsDisposed)
        //    this.Dispose();
    }

    public static void SetFormat(bool append, Encoding encode)
    {
        Logger.IsAppend = append;
        Logger.Encoding = encode;
    }

    public static void SetLogLevel(LogLevel level = LogLevel.DEBUG)
    {
        Logger.Level = level;
        if (level == LogLevel.OFF)
            Clear();
    }

    public static Logger Register(string name)
    {
        if (Logger.Level >= LogLevel.OFF)
            return null;

        try { FileInfo finfo = new FileInfo(name); }
        catch { throw new ArgumentException("参数有误，不正确！"); }

        Logger log = null;
        lock (Logger.GMonitor)
        {
            if (LFactory.TryGetValue(name, out log))
                return log;

            log = new Logger(name);
            LFactory.TryAdd(name, log);
        }

        return log;
    }

    public static void UnRegister(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        Logger log = null;
        lock (Logger.GMonitor)
        {
            if (LFactory.TryRemove(name, out log))
                log.Dispose();
        }
    }

    /// <summary>
    /// 关闭除了默认日志以外的所有日志
    /// </summary>
    public static void Clear()
    {
        lock (Logger.GMonitor)
        {
            foreach (var p in Logger.LFactory.ToDictionary(m => m.Key, m => m.Value))
            {
                if (p.Key.ToLower() == Logger.Default.ToLower())
                    continue;

                p.Value.Dispose();

                Logger log;
                while (!Logger.LFactory.TryRemove(p.Key, out log)) ;
            }
        }
    }

    private static Logger Get(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Logger log = null;
        LFactory.TryGetValue(name, out log);
        return log;
    }

    /// <summary>
    /// 写入一条日志
    /// </summary>
    /// <param name="log"></param>
    public static void WriteLine(string log, string logfile = Default, bool console = false, LogLevel level = LogLevel.DEBUG)
    {
        if (Logger.Level >= LogLevel.OFF)
            return;

        if (level < Logger.Level)
            return;

        Logger linstance = Logger.Get(logfile);
        if (linstance == null)
            linstance = Logger.Register(logfile);

        if (linstance != null)
            linstance.WriteLog(log, true, console);
    }


    /// <summary>
    /// 写入一行日志
    /// </summary>
    /// <param name="log"></param>
    public void WriteLog(string logLine, bool newline = true, bool console = true)
    {
        if (console)
        {
            if (newline)
                Console.WriteLine(logLine);
            else
                Console.Write(logLine);
        }
            
        lock (this.LMonitor)
        {
            if (this.LWriter != null)
            {
                if (newline)
                    this.LWriter.WriteLine(logLine);
                else
                    this.LWriter.Write(logLine);

                this.LWriter.Flush();
            }
        }
    }

    public void Flush()
    {
        if (this.LWriter == null)
            return;

        lock (this.LMonitor)
        {
            if (this.LWriter == null)
                return;

            this.LWriter.Flush();
        }
    }

    /// <summary>
    /// 关闭日志文件流
    /// </summary>
    public void Dispose()
    {
        if (this.IsDisposed)
            return;

        lock (this.LMonitor)
        {
            if (this.IsDisposed)
                return;

            if (this.LWriter != null)
            {
                this.IsDisposed = true;
                try { this.LWriter.Dispose(); }
                catch (Exception) { }
                this.LWriter = null;
            }
        }
    }
}