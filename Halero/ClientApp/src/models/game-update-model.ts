import { SnakeDirection } from "./snake-direction-model"

export type GameUpdate = {
    ID: string,
    score: number,
    gameOver: boolean
    gamePause: boolean,
    direction: SnakeDirection,
    tail: Array<[number, number]>,
    candies: Array<[number, number]>,
}