using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

class RdpIdentify{

    static string WinStationName="";

    static void Main(){
        MyTaskTray myTT = new MyTaskTray();
        IntPtr hhook = SetWinEventHook(EVENT_SYSTEM_SWITCHDESKTOP, EVENT_SYSTEM_SWITCHDESKTOP, IntPtr.Zero,
                procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        RdpIdentify.WinStationName = GetWinStationName();
        Application.Run();
        myTT.CloseTray();
        UnhookWinEvent(hhook);
    }

    delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
       hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
       uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    const uint EVENT_SYSTEM_SWITCHDESKTOP = 0x0020;
    const uint WINEVENT_OUTOFCONTEXT = 0;

    [DllImport("wtsapi32.dll")]
    public static extern bool WTSQuerySessionInformation(
        IntPtr hServer,
        uint sessionId,
        WTS_INFO_CLASS wtsInfoClass,
        out IntPtr ppBuffer,
        out uint iBytesReturned);

    public enum WTS_INFO_CLASS{
        WTSInitialProgram,
        WTSApplicationName,
        WTSWorkingDirectory,
        WTSOEMId,
        WTSSessionId,
        WTSUserName,
        WTSWinStationName,
        WTSDomainName,
        WTSConnectState,
        WTSClientBuildNumber,
        WTSClientName,
        WTSClientDirectory,
        WTSClientProductId,
        WTSClientHardwareId,
        WTSClientAddress,
        WTSClientDisplay,
        WTSClientProtocolType,
        WTSIdleTime,
        WTSLogonTime,
        WTSIncomingBytes,
        WTSOutgoingBytes,
        WTSIncomingFrames,
        WTSOutgoingFrames,
        WTSClientInfo,
        WTSSessionInfo,
        WTSConfigInfo,
        WTSValidationInfo,
        WTSSessionAddressV4,
        WTSIsRemoteSession
    }

    [DllImport("wtsapi32.dll")]
    static extern void WTSFreeMemory(IntPtr pMemory);

    static void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime){
        if(RdpIdentify.WinStationName != GetWinStationName()){
            Application.Exit();
        }
    }

    static string GetWinStationName(){
        string WinStationName="";
        IntPtr ppBuffer;
        uint iReturned;
        if (WTSQuerySessionInformation(IntPtr.Zero,
            uint.MaxValue,
            WTS_INFO_CLASS.WTSWinStationName,
            out ppBuffer,
            out iReturned))
        {
            WinStationName = Marshal.PtrToStringAnsi(ppBuffer);
        }
        WTSFreeMemory(ppBuffer);
        return WinStationName;
    }

    static WinEventDelegate procDelegate = new WinEventDelegate(WinEventProc);

}
 
class MyTaskTray : Form{
    public NotifyIcon icon;
    public MyTaskTray(){
        this.icon = new NotifyIcon();
        this.ShowInTaskbar = false;
        this.setComponents();
    }
 
    private void Close_Click(object sender, EventArgs e){
        this.CloseTray();
    }
 
    public void CloseTray(){
        this.icon.Visible = false;
        Application.Exit();
    }

    private void setComponents(){
        //icon.Icon = new Icon("app.ico");
        this.icon.Icon = SystemIcons.Information;
        this.icon.Visible = true;
        this.icon.Text = "RdpIdentify";
        ContextMenuStrip menu = new ContextMenuStrip();
        ToolStripMenuItem menuItem = new ToolStripMenuItem();
        menuItem.Text = "&終了";
        menuItem.Click += new EventHandler(Close_Click);
        menu.Items.Add(menuItem);
        this.icon.ContextMenuStrip = menu;
    }
}