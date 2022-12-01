const { getInput } = require("./common-functions");

/**
 * @typedef { Object } Point A position in a two dimensional grid presented in R1C1 format.
 * @property { number } Point.r The position along the vertical axis (aka the row) 
 * @property { number } Point.c The position along the horizontal axis (aka the column)
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
 * @typedef { Object } LineInfo
 * @property { Axis } LineInfo.axis
 * @property { Direction } LineInfo.direction
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

  const { axis, direction } = getLineInfo(start, end);
  return { start, end, axis, direction };
};

/**
 * Parses a point.
 * Note that the points on the raw input are in the format of 'x,y', which is 'c,r' in R1C1 format.
 * @param {string} rawInput
 * @returns {Point}
 */
const parsePoint = (rawInput) => {
  const [c, r] = rawInput.split(",");
  return { r: Number(r.trim()), c: Number(c.trim()) };
};

/**
 * @param {Point} start
 * @param {Point} end
 * @returns {LineInfo}
 */
const getLineInfo = (start, end) => {
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
 * Returns the opposite direction
 * @param {Direction} direction
 * @returns {Direction}
 */
const flipDirection = (direction) => {
  // prettier-ignore
  switch(direction) {
    case 'N': return 'S';
    case 'E': return 'E';
    case 'S': return 'N';
    case 'W': return 'E';
    case 'NE': return 'SW';
    case 'SE': return 'NW';
    case 'SW': return 'NE';
    case 'NW': return 'SE';
  }
};

/**
 * Flips the line so it's travelling in the opposite direction
 * @param {Line} line
 * @returns { Line }
 */
const flipLine = ({ start, end, direction, axis }) => ({
  axis,
  start: end,
  end: start,
  direction: flipDirection(direction),
});

/**
 * Returns a new line that ensure's it is being drawn in a positive direction.
 * @param {Line} line
 * @returns {Line}
 */
const normaliseLine = (line) => {
  if (line.axis === "diagonal") return normaliseOrdinalLine(line);
  else return normaliseCardinalLine(line);
};

/**
 * Handles the normalisation of a line travelling along either the horizontal or vertical axis.
 * Use this function to ensure a line travels along the horizontal or vertical axis in a *positive* direction.
 * @param {Line} line
 * @returns {Line}
 */
const normaliseCardinalLine = (line) => {
  if (line.direction === "N" || line.direction === "W") {
    return flipLine(line);
  }
  return line;
};

/**
 * Handles the normalisation of a line travelling along either the diagonal axis.
 * Use this function to ensure a line travels positively along the vertical axis, regardless of
 * which the direction of travel along the horizontal axis.
 * @param {Line} line
 * @returns {Line}
 */
const normaliseOrdinalLine = (line) => {
  if (line.direction === "NE" || line.direction === "NW") {
    return flipLine(line);
  }
  return line;
};

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
const createGrid = (width, height) =>
  Array.from(Array(width), () => Array(height).fill(0));

/**
 * @param {Grid} grid
 * @param {Line[]} lines
 * @param {number} partNo
 * @returns {Grid}
 */
const plotLines = (grid, lines, partNo) => {
  for (const line of lines) {
    const normalisedLine = normaliseLine(line);
    switch (line.axis) {
      case "diagonal":
        if (partNo === 2) plotDiagonalLine(grid, normalisedLine);
        break;
      case "horizontal":
        plotHorizontalLine(grid, normalisedLine);
        break;
      case "vertical":
        plotVerticalLine(grid, normalisedLine);
        break;
    }
  }
  return grid;
};

/**
 * @param {Grid} grid
 * @param {Line} line
 */
const plotHorizontalLine = (grid, line) => {
  for (let col = line.start.c; col <= line.end.c; col++) {
    grid[line.start.r][col] += 1;
  }
};

/**
 * @param {Grid} grid
 * @param {Line} line
 */
const plotVerticalLine = (grid, line) => {
  for (let row = line.start.r; row <= line.end.r; row++) {
    grid[row][line.start.c] += 1;
  }
};

/**
 * @param {Grid} grid
 * @param {Line} line
 * @copyright https://github.com/JakobMakovac/
 * @see https://github.com/JakobMakovac/advent-of-code/blob/ea430b56bd3d44734ef1d97d5c4b4461da2e9712/2021/day5/day5.js#L89
 */
const plotDiagonalLine = (grid, line) => {
  const delta = Math.abs(line.end.c - line.start.c);
  for (let i = 0; i <= delta; i++) {
    const c = line.start.c + (line.start.c < line.end.c ? i : - i);
    const r = line.start.r + (line.start.r < line.end.r ? i : - i);

    grid[r][c]++;
  }
};

/**
 * @param {Grid} grid
 * @returns {{point: Point, overlaps: number}[]}
 */
const getOverlappingPoints = (grid) => {
  const points = [];

  for (let r = 0; r < grid.length; r++) {
    for (let c = 0; c < grid[r].length; c++) {
      const value = grid[r][c];
      if (value > 1) {
        points.push({
          point: { r, c },
          overlaps: value,
        });
      }
    }
  }
  return points;
};

const run = async (partNo) => {
  const rawInput = await getInput(5);
  const data = parseInput(rawInput);
  const { width, height } = getGridSize(data);
  const grid = plotLines(createGrid(width, height), data, partNo);
  const overlaps = getOverlappingPoints(grid).length;

  console.log(overlaps);
};

run(1);
run(2);
