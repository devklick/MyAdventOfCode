const { getInput } = require("./common-functions");
/**
 * @typedef { Object } InputData
 * @property { BingoDraws } InputData.draws
 * @property { BingoCard[] } InputData.cards
 */
/**
 * @typedef { number[] } BingoDraws
 */
/**
 * @typedef { Object } BingoCard
 * @property { number } BingoCard.cardRef
 * @property { BingoNumber[][] } BingoCard.numbers
 * @property { boolean } BingoCard.inPlay
 */
/**
 * @typedef { Object } BingoNumber
 * @property { number } BingoNumber.value
 * @property { boolean } BingoNumber.hit
 */
/**
 * @typedef {BingoCard & { lastDrawnNumber: number}} BingoWinner
 */

/**
 * @param { string } rawInput
 * @returns { InputData }
 */
const parseInput = (rawInput) => {
  const lines = rawInput.split("\n");
  const draws = lines[0].split(",").map((n) => Number(n));
  /**
   * @type {BingoCard[]}
   */
  const cards = [];

  let cardRef = 1;
  for (
    let boardStartLine = 1;
    boardStartLine < lines.length;
    boardStartLine++
  ) {
    // Skip empty lines, incrementing the counter
    if (!lines[boardStartLine].trim()) {
      continue;
    }

    // We've found a non-empty line; start an inner counter
    // The +1 here is to cater for data where the last line is non-empty but is the end of the board.
    for (
      let boardEndLine = boardStartLine;
      boardEndLine < lines.length - boardStartLine + 1;
      boardEndLine++
    ) {
      // increment the inner counter until we find the next empty line
      const line = lines[boardEndLine];
      if (!line || !line.trim()) {
        // Pass all those non-empty lines through to buildCard,
        // increment the outer counter to reflect the lines counted by the inner counter,
        // and break out of the inner loop.
        cards.push(
          buildCard(cardRef, lines.slice(boardStartLine, boardEndLine))
        );
        cardRef++;

        // Increment the outer counter by the inner counter -1.
        // The -1 is to account for the automatic increment of the outer
        // counter on it's next itteration.
        boardStartLine = boardEndLine - 1;
        break;
      }
    }
  }
  return { cards, draws };
};

/**
 * @param {number} cardRef
 * @param {string[]} lines
 * @returns {BingoCard}
 */
const buildCard = (cardRef, lines) => {
  const numbers = [];
  for (let r = 0; r < lines.length; r++) {
    numbers[r] = [];
    const values = lines[r].split(" ").filter((v) => v);
    for (let c = 0; c < values.length; c++) {
      numbers[r][c] = {
        value: Number(values[c]),
        hit: false,
      };
    }
  }
  return { numbers, cardRef, inPlay: true };
};

/**
 * Runs through all number draws and updates each card, checking off the number if it's on the card.
 * Returns all cards in the order they won.
 * @param { BingoCard[] } cards
 * @param { BingoDraws } draws
 * @returns {BingoWinner[]}
 */
const playBingo = (cards, draws) => {
  const winners = [];
  for (const draw of draws) {
    for (const card of cards) {
      if (!card.inPlay) continue;
      const win = updateCard(card, draw);
      if (win) {
        card.inPlay = false;
        winners.push({ ...card, lastDrawnNumber: draw })
      };
    }
  }
  return winners;
};

/**
 * Checks if the specified value is on the specified card, and if so, checks it off.
 * Returns true or false to indicate whether that card has won.
 * @param {BingoCard} card
 * @param {number} value
 * @returns {boolean}
 */
const updateCard = (card, value) => {
  for (let r = 0; r < card.numbers.length; r++) {
    for (let c = 0; c < card.numbers[r].length; c++) {
      if (card.numbers[r][c].value === value) {
        card.numbers[r][c].hit = true;

        if (winningCard(card, r, c)) {
          return true;
        }
      }
    }
  }
  return false;
};

/**
 * Checks if the specified card has hit all numbers on either a horizontal
 * or vertical axis starting from the number at the position rStart, cStart
 * @param {BingoCard} card
 * @param {number} rStart
 * @param {number} cStart
 * @returns {boolean}
 */
const winningCard = (card, rStart, cStart) =>
  horizontalWin(card, rStart) || verticalWin(card, cStart);

/**
 * Checks if the specified card has all hit all numbers along the
 * varitical axis of the specified column
 * @param {BingoCard} card
 * @param {number} c
 * @returns {boolean}
 */
const verticalWin = (card, c) => {
  for (let r = 0; r < card.numbers.length; r++) {
    if (!card.numbers[r][c].hit) return false;
  }
  return true;
};

/**
 * Checks if the specified card has all hit all numbers along the
 * horizontal axis of the specified row
 * @param {BingoCard} card
 * @param {number} r
 * @returns {boolean}
 */
const horizontalWin = (card, r) => {
  for (let c = 0; c < card.numbers[r].length; c++) {
    if (!card.numbers[r][c].hit) return false;
  }
  return true;
};

/**
 * Calulates the final score of the winning card.
 * @param {BingoWinner} winner
 * @returns {number}
 */
const calculateScore = (winner) => {
  let score = 0;
  for (let r = 0; r < winner.numbers.length; r++) {
    for (let c = 0; c < winner.numbers[r].length; c++) {
      const number = winner.numbers[r][c];
      if (!number.hit) score += number.value;
    }
  }
  return score * winner.lastDrawnNumber;
};

const run = async () => {
  const rawInput = await getInput(4);
  const data = parseInput(rawInput);
  const winners = playBingo(data.cards, data.draws);

  const winner = winners[0];
  const winnerScore = calculateScore(winner);
  console.log(winnerScore);

  const loser = winners[winners.length - 1];
  const loserScore = calculateScore(loser);
  console.log(loserScore);
};

run();
