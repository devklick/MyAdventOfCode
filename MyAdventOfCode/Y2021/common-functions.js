const fs = require('fs/promises');

/**
 * @returns {Promise<string>}
 */
const getInput = async (day) =>
  await fs.readFile(`./data/d${day}.txt`).then((buffer) => buffer.toString("utf-8"));

module.exports = {
  getInput,
};
