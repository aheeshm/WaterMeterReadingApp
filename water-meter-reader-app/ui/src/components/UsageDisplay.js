import React from 'react';

const UsageDisplay = ({ waterUsage, cost }) => {
    return (
        <div className="usage-display">
            <h2>Water Usage Information</h2>
            {waterUsage !== null && cost !== null ? (
                <>
                    <p>Water Usage: {waterUsage} gallons</p>
                    <p>Cost: ${cost.toFixed(2)}</p>
                </>
            ) : (
                <p>No usage data available.</p>
            )}
        </div>
    );
};

export default UsageDisplay;