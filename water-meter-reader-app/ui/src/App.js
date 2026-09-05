import React, { useEffect, useState } from 'react';
import './styles/App.css';
import UploadForm from './components/UploadForm';
import UsageDisplay from './components/UsageDisplay';
import RegisterPage from './components/RegisterPage';
import LoginPage from './components/LoginPage';
import { getUserHistory, isMockMode } from './services/api';

function App() {
    const [reading, setReading] = useState(null);
    const [cost, setCost] = useState(null);
    const [history, setHistory] = useState([]);
    const [historyError, setHistoryError] = useState('');
    const [message, setMessage] = useState('');
    const [page, setPage] = useState('login');
    const [user, setUser] = useState(null);
    const mockMode = isMockMode();

    const loadHistory = (userId) => {
        setHistoryError('');

        return getUserHistory(userId)
            .then(setHistory)
            .catch((error) => {
                setHistory([]);
                setHistoryError(error.message || 'Unable to load history.');
            });
    };

    const handleUploadSuccess = (data) => {
        setReading(data.reading);
        setCost(data.cost);
        setMessage(data.message || 'Upload successful!');

        if (user) {
            loadHistory(user.userId);
        }
    };

    const handleLoginSuccess = (userData) => {
        setUser(userData);
        setPage('capture');
    };

    const handleRegisterSuccess = () => {
        setPage('login');
    };

    useEffect(() => {
        if (!user) {
            setHistory([]);
            setHistoryError('');
            return;
        }

        loadHistory(user.userId);
    }, [user]);

    return (
        <div className="container">
            <h1>Water Meter Reader</h1>
            {mockMode && (
                <div className="demo-banner">
                    <strong>Demo mode:</strong> GitHub Pages uses mocked authentication, uploads, and history so the UI can run without the backend API or database.
                </div>
            )}
            <nav style={{ marginBottom: 20 }}>
                {user ? (
                    <>
                        <button onClick={() => setPage('capture')}>Capture</button>
                        <button onClick={() => setPage('history')}>History</button>
                        <button onClick={() => { setUser(null); setPage('login'); }}>Logout</button>
                    </>
                ) : (
                    <>
                        <button onClick={() => setPage('login')}>Login</button>
                        <button onClick={() => setPage('register')}>Register</button>
                    </>
                )}
            </nav>
            {page === 'login' && <LoginPage onLoginSuccess={handleLoginSuccess} />}
            {page === 'register' && <RegisterPage onRegisterSuccess={handleRegisterSuccess} />}
            {page === 'capture' && user && (
                <>
                    <UploadForm onUploadSuccess={handleUploadSuccess} setMessage={setMessage} userId={user.userId} />
                    {message && <p>{message}</p>}
                    <UsageDisplay waterUsage={reading} cost={cost} />
                </>
            )}
            {page === 'history' && user && (
                <div className="usage-display">
                    <h2>{mockMode ? 'Demo Upload History' : 'History'}</h2>
                    {history.length > 0 ? (
                        <ul className="history-list">
                            {history.map((entry) => (
                                <li key={entry.id}>
                                    <strong>{entry.fileName}</strong> — {entry.reading} gallons
                                    {typeof entry.cost === 'number' && <> — ${Number(entry.cost).toFixed(2)}</>}
                                </li>
                            ))}
                        </ul>
                    ) : historyError ? (
                        <p>{historyError}</p>
                    ) : (
                        <p>{mockMode ? 'No demo uploads yet. Upload an image to see a mocked reading here.' : 'No saved readings yet.'}</p>
                    )}
                    {mockMode && <p className="hint-text">Demo history is stored only in this browser.</p>}
                </div>
            )}
        </div>
    );
}

export default App;