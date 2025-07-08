import React, { useState } from 'react';

const UploadForm = ({ onUploadSuccess, setMessage, userId }) => {
    const [selectedFile, setSelectedFile] = useState(null);

    const handleFileChange = (event) => {
        setSelectedFile(event.target.files[0]);
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        if (!selectedFile) {
            setMessage('Please select a file to upload.');
            return;
        }

        const formData = new FormData();
        formData.append('file', selectedFile);
        formData.append('userId', userId);

        try {
            const response = await fetch('/api/watermeter/upload', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                const data = await response.json();
                onUploadSuccess(data);
            } else {
                setMessage('Upload failed. Please try again.');
            }
        } catch (error) {
            setMessage('An error occurred while uploading the file.');
        }
    };

    return (
        <div className="upload-form">
            <h2>Upload Water Meter Image</h2>
            <form onSubmit={handleSubmit}>
                <input type="file" accept="image/*" onChange={handleFileChange} />
                <button type="submit">Upload</button>
            </form>
        </div>
    );
};

export default UploadForm;