from flask import Flask, render_template, jsonify
from db_models import Database
from packet_analyzer import TLSAnalyzer
import os

app = Flask(__name__)
db = Database()
analyzer = TLSAnalyzer(db)

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/api/start_capture')
def start_capture():
    if not analyzer.is_capturing:
        analyzer.start_capture()
        return jsonify({'status': 'started'})
    return jsonify({'status': 'already_running'})

@app.route('/api/stop_capture')
def stop_capture():
    if analyzer.is_capturing:
        stats = analyzer.stop_capture()
        return jsonify({
            'status': 'stopped',
            'stats': stats
        })
    return jsonify({'status': 'not_running'})

@app.route('/api/status')
def get_status():
    return jsonify({
        'is_capturing': analyzer.is_capturing,
        'captured_packets': analyzer.captured_packets,
        'processed_packets': analyzer.processed_packets
    })

if __name__ == '__main__':
    app.run(debug=True)