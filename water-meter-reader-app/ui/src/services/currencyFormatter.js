/**
 * Format a number as South African Rands (ZAR)
 * @param {number} amount - The amount to format
 * @returns {string} - Formatted currency string (e.g., "R1,234.56")
 */
export const formatZAR = (amount) => {
    if (typeof amount !== 'number' || isNaN(amount)) {
        return 'R0.00';
    }

    return `R${amount.toLocaleString('en-ZA', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
    })}`;
};

export default formatZAR;
