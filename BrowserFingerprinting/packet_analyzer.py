from scapy.all import *
from scapy.layers.tls.all import *
import hashlib
import threading
import queue


class TLSAnalyzer:
    def __init__(self, database):
        self.db = database
        self.packet_queue = queue.Queue(maxsize=1000)  # 限制队列大小
        self.is_capturing = False
        self.capture_thread = None
        self.process_thread = None
        self.captured_packets = 0
        self.processed_packets = 0

    # noinspection PyMethodMayBeStatic
    def calculate_fingerprint_hash(self, tls_info):
        """计算TLS指纹哈希"""
        fingerprint_str = (
            f"{tls_info['tls_version']},{tls_info['cipher_suites']},"
            f"{tls_info['extensions']},{tls_info['curves']},"
            f"{tls_info['point_formats']}"
        )
        return hashlib.sha256(fingerprint_str.encode()).hexdigest()

    def extract_tls_info(self, packet):
        """从数据包提取TLS信息"""
        if not packet.haslayer(TLS) or not packet.haslayer(TLSClientHello):
            return None

        client_hello = packet[TLSClientHello]

        tls_info = {
            'tls_version': hex(client_hello.version),
            'cipher_suites': ','.join(hex(cipher) for cipher in client_hello.ciphers),
            'extensions': '',
            'curves': '',
            'point_formats': ''
        }

        if client_hello.haslayer(TLS_Ext_SupportedGroups):
            tls_info['curves'] = ','.join(
                str(group) for group in client_hello[TLS_Ext_SupportedGroups].groups
            )

        if client_hello.haslayer(TLS_Ext_SupportedPointFormat):
            tls_info['point_formats'] = ','.join(
                str(fmt) for fmt in client_hello[TLS_Ext_SupportedPointFormat].ecpl
            )

        tls_info['extensions'] = ','.join(
            hex(ext.type) for ext in client_hello.extensions
        )

        tls_info['hash'] = self.calculate_fingerprint_hash(tls_info)
        return tls_info

    def packet_callback(self, packet):
        """数据包回调处理函数"""
        try:
            self.packet_queue.put(packet, block=False)
            self.captured_packets += 1
        except queue.Full:
            print("Warning: Packet queue is full, dropping packet")

    def process_packets(self):
        """处理数据包队列"""
        while self.is_capturing or not self.packet_queue.empty():
            try:
                packet = self.packet_queue.get(timeout=1)
                tls_info = self.extract_tls_info(packet)
                if tls_info:
                    self.db.add_fingerprint(tls_info)
                self.processed_packets += 1
            except queue.Empty:
                continue
            except Exception as e:
                print(f"Error processing packet: {e}")

    def start_capture(self, interface="any"):
        """开始捕获"""
        self.is_capturing = True
        self.captured_packets = 0
        self.processed_packets = 0

        # 启动处理线程
        self.process_thread = threading.Thread(target=self.process_packets)
        self.process_thread.start()

        # 启动捕获线程
        self.capture_thread = threading.Thread(
            target=lambda: sniff(
                iface=interface,
                filter="tcp port 443",
                prn=self.packet_callback,
                store=0,
                stop_filter=lambda x: not self.is_capturing
            )
        )
        self.capture_thread.start()

    def stop_capture(self):
        """停止捕获"""
        self.is_capturing = False
        if self.capture_thread:
            self.capture_thread.join()
        if self.process_thread:
            self.process_thread.join()
        return {
            'captured': self.captured_packets,
            'processed': self.processed_packets
        }
