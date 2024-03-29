import React, { useState } from 'react';
import './App.css';

const App: React.FC = () => {
    const [response, setResponse] = useState<string>('');
    const [loading, setLoading] = useState<boolean>(false);

    const sendCommand = (buttonNumber: number) => {
        setLoading(true);
        fetch('/api/commands/send_and_wait', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ buttonNumber }),
        })
            .then(response => response.text())
            .then(data => {
                setLoading(false); 
                setResponse(`${data}`);
            })
            .catch(error => {
                console.error('Error:', error);
                setLoading(false); 
            });
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
                <div>
                    <p> {loading ? <span className="waiting-dots">waiting</span> : response} </p>
                </div>
            </header>
            <footer className="App-footer">
                <button onClick={sendTestMessage}>Test massage</button>
            </footer>
        </div>
    );
}

export default App;
