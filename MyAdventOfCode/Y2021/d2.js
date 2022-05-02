const { getInput } = require("./common-functions");

/**
 * @typedef {'forward'|'down'|'up'} NavigationDirection
 */

/**
 * @typedef {Object} NavigationInstruction
 * @property {NavigationDirection} NavigationInstruction.direction
 * @property {number} NavigationInstruction.numberOfUnits
 */

/**
 * @typedef {Object} Position
 * @property {number} Position.horizontal
 * @property {number} Position.depth
 * @property {number} Position.aim
 */

/**
 * @returns {Position}
 */
const intPosition = () => ({ depth: 0, horizontal: 0, aim: 0 });

/**
 * Mutates the input position to reflect the instruction being processed.
 * @param {NavigationInstruction} instruction
 * @param {Position} position
 * @param {number} partNo
 */
const processInstruction = (instruction, position, partNo) => {
  switch (instruction.direction) {
    case "down":
      return processDownInstruction(
        position,
        instruction.numberOfUnits,
        partNo
      );
    case "up":
      return processUpInstruction(position, instruction.numberOfUnits, partNo);
    case "forward":
      return processForwardInstruction(
        position,
        instruction.numberOfUnits,
        partNo
      );
    default:
      throw new Error(
        `Invalid instruction direction: ${instruction.direction}`
      );
  }
};

/**
 * Handles the logic for processing a 'down' instruction
 * based on the part the instruction is associated with
 * @param {Position} position
 * @param {number} numberOfUnits
 * @param {number} partNo
 */
const processDownInstruction = (position, numberOfUnits, partNo) => {
  if (partNo === 1) position.depth += numberOfUnits;
  else if (partNo === 2) position.aim += numberOfUnits;
  else throw new Error("Invalid PartNo");
};
/**
 * Handles the logic for processing a 'up' instruction
 * based on the part the instruction is associated with
 * @param {Position} position
 * @param {number} numberOfUnits
 * @param {number} partNo
 */
const processUpInstruction = (position, numberOfUnits, partNo) => {
  if (partNo === 1) position.depth -= numberOfUnits;
  else if (partNo === 2) position.aim -= numberOfUnits;
  else throw new Error("Invalid PartNo");
};
/**
 * Handles the logic for processing a 'forward' instruction
 * based on the part the instruction is associated with
 * @param {Position} position
 * @param {number} numberOfUnits
 * @param {number} partNo
 */
const processForwardInstruction = (position, numberOfUnits, partNo) => {
  if (partNo === 1) position.horizontal += numberOfUnits;
  else if (partNo === 2) {
    position.horizontal += numberOfUnits;
    position.depth += position.aim * numberOfUnits;
  } else throw new Error("Invalid PartNo");
};

/**
 * @param {string} direction
 * @returns {NavigationDirection}
 */
const getNavigationDirection = (direction) => {
  if (direction === "forward" || direction === "down" || direction === "up") {
    return direction;
  }
  throw new Error("Invalid direction");
};

/**
 * @param {string} rawInput
 * @returns {NavigationInstruction[]}
 */
const parseInput = (rawInput) => {
  return rawInput.split("\n").map((line) => {
    const values = line.split(" ");
    return {
      direction: getNavigationDirection(values[0]),
      numberOfUnits: Number(values[1]),
    };
  });
};

const run = async (partNo) => {
  const position = intPosition();
  const instructions = parseInput(await getInput(2));

  instructions.forEach((instruction) => {
    processInstruction(instruction, position, partNo);
  });

  console.log(position.depth * position.horizontal);
};

run(1);
run(2);
