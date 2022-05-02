const fs = require("fs/promises");

/**
 *
 * @param {any[]} values
 * @param {number} windowSize
 * @returns {any[]}
 */
const slidingWindow = (values, windowSize) =>
  values.flatMap((_, i) =>
    i <= values.length - windowSize ? [values.slice(i, i + windowSize)] : []
  );

/**
 * @param {string} rawInput
 * @returns {{partNo: 1, data: number[]} | {partNo: 2, data: Array<Array<number>>}}
 */
const parseInput = (rawInput, partNo) => {
  const split = rawInput.split("\n");
  const numbers = split.map((n, i) => {
    const number = Number(n);
    if (isNaN(number))
      throw new Error(`Invalid input on line ${i}, expected number`);
    return number;
  });

  if (partNo === 1) {
    return { partNo, data: numbers };
  }

  return { partNo, data: slidingWindow(numbers, 3) };
};

/**
 * @param {number[]} numbers
 * @returns {number}
 */
const getTotalIncrements = (numbers) => {
  // Bit of a mis-use here just to get the current
  // and previous number in scope at the same time
  let total = 0;
  numbers.sort((a, b) => {
    if (a > b) total++;
    return 0;
  });
  return total;
};

/**
 * @returns {Promise<string>}
 */
const getInput = async () =>
  await fs.readFile(`./data/d1.txt`).then((buffer) => buffer.toString("utf-8"));

const run = async (partNo) => {
  const input = parseInput(await getInput(), partNo);

  if (input.partNo === 1) {
    return console.log(getTotalIncrements(input.data));
  }

  return console.log(
    getTotalIncrements(input.data.map((d) => d.reduce((a, b) => a + b, 0)))
  );
};

run(1);
run(2);
