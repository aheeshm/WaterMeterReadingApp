import React, { useState } from 'react';

const RegisterPage = ({ onRegisterSuccess }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [propertyAddress, setPropertyAddress] = useState('');
    const [message, setMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        try {
            const response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password, propertyAddress })
            });
            if (response.ok) {
                setMessage('Registration successful! You can now log in.');
                setUsername(''); setPassword(''); setPropertyAddress('');
                if (onRegisterSuccess) onRegisterSuccess();
            } else {
                const data = await response.text();
                setMessage(data || 'Registration failed.');
            }
        } catch (err) {
            setMessage('Error connecting to server.');
        }
    };

    return (
        <div className="container">
            <h2>Register</h2>
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
