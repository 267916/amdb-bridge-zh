// AMDB Bridge 中文壳 (AMDB Bridge Chinese Shell)
// 运行时把 AMDB Bridge 的 Win32 控件文字替换成中文，不修改原程序文件。
// 编译: csc /target:exe /codepage:65001 /out:AMDB-ZH.exe AmdbZhFinal.cs

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Threading;

class AmdbZh
{
    // ---------------- Win32 ----------------
    delegate bool EnumProc(IntPtr h, IntPtr l);

    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc cb, IntPtr l);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr p, EnumProc cb, IntPtr l);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowTextW(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern bool SetWindowTextW(IntPtr h, string s);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassNameW(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr SendMessageW(IntPtr h, uint m, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("kernel32.dll")] static extern IntPtr OpenProcess(uint access, bool inherit, uint pid);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
    [DllImport("kernel32.dll")] static extern IntPtr VirtualAllocEx(IntPtr hp, IntPtr addr, uint size, uint type, uint protect);
    [DllImport("kernel32.dll")] static extern bool VirtualFreeEx(IntPtr hp, IntPtr addr, uint size, uint type);
    [DllImport("kernel32.dll")] static extern bool WriteProcessMemory(IntPtr hp, IntPtr addr, byte[] buf, uint size, out IntPtr written);
    [DllImport("kernel32.dll")] static extern bool ReadProcessMemory(IntPtr hp, IntPtr addr, byte[] buf, uint size, out IntPtr read);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] static extern bool SetCurrentConsoleFontEx(IntPtr hOut, bool max, ref CONSOLE_FONT_INFOEX info);
    [DllImport("kernel32.dll")] static extern IntPtr GetStdHandle(int n);

    [StructLayout(LayoutKind.Sequential)] struct RECT { public int L, T, R, B; }
    [StructLayout(LayoutKind.Sequential)] struct COORD { public short X, Y; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct CONSOLE_FONT_INFOEX
    {
        public uint cbSize; public uint nFont; public COORD dwFontSize;
        public uint FontFamily; public uint FontWeight;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string FaceName;
    }

    const uint PROCESS_VM_OPERATION = 0x0008, PROCESS_VM_READ = 0x0010,
               PROCESS_VM_WRITE = 0x0020, PROCESS_QUERY_INFORMATION = 0x0400;
    const uint MEM_COMMIT_RESERVE = 0x3000, PAGE_READWRITE = 0x04, MEM_RELEASE = 0x8000;

    const uint HDM_GETITEMCOUNT = 0x1200, HDM_SETITEMW = 0x120C;
    const uint LVM_GETITEMCOUNT = 0x1004, LVM_GETITEMTEXTW = 0x1073, LVM_SETITEMTEXTW = 0x1074;
    const uint HDI_TEXT = 0x0002, LVIF_TEXT = 0x0001;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct HDITEMW
    {
        public uint mask; public int cxy; public IntPtr pszText; public int cchTextMax;
        public int fmt; public IntPtr lParam; public int iImage; public int iOrder;
        public uint type; public IntPtr pvFilter; public uint state;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct LVITEMW
    {
        public uint mask; public int iItem; public int iSubItem; public uint state; public uint stateMask;
        public IntPtr pszText; public int cchTextMax; public int iImage; public IntPtr lParam;
        public int iIndent; public int iGroupId; public uint cColumns; public IntPtr puColumns;
        public IntPtr piColFmt; public int iGroup;
    }

    static string Text(IntPtr h) { var sb = new StringBuilder(2048); GetWindowTextW(h, sb, 2048); return sb.ToString(); }

    static string Reverse(string cn)
    {
        foreach (var kv in Dict) if (kv.Value == cn) return kv.Key;
        return null;
    }
    static string Cls(IntPtr h) { var sb = new StringBuilder(256); GetClassNameW(h, sb, 256); return sb.ToString(); }

    // ---------------- 词表 ----------------
    static readonly Dictionary<string, string> Dict = new Dictionary<string, string>
    {
        { "Version", "版本" },
        { "Airport moving maps for your simulator, built from free open data.",
          "用免费开放数据为你的模拟器生成机场移动地图。" },

        { "Start", "开始" }, { "Stop", "停止" }, { "Starting", "正在启动" },
        { "Not serving", "未提供服务" }, { "Serving maps", "正在提供地图" },
        { "Stopped", "已停止" }, { "Could not start", "无法启动" }, { "Try again", "重试" },
        { "Getting ready. This takes a moment the first time.", "正在准备，首次需要一点时间。" },
        { "Press Start before loading your aircraft.", "加载机模前请先点“开始”。" },

        { "Simulators and aircraft", "模拟器与机模" }, { "Approach charts", "进近航图" },
        { "Options", "选项" }, { "Activity", "活动记录" }, { "Airport", "机场" },
        { "Where", "位置" }, { "What", "内容" }, { "Status", "状态" }, { "Aircraft", "机模" },
        { "Approach", "进近程序" }, { "Runway", "跑道" }, { "Arrivals that feed it", "衔接的进场" },

        { "Install or update A220 map", "安装或更新 A220 地图" },
        { "Remove A220 map", "移除 A220 地图" },
        { "Refresh", "刷新" }, { "Find procedures", "查找程序" }, { "Draw chart", "绘制航图" },
        { "Aircraft report", "机模报告" }, { "Airports folder", "机场文件夹" },
        { "Save log", "保存日志" }, { "Change\u2026", "更改\u2026" }, { "Change", "更改" },
        { "Menu", "菜单" }, { "Limit", "上限" }, { "none", "无" },
        { "Tablet charts: OFF", "平板航图：关" }, { "Tablet charts: ON", "平板航图：开" },

        { "Start serving as soon as AMDB Bridge opens", "AMDB Bridge 打开后立即开始服务" },
        { "Open AMDB Bridge in the notification area when Windows starts", "Windows 启动时在通知区域打开 AMDB Bridge" },
        { "Open AMDB Bridge when Microsoft Flight Simulator starts", "Microsoft Flight Simulator 启动时打开 AMDB Bridge" },
        { "Install the X-Plane 12 moving map when serving starts", "开始服务时安装 X-Plane 12 移动地图" },
        { "Also serve the iniBuilds A350 and FlyByWire A380X (asks for administrator permission once)",
          "同时服务 iniBuilds A350 与 FlyByWire A380X（需要一次管理员授权）" },
        { "Keep built airports on disk, so they load instantly next time",
          "把生成的机场保存在磁盘上，下次即刻载入" },
        { "Serve the A350 and A380X", "服务 A350 与 A380X" },

        { "Synaptic A220 moving map", "Synaptic A220 移动地图" },
        { "Tablet charts: iniBuilds A350", "平板航图：iniBuilds A350" },
        { "Not installed", "未安装" }, { "Ready", "就绪" }, { "Missing", "缺失" },
        { "Its own (Navigraph)", "自带（Navigraph）" },
        { "its own (Navigraph)", "自带（Navigraph）" },
        { "patched", "已打补丁" }, { "NOT patched", "未打补丁" }, { "redirect", "重定向" },

        { "Type an airport's ICAO code first, like LPMA or KJFK", "请先输入机场 ICAO 代码，例如 LPMA 或 KJFK" },
        { "Storage changes apply the next time serving starts", "存储设置将在下次开始服务时生效" },
        { "Where should built airports be kept?", "生成的机场保存在哪里？" },
        { "There is no log yet", "暂时还没有日志" },
        { "No chart tablets found", "未找到平板航图设备" },
        { "Collecting aircraft information", "正在收集机模信息" },
        { "Send both files with your report", "提交报告时请附上这两个文件" },
        { "The A220 map and X-Plane do not need this.", "A220 地图与 X-Plane 不需要此项。" },
        { "Any other A220 map it set aside is put back.", "被它挪开的其他 A220 地图会被放回原位。" },
        { "Remove the A220 moving map from every simulator on this computer?", "从本机所有模拟器中移除 A220 移动地图？" },
        { "The A220 map is installed. Restart Microsoft Flight Simulator to load it.",
          "A220 地图已安装。请重启 Microsoft Flight Simulator 以加载。" },
        { "AMDB Bridge is in the notification area. Right-click the icon to exit.",
          "AMDB Bridge 已缩到通知区域。右键图标可退出。" },
        { "Still serving maps. Open AMDB Bridge or exit it from this icon.",
          "仍在提供服务。可打开 AMDB Bridge，或从此图标退出。" },

        { "The iniBuilds A350 and FlyByWire A380X ask Navigraph's map server for airports directly. To answer them, AMDB Bridge points that address at this computer, installs a local certificate, and patches the A350's EFB so it does not ask you to sign in.",
          "iniBuilds A350 与 FlyByWire A380X 会直接向 Navigraph 的地图服务器请求机场数据。为了应答它们，AMDB Bridge 会把这个地址指向本机、安装一个本地证书，并改写 A350 的 EFB，使其不再要求你登录。" },
        { "Windows asks for administrator permission once. While this option is on, those aircraft get their airport maps from AMDB Bridge, so keep it running when you fly them. Untick it, or uninstall AMDB Bridge, to undo all of it.",
          "Windows 会要求一次管理员授权。此选项开启期间，这些机模的机场地图由 AMDB Bridge 提供，因此飞行时请保持其运行。取消勾选或卸载 AMDB Bridge 即可完全撤销。" },
        { "no simulator has an exe.xml yet - start Microsoft Flight Simulator once, then try again",
          "还没有任何模拟器生成 exe.xml —— 请先启动一次 Microsoft Flight Simulator，然后重试" },
        { "please enter a number of megabytes", "请输入一个兆字节(MB)数值" },
        { "please give a full path (e.g. D:/amdb-cache)", "请输入完整路径（例如 D:/amdb-cache）" },
        { "please answer y or n", "请回答 y 或 n" },
        { "could not change the Windows start-up entry: ", "无法修改 Windows 启动项：" },
        { "the hosts-file redirect and certificate need administrator rights to remove",
          "移除 hosts 重定向与证书需要管理员权限" },
    };

    // 列头：按纵坐标排序，第 1 个是机模表，第 2 个是进近程序表
    static readonly string[][] HeaderZh = {
        new[] { "位置", "内容", "状态" },
        new[] { "进近程序", "跑道", "衔接的进场" },
    };
    static readonly string[][] HeaderEn = {
        new[] { "Where", "What", "Status" },
        new[] { "Approach", "Runway", "Arrivals that feed it" },
    };

    static readonly string[] SkipClasses = { "Edit", "SysHeader32", "msctfime ui", "IME", "tooltips_class32" };

    // ---------------- 动态文本 ----------------
    static string Translate(string t)
    {
        if (string.IsNullOrWhiteSpace(t)) return null;
        string v;
        if (Dict.TryGetValue(t, out v)) return v;
        Match m;

        m = Regex.Match(t, @"^Version\s+(.+)$");
        if (m.Success) return "版本 " + m.Groups[1].Value;

        m = Regex.Match(t, @"^●\s*Serving maps$");
        if (m.Success) return "●  正在提供地图";

        m = Regex.Match(t, @"^Tablet charts:\s*(ON|OFF)$");
        if (m.Success) return m.Groups[1].Value == "ON" ? "平板航图：开" : "平板航图：关";

        m = Regex.Match(t, @"^(.*?)\s*·\s*([\d.]+) MB of (\d+) MB used on disk\s*·\s*A350/A380X\s+(on|off)(.*)$");
        if (m.Success)
        {
            string lead = m.Groups[1].Value.Trim();
            if (lead == "no airports loaded yet") lead = "尚未载入机场";
            else if (lead == "1 airport loaded") lead = "已载入 1 个机场";
            else if (lead == "airports are not kept on disk") lead = "机场不保存在磁盘上";
            else { var mm = Regex.Match(lead, @"^(\d+) airports loaded$"); if (mm.Success) lead = "已载入 " + mm.Groups[1].Value + " 个机场"; }
            string tail = m.Groups[4].Value == "on" ? "A350/A380X 已开启" : "A350/A380X 已关闭";
            return lead + "  ·  磁盘已用 " + m.Groups[2].Value + " MB / " + m.Groups[3].Value + " MB  ·  " + tail;
        }

        string[,] pref = {
            { "Could not start: ",        "无法启动：" },
            { "Could not save the log: ", "无法保存日志：" },
            { "Could not save settings: ", "无法保存设置：" },
            { "Log saved to Downloads: ", "日志已保存到下载文件夹：" },
            { "Saved to Downloads: ",     "已保存到下载文件夹：" },
            { "Could not change the tablets in ", "无法更改平板航图：" },
        };
        for (int i = 0; i < pref.GetLength(0); i++)
            if (t.StartsWith(pref[i, 0])) return pref[i, 1] + t.Substring(pref[i, 0].Length);
        return null;
    }

    // ---------------- 远程内存 ----------------
    static IntPtr RemoteAlloc(IntPtr hp, int size)
    {
        return VirtualAllocEx(hp, IntPtr.Zero, (uint)size, MEM_COMMIT_RESERVE, PAGE_READWRITE);
    }

    // ---------------- 列头：盲写 ----------------
    static void DoHeaders(IntPtr hp, List<KeyValuePair<IntPtr, int>> hdrs, bool restore, ref int n)
    {
        hdrs.Sort((a, b) => a.Value.CompareTo(b.Value));
        int sz = Marshal.SizeOf(typeof(HDITEMW));
        int textBytes = 512;
        for (int k = 0; k < hdrs.Count; k++)
        {
            IntPtr hdr = hdrs[k].Key;
            if (!restore && HeaderDone.Contains(hdr)) continue;
            HeaderDone.Add(hdr);
            string[] names = restore ? (k < HeaderEn.Length ? HeaderEn[k] : null)
                                     : (k < HeaderZh.Length ? HeaderZh[k] : null);
            if (names == null) continue;
            int cnt = (int)SendMessageW(hdr, HDM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
            for (int i = 0; i < cnt && i < names.Length; i++)
            {
                IntPtr remote = RemoteAlloc(hp, sz + textBytes);
                if (remote == IntPtr.Zero) continue;
                try
                {
                    byte[] raw = Encoding.Unicode.GetBytes(names[i]);
                    byte[] pad = new byte[textBytes];
                    Array.Copy(raw, pad, Math.Min(raw.Length, textBytes - 2));
                    IntPtr wr;
                    WriteProcessMemory(hp, (IntPtr)((long)remote + sz), pad, (uint)textBytes, out wr);

                    var hd = new HDITEMW();
                    hd.mask = HDI_TEXT;
                    hd.pszText = (IntPtr)((long)remote + sz);
                    hd.cchTextMax = textBytes / 2;

                    IntPtr lp = Marshal.AllocHGlobal(sz);
                    Marshal.StructureToPtr(hd, lp, false);
                    byte[] sb = new byte[sz];
                    Marshal.Copy(lp, sb, 0, sz);
                    Marshal.FreeHGlobal(lp);

                    WriteProcessMemory(hp, remote, sb, (uint)sz, out wr);
                    SendMessageW(hdr, HDM_SETITEMW, (IntPtr)i, remote);
                    n++;
                }
                finally { VirtualFreeEx(hp, remote, 0, MEM_RELEASE); }
            }
        }
    }

    // ---------------- ListView 行 ----------------
    static void DoRows(IntPtr hp, IntPtr lv, bool restore, ref int n)
    {
        int sz = Marshal.SizeOf(typeof(LVITEMW));
        int textBytes = 512;
        int rows = (int)SendMessageW(lv, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
        if (rows <= 0 || rows > 500) return;
        IntPtr remote = RemoteAlloc(hp, sz + textBytes);
        if (remote == IntPtr.Zero) return;
        try
        {
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < 6; c++)
                {
                    byte[] st = new byte[sz];
                    BitConverter.GetBytes(LVIF_TEXT).CopyTo(st, 0);
                    BitConverter.GetBytes(r).CopyTo(st, 4);
                    BitConverter.GetBytes(c).CopyTo(st, 8);
                    BitConverter.GetBytes((long)remote + sz).CopyTo(st, 24);
                    BitConverter.GetBytes(256).CopyTo(st, 32);
                    IntPtr wr;
                    WriteProcessMemory(hp, remote, st, (uint)sz, out wr);
                    SendMessageW(lv, LVM_GETITEMTEXTW, (IntPtr)r, remote);
                    byte[] tb = new byte[textBytes];
                    IntPtr rd;
                    ReadProcessMemory(hp, (IntPtr)((long)remote + sz), tb, (uint)textBytes, out rd);
                    string cur = Encoding.Unicode.GetString(tb);
                    int z = cur.IndexOf('\0'); if (z >= 0) cur = cur.Substring(0, z);
                    if (string.IsNullOrEmpty(cur)) continue;

                    string nw = null;
                    if (restore) { foreach (var kv in Dict) if (kv.Value == cur) { nw = kv.Key; break; } }
                    else nw = Translate(cur);
                    if (nw == null || nw == cur) continue;

                    byte[] raw = Encoding.Unicode.GetBytes(nw);
                    byte[] pad = new byte[textBytes];
                    Array.Copy(raw, pad, Math.Min(raw.Length, textBytes - 2));
                    WriteProcessMemory(hp, (IntPtr)((long)remote + sz), pad, (uint)textBytes, out wr);

                    byte[] st2 = new byte[sz];
                    BitConverter.GetBytes(LVIF_TEXT).CopyTo(st2, 0);
                    BitConverter.GetBytes(r).CopyTo(st2, 4);
                    BitConverter.GetBytes(c).CopyTo(st2, 8);
                    BitConverter.GetBytes((long)remote + sz).CopyTo(st2, 24);
                    BitConverter.GetBytes(256).CopyTo(st2, 32);
                    WriteProcessMemory(hp, remote, st2, (uint)sz, out wr);
                    SendMessageW(lv, LVM_SETITEMTEXTW, (IntPtr)r, remote);
                    n++;
                }
        }
        finally { VirtualFreeEx(hp, remote, 0, MEM_RELEASE); }
    }

    // ---------------- 主流程 ----------------
    static uint targetPid;
    static readonly List<IntPtr> Handles = new List<IntPtr>();
    static readonly List<IntPtr> Targets = new List<IntPtr>();
    // Hot: 被程序改回英文过的控件（即真正会变的），快档只查这些
    static readonly List<IntPtr> Hot = new List<IntPtr>();
    static readonly Dictionary<IntPtr, string> LastSeen = new Dictionary<IntPtr, string>();
    static int fullCount = 0;
    static readonly HashSet<IntPtr> HeaderDone = new HashSet<IntPtr>();
    static volatile bool stop = false;

    static uint FindApp()
    {
        var procs = System.Diagnostics.Process.GetProcessesByName("AMDB Bridge");
        return procs.Length == 0 ? 0 : (uint)procs[0].Id;
    }

    static void Collect()
    {
        Handles.Clear();
        Targets.Clear();
        EnumWindows((h, l) =>
        {
            uint p; GetWindowThreadProcessId(h, out p);
            if (p == targetPid)
            {
                Handles.Add(h);
                EnumChildWindows(h, (k, l2) =>
                {
                    Handles.Add(k);
                    string c = Cls(k);
                    if (c != "SysHeader32" && c != "SysListView32" && Array.IndexOf(SkipClasses, c) < 0)
                        Targets.Add(k);
                    return true;
                }, IntPtr.Zero);
            }
            return true;
        }, IntPtr.Zero);
    }

    // full=true 做完整枚举 + 列头/表格（较贵，约每秒一次）
    // full=false 只检查已缓存的控件文字（极快，用于压制每秒的英文闪回）
    static int ApplyPass(bool restore, bool full)
    {
        if (full) Collect();
        int n = 0;
        var hdrs = new List<KeyValuePair<IntPtr, int>>();
        var lvs = new List<IntPtr>();
        IntPtr hp = IntPtr.Zero;
        if (full)
            hp = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION, false, targetPid);
        try
        {
            if (full)
            {
                foreach (IntPtr h in Handles)
                {
                    string cls = Cls(h);
                    if (cls == "SysHeader32") { RECT rr; GetWindowRect(h, out rr); hdrs.Add(new KeyValuePair<IntPtr, int>(h, rr.T)); }
                    else if (cls == "SysListView32") lvs.Add(h);
                }
            }

            // 慢档：查全部，并识别出哪些控件会被程序改回去
            if (full)
            {
                foreach (IntPtr h in Targets)
                {
                    string cur = Text(h);
                    if (string.IsNullOrWhiteSpace(cur)) continue;
                    string prev;
                    if (LastSeen.TryGetValue(h, out prev) && prev != cur && !Hot.Contains(h)) Hot.Add(h);
                    LastSeen[h] = cur;

                    string nw = restore ? Reverse(cur) : Translate(cur);
                    if (nw != null && nw != cur) { SetWindowTextW(h, nw); n++; LastSeen[h] = nw; }
                }
            }
            else
            {
                // 快档：只查会变的少数几个控件，代价极低
                foreach (IntPtr h in Hot)
                {
                    string cur = Text(h);
                    if (string.IsNullOrWhiteSpace(cur)) continue;
                    string nw = restore ? Reverse(cur) : Translate(cur);
                    if (nw != null && nw != cur) { SetWindowTextW(h, nw); n++; }
                }
            }

            if (full && hp != IntPtr.Zero)
            {
                DoHeaders(hp, hdrs, restore, ref n);
                if (restore || (++fullCount % 4 == 0))
                    foreach (IntPtr lv in lvs) DoRows(hp, lv, restore, ref n);
            }
        }
        finally { if (hp != IntPtr.Zero) CloseHandle(hp); }
        return n;
    }

    static void SetConsoleCjkFont()
    {
        try
        {
            var f = new CONSOLE_FONT_INFOEX();
            f.cbSize = (uint)Marshal.SizeOf(typeof(CONSOLE_FONT_INFOEX));
            f.dwFontSize = new COORD { X = 0, Y = 18 };
            f.FontFamily = 54; f.FontWeight = 400;
            foreach (string face in new string[] { "新宋体", "宋体", "SimSun", "Microsoft YaHei", "微软雅黑", "MS Gothic" })
            {
                f.FaceName = face;
                if (SetCurrentConsoleFontEx(GetStdHandle(-11), false, ref f)) return;
            }

        }
        catch { }
    }

    // 外置词表 zh.txt：每行 "英文=中文"，# 开头为注释。可新增或覆盖内置词条。
    static void LoadExternalDict()
    {
        try
        {
            string f = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zh.txt");
            if (!System.IO.File.Exists(f)) return;
            int n = 0;
            foreach (string line in System.IO.File.ReadAllLines(f, Encoding.UTF8))
            {
                string s = line.Trim();
                if (s.Length == 0 || s.StartsWith("#")) continue;
                int eq = s.IndexOf('=');
                if (eq <= 0) continue;
                string k = s.Substring(0, eq).Trim();
                string v = s.Substring(eq + 1).Trim();
                if (k.Length > 0 && v.Length > 0) { Dict[k] = v; n++; }
            }
            if (n > 0) Console.WriteLine("已载入外置词表 zh.txt（" + n + " 条）");
        }
        catch { }
    }
    const string Ver = "1.1";
    // 轮询间隔(ms)。程序每个刷新周期会重写英文，间隔越小"闪回英文"的窗口越短。
    static int PollMs = 30;   // 可用 --poll N 调整（5~2000）

    static string StatePath()
    {
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AMDB-ZH");
        try { Directory.CreateDirectory(dir); } catch { }
        return Path.Combine(dir, "state.txt");
    }

    // 首次运行（或换过文件夹）时显示使用说明与免责声明，并询问是否创建桌面快捷方式
    static void MaybeFirstRun(bool force)
    {
        string mine = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
        string state = StatePath();
        string prev = "";
        try { if (File.Exists(state)) prev = File.ReadAllText(state, Encoding.UTF8).Trim(); } catch { }
        if (!force && prev == mine) return;

        string msg =
            "【这是什么】\r\n" +
            "把 AMDB Bridge 的英文界面实时翻译成中文。\r\n" +
            "它不修改 AMDB Bridge 的任何文件，AMDB Bridge 升级后仍可使用。\r\n" +
            "\r\n" +
            "【怎么用】\r\n" +
            "点“是”会在桌面创建快捷方式；以后双击桌面图标，\r\n" +
            "它会自动启动中文壳与 AMDB Bridge。\r\n" +
            "要停止汉化：关闭本工具的黑色窗口，界面会自动恢复英文。\r\n" +
            "\r\n" +
            "【前提条件】\r\n" +
            "· AMDB Bridge 版本为 v1.0.0（其他版本可能部分内容不翻译）\r\n" +
            "· AMDB Bridge 必须以普通用户身份运行，不要用管理员身份\r\n" +
            "· AMDB Bridge 必须在运行，本工具才有作用\r\n" +
            "\r\n" +
            "【免责声明】\r\n" +
            "本工具为非官方第三方汉化，与 AMDB Bridge 作者无关，\r\n" +
            "未获其授权或认可，也未修改其任何程序文件。\r\n" +
            "本工具按“原样”提供，不附带任何明示或默示担保，\r\n" +
            "使用风险由使用者自行承担。\r\n" +
            "AMDB Bridge 本身是 MIT 协议的开源项目。\r\n" +
            "\r\n" +
            "是否在桌面创建快捷方式？";

        DialogResult r = MessageBox.Show(msg, "AMDB Bridge 中文壳 v" + Ver,
            MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (r == DialogResult.Yes) CreateDesktopShortcut(mine);
        try { File.WriteAllText(state, mine, Encoding.UTF8); } catch { }
    }

    // 用 WScript.Shell 延迟绑定创建 .lnk（不依赖任何额外引用）
    static void CreateDesktopShortcut(string dir)
    {
        try
        {
            string exe = Path.Combine(dir, "AMDB-ZH.exe");
            string ico = Path.Combine(dir, "AMDB-ZH.ico");
            string lnk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "AMDB Bridge 中文.lnk");
            Type t = Type.GetTypeFromProgID("WScript.Shell");
            if (t == null) { Console.WriteLine("创建快捷方式失败：找不到 WScript.Shell"); return; }
            object sh = Activator.CreateInstance(t);
            object sc = t.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, sh, new object[] { lnk });
            Type st = sc.GetType();
            st.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, sc, new object[] { exe });
            st.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, sc, new object[] { dir });
            st.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, sc, new object[] { "AMDB Bridge 中文壳" });
            st.InvokeMember("WindowStyle", System.Reflection.BindingFlags.SetProperty, null, sc, new object[] { 1 });
            if (File.Exists(ico))
                st.InvokeMember("IconLocation", System.Reflection.BindingFlags.SetProperty, null, sc, new object[] { ico + ",0" });
            st.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, sc, null);
            Console.WriteLine("已在桌面创建快捷方式：" + lnk);
        }
        catch (Exception e) { Console.WriteLine("创建快捷方式失败：" + e.Message); }
    }

    // 找 AMDB Bridge 主程序：先默认安装位置，再查卸载注册表项
    static string FindAppExe()
    {
        try
        {
            string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    @"Programs\AMDB Bridge\AMDB Bridge.exe");
            if (File.Exists(p)) return p;
        }
        catch { }
        foreach (RegistryKey root in new RegistryKey[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            try
            {
                using (RegistryKey k = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall"))
                {
                    if (k == null) continue;
                    foreach (string sub in k.GetSubKeyNames())
                    {
                        using (RegistryKey sk = k.OpenSubKey(sub))
                        {
                            if (sk == null) continue;
                            object dn = sk.GetValue("DisplayName");
                            if (dn == null || !dn.ToString().Contains("AMDB Bridge")) continue;
                            object loc = sk.GetValue("InstallLocation");
                            if (loc == null) continue;
                            string cand = Path.Combine(loc.ToString().TrimEnd('\\'), "AMDB Bridge.exe");
                            if (File.Exists(cand)) return cand;
                        }
                    }
                }
            }
            catch { }
        }
        return null;
    }

    // AMDB Bridge 没在运行就启动它（找不到就只提示，不报错）
    static void EnsureApp()
    {
        if (FindApp() != 0) return;
        string exe = FindAppExe();
        if (exe == null) { Console.WriteLine("未找到 AMDB Bridge，请自行启动它。"); return; }
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo(exe);
            psi.WorkingDirectory = Path.GetDirectoryName(exe);
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
            Console.WriteLine("已启动 AMDB Bridge。");
        }
        catch (Exception e) { Console.WriteLine("启动 AMDB Bridge 失败：" + e.Message); }
    }
    static void Main(string[] args)
    {
        Console.CancelKeyPress += (s, e) => { e.Cancel = true; stop = true; };
        try { Console.OutputEncoding = Encoding.UTF8; } catch { }
        SetConsoleCjkFont();
        LoadExternalDict();

        bool restore = Array.IndexOf(args, "--restore") >= 0;
        bool noApp   = Array.IndexOf(args, "--no-app") >= 0;
        bool setup   = Array.IndexOf(args, "--setup") >= 0;
        bool noSetup = Array.IndexOf(args, "--no-setup") >= 0;
        for (int ai = 0; ai < args.Length - 1; ai++)
            if (args[ai] == "--poll")
            {
                int v;
                if (int.TryParse(args[ai + 1], out v) && v >= 5 && v <= 2000) PollMs = v;
            }

        if (Array.IndexOf(args, "--status") >= 0)
        {
            uint sp = FindApp();
            string zhPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zh.txt");
            string st = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "AMDB-ZH", "state.txt");
            Console.WriteLine("AMDB Bridge：" + (sp == 0 ? "未运行（中文壳会一直等待它启动）" : "运行中，PID " + sp));
            Console.WriteLine("词表 zh.txt：" + (System.IO.File.Exists(zhPath) ? "已找到" : "缺失！请确认它和本程序在同一文件夹"));
            Console.WriteLine("首次运行向导：" + (System.IO.File.Exists(st) ? "已完成" : "尚未完成"));
            Console.WriteLine();
            Console.WriteLine("中文壳是常驻的：它运行期间，AMDB Bridge 的界面即为中文。");
            Console.WriteLine("关闭中文壳窗口 = 停止汉化并恢复英文。");
            return;
        }
        Console.WriteLine("AMDB Bridge 中文壳 v" + Ver);
        if (!restore && !noSetup) MaybeFirstRun(setup);
        if (!restore && !noApp) EnsureApp();
        Console.WriteLine("关闭本窗口 = 停止汉化并恢复英文。");
        Console.WriteLine();

        targetPid = FindApp();
        while (targetPid == 0) { Thread.Sleep(1500); targetPid = FindApp(); }

        if (restore)
        {
            Console.WriteLine("已恢复英文：" + ApplyPass(true, true) + " 处。");
            return;
        }

        Console.WriteLine("已找到 AMDB Bridge (PID " + targetPid + ")，开始汉化。");
        Console.WriteLine();

        int last = -1;
        DateTime lastLog = DateTime.MinValue;
        DateTime lastFull = DateTime.MinValue;
        while (!stop)
        {
            uint now = FindApp();
            if (now == 0)
            {
                if (targetPid != 0)
                {
                    Console.WriteLine("AMDB Bridge 已退出，恢复英文，继续等待...");
                    try { ApplyPass(true, true); } catch { }
                    targetPid = 0; HeaderDone.Clear(); Hot.Clear(); LastSeen.Clear(); last = -1;
                }
                Thread.Sleep(1500);
                targetPid = FindApp();
                continue;
            }
            if (now != targetPid) { targetPid = now; Console.WriteLine("PID 变为 " + now); }
            try
            {
                bool full = (DateTime.Now - lastFull).TotalSeconds >= 1.0;
                if (full) lastFull = DateTime.Now;
                int n = ApplyPass(false, full);
                if (n > 0 && n != last && (DateTime.Now - lastLog).TotalSeconds > 30)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  汉化 " + n + " 处");
                    lastLog = DateTime.Now;
                }
                last = n;
            }
            catch (Exception e) { Console.WriteLine("  错误: " + e.Message); }
            Thread.Sleep(PollMs);
        }

        try { ApplyPass(true, true); Console.WriteLine("已恢复英文。"); } catch { }
        Console.WriteLine("按回车退出。");
        Console.ReadLine();
    }
}
