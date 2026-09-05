import React, { useState } from 'react';
import { isMockMode, registerUser } from '../services/api';

const RegisterPage = ({ onRegisterSuccess }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [propertyAddress, setPropertyAddress] = useState('');
    const [message, setMessage] = useState('');
    const mockMode = isMockMode();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        try {
            const result = await registerUser({ username, password, propertyAddress });
            setMessage(result.message);
            setUsername(''); setPassword(''); setPropertyAddress('');
            if (onRegisterSuccess) onRegisterSuccess();
        } catch (err) {
            setMessage(err.message || 'Error connecting to server.');
        }
    };

    return (
        <div className="container">
            <h2>Register</h2>
            {mockMode && <p className="hint-text">Demo registrations are stored in this browser only.</p>}
            <form onSubmit={handleSubmit}>
                <input type="text" placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} required />
                <input type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} required />
                <input type="text" placeholder="Property Address" value={propertyAddress} onChange={e => setPropertyAddress(e.target.value)} required />
                <button type="submit">Register</button>
            </form>
            {message && <p>{message}</p>}
        </div>
    );
};

export default RegisterPage;
