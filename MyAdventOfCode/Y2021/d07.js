// https://github.com/janfabian/adventofcode/blob/main/2021/07/index.js
const { csvNumberArray } = require("./common-functions");

/*
    A = 1, B = 2, C = 3, D = 4;

    [ fromA [ toB: distance,
            toC: distance
            toD: distance
        ],
        fromB [
            toA: distance,
            toC: distance
            toD: distance
        ],
        fromC [
            toA: distance,
            toB: distance
            toD: distance
        ],
        fromD [
            toA: distance,
            toB: distance
            toC: distance
        ],
    ]

*/

const parseInput = (rawInput) => csvNumberArray(rawInput);

/**
 * @typedef {Object} Maneuver
 * @property {number} Maneuver.from
 * @property {number} Maneuver.to
 * @property {number} Maneuver.count
 * @property {number} Maneuver.distance
 * @property {number} Maneuver.fuelCost
 */
/**
 * @typedef {Maneuver[][]} ManeuverOptions
 */

/**
 * @typedef {Object} 
 */

/**
 * @param {number[]} positions
 * @returns {ManeuverOptions}
 * @todo // TODO: May need to travel to positions that are not in the position array.
 */
const calculateManeuvers = (positions) => {
  /** @type {ManeuverOptions} */
  const maneuverOptions = [];
  positions.forEach((from) => {
    for (let to = Math.min(...positions); to <= Math.max(...positions); to++) {
      if (!maneuverOptions[from]) {
        maneuverOptions[from] = [];
      }
      if (!maneuverOptions[from][to]) {
        maneuverOptions[from][to] = createManeuver(from, to);
      } else {
        incrementManeuver(maneuverOptions[from][to]);
      }
    }
  });
  return maneuverOptions;
};

/**
 * @param {Maneuver} maneuver
 * @returns {Maneuver}
 */
const incrementManeuver = (maneuver) => {
  maneuver.count++;
  maneuver.fuelCost = maneuver.distance * maneuver.count;
  return maneuver;
};
/**
 * @param {number} from
 * @param {number} to
 * @returns {Maneuver}
 */
const createManeuver = (from, to) => {
  var distance = Math.abs(from - to);
  return { from, to, count: 1, distance, fuelCost: distance };
};

/**
 *
 * @param {ManeuverOptions} maneuverOptions
 * @returns {{maneuvers: Maneuver[], fuelCost: number}}
 */
const calculateBestManeuver = (maneuverOptions) => {
  console.log(maneuverOptions);
  const r = maneuverOptions.reduce((prev, cur) => {
    /**
     * @param {Maneuver[]} maneuver
     */
    const calculateFuelCost = (maneuver) => {
      return maneuver.map(maneuver => maneuver.fuelCost).reduce((total, fuelCost) => total + fuelCost, 0);
    }
    const prevCost = calculateFuelCost(prev);
    const currentCost = calculateFuelCost(cur);
    return prevCost < currentCost ? prev : cur;
  });
  return r;

};

const run = async (partNo) => {
  const rawInput = "16,1,2,0,4,2,7,1,2,14";
  const data = parseInput(rawInput);
  const maneuvers = calculateManeuvers(data);
  const best = calculateBestManeuver(maneuvers);
  
  console.log(best);
};

run(1);
