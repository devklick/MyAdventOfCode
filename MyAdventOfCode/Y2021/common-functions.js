const fs = require('fs/promises');
const { join } = require('path');

/**
 * @param {number} day
 * @returns {Promise<string>}
 */
const getInput = async (day) =>
  await fs.readFile(join(__dirname, 'data', `d${day.toString().padStart(2, '0')}.txt`)).then((buffer) => buffer.toString("utf-8"));

/**
 * @param {string} rawInput 
 * @returns 
 */
const csvNumberArray = (rawInput) => rawInput.trim().split(',').map(Number);

module.exports = {
  getInput,
  csvNumberArray
};
