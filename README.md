# AMDB Bridge 中文壳

把 **AMDB Bridge** 的英文界面实时显示为中文。

**本仓库只包含中文壳，不包含 AMDB Bridge 本体。**

> ### AMDB Bridge 本体去哪下载
>
> | 渠道 | 地址 |
> |---|---|
> | 官方发布页（Flightsim.to） | https://flightsim.to/addon/115176/free-oans-anf-btv-for-msfs2020-and-msfs2024 |
> | 开源仓库（GitHub） | https://github.com/Vihaan2012-cmyk/Free-Airport-Mapping-DB |
> | 直接下载最新版 | https://github.com/Vihaan2012-cmyk/Free-Airport-Mapping-DB/releases/latest |
>
> 先装好 AMDB Bridge，再用本中文壳。本工具会自动找到它。

---

## 这是什么

一个运行时翻译层。它在内存中把 AMDB Bridge 的窗口文字替换成中文，**不修改 AMDB Bridge 的任何文件**。

AMDB Bridge 的界面英文是编译进程序里的，没有语言文件可用。中文壳通过 Windows API 直接读写它的窗口控件文字，因此：

- AMDB Bridge 升级后，中文壳依然可用，不需要跟着更新
- 停止中文壳，界面立即恢复英文

---

## 操作说明

### 使用

1. 把本仓库的文件解压到**同一个文件夹**（`AMDB-ZH.exe` 与 `zh.txt` 必须在同一目录）
2. 双击 `AMDB-ZH.exe`
3. 首次运行会弹出说明窗口，并询问是否在桌面创建快捷方式 —— 点「是」
4. 以后双击桌面图标即可。它会自动启动中文壳，也会在 AMDB Bridge 没运行时帮你启动它

### 停止汉化

关闭中文壳的黑色窗口 → 界面自动恢复英文。

若窗口被强制结束、没来得及恢复，双击 `恢复英文.cmd`。

### 让它常驻（可选）

中文壳启动后会一直等待 AMDB Bridge，程序关掉也不会退出。
按 `Win+R` 输入 `shell:startup`，把 `AMDB-ZH.exe` 的快捷方式放进去即可 —— 这样无论何时启动 AMDB Bridge 都会被汉化。

### 前提条件

| 条件 | 不满足会怎样 |
|---|---|
| 已安装 AMDB Bridge | 中文壳会提示找不到，并继续等待 |
| AMDB Bridge 版本为 v1.0.0 | 文案对不上 → 那几条保持英文，不会出错 |
| AMDB Bridge 以**普通用户身份**运行 | 若它提权运行，Windows 会拦截跨进程操作，中文壳静默失效 |
| 系统有中文字体 | 无中文字体会显示方框 |

### 自定义词表

`zh.txt` 可以新增或覆盖词条，格式：

```
英文原文=中文译文
```

`#` 开头为注释。**UTF-8 与 ANSI/GBK 都能正确读取**，用记事本随手保存即可，不必特意改编码。改完重启中文壳即生效，**不需要重新编译**。

### 自行编译

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe ^
  /nologo /target:exe /codepage:65001 /r:System.Windows.Forms.dll ^
  /out:AMDB-ZH.exe AmdbZhFinal.cs
```

> `AmdbZhFinal.cs` 必须存为 **UTF-8 带 BOM**，否则编译器会把中文读成乱码。

---

## 免责声明

1. **非官方**：本工具是第三方汉化，与 AMDB Bridge 的作者、发布者没有任何隶属关系，未获其授权或认可。

2. **不修改原程序**：本工具只在运行时读写 AMDB Bridge 的窗口文字，不修改、不替换、不重新分发 AMDB Bridge 的任何文件。本仓库也不包含 AMDB Bridge 的程序或源码。

3. **AMDB Bridge 本身**是 MIT 协议的开源项目，版权归其作者所有。

4. **不联网**：本工具完全在本机运行，不收集、不上传任何信息。所有译文都在随附的 `zh.txt` 纯文本中，可自行查看。

5. **可能被杀毒软件误报**：本工具需要读写其他进程的窗口文字，这一行为特征与部分恶意软件相似，因此可能被安全软件拦截。源码随附，可自行审阅或自行编译验证。

6. **无担保**：本工具按「原样」提供，不附带任何明示或默示的担保。使用风险由使用者自行承担。

7. 文中提及的 Navigraph、Microsoft Flight Simulator、iniBuilds 等名称均为各自权利人的商标或注册商标，本工具与这些公司均无关联。
