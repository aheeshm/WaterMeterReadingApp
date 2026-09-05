import React, { useState } from 'react';
import { isMockMode, uploadWaterMeterImage } from '../services/api';

const UploadForm = ({ onUploadSuccess, setMessage, userId }) => {
    const [selectedFile, setSelectedFile] = useState(null);
    const mockMode = isMockMode();

    const handleFileChange = (event) => {
        setSelectedFile(event.target.files[0]);
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        if (!selectedFile) {
            setMessage('Please select a file to upload.');
            return;
        }

        try {
            const data = await uploadWaterMeterImage({ file: selectedFile, userId });
            onUploadSuccess(data);
        } catch (error) {
            setMessage(error.message || 'An error occurred while uploading the file.');
        }
    };

    return (
        <div className="upload-form">
            <h2>Upload Water Meter Image</h2>
            {mockMode && <p className="hint-text">Uploads stay in your browser and return a mocked reading in the GitHub Pages demo.</p>}
            <form onSubmit={handleSubmit}>
                <input type="file" accept="image/*" onChange={handleFileChange} />
                <button type="submit">Upload</button>
            </form>
        </div>
    );
};

export default UploadForm;