using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Text;

class Program
{
    // 定义OID请求结构
    [StructLayout(LayoutKind.Sequential)]
    public struct PACKET_OID_DATA
    {
        public uint Oid;
        public uint Length;
        public IntPtr Data;
    }

    [DllImport("packet.dll", CharSet = CharSet.Ansi)]
    private static extern IntPtr PacketOpenAdapter(
        [MarshalAs(UnmanagedType.LPStr)] string adaptername
    );

    [DllImport("packet.dll")]
    private static extern bool PacketRequest(
        IntPtr AdapterObject,
        bool Set,
        IntPtr OidData
    );

    [DllImport("packet.dll")]
    private static extern bool PacketCloseAdapter(
        IntPtr AdapterObject
    );

    // OID常量
    private const uint OID_GEN_CURRENT_PACKET_FILTER = 0x0001010E;
    private const uint NDIS_PACKET_TYPE_PROMISCUOUS = 0x00000020;

    static void Main()
    {
        // 设置控制台编码以正确显示中文
        Console.OutputEncoding = Encoding.UTF8;

        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface adapter in interfaces)
        {
            Console.WriteLine($"\n检查网卡: {adapter.Name}");
            Console.WriteLine($"描述: {adapter.Description}");

            try
            {
                // 构造适配器名称
                string adapterName = @"\Device\NPF_" + adapter.Id;

                // 打开适配器
                IntPtr adapterHandle = PacketOpenAdapter(adapterName);

                if (adapterHandle != IntPtr.Zero)
                {
                    try
                    {
                        // 分配内存
                        int dataSize = sizeof(uint);
                        IntPtr oidDataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<PACKET_OID_DATA>() + dataSize);
                        IntPtr dataPtr = Marshal.AllocHGlobal(dataSize);

                        try
                        {
                            // 初始化OID数据结构
                            PACKET_OID_DATA oidData = new PACKET_OID_DATA
                            {
                                Oid = OID_GEN_CURRENT_PACKET_FILTER,
                                Length = (uint)dataSize,
                                Data = dataPtr
                            };

                            // 将结构复制到非托管内存
                            Marshal.StructureToPtr(oidData, oidDataPtr, false);

                            // 发送请求
                            if (PacketRequest(adapterHandle, false, oidDataPtr))
                            {
                                // 读取返回的数据
                                uint mode = (uint)Marshal.ReadInt32(dataPtr);
                                bool isPromiscuous = (mode & NDIS_PACKET_TYPE_PROMISCUOUS) != 0;
                                Console.WriteLine($"混杂模式: {(isPromiscuous ? "开启" : "关闭")}");
                            }
                            else
                            {
                                int error = Marshal.GetLastWin32Error();
                                Console.WriteLine($"无法获取网卡模式 (错误码: {error})");
                            }
                        }
                        finally
                        {
                            // 释放分配的内存
                            Marshal.FreeHGlobal(dataPtr);
                            Marshal.FreeHGlobal(oidDataPtr);
                        }
                    }
                    finally
                    {
                        // 关闭适配器
                        PacketCloseAdapter(adapterHandle);
                    }
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"无法打开网卡 (错误码: {error})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                Console.WriteLine($"堆栈: {ex.StackTrace}");
            }
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }
}
