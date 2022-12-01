const { getInput } = require("./common-functions");

/**
 * @typedef {Object} Group
 * @property {any} Group.value
 * @property {number} Group.count
 */

/**
 * Groups all elements in the array and counts how often they occur.
 * The returned elements will be ordered from most frequent to least frequent.
 * @param {any[]} values
 * @returns {Group[]}
 */
const group = (values) => {
  const map = new Map();
  values.forEach((value) => {
    if (map.has(value)) map.set(value, map.get(value) + 1);
    else map.set(value, 1);
  });

  const groups = [];
  map.forEach((count, value) => groups.push({ value, count }));

  return groups.sort((a, b) => b.count - a.count);
};

/**
 * Get the element that occurs *most* often.
 * If there are multiple values that occur most often, the `shared` flag will be true. Otherwise, it'll be false.
 * @param {any[]} values
 * @returns {{value: any, shared: boolean}}
 */
const mostCommon = (values) => {
  const groups = group(values);
  const grouping = groups[0] ? groups[0] : undefined;
  const shared = groups[1] ? groups[1].count === grouping.count : false;
  const { value } = grouping;
  return { value, shared };
};

/**
 * Get the element that occurs *least* often.
 * @param {any[]} values
 * @returns {{value: any, shared: boolean}}
 */
const leastCommon = (values) => {
  const groups = group(values);
  const grouping = groups[groups.length - 1]
    ? groups[groups.length - 1]
    : undefined;
  const shared = groups[groups.length - 2]
    ? groups[groups.length - 2].count === grouping.count
    : false;
  const { value } = grouping;
  return { value, shared };
};

/**
 * Creates an array where each binary value in the binaryValues array
 * is present as an array of numbers.
 * Eg [00100,11110] => [[0,0,1,0,0],[1,1,1,1,0]]
 * @param {string[]} binaryValues
 * @returns {number[][]}
 */
const binaryAsNumbers = (binaryValues) =>
  binaryValues.map((b) =>
    b
      .toString()
      .split("")
      .map((n) => Number(n))
  );

/**
 * @param {string[]} binaryValues
 * @param {'gamma' | 'epsilon'} xType
 * @returns {number}
 */
const getXRate = (binaryValues, xType) => {
  const binaryNos = binaryAsNumbers(binaryValues);

  const xRateBinary = [];
  for (let i = 0; i < binaryNos[0].length; i++) {
    const valuesAtI = binaryNos.map((b) => b[i]);
    const bit =
      xType === "gamma"
        ? mostCommon(valuesAtI)
        : leastCommon(valuesAtI);
    xRateBinary.push(bit.value);
  }
  return parseInt(xRateBinary.join(''), 2);
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

/**
 * @param {string[]} binaryValues
 * @param {'o2'|'co2'} xo2Type
 * @returns {number}
 */
const getXo2Rating = (binaryValues, xo2Type) => {
  let candidates = binaryAsNumbers(binaryValues);
  for (let i = 0; i < binaryValues[0].length; i++) {
    const iNos = candidates.map((b) => b[i]);
    const xCommon = xo2Type === "o2" ? mostCommon(iNos) : leastCommon(iNos);
    if (!xCommon.shared) {
      candidates = candidates.filter((c) => c[i] === xCommon.value);
    } else {
      candidates = candidates.filter(
        (c) => c[i] === (xo2Type === "o2" ? 1 : 0)
      );
    }
    if (candidates.length === 1) break;
  }
  if (candidates.length !== 1)
    throw new Error("Unable to reduce down set to 1 item");

  return parseInt(candidates[0].join(""), 2);
};

/**
 * @param {string[]} binaryValues
 * @returns {number}
 */
const getO2GeneratorRating = (binaryValues) => getXo2Rating(binaryValues, "o2");

/**
 * @param {string[]} binaryValues
 * @returns {number}
 */
const getCo2ScrubberRating = (binaryValues) =>
  getXo2Rating(binaryValues, "co2");

const run = async () => {
  const data = await getInput(3).then((d) => d.split("\n"));
  const epsilon = getEpsilonRate(data);
  const gamma = getGammaRate(data);
  const o2Gen = getO2GeneratorRating(data);
  const co2Scrub = getCo2ScrubberRating(data);
  console.log(epsilon * gamma);
  console.log(o2Gen * co2Scrub);
};

run();
