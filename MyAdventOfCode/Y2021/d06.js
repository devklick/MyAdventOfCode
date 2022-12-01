const { getInput, csvNumberArray } = require("./common-functions");

/**
 * @param {string} rawInput
 * @returns {number[]}
 */
const parseInput = (rawInput) => csvNumberArray(rawInput);

/**
 * Gets the number of days for the specific partNo
 * @param {number} partNo
 * @returns {number} The number of days to run the emulation for
 * @throws Error when `partNo` not supported.
 */
const getDays = (partNo) => {
  if (partNo === 1) return 80;
  if (partNo === 2) return 256;
  throw new Error("Invalid partNo");
};

/**
 * Groups the fishes by their age.
 * Rather than an array containing an entry for every fish, where the entry represents the age,
 * it produces an array of ages, where every is represents the number of fish who have that age.
 * @param {number[]} fishes The fishes to be grouped by age
 * @returns {number[]}
 */
const groupByAge = (fishes) => {
  // Since fishes can be anywhere from 0-8 days old,
  // We can group them in an array with length of 9.
  const groupedFishes = Array(9).fill(0);
  // Increment the count based on the age=index
  fishes.forEach((fish) => groupedFishes[fish]++);
  return groupedFishes;
};

/**
 * Emulates the fishes aging and respawning over the specified number of days.
 * @param {number[]} fishesByAge The fishes, grouped by age
 * @param {number} days The number of days to run the emulation for
 * @returns {number[]} The new fishes grouped by age
 */
const emulate = (fishesByAge, days) => {
  for (let day = 0; day < days; day++) {
    // Remove the fishes that are at the start of the array.
    // This tells us how many are ready to respawn. It also shifts
    // all fishes down the array, emulating that a single day has passed.
    const readyForSpawn = fishesByAge.shift();

    // The fishes which have spawned restart back at 6,
    // so we add the spawn count to count at position 6.
    fishesByAge[6] += readyForSpawn;

    // Add the spawnned fish back to the array in the correct position.
    // They have 8 days before they're ready for spawn,
    // so we set them at position 8
    fishesByAge[8] = readyForSpawn;
  }
  return fishesByAge;
};

const run = async (partNo) => {
  const rawInput = await getInput(6);
  const data = parseInput(rawInput);
  const groupedFishes = groupByAge(data);
  const days = getDays(partNo);
  const result = emulate(groupedFishes, days);
  const total = result.reduce((prev, cur) => prev + cur, 0);
  console.log(total);
};

run(1);
run(2);
