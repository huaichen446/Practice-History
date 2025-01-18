import sqlite3
from datetime import datetime


class Database:
    def __init__(self, db_file="fingerprints.db"):
        self.db_file = db_file
        self.init_db()

    def init_db(self):
        with sqlite3.connect(self.db_file) as conn:
            cursor = conn.cursor()
            cursor.execute('''
                CREATE TABLE IF NOT EXISTS tls_fingerprints (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    fingerprint_hash TEXT UNIQUE,
                    tls_version TEXT,
                    cipher_suites TEXT,
                    extensions TEXT,
                    curves TEXT,
                    point_formats TEXT,
                    first_seen DATETIME,
                    last_seen DATETIME,
                    count INTEGER DEFAULT 1
                )
            ''')
            conn.commit()

    def add_fingerprint(self, fingerprint_data):
        with sqlite3.connect(self.db_file) as conn:
            cursor = conn.cursor()
            current_time = datetime.now()

            # 检查指纹是否存在
            cursor.execute(
                "SELECT id, count FROM tls_fingerprints WHERE fingerprint_hash = ?",
                (fingerprint_data['hash'],)
            )
            result = cursor.fetchone()

            if result:
                # 更新已存在的指纹
                cursor.execute('''
                    UPDATE tls_fingerprints 
                    SET count = ?, last_seen = ?
                    WHERE id = ?
                ''', (result[1] + 1, current_time, result[0]))
            else:
                # 插入新指纹
                cursor.execute('''
                    INSERT INTO tls_fingerprints (
                        fingerprint_hash, tls_version, cipher_suites,
                        extensions, curves, point_formats,
                        first_seen, last_seen
                    ) VALUES (?, ?, ?, ?, ?, ?, ?, ?)
                ''', (
                    fingerprint_data['hash'],
                    fingerprint_data['tls_version'],
                    fingerprint_data['cipher_suites'],
                    fingerprint_data['extensions'],
                    fingerprint_data['curves'],
                    fingerprint_data['point_formats'],
                    current_time,
                    current_time
                ))
            conn.commit()
