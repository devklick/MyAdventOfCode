const  {getInput} = require('./common-functions');
/**
 * @typedef {Object} Group
 * @property {any} Group.value
 * @property {number} Group.count
 */

/**
 *
 * @param {any[]} values
 * @returns {Group[]}
 */
const group = (values) => {
  const map = new Map();
  values.forEach((value) => {
    if (map.has(value)) map.set(value, map.get(value) + 1);
    else map.set(value, 1);
  });
  /**
   * @type {Group[]}
   */
  const groups = [];
  map.forEach((count, value) => groups.push({ value, count }));

  return groups.sort((a, b) => a.count - b.count);
};

/**
 * @param {any[]} values
 * @returns {any}
 */
const mostCommon = (values) => {
  const grouped = group(values);
  return grouped[0] ? grouped[0].value : undefined;
};
const leastCommon = (values) => {
  const grouped = group(values);
  return grouped[grouped.length -1 ] ? grouped[grouped.length -1].value : undefined;
};

/**
 * @param {string[]} binaryValues
 * @param {'gamma' | 'epsilon'} xType
 * @returns {number}
 */
const getXRate = (binaryValues, xType) => {
  // An array where each binary value in the binaryValues array
  // is present as an array of numbers.
  // Eg [00100,11110] => [[0,0,1,0,0],[1,1,1,1,0]]
  const binaryAsNumbers = binaryValues.map((b) =>
    b.toString().split("").map(n => Number(n))
  );

  let xRateBinary = "";
  for (let i = 0; i < binaryAsNumbers[0].length; i++) {
    const valuesAtI = binaryAsNumbers.map((b) => b[i]);
    const bit =
      xType === "gamma" ? mostCommon(valuesAtI) : leastCommon(valuesAtI);
    xRateBinary += bit;
  }
  return parseInt(xRateBinary, 2);
};

/**
 * @param {string[]} binaryValues
 * @returns {number}
 */
const getGammaRate = (binaryValues) => getXRate(binaryValues, "gamma");

/**
 * @param {string[]} binaryValues
 * @returns {number}
 */
const getEpsilonRate = (binaryValues) => getXRate(binaryValues, "epsilon");

const run = async () => {
  const data = await (await getInput(3)).split('\n');
  const epsilon = getEpsilonRate(data);
  const gamma = getGammaRate(data);
  console.log(epsilon * gamma);
}

run();