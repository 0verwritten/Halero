import { GameState } from "../models/game-state-model";

export interface IGamePlayer{
    snakeColor: string;
    score: number;
    gameState: GameState;

    // used for multiplayer
    socket: WebSocket | undefined;

    onScoreChange: ((score: number) => void) | undefined;
    onGameStateChange: ((gameState: GameState) => void) | undefined;

    // returns snake current tail location
    getTail(): { tail: Array<[number, number]> };

    // updates the frame
    drawSnake(): void;

    // starts game
    startGame(): void;

    // sets game on pause
    pauseGame(): void;

    // gets new candy location is there is one
    updateData(candies: Array<[number, number]>, opponent: IGamePlayer | undefined): void;
    
    // checks is current snakes intercapts with another
    checkSnakeInterception(player: {tail: Array<[number, number]>}): boolean;
    
    //// Features in most for main player
    // changes snake's direction
    changeDirection(event: KeyboardEvent): void;

}