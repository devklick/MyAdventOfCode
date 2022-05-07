const { getInput } = require('./common-functions');

/**
 * @typedef { Object } Point A position in a two dimensional grid.
 * @property { number } Point.c The position along the vertical axis (aka the column)
 * @property { number } Point.r The position along the horizontal axis (aka the row)
 */

/**
 * @typedef {'N' | 'E' | 'S' | 'W' } CardinalDirection
 */

/**
 * @typedef {'NE' | 'SE' | 'NW' | 'SW' } OrdinalDirection
 */

/**
 * @typedef {CardinalDirection | OrdinalDirection} Direction
 */

/**
 * @typedef {'horizontal'|'vertical'|'diagonal'} Axis
 */

/**
 * @typedef { Object } LinnearInfo
 * @property { Axis } LinnearInfo.axis
 * @property { Direction } LinnearInfo.direction
 */

/**
 * @typedef { Object } Line
 * @property { Point } Line.start
 * @property { Point } Line.end
 * @property { Axis } Line.axis
 * @property { Direction } Line.direction
 */

/**
 * @typedef {number[][]} Grid
 */

/**
 * Parses a point
 * @param {string} rawInput
 * @returns {Point}
 */
const parsePoint = (rawInput) => {
  const [c, r] = rawInput.split(",");
  return { c: Number(c.trim()), r: Number(r.trim()) };
};

/**
 * @param {Point} start
 * @param {Point} end
 * @returns {LinnearInfo}
 */
const getLinnearInfo = (start, end) => {
  const cDiff = end.c - start.c;
  const rDiff = end.r - start.r;

  /** @type {Axis} */
  let axis;
  // either horizontal or vertical
  if (cDiff === 0 || rDiff === 0) {
    axis = "horizontal";
    if (cDiff > 0) return { direction: "E", axis };
    if (cDiff < 0) return { direction: "W", axis };

    axis = "vertical";
    if (rDiff > 0) return { direction: "S", axis };
    if (rDiff < 0) return { direction: "N", axis };
  }
  // diagonal
  else {
    axis = "diagonal";
    if (cDiff > 0 && rDiff > 0) return { direction: "SE", axis };
    if (cDiff < 0 && rDiff < 0) return { direction: "NW", axis };
    if (cDiff > 0 && rDiff < 0) return { direction: "NE", axis };
    if (cDiff < 0 && rDiff > 0) return { direction: "SW", axis };
  }

  throw new Error("Ive clearly missed something");
};

/**
 * Parses a line of the input data, which coincientally, contains
 * information about the start and end points of a line.
 * @param {string} rawInput
 * @returns {Line}
 */
const parseLine = (rawInput) => {
  const [rawStart, rawEnd] = rawInput
    .split(" -> ")
    .filter((line) => line.trim());

  const start = parsePoint(rawStart);
  const end = parsePoint(rawEnd);

  const { axis, direction } = getLinnearInfo(start, end);
  return { start, end, axis, direction };
};

/**
 * Returns a new line that ensure's it is being drawn in a positive direction.
 * @param {Line} line
 */
const normaliseLine = (line) => {
  const { start, end } = line;
  if (start.c > end.c || start.r > end.r) {
    return { ...line, start: end, end: start };
  }
  return line;
};

/**
 * Parses the input data into an array of structured objects.
 * @param { string } rawInput
 * @returns { Line[] }
 */
const parseInput = (rawInput) =>
  rawInput
    .trim()
    .split("\n")
    .map((line) => parseLine(line));

/**
 * @param {Line[]} data
 * @returns {{width: number, height: number}}
 */
const getGridSize = (data) => {
  const width =
    data
      .flatMap((d) => [d.start.c, d.end.c])
      .sort()
      .reverse()[0] + 1;
  const height =
    data
      .flatMap((d) => [d.start.r, d.end.r])
      .sort()
      .reverse()[0] + 1;
  return { width, height };
};

/**
 *
 * @param {number} width
 * @param {number} height
 */
const createGrid = (width, height) => Array.from(Array(width), () => Array(height).fill(0));

/**
 * @param {Grid} grid
 * @param {Line[]} lines
 * @returns {Grid}
 */
const plotLines = (grid, lines) => {
  for (const line of lines) {
    const normLine = normaliseLine(line);
    switch (line.axis) {
      case "diagonal":
        continue;
      case "horizontal":
        for (let col = normLine.start.c; col <= normLine.end.c; col++) {
          grid[col][normLine.start.r] += 1;
        }
        break;
      case "vertical":
        for (let row = normLine.start.r; row <= normLine.end.r; row++) {
          grid[normLine.start.c][row] += 1;
        }
        break;
    }
  }
  return grid;
};

/**
 * @param {Grid} grid 
 * @returns {{point: Point, overlaps: number}[]}
 */
const getOverlappingPoints = (grid) => {
  const points = [];
  for (let c = 0; c < grid.length; c++) {
    for (let r = 0; r < grid[c].length; r++) {
      const value = grid[c][r];
      if (value > 1) {
        points.push({
          point: { c, r }, 
          overlaps: value
        })
      }
    }
  }
  return points;
}

const run = async () => {
  const rawInput = await getInput(5);
  const data = parseInput(rawInput);
  const { width, height } = getGridSize(data);
  const grid = plotLines(createGrid(width, height), data);
  const overlaps = getOverlappingPoints(grid).length;

  console.log(overlaps);
};

run();
