import React, { useState } from 'react';
import './App.css';

const App: React.FC = () => {
    const [response, setResponse] = useState<string>('');

    const sendCommand = (buttonNumber: number) => {
        fetch('/api/commands/send', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ buttonNumber }),
        })
            .then(response => response.text())
            .then(data => {
                setResponse(`Job done. Response: ${data}`);
            })
            .catch(error => console.error('Error:', error));
    };

    const sendTestMessage = () => {
        fetch('/api/rabbit/sendtest')
            .then(response => response.text())
            .then(message => {
                setResponse(`Test message sent. Response: ${message}`);
            })
            .catch(error => console.error('There was an error!', error));
    };

    return (
        <div className="App">
            <header className="App-header">
                <p>Select the number of keystrokes:</p>
                <div className="button-container">
                    {[1, 2, 3, 4].map((number) => (
                        <button key={number} onClick={() => sendCommand(number)}>
                            {number}
                        </button>
                    ))}
                </div>
                <p>{response}</p>
            </header>
            <footer className="App-footer">
                <button onClick={sendTestMessage}>Test massage</button>
            </footer>
        </div>
    );
}

export default App;
