import React, { useState } from 'react';
import { isMockMode, loginUser } from '../services/api';

const LoginPage = ({ onLoginSuccess }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState('');
    const mockMode = isMockMode();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        try {
            const data = await loginUser({ username, password });
            setMessage('Login successful!');
            if (onLoginSuccess) onLoginSuccess(data);
        } catch (err) {
            setMessage(err.message || 'Error connecting to server.');
        }
    };

    return (
        <div className="container">
            <h2>Login</h2>
            {mockMode && <p className="hint-text">Use <strong>demo</strong> / <strong>demo123</strong>, or register a new demo account below.</p>}
            <form onSubmit={handleSubmit}>
                <input type="text" placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} required />
                <input type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} required />
                <button type="submit">Login</button>
            </form>
            {message && <p>{message}</p>}
        </div>
    );
};

export default LoginPage;
