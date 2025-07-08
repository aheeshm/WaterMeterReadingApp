import React, { useState } from 'react';
import './styles/App.css';
import UploadForm from './components/UploadForm';
import UsageDisplay from './components/UsageDisplay';
import RegisterPage from './components/RegisterPage';
import LoginPage from './components/LoginPage';

function App() {
    const [reading, setReading] = useState(null);
    const [cost, setCost] = useState(null);
    const [message, setMessage] = useState('');
    const [page, setPage] = useState('login');
    const [user, setUser] = useState(null);

    const handleUploadSuccess = (data) => {
        setReading(data.reading);
        setCost(data.cost);
        setMessage('Upload successful!');
    };

    const handleLoginSuccess = (userData) => {
        setUser(userData);
        setPage('capture');
    };

    const handleRegisterSuccess = () => {
        setPage('login');
    };

    return (
        <div className="container">
            <h1>Water Meter Reader</h1>
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
            {/* History page will be added later */}
        </div>
    );
}

export default App;