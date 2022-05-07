const fs = require('fs/promises');
const { join } = require('path');

/**
 * @param {number} day
 * @returns {Promise<string>}
 */
const getInput = async (day) =>
  await fs.readFile(join(__dirname, 'data', `d${day}.txt`)).then((buffer) => buffer.toString("utf-8"));

module.exports = {
  getInput,
};
